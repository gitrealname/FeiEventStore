using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace FeiEventStore.EventQueue
{

    public class BroadcastCell
    {
        public long CurrentVersion;

        public ConcurrentDictionary<long, TaskCompletionSource<bool>> VersionAwaiterMap;

        public BroadcastCell()
        {
            VersionAwaiterMap = new ConcurrentDictionary<long, TaskCompletionSource<bool>>();
        }
    }

    public class EventQueueAwaiter : IEventQueueAwaiter
    {
        private readonly ConcurrentDictionary<TypeId, BroadcastCell> _projectionMap = new ConcurrentDictionary<TypeId, BroadcastCell>(); 

        public void Post(TypeId queueTypeId, long version)
        {
            var cell = new BroadcastCell();
            cell = _projectionMap.GetOrAdd(queueTypeId, cell);

            Interlocked.Exchange(ref cell.CurrentVersion, version);

            var reached = cell.VersionAwaiterMap.Keys.Where(v => v <= version);
            foreach(var v in reached)
            {
                TaskCompletionSource<bool> tcs;
                if(cell.VersionAwaiterMap.TryRemove(v, out tcs))
                {
                    tcs.SetResult(true);
                }
            }
        }


        private async Task AwaitWithTimeout(Task task, long timeout, CancellationToken cancellationToken, Action<Task> success,  Action error)
        {
            if(await Task.WhenAny(task, Task.Delay((int)timeout, cancellationToken)).ConfigureAwait(false) == task)
            {
                success(task);
            } else
            {
                error();
            }
        }

        public async Task<bool> AwaitAsync(TypeId queueTypeId, long version, long timeoutMsc, CancellationToken cancellationToken)
        {
            var cell = new BroadcastCell();
            cell = _projectionMap.GetOrAdd(queueTypeId, cell);

            if(cell.CurrentVersion >= version)
            {
                return true;
            }

            var tcs = new TaskCompletionSource<bool>();
            tcs = cell.VersionAwaiterMap.GetOrAdd(version, tcs);

            //test version again
            if(cell.CurrentVersion >= version)
            {
                return true;
            }


            bool timedout = false;
            await AwaitWithTimeout(tcs.Task, timeoutMsc, cancellationToken, (t) => { }, () => { timedout = true; });

            return !timedout;
        }

        public Task<bool> AwaitAsync(TypeId queueTypeId, long version, long timeoutMsc)
        {
            return AwaitAsync(queueTypeId, version, timeoutMsc, CancellationToken.None);
        }
    }
}
