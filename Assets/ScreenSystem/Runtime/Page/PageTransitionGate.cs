using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ScreenSystem.Page
{
	internal sealed class PageTransitionGate
	{
		private int _isAcquired;

		public async UniTask<IDisposable> AcquireAsync(CancellationToken cancellationToken)
		{
			IDisposable lease;
			while (!TryAcquire(out lease))
			{
				await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
			}

			return lease;
		}

		internal bool TryAcquire(out IDisposable lease)
		{
			if (Interlocked.CompareExchange(ref _isAcquired, 1, 0) != 0)
			{
				lease = null;
				return false;
			}

			lease = new Lease(this);
			return true;
		}

		private void Release()
		{
			Interlocked.Exchange(ref _isAcquired, 0);
		}

		private sealed class Lease : IDisposable
		{
			private PageTransitionGate _owner;

			public Lease(PageTransitionGate owner)
			{
				_owner = owner;
			}

			public void Dispose()
			{
				var owner = Interlocked.Exchange(ref _owner, null);
				if (owner != null)
				{
					owner.Release();
				}
			}
		}
	}
}
