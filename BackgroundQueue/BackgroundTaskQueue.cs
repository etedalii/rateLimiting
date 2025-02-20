using System.Collections.Concurrent;

namespace RateLimiting.BackgroundQueue
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            _workItems.Enqueue(workItem);
            _signal.Release(); // Signal that a new task is available
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken); // Wait until a task is available
            _workItems.TryDequeue(out var workItem);
            return workItem!;
        }
    }

}