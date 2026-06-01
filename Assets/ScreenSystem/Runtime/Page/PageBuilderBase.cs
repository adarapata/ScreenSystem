using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ScreenSystem.Attributes;
using ScreenSystem.VContainerExtension;
using UnityScreenNavigator.Runtime.Core.Page;
using VContainer;
using VContainer.Unity;

namespace ScreenSystem.Page
{
    public abstract class PageBuilderBase<TPage, TPageView> : IPageBuilder
        where TPage : IPage
        where TPageView : PageViewBase
    {
        private readonly bool _playAnimation;
        private readonly bool _isStack;
        private readonly string _overridePrefabName;

        public PageBuilderBase(bool playAnimation = true, bool stack = true, string overridePrefabName = null)
        {
            _playAnimation = playAnimation;
            _isStack = stack;
            _overridePrefabName = overridePrefabName;
        }

        public async UniTask<IPage> Build(PageContainer pageContainer, LifetimeScope parent, CancellationToken cancellationToken)
        {
            var nameAttr = Attribute.GetCustomAttribute(typeof(TPage), typeof(AssetNameAttribute)) as AssetNameAttribute;
            var source = new UniTaskCompletionSource<IPage>();
            var prefabName = string.IsNullOrEmpty(_overridePrefabName) ? nameAttr.PrefabName : _overridePrefabName;
            var loadHandler = ResolveScreenLoadHandler(parent);
            var loadContext = new ScreenLoadContext(ScreenKind.Page, typeof(TPage), prefabName);
            loadHandler?.OnLoadStart(loadContext);
            using (LifetimeScope.EnqueueParent(parent))
            {
                var pageTask = pageContainer.Push(prefabName, playAnimation: _playAnimation, stack: _isStack, onLoad: result =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        source.TrySetCanceled(cancellationToken);
                        return;
                    }

                    var pageView = result.page as TPageView;
                    var lts = pageView.gameObject.GetComponentInChildren<LifetimeScope>();
                    SetUpParameter(lts);
                    lts.Build();
                    var pageInstance = lts.Container.Resolve<TPage>();
                    source.TrySetResult(pageInstance);
                    loadHandler?.OnPrefabLoaded(loadContext);
                });

                var page = await source.Task;
                cancellationToken.ThrowIfCancellationRequested();
                await pageTask.Task;
                loadHandler?.OnLoadComplete(loadContext);
                return page;
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

    public abstract class PageBuilderBase<TPage, TPageView, TParameter> : PageBuilderBase<TPage, TPageView>
        where TPage : IPage
        where TPageView : PageViewBase
    {
        private readonly TParameter _parameter;
		
        public PageBuilderBase(TParameter parameter, bool playAnimation = true, bool stack = true, string overridePrefabName = null) : base(playAnimation, stack, overridePrefabName)
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