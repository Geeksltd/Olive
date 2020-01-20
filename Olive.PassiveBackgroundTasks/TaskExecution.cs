﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.PassiveBackgroundTasks
{
    class TaskExecution : IDisposable
    {
        IBackgourndTask BackgroundTask;
        CancellationTokenSource CancellationTokenSource;
        CancellationToken CancellationToken;
        Task HeartbeatTask;
        TaskExecution()
        {

        }

        TaskExecution(IBackgourndTask backgroundTask)
        {
            BackgroundTask = backgroundTask;
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            HeartbeatTask = CreateHeartbeatTask();
        }

        internal static async Task Run(IBackgourndTask task)
        {
            using (var runner = new TaskExecution(task))
            {
                await runner.DoRun();
            }
        }

        async Task DoRun()
        {
            var start = LocalTime.Now;
            Log.For(this).Info($"Execution started for {BackgroundTask.Name} at {start}");

            if (await EnsureIAmTheOnlyRunningInstance() == false) return;

            Task.Run(() => HeartbeatTask);

            await BackgroundTask.GetActionTask();

            await BackgroundTask.RecordExecution();

            Log.For(this).Info($"Execution Finished for {BackgroundTask.Name} at {LocalTime.Now}. Took : {LocalTime.Now.Subtract(start).ToNaturalTime()}");
        }

        async Task<bool> EnsureIAmTheOnlyRunningInstance()
        {
            return (await Context.Current.Database().Reload(BackgroundTask)).ExecutingInstance == ExecutionEngine.Id;
        }

        const int HEARTBEAT_DELAY = 1000;
        private Task CreateHeartbeatTask()
        {
            return Task.Run(async () =>
            {
                while (true)
                {

                    if (CancellationToken.IsCancellationRequested)
                        return;
                    try
                    {
                        BackgroundTask = await BackgroundTask.SendHeartbeat();
                    }
                    catch (Exception ex)
                    {
                        Log.For(this).Error(ex, "Failed to log heartbeat.");
                    }

                    await Task.Delay(HEARTBEAT_DELAY);
                }
            }, CancellationToken);
        }

        public void Dispose()
        {
            CancellationTokenSource?.Cancel();
        }
    }
}
