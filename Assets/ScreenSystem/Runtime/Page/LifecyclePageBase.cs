using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Page;

namespace ScreenSystem.Page
{
	public abstract class LifecyclePageBase : IPage, IPageLifecycleEvent, IDisposable
	{
		private readonly UnityScreenNavigator.Runtime.Core.Page.Page _page;

		private CancellationTokenSource _pageActiveCancellationTokenSource;
		protected CancellationToken PageActiveToken => _pageActiveCancellationTokenSource.Token;
		
		private readonly CancellationTokenSource _disposeCancellationTokenSource;

		protected LifecyclePageBase(UnityScreenNavigator.Runtime.Core.Page.Page page)
		{
			_page = page;
			_page.AddLifecycleEvent(this);
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
			var cts = BuildCancellationTokenSourceOnDispose();
			yield return WillPushExitAsync(cts.Token).ToCoroutine();
			cts.Cancel();
		}

		protected virtual UniTask WillPushExitAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;

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
			_page.RemoveLifecycleEvent(this);
			_disposeCancellationTokenSource.Cancel();
			_disposeCancellationTokenSource.Dispose();
		}

		private void EnableActiveTokenSource(bool enable)
		{
			if (enable)
			{
				_pageActiveCancellationTokenSource = BuildCancellationTokenSourceOnDispose();
			}
			else
			{
				_pageActiveCancellationTokenSource.Cancel();
			}
		}

		protected CancellationTokenSource BuildCancellationTokenSourceOnDispose()
		{
			return CancellationTokenSource.CreateLinkedTokenSource(_disposeCancellationTokenSource.Token);
		}
	}
}