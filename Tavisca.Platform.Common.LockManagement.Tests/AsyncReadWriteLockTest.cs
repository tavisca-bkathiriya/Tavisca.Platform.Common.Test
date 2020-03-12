using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Xunit;

namespace Tavisca.Libraries.LockManagement.Tests
{
    public class AsyncReadWriteLockTest
    {
        private Func<AsyncReadWriteLock, CountdownEvent, Task<DateTime>> asyncReadLockAction =
           async (asyncLock, waitHandle) =>
           {
               using (await asyncLock.ReadLockAsync())
               {
                   Thread.Sleep(2000);
                   waitHandle.Signal();
                   return DateTime.Now;
               }
           };

        private Func<AsyncReadWriteLock, CountdownEvent, Task<DateTime>> asyncWriteLockAction =
        async (asyncLock, waitHandle) =>
        {
            using (await asyncLock.WriteLockAsync())
            {
                Thread.Sleep(2000);
                waitHandle.Signal();
                return DateTime.Now;
            }
        };

        [Fact]
        public async Task Should_Be_Able_To_Acquire_Read_And_Write_Lock_In_Async_Code()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            var isReadLockAcquired = false;
            var isWriteLockAcquired = false;
            using (await asyncLock.ReadLockAsync())
            {
                isReadLockAcquired = true;
            }
            using (await asyncLock.WriteLockAsync())
            {
                isWriteLockAcquired = true;
            }
            Assert.True(isReadLockAcquired);
            Assert.True(isWriteLockAcquired);
        }


        [Fact]
        public async Task Calling_Dispose_Should_Release_Read_Write_Lock()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            var isReadLockReleasedByPreviousCode = false;
            using (await asyncLock.ReadLockAsync())
            {
                Thread.Sleep(100);
            }
            using (await asyncLock.ReadLockAsync())
            {
                isReadLockReleasedByPreviousCode = true;
            }

            var isWriteLockReleasedByPreviousCode = false;
            using (await asyncLock.WriteLockAsync())
            {
                Thread.Sleep(100);
            }
            using (await asyncLock.WriteLockAsync())
            {
                isWriteLockReleasedByPreviousCode = true;
            }

            Assert.True(isReadLockReleasedByPreviousCode);
            Assert.True(isWriteLockReleasedByPreviousCode);
        }


        [Fact]
        public async Task All_Read_Write_Lock_Should_Wait_For_Completion_Of_Write_Lock()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            CountdownEvent waitHandle = new CountdownEvent(3);
            Task<DateTime> threadTime1Task = null, threadTime2Task = null, threadTime3Task = null;

            Parallel.Invoke(
                () =>
                {
                    threadTime1Task = asyncWriteLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime2Task = asyncReadLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime3Task = asyncWriteLockAction(asyncLock, waitHandle);
                });

            var threadTime1 = threadTime1Task.Result;
            var threadTime2 = threadTime2Task.Result;
            var threadTime3 = threadTime3Task.Result;
            waitHandle.Wait();

            var timeDiffWriteLock = (threadTime3 - threadTime1).TotalMilliseconds;
            var timeDiffReadLock = (threadTime2 - threadTime1).TotalMilliseconds;
            Assert.True(timeDiffWriteLock > 2000);
            Assert.True(timeDiffReadLock > 2000);
        }

        [Fact]
        public async Task Prefrence_For_Write_Operation_When_ReadWrite_Operations_In_Queue()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            CountdownEvent waitHandle = new CountdownEvent(3);
            Task<DateTime> threadTime1Task = null, threadTime2Task = null, threadTime3Task = null;

            //First acquire write lock then put read & write lock in wait (first read, then write lock)
            Parallel.Invoke(
                () =>
                {
                    threadTime1Task = asyncWriteLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime2Task = asyncReadLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(500);
                    threadTime3Task = asyncWriteLockAction(asyncLock, waitHandle);
                });

            var threadTime1 = threadTime1Task.Result;
            var threadTime2 = threadTime2Task.Result;
            var threadTime3 = threadTime3Task.Result;
            waitHandle.Wait();
            var timeDiffWriteLock = (threadTime3 - threadTime1).TotalMilliseconds;
            var timeDiffReadLock = (threadTime2 - threadTime1).TotalMilliseconds;
            Assert.InRange(timeDiffWriteLock, 2000, 2500);
            Assert.InRange(timeDiffReadLock, 4000, 4500);
        }

        [Fact]
        public void Multiple_Read_Operation_In_Parallel()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            CountdownEvent waitHandle = new CountdownEvent(3);

            DateTime threadTime1 = DateTime.Now, threadTime2 = DateTime.Now, threadTime3 = DateTime.Now;
            Parallel.Invoke(async () => { threadTime1 = await asyncReadLockAction(asyncLock, waitHandle); },
                async () => { threadTime2 = await asyncReadLockAction(asyncLock, waitHandle); },
                async () => { threadTime3 = await asyncReadLockAction(asyncLock, waitHandle); });
            waitHandle.Wait();
            var timeDiff = (threadTime2 - threadTime1).TotalMilliseconds;
            var timeDiff1 = (threadTime3 - threadTime1).TotalMilliseconds;
            Assert.InRange(Math.Abs(timeDiff), 0, 100);
            Assert.InRange(Math.Abs(timeDiff1), 0, 100);
        }

        [Fact]
        public async Task WriteLocks_Should_Wait_For_Completion_Of_Read_Operation()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            CountdownEvent waitHandle = new CountdownEvent(3);
            Task<DateTime> threadTime1Task = null, threadTime2Task = null, threadTime3Task = null;

            Parallel.Invoke(
                () =>
                {
                    threadTime1Task = asyncReadLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime2Task = asyncWriteLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime3Task = asyncWriteLockAction(asyncLock, waitHandle);
                });

            var threadTime1 = threadTime1Task.Result;
            var threadTime2 = threadTime2Task.Result;
            var threadTime3 = threadTime3Task.Result;
            waitHandle.Wait();
            var timeDiff1 = (threadTime3 - threadTime1).TotalMilliseconds;
            var timeDiff2 = (threadTime2 - threadTime1).TotalMilliseconds;
            Assert.True(timeDiff1 > 2000);
            Assert.True(timeDiff2 > 2000);
        }

        [Fact]
        public async Task WriteLocks_Waiting_In_Queue_Should_Be_Executed_Sequentially()
        {
            AsyncReadWriteLock asyncLock = new AsyncReadWriteLock();
            CountdownEvent waitHandle = new CountdownEvent(3);
            Task<DateTime> threadTime1Task = null, threadTime2Task = null, threadTime3Task = null;

            Parallel.Invoke(
                () =>
                {
                    threadTime1Task = asyncReadLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime2Task = asyncWriteLockAction(asyncLock, waitHandle);
                },
                () =>
                {
                    Thread.Sleep(200);
                    threadTime3Task = asyncWriteLockAction(asyncLock, waitHandle);
                });

            var threadTime2 = threadTime2Task.Result;
            var threadTime3 = threadTime3Task.Result;
            waitHandle.Wait();
            var timeDiff = Math.Abs((threadTime3 - threadTime2).TotalMilliseconds);
            Assert.True(timeDiff > 2000);
        }
    }
}
