using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Modules.Box2D.ClientServer.Components
{
    [DontSerialize]
    [System.Serializable]
    public struct Box2DWorldComponent
    {
        public System.IntPtr WorldReference;
    }
}
