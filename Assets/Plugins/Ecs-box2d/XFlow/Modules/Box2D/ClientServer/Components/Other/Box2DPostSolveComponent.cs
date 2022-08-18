using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Modules.Box2D.ClientServer.Components.Other
{
    [DontSerialize]
    public struct Box2DPostSolveComponent
    {
        public Box2DContactComponentData Data;
    }
}