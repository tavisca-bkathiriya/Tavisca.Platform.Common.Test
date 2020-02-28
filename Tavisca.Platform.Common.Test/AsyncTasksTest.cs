using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using Tavisca.Platform.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Tavisca.Platform.Common.Internal;

namespace Tavisca.Libraries.Tasks.Tests
{
    public class AsyncTasksTest
    {

        [Fact]
        public void AsyncTask_Should_Run_Actions_In_Created_Pool()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(10);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            AsyncTasks.AddPool("testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            AsyncTasks.Run(runTask, "testPool");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(Environment.ProcessorCount, threadIds.Distinct().Count());
        }

        [Fact]
        public void AsyncTask_Should_Run_Actions_In_Created_Pool_With_GivenSize()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(4);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            int poolSize = 2;
            AsyncTasks.AddPool("testPool1", poolSize);
            AsyncTasks.Run(runTask, "testPool1");
            AsyncTasks.Run(runTask, "testPool1");
            AsyncTasks.Run(runTask, "testPool1");
            AsyncTasks.Run(runTask, "testPool1");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(poolSize, threadIds.Distinct().Count());
        }

        [Fact]
        public void AsyncTask_Should_Run_Actions_In_Created_Pool_With_GivenSize2()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(4);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            int poolSize = 2;
            var taskPool = new TaskPool(poolSize);
            AsyncTasks.AddPool("testPool2", taskPool);
            AsyncTasks.Run(runTask, "testPool2");
            AsyncTasks.Run(runTask, "testPool2");
            AsyncTasks.Run(runTask, "testPool2");
            AsyncTasks.Run(runTask, "testPool2");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(poolSize, threadIds.Distinct().Count());
        }


        [Fact]
        public void AsyncTask_Should_Run_Actions_In_Created_RoundRobinPool()
        {
            var waitHandle = new CountdownEvent(10);

            Func<int> runTask = () => {
                waitHandle.Signal();
                return Thread.CurrentThread.ManagedThreadId;
            };

            int threadId1 = 0, threadId2 = 0, threadId3 = 0, threadId4 = 0, threadId5 = 0;
            int threadId6 = 0, threadId7 = 0, threadId8 = 0, threadId9 = 0, threadId10 = 0;
            AsyncTasks.UseRoundRobinPool();
            AsyncTasks.AddPool("testPool3");
            AsyncTasks.Run(() => { threadId1 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId2 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId3 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId4 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId5 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId6 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId7 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId8 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId9 = runTask(); }, "testPool3");
            AsyncTasks.Run(() => { threadId10 = runTask(); }, "testPool3");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            //Works only if Environment.ProcessorCount = 8
            Assert.Equal(threadId1, threadId9);
            Assert.Equal(threadId2, threadId10);
            Assert.NotEqual(threadId1, threadId6);
            Assert.NotEqual(threadId4, threadId7);
        }

        [Fact]
        public void AsyncTask_Should_Run_Actions_In_Created_RoundRobinPool_With_GivenSize()
        {
            var waitHandle = new CountdownEvent(5);

            Func<int> runTask = () => {
                waitHandle.Signal();
                return Thread.CurrentThread.ManagedThreadId;
            };

            int poolSize = 2;
            int threadId1 = 0, threadId2 = 0, threadId3 = 0, threadId4 = 0, threadId5 = 0;
            AsyncTasks.UseRoundRobinPool();
            AsyncTasks.AddPool("testPool4", poolSize);
            AsyncTasks.Run(() => { threadId1 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId2 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId3 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId4 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId5 = runTask(); }, "testPool4");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(threadId1, threadId3);
            Assert.Equal(threadId1, threadId5);
            Assert.Equal(threadId2, threadId2);
            Assert.NotEqual(threadId1, threadId2);
        }

        [Fact]
        public void AsyncTask_Should_Run_Actions_In_Created_RoundRobinPool_With_GivenSize2()
        {
            var waitHandle = new CountdownEvent(5);

            Func<int> runTask = () => {
                waitHandle.Signal();
                return Thread.CurrentThread.ManagedThreadId;
            };

            int poolSize = 2;
            int threadId1 = 0, threadId2 = 0, threadId3 = 0, threadId4 = 0, threadId5 = 0;
            var pool = new RoundRobinPool(poolSize);
            AsyncTasks.AddPool("testPool5", pool);
            AsyncTasks.Run(() => { threadId1 = runTask(); }, "testPool5");
            AsyncTasks.Run(() => { threadId2 = runTask(); }, "testPool5");
            AsyncTasks.Run(() => { threadId3 = runTask(); }, "testPool5");
            AsyncTasks.Run(() => { threadId4 = runTask(); }, "testPool5");
            AsyncTasks.Run(() => { threadId5 = runTask(); }, "testPool5");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(threadId1, threadId3);
            Assert.Equal(threadId1, threadId5);
            Assert.Equal(threadId2, threadId4);
            Assert.NotEqual(threadId1, threadId2);
        }
    }
}
