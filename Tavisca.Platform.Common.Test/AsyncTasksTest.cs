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
        private static object _lock = new object();

        [Fact]
        public void Should_Be_Able_To_Configure_Default_Task_Pool_With_Default_Size()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(10);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            lock (_lock)
            {
                AsyncTasks.UseDefaultPool();
                AsyncTasks.AddPool("testPool");
            }
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
        public void Should_Be_Able_To_Enqueue_Action_In_Configured_Taskpool()
        {
            var isActionExcecuted = new ConcurrentBag<bool>();
            var waitHandle = new CountdownEvent(10);

            Action runTask = () => {
                Thread.Sleep(1000);
                isActionExcecuted.Add(true);
                waitHandle.Signal();
            };

            lock (_lock)
            {
                AsyncTasks.UseDefaultPool();
                AsyncTasks.AddPool("testPool");
            }
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

            var expected = 10;
            Assert.Equal(expected, isActionExcecuted.Count);

        }

        [Fact]
        public void Should_Be_Able_To_Configure_Default_Taskpool_With_Specific_Size()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(4);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };
            
            int poolSize = 2;
            lock (_lock)
            {
                AsyncTasks.UseDefaultPool();
                AsyncTasks.AddPool("testPool1", poolSize);
            }
            AsyncTasks.Run(runTask, "testPool1");
            AsyncTasks.Run(runTask, "testPool1");
            AsyncTasks.Run(runTask, "testPool1");
            AsyncTasks.Run(runTask, "testPool1");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(poolSize, threadIds.Distinct().Count());
        }

        [Fact]
        public void Shoul_Be_Able_To_Configure_Default_Taskpool()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(4);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            int poolSize = 2;
            lock (_lock)
            {
                var taskPool = new TaskPool(poolSize);
                AsyncTasks.AddPool("testPool2", taskPool);
            }
            AsyncTasks.Run(runTask, "testPool2");
            AsyncTasks.Run(runTask, "testPool2");
            AsyncTasks.Run(runTask, "testPool2");
            AsyncTasks.Run(runTask, "testPool2");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(poolSize, threadIds.Distinct().Count());
        }

        
        [Fact]
        public void Should_Be_Able_To_Configure_Roundrobin_Taskpool()
        {
            var waitHandle = new CountdownEvent(10);

            Func<int> runTask = () => {
                waitHandle.Signal();
                return Thread.CurrentThread.ManagedThreadId;
            };

            int threadId1 = 0, threadId2 = 0, threadId3 = 0, threadId4 = 0, threadId5 = 0;
            int threadId6 = 0, threadId7 = 0, threadId8 = 0, threadId9 = 0, threadId10 = 0;

            lock (_lock)
            {
                AsyncTasks.UseRoundRobinPool();
                AsyncTasks.AddPool("testPool3");
            }
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
        public void Should_Be_Able_To_Configure_Roundrobin_Taskpool_With_A_Given_Size()
        {
            var waitHandle = new CountdownEvent(5);

            Func<int> runTask = () => {
                waitHandle.Signal();
                return Thread.CurrentThread.ManagedThreadId;
            };

            int poolSize = 3;
            int threadId1 = 0, threadId2 = 0, threadId3 = 0, threadId4 = 0, threadId5 = 0;
            lock (_lock)
            {
                AsyncTasks.UseRoundRobinPool();
                AsyncTasks.AddPool("testPool4", poolSize);
            }
            AsyncTasks.Run(() => { threadId1 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId2 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId3 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId4 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId5 = runTask(); }, "testPool4");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(threadId1, threadId4);
            Assert.Equal(threadId2, threadId5);
            Assert.NotEqual(threadId1, threadId2);
            Assert.NotEqual(threadId1, threadId3);
        }

        [Fact]
        public void Actions_Should_Be_Enqueued_In_RoundRobin_Manner()
        {
            var waitHandle = new CountdownEvent(5);

            Func<int> runTask = () => {
                waitHandle.Signal();
                return Thread.CurrentThread.ManagedThreadId;
            };

            int poolSize = 2;
            int threadId1 = 0, threadId2 = 0, threadId3 = 0, threadId4 = 0, threadId5 = 0;
            lock (_lock)
            {
                AsyncTasks.UseRoundRobinPool();
                AsyncTasks.AddPool("testPool4", poolSize);
            }
            AsyncTasks.Run(() => { threadId1 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId2 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId3 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId4 = runTask(); }, "testPool4");
            AsyncTasks.Run(() => { threadId5 = runTask(); }, "testPool4");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(threadId1, threadId3);
            Assert.Equal(threadId2, threadId4);
            Assert.NotEqual(threadId1, threadId2);
            Assert.NotEqual(threadId1, threadId4);
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
            Assert.Equal(threadId2, threadId2);
            Assert.NotEqual(threadId1, threadId2);
        }

        [Fact]
        public void Should_Be_Able_To_Enqueue_Actions_Without_Configuring_Taskpool()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(10);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            AsyncTasks.Run(runTask, "testPool6");
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(Environment.ProcessorCount, threadIds.Distinct().Count());
        }

        [Fact]
        public void Should_Be_Able_To_Enqueue_Actions_Without_Configuring_Taskpool_And_Name()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(10);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };

            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            AsyncTasks.Run(runTask);
            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(Environment.ProcessorCount, threadIds.Distinct().Count());
        }

        [Fact]
        public void Should_Be_Able_To_Enqueue_Actions_From_Multiple_Threads()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(4);

            Action runTask = () => {
                Thread.Sleep(1000);
                threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                waitHandle.Signal();
            };


            AsyncTasks.AddPool("testPool7", 2);
            Parallel.Invoke(
                () => {
                    AsyncTasks.Run(runTask, "testPool7");
                },
                () => {
                    AsyncTasks.AddPool("testPool8", 1);
                    AsyncTasks.Run(runTask, "testPool8");
                },
                () => {
                    AsyncTasks.Run(runTask, "testPool7");
                },
                () => {
                    AsyncTasks.AddPool("testPool9", 1);
                    AsyncTasks.Run(runTask, "testPool9");
                }
            );

            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.Equal(4, threadIds.Distinct().Count());
        }

        [Fact]
        public void AsyncTask_Default_Pool_Should_Execute_Action_In_FIFO_manner()
        {
            var threadIds = new ConcurrentBag<int>();
            var waitHandle = new CountdownEvent(3);

            DateTime dateTime1 = new DateTime(), dateTime2 = new DateTime(), dateTime3 = new DateTime(), dateTime4 = new DateTime();
            Action runTask1 = () => {
                Thread.Sleep(1000);
                waitHandle.Signal();
                dateTime1 = DateTime.Now;
            };

            Action runTask2 = () => {
                Thread.Sleep(1000);
                waitHandle.Signal();
                dateTime2 = DateTime.Now;
            };

            Action runTask3 = () => {
                Thread.Sleep(1000);
                waitHandle.Signal();
                dateTime3 = DateTime.Now;
            };

            int poolSize = 2;
            lock (_lock)
            {
                AsyncTasks.UseDefaultPool();
                AsyncTasks.AddPool("testPool10", poolSize);
            }

            Parallel.Invoke(
               () => {
                   Thread.Sleep(500);
                   AsyncTasks.Run(runTask1, "testPool10");
               },
               () => {
                   AsyncTasks.Run(runTask2, "testPool10");
               },
               () => {
                   Thread.Sleep(200);
                   AsyncTasks.Run(runTask3, "testPool10");
               }
           );

            waitHandle.Wait();
            AsyncTasks.RemoveAll();

            Assert.True(dateTime1 > dateTime2);
            Assert.True(dateTime1 > dateTime3);
            Assert.True(dateTime3 > dateTime2);
        }
    }
}
