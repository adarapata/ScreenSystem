using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace ScreenSystem.Modal
{
	public abstract class LifecycleModalBase : IModal, IModalLifecycleEvent, IDisposable
	{
		private readonly UnityScreenNavigator.Runtime.Core.Modal.Modal _modal;

		private CancellationTokenSource _activeCancellationTokenSource;
		protected CancellationToken ActiveToken => _activeCancellationTokenSource.Token;

		protected readonly CancellationTokenSource _disposeCancellationTokenSource;

		protected LifecycleModalBase(UnityScreenNavigator.Runtime.Core.Modal.Modal modal)
		{
			_modal = modal;
			_modal.AddLifecycleEvent(this);
			_disposeCancellationTokenSource = new CancellationTokenSource();
		}
		
		public IEnumerator Initialize()
		{
			var cts = BuildCancellationTokenSourceOnDispose();
			yield return InitializeAsync(cts.Token).ToCoroutine();
			cts.Cancel();
		}

		protected virtual UniTask InitializeAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

		public IEnumerator WillPushEnter()
		{
			var cts = BuildCancellationTokenSourceOnDispose();
			yield return WillPushEnterAsync(cts.Token).ToCoroutine();
			cts.Cancel();
		}

		protected virtual UniTask WillPushEnterAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

		public virtual void DidPushEnter()
		{
			EnableActiveTokenSource(true);
		}

		public IEnumerator WillPushExit()
		{
			EnableActiveTokenSource(false);
			yield return WillPushExitAsync().ToCoroutine();
		}

		protected virtual UniTask WillPushExitAsync() => UniTask.CompletedTask;

		public virtual void DidPushExit() { }

		public IEnumerator WillPopEnter()
		{
			var cts = BuildCancellationTokenSourceOnDispose();
			yield return WillPopEnterAsync(cts.Token).ToCoroutine();
			cts.Cancel();
		}

		protected virtual UniTask WillPopEnterAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

		public virtual void DidPopEnter()
		{
			EnableActiveTokenSource(true);
		}

		public IEnumerator WillPopExit()
		{
			EnableActiveTokenSource(false);
			var cts = BuildCancellationTokenSourceOnDispose();
			yield return WillPopExitAsync(cts.Token).ToCoroutine();
			cts.Cancel();
		}

		protected virtual UniTask WillPopExitAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

		public virtual void DidPopExit() { }

		public IEnumerator Cleanup()
		{
			var cts = BuildCancellationTokenSourceOnDispose();
			yield return CleanUpAsync(cts.Token).ToCoroutine();
			cts.Cancel();
		}

		protected virtual UniTask CleanUpAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

		public virtual void Dispose()
		{
			_modal.RemoveLifecycleEvent(this);
			_disposeCancellationTokenSource.Cancel();
			_disposeCancellationTokenSource.Dispose();
		}

		private void EnableActiveTokenSource(bool enable)
		{
			if (enable)
			{
				_activeCancellationTokenSource = BuildCancellationTokenSourceOnDispose();
			}
			else
			{
				_activeCancellationTokenSource.Cancel();
			}
		}

		private CancellationTokenSource BuildCancellationTokenSourceOnDispose()
		{
			return CancellationTokenSource.CreateLinkedTokenSource(_disposeCancellationTokenSource.Token);
		}
	}
}