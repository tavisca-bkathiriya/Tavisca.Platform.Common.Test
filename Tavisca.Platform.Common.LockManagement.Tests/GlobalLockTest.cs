using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Libraries.Lock;
using Tavisca.Platform.Common.ConfigurationHandler;
using Tavisca.Platform.Common.LockManagement;
using Xunit;

namespace Tavisca.Libraries.LockManagement.Tests
{
    public class GlobalLockTest
    {
        //Should be able to acquirq read/write lock for any code block in a sync way
        //Calling dispose on read/write lock, should release the lock

        [Fact]
        public async Task Should_be_able_to_acquire_read_and_write_locks_in_async_way()
        {
            ILockProvider lockProvider = new LockProvider();

            var globalLockProvider = new GlobalLock(lockProvider);

            var isReadLockAcquired = false;
            var isWriteLockAcquired = false;
            using (await globalLockProvider.EnterReadLock("test"))
            {
                isReadLockAcquired = true;
            }
            using (await globalLockProvider.EnterWriteLock("test"))
            {
                isWriteLockAcquired = true;
            }
            Assert.True(isReadLockAcquired);
            Assert.True(isWriteLockAcquired);
        }

        [Fact]
        public async Task Should_be_able_to_release_read_and_write_locks_in_async_way()
        {
            ILockProvider lockProvider = new LockProvider();

            var globalLockProvider = new GlobalLock(lockProvider);

            var isReadLockReleasedByPreviousCode = false;
            using (await globalLockProvider.EnterReadLock("test"))
            {
                Thread.Sleep(100);
            }
            using (await globalLockProvider.EnterReadLock("test"))
            {
                isReadLockReleasedByPreviousCode = true;
            }

            var isWriteLockReleasedByPreviousCode = false;
            using (await globalLockProvider.EnterWriteLock("test"))
            {
                Thread.Sleep(100);
            }
            using (await globalLockProvider.EnterWriteLock("test"))
            {
                isWriteLockReleasedByPreviousCode = true;
            }

            Assert.True(isReadLockReleasedByPreviousCode);
            Assert.True(isWriteLockReleasedByPreviousCode);
        }

        [Fact]
        public async Task ReadWriteLocks_Should_Wait_For_Completion_Of_WriteLock()
        {
            ILockProvider lockProvider = new LockProvider();

            var globalLockProvider = new GlobalLock(lockProvider);
            Func<Task<DateTime>> writeLockBlockingAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> writeLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> readLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                }
                return lockAcquiredTime;
            };

            Task<DateTime> dateTimeTask1 = null, dateTimeTask2 = null, dateTimeTask3 = null;
            Parallel.Invoke(
                () => { dateTimeTask1 = writeLockBlockingAction(); },
                () => {
                    Thread.Sleep(100);
                    dateTimeTask2 = writeLockAction();
                },
                () => {
                    Thread.Sleep(100);
                    dateTimeTask3 = readLockAction();
                }
            );

            var dateTime1 = dateTimeTask1.Result;
            var dateTime2 = dateTimeTask2.Result;
            var dateTime3 = dateTimeTask3.Result;

            var timeDiff = (dateTime3 - dateTime1).TotalMilliseconds;
            Assert.True(timeDiff > 500);

