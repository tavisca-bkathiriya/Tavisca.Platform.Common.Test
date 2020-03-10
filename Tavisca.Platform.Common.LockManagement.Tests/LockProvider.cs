using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.LockManagement;

namespace Tavisca.Libraries.LockManagement.Tests
{
    internal class LockProvider : ILockProvider
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        public async Task<bool> TryGetLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken)
        {
            return await _semaphore.WaitAsync(1);
        }
        public async Task ReleaseLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken)
        {
            await Task.Run(() => { _semaphore.Release(); });
        }
    }
}
