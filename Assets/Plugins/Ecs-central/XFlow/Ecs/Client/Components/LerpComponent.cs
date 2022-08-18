using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Ecs.Client.Components
{
    [DontSerialize]
    public struct LerpComponent
    {
        public float value;
    }
}