            var timeDiff1 = (dateTime2 - dateTime1).TotalMilliseconds;
            Assert.True(timeDiff1 > 500);
        }

        //If the object acquire read lock, all the read/write operations should wait for its completion.

        [Fact]
        public async Task ReadWriteLocks_Should_Wait_For_Completion_Of_ReadLock()
        {
            ILockProvider lockProvider = new LockProvider();

            var globalLockProvider = new GlobalLock(lockProvider);
            Func<Task<DateTime>> readLockBlockingAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> writeLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> readLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                }
                return lockAcquiredTime;
            };

            Task<DateTime> dateTimeTask1 = null, dateTimeTask2 = null, dateTimeTask3 = null;
            Parallel.Invoke(
                () => { dateTimeTask1 = readLockBlockingAction(); },
                () => {
                    Thread.Sleep(100);
                    dateTimeTask2 = writeLockAction();
                },
                () => {
                    Thread.Sleep(100);
                    dateTimeTask3 = readLockAction();
                }
            );

            var dateTime1 = dateTimeTask1.Result;
            var dateTime2 = dateTimeTask2.Result;
            var dateTime3 = dateTimeTask3.Result;

            var timeDiff = (dateTime2 - dateTime1).TotalMilliseconds;
            Assert.True(timeDiff > 500);

            var timeDiff1 = (dateTime3 - dateTime1).TotalMilliseconds;
            Assert.True(timeDiff1 > 500);
        }


        [Fact]
        public async Task ReadWrite_Operations_In_Queue_Should_Be_Executed_Sequentially()
        {
            ILockProvider lockProvider = new LockProvider();

            var globalLockProvider = new GlobalLock(lockProvider);
            Func<Task<DateTime>> readLockBlockingAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> writeLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> readLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Task<DateTime> dateTimeTask1 = null, dateTimeTask2 = null, dateTimeTask3 = null;
            Parallel.Invoke(
                () => { dateTimeTask1 = readLockBlockingAction(); },
                () => {
                    Thread.Sleep(100);
                    dateTimeTask2 = writeLockAction();
                },
                () => {
                    Thread.Sleep(100);
                    dateTimeTask3 = readLockAction();
                }
            );

            var dateTime1 = dateTimeTask1.Result;
            var dateTime2 = dateTimeTask2.Result;
            var dateTime3 = dateTimeTask3.Result;

            var timeDiff = Math.Abs((dateTime3 - dateTime2).TotalMilliseconds);
            Assert.True(timeDiff > 500);
        }

        //In case any operation is waiting to acquire the lock, it will retry based on configured retry policy

        [Fact]
        public async Task Retry_based_on_configured_policy()
        {
            ILockProvider lockProvider = new LockProvider();
            var exponentialRetryControllerObj = new ExponentialRetryController(new ExponentialRetrySettingsProvider());
            var globalLockProvider = new GlobalLock(lockProvider, exponentialRetryControllerObj);//
            Func<Task<DateTime>> writeLockBlockingAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> writeLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                }
                return lockAcquiredTime;
            };

            Task<DateTime> dateTimeTask1 = null, dateTimeTask2 = null;
            Parallel.Invoke(
                () => { dateTimeTask1 = writeLockBlockingAction(); },
                () => {
                    Thread.Sleep(20);
                    dateTimeTask2 = writeLockAction();
                }
            );

            var dateTime1 = dateTimeTask1.Result;
            var dateTime2 = dateTimeTask2.Result;

            var timeDiff = (dateTime2 - dateTime1).TotalMilliseconds;
            Assert.InRange(timeDiff, 1200, 1400);            // (int)Math.Pow(3, _retryCount) * 10;) for ExponentialRetry. 30 + 90 + 270 + 810 = 1200
        }

        [Fact]
        public async Task Retry_based_on_configured_linear_policy()
        {
            ILockProvider lockProvider = new LockProvider();
            var linearRetryControllerObj = new LinearRetryController(new LinearRetrySettingsProvider());
            var globalLockProvider = new GlobalLock(lockProvider, linearRetryControllerObj);//
            Func<Task<DateTime>> writeLockBlockingAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                    Thread.Sleep(500);
                }
                return lockAcquiredTime;
            };

            Func<Task<DateTime>> writeLockAction = async () =>
            {
                DateTime lockAcquiredTime = new DateTime();
                using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                {
                    lockAcquiredTime = DateTime.Now;
                }
                return lockAcquiredTime;
            };

            Task<DateTime> dateTimeTask1 = null, dateTimeTask2 = null;
            Parallel.Invoke(
                () => { dateTimeTask1 = writeLockBlockingAction(); },
                () => {
                    Thread.Sleep(20);
                    dateTimeTask2 = writeLockAction();
                }
            );

            var dateTime1 = dateTimeTask1.Result;
            var dateTime2 = dateTimeTask2.Result;

            var timeDiff = (dateTime2 - dateTime1).TotalMilliseconds;
            Assert.InRange(timeDiff, 600, 800);            // 200 + 200 + 200 = 600
        }

        //In case any operation is not able to acquire he lock within the configured retry policy, it should throw timeout exception

        [Fact]
        public void Should_throw_timeout_exception_if_Lock_cannot_be_acquired_within_configured_retrypolicy()
        {
            ILockProvider lockProvider = new LockProvider();
            var globalLockProvider = new GlobalLock(lockProvider);
            var timeOutException = false;

            Func<Task> writeLockBlockingAction = async () =>
            {
                using (var globalLock = await globalLockProvider.EnterReadLock("test"))
                {
                    Thread.Sleep(3000);
                }
            };

            Func<Task> writeLockAction = async () =>
            {
                Thread.Sleep(100);
                try
                {
                    using (var globalLock = await globalLockProvider.EnterWriteLock("test"))
                    {
                    }
                }
                catch (TimeoutException ex)
                {
                    timeOutException = true;
                }
            };
            Parallel.Invoke(async () => { await writeLockBlockingAction(); }, async () => { await writeLockAction(); });
            Assert.True(timeOutException);
        }
    }
}
