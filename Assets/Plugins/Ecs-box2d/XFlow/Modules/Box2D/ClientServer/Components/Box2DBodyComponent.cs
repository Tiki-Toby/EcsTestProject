using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Modules.Box2D.ClientServer.Components
{
    [DontSerialize]
    public struct Box2DBodyComponent
    {
        public System.IntPtr BodyReference;
    }
}
