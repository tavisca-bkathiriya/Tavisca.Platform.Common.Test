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
        [Fact]
        public void AsyncLock_Should_Give_Serialise_Access_To_Object_In_Asynchronous_Way()
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
