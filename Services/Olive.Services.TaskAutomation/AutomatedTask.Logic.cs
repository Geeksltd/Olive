using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Olive.Entities;

namespace Olive.Services.TaskAutomation
{
    partial class AutomatedTask
    {
        Action<AutomatedTask> Action;

        public TimeSpan Intervals { get; set; }

        Task RunnerTask;

        CancellationTokenSource CancellationTokenSource;

        static AsyncLock ExecutionPersistenceAsyncLock = new AsyncLock();

        /// <summary>
        /// Creates a new AutomatedTask instance.
        /// </summary>
        public AutomatedTask(Action<AutomatedTask> action)
            : this()
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));

            CancellationTokenSource = new CancellationTokenSource();

            Status = AutomatedTaskStatus.AwaitingFirstRun;
        }

        public AutomatedTaskStatus Status { get; private set; }

        /// <summary>
        /// Starts this automated task.
        /// </summary>
        public void Start() => RunnerTask = Process(CancellationTokenSource.Token);

        /// <summary>
        /// Restarts this task.
        /// </summary>
        public void Restart()
        {
            try
            {
                CancellationTokenSource.Cancel();
            }
            catch
            {
                // No Logging needed
            }

            CancellationTokenSource = new CancellationTokenSource();
            RunnerTask = Process(CancellationTokenSource.Token);
        }

        #region Persistent Execution Log

        async Task<DateTime> GetInitialNextTry()
        {
            var result = LocalTime.Now;

            using (await ExecutionPersistenceAsyncLock.Lock())
            {
                if (ShouldPersistExecution())
                {
                    var file = GetExecutionStatusPath();

                    if (file.Exists())
                    {
                        try
                        {
                            var content = await file.ReadAllText();
                            if (content.HasValue())
                            {
                                var taskData = XElement.Parse(content).Elements().FirstOrDefault(e => e.GetValue<string>("@Name") == Name);

                                if (taskData != null)
                                {
                                    result = DateTime.FromOADate(taskData.GetValue<string>("@LastRun").To<double>()).ToLocalTime();
                                    result = result.Add(Intervals);
                                }
                            }
                        }
                        catch
                        {
                            // No Logging needed
                            // The file is perhaps corrupted.
                        }
                    }
                }
            }

            return result;
        }

        static FileInfo GetExecutionStatusPath()
        {
            var result = Config.Get("Automated.Tasks:Status.Path");

            if (result.HasValue())
            {
                if (!result.StartsWith("\\") && result[1] != ':')
                {
                    // Relative pth:
                    result = AppDomain.CurrentDomain.GetPath(result);
                }

                result.AsFile().Directory.EnsureExists();
                return result.AsFile();
            }

            return Blob.GetPhysicalFilesRoot(Blob.AccessMode.Secure).EnsureExists().GetFile("AutomatedTasks.Status.xml");
        }

        static bool ShouldPersistExecution() => Config.Get("Automated.Tasks:Persist.Execution", defaultValue: false);

        public static async Task DeleteExecutionStatusHistory()
        {
            using (await ExecutionPersistenceAsyncLock.Lock())
                await GetExecutionStatusPath().Delete(harshly: true);
        }

        async Task PersistExecution()
        {
            if (!ShouldPersistExecution()) return;

            var path = GetExecutionStatusPath();

            using (await ExecutionPersistenceAsyncLock.Lock())
            {
                var data = new XElement("Tasks");

                if (path.Exists())
                {
                    await new Func<Task>(async () =>
                    {
                        try
                        {
                            var content = await path.ReadAllText();
                            if (content.HasValue()) data = XElement.Parse(content);
                        }
                        catch (FileNotFoundException)
                        {
                            // Somehow another thread has deleted it.
                        }
                    }).Invoke(10, TimeSpan.FromMilliseconds(300));
                }

                var element = data.Elements().FirstOrDefault(e => e.GetValue<string>("@Name") == Name);

                if (element == null)
                    data.Add(new XElement("Task", new XAttribute("Name", Name), new XAttribute("LastRun", LocalTime.Now.ToUniversalTime().ToOADate().ToString())));
                else
                    element.Attribute("LastRun").Value = LocalTime.Now.ToUniversalTime().ToOADate().ToString();

                try
                {
                    await path.WriteAllText(data.ToString());
                }
                catch
                {
                    // No Logging needed
                    // Error?
                }
            }
        }

        #endregion

        [System.Diagnostics.DebuggerStepThrough]
        async Task Process(CancellationToken cancellationToken)
        {
            NextTry = await GetInitialNextTry();

            // Startup delay:
            if (Delay > TimeSpan.Zero)
            {
                NextTry = NextTry.Value.Add(Delay);
                await Task.Delay(Delay, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested) return;

            // Should we still wait?
            var stillWait = NextTry.Value - LocalTime.Now;
            if (stillWait.TotalMilliseconds > int.MaxValue) await Task.Delay(int.MaxValue, cancellationToken);
            else if (stillWait > TimeSpan.Zero) await Task.Delay(stillWait, cancellationToken);

            for (; /* ever */ ; )
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Execute();

                if (cancellationToken.IsCancellationRequested)
                    break;

                // Now wait for the next itnerval:
                await WaitEnough(cancellationToken);
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        async Task WaitEnough(CancellationToken cancellationToken) => await Task.Delay(Intervals, cancellationToken);

        public async Task Execute()
        {
            NextTry = null;

            if (AsyncGroup != null)
            {
                Status = AutomatedTaskStatus.WaitingForLock;
                using (await AsyncGroup.Lock()) await DoExecute();
            }
            else
            {
                await DoExecute();
            }

            NextTry = LocalTime.Now.Add(Intervals);
        }

        async Task DoExecute()
        {
            CurrentStartTime = LastRunStart = LocalTime.Now;

            try
            {
                Status = AutomatedTaskStatus.Running;
                Action?.Invoke(this);

                if (RecordSuccess)
                {
                    try { await ApplicationEventManager.RecordScheduledTask(Name, CurrentStartTime.Value); }
                    catch { /*Problem in logging*/ }
                }

                Status = AutomatedTaskStatus.CompletedAwaitingNextRun;
            }
            catch (Exception ex)
            {
                // if (!WebTestManager.IsTddExecutionMode())
                {
                    if (RecordFailure)
                    {
                        try { await ApplicationEventManager.RecordScheduledTask(Name, CurrentStartTime.Value, ex); }
                        catch { /*Problem in logging*/ }
                    }
                }

                Status = AutomatedTaskStatus.FailedAwaitingNextRun;
            }
            finally
            {
                CurrentStartTime = null;
                LastRunEnd = LocalTime.Now;
                await PersistExecution();
            }
        }

        public static IEnumerable<AutomatedTask> GetAllTasks()
        {
            var classes = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("TaskManager")).ExceptNull().Distinct().ToList();

            if (classes.None())
                throw new Exception("There is no class named TaskManager in the current application domain.");

            if (classes.HasMany())
                throw new Exception("There are multiple classes named TaskManager in the current application domain.");

            var tasks = classes.First().GetProperty("Tasks", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null)
                 as IEnumerable<AutomatedTask>;

            if (tasks == null)
                throw new Exception("Class TaskManager doesn't have a property named Tasks of type IEnumerable<AutomatedTask>.");

            return tasks;
        }
    }
}