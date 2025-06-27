using Cysharp.Threading.Tasks;
using System;

namespace Shared.Disposable
{
    public class DisposeObserver : IDisposable
    {
        private Action _onDispose;

        public DisposeObserver(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose.Invoke();
        }
    }
}

