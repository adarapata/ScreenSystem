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
    }
}
