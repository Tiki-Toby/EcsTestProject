using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Systems;

namespace XFlow.Modules.Tick
{
    public static class TickModule
    {
        public static IEcsSystem[] GetSystems()
        {
            return new IEcsSystem[]{new TickSystem()};
        }
    }
}