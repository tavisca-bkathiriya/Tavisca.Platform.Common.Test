using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Xunit;

namespace Tavisca.Libraries.LockManagement.Tests
{
    public class AsyncLockTest
    {
        //Should be able to acquire lock on any code block invoked in an async way
        //Calling dispose on async lock, should release the lock
        //Any thread which is trying to access the code block within async lock should wait for another thread to complete the execution

        [Fact]
        public async Task Should_Be_Able_To_Acquire_Lock_In_Async_Way()
        {
            AsyncLock asyncLock = new AsyncLock();
            var isLockAcquired = false;
            using (await asyncLock.LockAsync())
            {
                isLockAcquired = true;
            }
            Assert.True(isLockAcquired);
        }

        [Fact]
        public async Task Calling_Dispose_Should_Release_Lock()
        {
            AsyncLock asyncLock = new AsyncLock();
            var isLockReleasedByPreviousCode = false;
            using (await asyncLock.LockAsync())
            {
                Thread.Sleep(100);
            }
            using (await asyncLock.LockAsync())
            {
                isLockReleasedByPreviousCode = true;
            }
            Assert.True(isLockReleasedByPreviousCode);
        }

        [Fact]
        public void Thread_Trying_To_Acquire_Lock_Should_Wait_For_Other_Thread_To_Complete_Execution()
        {
            AsyncLock asyncLock = new AsyncLock();
            CountdownEvent waitHandle = new CountdownEvent(2);
            Func<Task<DateTime>> asyncLockAction = async () =>
            {
                using (await asyncLock.LockAsync())
                {
                    Thread.Sleep(2000);
                    waitHandle.Signal();
                    return DateTime.Now;
                }
            };
            DateTime threadTime1 = DateTime.Now, threadTime2 = DateTime.Now;
            Parallel.Invoke(async () => { threadTime1 = await asyncLockAction(); }, async () => { threadTime2 = await asyncLockAction(); });
            waitHandle.Wait();
            var timeDiff = (threadTime2 - threadTime1).TotalMilliseconds;
            Assert.InRange(Math.Abs(timeDiff), 2000, 4000);
        }
    }
}
