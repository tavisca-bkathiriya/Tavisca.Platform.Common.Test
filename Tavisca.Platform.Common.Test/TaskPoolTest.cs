using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tavisca.Platform.Common.Internal;
using Xunit;

namespace Tavisca.Libraries.Tasks.Tests
{
    public class TaskPoolTest
    {


        [Fact]
        public void TaskPool_Should_Create_and_Enqueue_Actions_in_Thread()
        {
            var threadIds = new List<int>();
            var lockObject = new object();
            var waitHandle = new CountdownEvent(10);

            TaskPool taskPool = new TaskPool(3);
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            taskPool.Enqueue(() => getTask(lockObject, waitHandle, threadIds));
            waitHandle.Wait();
            waitHandle.Reset();
            Assert.Equal(3, threadIds.Distinct().Count());
        }

        private void getTask(Object lockObject, CountdownEvent waitHandle, List<int> threadIds)
        {
            lock (lockObject)
            {
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(1000);
            }
            waitHandle.Signal();
        }
    }
}
