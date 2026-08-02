using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;
using VContainer.Unity;

namespace ScreenSystem.Page
{
	public class PageManager : IPageNavigator, IInitializable, IDisposable
	{
		private static readonly PageTransitionGate TransitionGate = new PageTransitionGate();

		private readonly PageContainer _pageContainer;
		private readonly LifetimeScope _lifetimeScope;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly IPageEventSubscriber _pageEventSubscriber;

		[Inject]
		public PageManager(PageContainer pageContainer, 
			LifetimeScope lifetimeScope,
			IPageEventSubscriber pageEventSubscriber)
		{
			_pageContainer = pageContainer;
			_lifetimeScope = lifetimeScope;
			_pageEventSubscriber = pageEventSubscriber;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public async UniTask<IPage> Push(IPageBuilder builder, CancellationToken cancellationToken)
		{
			using var scope = await TransitionGate.AcquireAsync(cancellationToken);
			var page = await builder.Build(_pageContainer, _lifetimeScope, cancellationToken);
			return page;
		}

		public async UniTask Pop(bool playAnimation, CancellationToken cancellationToken)
		{
			using var scope = await TransitionGate.AcquireAsync(cancellationToken);
			if (_pageContainer.Pages.Count < 1)
			{
				return;
			}
			var handle = _pageContainer.Pop(playAnimation);
			await handle.WithCancellation(cancellationToken);
		}

		public void Initialize()
		{
			_pageEventSubscriber.OnPagePushAsyncEnumerable()
				.ForEachAwaitAsync(message => Push(message.Builder, _cancellationTokenSource.Token).SuppressCancellationThrow(), 
					_cancellationTokenSource.Token)
				.SuppressCancellationThrow()
				.Forget();

			_pageEventSubscriber.OnPagePopAsyncEnumerable()
				.ForEachAwaitAsync(message => Pop(message.PlayAnimation, _cancellationTokenSource.Token).SuppressCancellationThrow(),
					_cancellationTokenSource.Token)
				.SuppressCancellationThrow()
				.Forget();
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}
	}
}
