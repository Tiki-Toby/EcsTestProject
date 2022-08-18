using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.Other;

namespace XFlow.Modules.Tick.ClientServer.Systems
{
    public class TickSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld world;
        private EcsPool<TickComponent> poolTick;
        private EcsPool<TickDeltaComponent> poolTickDelta;
        private EcsFilter filter;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            poolTick = world.GetPool<TickComponent>();
            poolTickDelta = world.GetPool<TickDeltaComponent>();
            filter = world.Filter<TickComponent>().Inc<TickDeltaComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (int entity in filter)
            {
                TickDelta dt = poolTickDelta.Get(entity).Value;
                if (dt.Value == 0)
                    continue;
                
                ref var tickComponent = ref poolTick.GetRef(entity);
                tickComponent.Value += dt;
            }
        }
    }
}