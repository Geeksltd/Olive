
// TODO:
// Implement the same functionality as Olive.Aws.EventBus.
// Instead of a real queue, it should use an IO Queue.
// The queue Url should become path safe, named {QueueFolder}. Also drop the beginning http:// or https://
// If it does not exist, a folder should be created in %Temp%\Olive\IO.Queue\{QueueFolder}
// Inside that folder, one file should be created per published message.
// Everytime a message is published, it should do a Thread.Sleep(50) to ensure a bit of time has passed. Then a file named {DateTime.UtcNow.ToOADate()} is created.

// The subscriber will work under two triggers:
//    - When first called, it should go through that folder, and fire the Subscribe event for each file (sorted by file name, so the earliest message is processed first).
//    - After that it should register a FileSystemWatcher, and every time a file is added, it should be trigger also.
// Once the message is processed, the file shall be deleted.

// In Startup.cs file, under ConfigureServices, we will call services.AddIOEventBus();