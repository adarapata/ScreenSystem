using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ScreenSystem.Attributes;
using ScreenSystem.VContainerExtension;
using UnityScreenNavigator.Runtime.Core.Modal;
using VContainer;
using VContainer.Unity;

namespace ScreenSystem.Modal
{
    public abstract class ModalBuilderBase<TModal, TModalView> : IModalBuilder
        where TModal : IModal
        where TModalView : ModalViewBase
    {
        private readonly bool _playAnimation;
        private readonly string _overridePrefabName;
        public ModalBuilderBase(bool playAnimation = true, string overridePrefabName = null)
        {
            _playAnimation = playAnimation;
            _overridePrefabName = overridePrefabName;
        }

        public async UniTask<IModal> Build(ModalContainer modalContainer, LifetimeScope parent, CancellationToken cancellationToken)
        {
            var nameAttr = Attribute.GetCustomAttribute(typeof(TModal), typeof(AssetNameAttribute)) as AssetNameAttribute;
            var source = new UniTaskCompletionSource<IModal>();
            var prefabName = string.IsNullOrEmpty(_overridePrefabName) ? nameAttr.PrefabName : _overridePrefabName;
            var loadHandler = ResolveScreenLoadHandler(parent);
            var loadContext = new ScreenLoadContext(ScreenKind.Modal, typeof(TModal), prefabName);
            loadHandler?.OnLoadStart(loadContext);
            using (LifetimeScope.EnqueueParent(parent))
            {
                var modalTask = modalContainer.Push(prefabName, playAnimation: _playAnimation, onLoad: modal =>
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
                    loadHandler?.OnPrefabLoaded(loadContext);
                });

                var modal = await source.Task;
                await modalTask.Task;
                cancellationToken.ThrowIfCancellationRequested();
                loadHandler?.OnLoadComplete(loadContext);
                return modal;
            }
        }

        protected virtual void SetUpParameter(LifetimeScope lifetimeScope)
        {
        }

        private static IScreenLoadHandler ResolveScreenLoadHandler(LifetimeScope parent)
        {
            try
            {
                return parent.Container.Resolve<IScreenLoadHandler>();
            }
            catch (VContainerException)
            {
                return null;
            }
        }
    }

    public abstract class ModalBuilderBase<TModal, TModalView, TParameter> : ModalBuilderBase<TModal, TModalView>
        where TModal : IModal
        where TModalView : ModalViewBase
    {
        private readonly TParameter _parameter;

        public ModalBuilderBase(TParameter parameter, bool playAnimation = true, string overridePrefabName = null) : base(playAnimation, overridePrefabName)
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
}