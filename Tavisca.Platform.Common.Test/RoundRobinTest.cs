using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using System.Linq;
using Tavisca.Platform.Common.Internal;

namespace Tavisca.Libraries.Tasks.Tests
{
    public class RoundRobinTest
    {

        [Fact]
        public void Tasks_Should_Be_Add_in_RoundRobin_Manner()
        {
            //Arrange
            var threadId = new List<int>();
            var lockObject = new Object();
            var waitHandle = new CountdownEvent(10);
            RoundRobinPool roundRobinPool = new RoundRobinPool(3);

            //Act
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            roundRobinPool.Enqueue(() => getTask(lockObject, waitHandle, threadId));
            waitHandle.Wait();

            //Assert
            var threadCounts = threadId.GroupBy(x => x).Select(x => x.Count()).OrderByDescending(x => x);
            Assert.Equal(3, threadCounts.Count());
            Assert.Equal(4, threadCounts.FirstOrDefault());
            Assert.Equal(3, threadCounts.LastOrDefault());
        }

        private void getTask(Object lockObject, CountdownEvent waitHandle, List<int> threadId)
        {
            lock (lockObject)
            {
                threadId.Add(Thread.CurrentThread.ManagedThreadId);
            }
            waitHandle.Signal();
        }
    }
}
