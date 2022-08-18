using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer.Ecs.Components.Input;

namespace XFlow.Net.ClientServer.Ecs.Systems
{
    /**
     * система удаляет гарантированно устаревший input, в которых указан input 
     */
    public class DeleteOutdatedInputEntitiesSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputTickComponent>().End();
            var pool = inputWorld.GetPool<InputTickComponent>();
            var tick = systems.GetWorld().GetTick();
            
            foreach (var entity in filter)
            {
                if (pool.Get(entity).Tick < tick)
                {
                    inputWorld.DelEntity(entity);
                }
            }
        }
    }
}