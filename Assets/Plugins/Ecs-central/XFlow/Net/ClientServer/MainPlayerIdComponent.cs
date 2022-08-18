using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Net.ClientServer
{
    [DontSerialize]
    public struct MainPlayerIdComponent
    {
        public int value;
    }
}