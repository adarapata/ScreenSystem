using System;

namespace ScreenSystem
{
    public enum ScreenKind
    {
        Page,
        Modal,
    }

    public readonly struct ScreenLoadContext
    {
        public ScreenKind Kind { get; }
        public Type ScreenType { get; }
        public string PrefabName { get; }

        public ScreenLoadContext(ScreenKind kind, Type screenType, string prefabName)
        {
            Kind = kind;
            ScreenType = screenType;
            PrefabName = prefabName;
        }
    }

    public interface IScreenLoadHandler
    {
        void OnLoadStart(in ScreenLoadContext context);
        void OnPrefabLoaded(in ScreenLoadContext context);
        void OnLoadComplete(in ScreenLoadContext context);

        /// <summary>
        /// prefab のロードや遷移コルーチンが例外で終了したときに呼ばれる。
        /// 既存実装の後方互換のため default 実装を持つ。
        /// </summary>
        void OnLoadFailed(in ScreenLoadContext context, Exception exception)
        {
        }
    }
}
