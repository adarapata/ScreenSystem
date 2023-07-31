using System;
using System.Linq;
using System.Threading;
using ScreenSystem.Attributes;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using ScreenSystem.VContainerExtension;
using UnityScreenNavigator.Runtime.Core.Modal;
using VContainer;
using VContainer.Unity;

namespace ScreenSystem.Modal
{
	public interface IModal
	{
	}

	public interface IModalBuilder
	{
		UniTask<IModal> Build(ModalContainer modalContainer, LifetimeScope parent, CancellationToken cancellationToken);
	}

	public abstract class ModalBuilderBase<TModal, TModalView> : IModalBuilder
		where TModal : IModal
		where TModalView : ModalViewBase
	{
		private readonly bool _playAnimation;
		public ModalBuilderBase(bool playAnimation = true)
		{
			_playAnimation = playAnimation;
		}

		public async UniTask<IModal> Build(ModalContainer modalContainer, LifetimeScope parent, CancellationToken cancellationToken)
		{
			var nameAttr = Attribute.GetCustomAttribute(typeof(TModal), typeof(AssetNameAttribute)) as AssetNameAttribute;
			var source = new UniTaskCompletionSource<IModal>();
			using (LifetimeScope.EnqueueParent(parent))
			{
				var modalTask = modalContainer.Push(nameAttr.PrefabName, playAnimation: _playAnimation, onLoad: modal =>
				{
					if (cancellationToken.IsCancellationRequested)
					{
						source.TrySetCanceled(cancellationToken);
						return;
					}
					var modalView = modal.modal as TModalView;
					var lts = modalView.gameObject.GetComponentInChildren<LifetimeScope>();
					SetUpParameter(lts);
					lts.Build();
					var pageInstance = lts.Container.Resolve<TModal>();
					source.TrySetResult(pageInstance);
				});

				var modal = await source.Task;
				await modalTask.Task;
				cancellationToken.ThrowIfCancellationRequested();
				return modal;
			}
		}

		protected virtual void SetUpParameter(LifetimeScope lifetimeScope)
		{
		}
	}
	
	public abstract class ModalBuilderBase<TModal, TModalView, TParameter> : ModalBuilderBase<TModal, TModalView>
		where TModal : IModal
		where TModalView : ModalViewBase
	{
		private readonly TParameter _parameter;
		
		public ModalBuilderBase(TParameter parameter, bool playAnimation = true) : base(playAnimation)
		{
			_parameter = parameter;
		}

		protected override void SetUpParameter(LifetimeScope lifetimeScope)
		{
			if (lifetimeScope is LifetimeScopeWithParameter<TParameter> withParameter)
			{
				withParameter.SetParameter(_parameter);
			}
		}
	}

	public class ModalManager : IDisposable
	{
		class ModalTransitionScope : IDisposable
		{
			public static bool IsModalTransition
			{
				get;
				private set;
			}

			public static IDisposable Transition()
			{
				return new ModalTransitionScope();
			}

			private ModalTransitionScope()
			{
				IsModalTransition = true;
			}

			public void Dispose()
			{
				IsModalTransition = false;
			}

			public static UniTask WaitTransition(CancellationToken token)
			{
				return UniTask.WaitUntil(() => !IsModalTransition, cancellationToken: token);
			}
		}
		
		private readonly ModalContainer _modalContainer;
		private readonly LifetimeScope _lifetimeScope;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private IDisposable _disposable;


		[Inject]
		public ModalManager(ModalContainer modalContainer, LifetimeScope lifetimeScope)
		{
			_modalContainer = modalContainer;
			_lifetimeScope = lifetimeScope;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public async UniTask<IModal> Push(IModalBuilder builder)
		{
			if (ModalTransitionScope.IsModalTransition)
			{
				await ModalTransitionScope.WaitTransition(_cancellationTokenSource.Token);
			}
			
			using var scope = ModalTransitionScope.Transition();	
			var page = await builder.Build(_modalContainer, _lifetimeScope, _cancellationTokenSource.Token);
			return page;
		}

		public async UniTask Pop(bool playAnimation)
		{
			if (ModalTransitionScope.IsModalTransition)
			{
				await ModalTransitionScope.WaitTransition(_cancellationTokenSource.Token);
			}
			
			using var scope = ModalTransitionScope.Transition();
			if (_modalContainer.Modals.Any())
			{
				var handle = _modalContainer.Pop(playAnimation);
				await handle.WithCancellation(_cancellationTokenSource.Token);
			}
		}

		public async UniTask AllPop(bool animation)
		{
			while (_modalContainer.Modals.Any())
			{
				await Pop(animation).SuppressCancellationThrow();
			}
		}
		
		public void Dispose()
		{
			_cancellationTokenSource?.Cancel();
		}
	}
}