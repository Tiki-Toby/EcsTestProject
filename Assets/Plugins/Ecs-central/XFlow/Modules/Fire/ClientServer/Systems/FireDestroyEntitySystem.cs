using XFlow.EcsLite;
using XFlow.Modules.Fire.ClientServer.Components;

namespace XFlow.Modules.Fire.ClientServer.Systems
{
    public class FireDestroyEntitySystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        EcsFilter filter;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world
                .Filter<FireComponent>().Inc<BurnedOutComponent>()
                .End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter) 
                world.DelEntity(entity);
        }
    }
}