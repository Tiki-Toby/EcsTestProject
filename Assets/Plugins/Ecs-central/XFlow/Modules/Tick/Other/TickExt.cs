using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Components;

namespace XFlow.Modules.Tick.Other
{
    public static class TickExt
    {
        public static float GetTime(this EcsWorld world)
        {
            var tick = world.GetUnique<TickComponent>().Value;
            var dt = world.GetUnique<TickDeltaComponent>().Value.Seconds;

            return tick.Value * dt;
        }
        
        public static int GetTick(this EcsWorld world)
        {
            return world.GetUnique<TickComponent>().Value.Value;
        }
        
        public static float GetDeltaSeconds(this EcsWorld world)
        {
            return world.GetUnique<TickDeltaComponent>().Value.Seconds;
        }
    }
}