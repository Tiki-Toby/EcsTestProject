using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace ECS.Server
{
    public class CreatePhysicsBodySystem : IEcsInitSystem
    {
        public void Init(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world.Filter<Box2DRigidbodyDefinitionComponent>().End();

            foreach (int physicEntity in filter)
                Box2DServices.CreateBodyNow(world, physicEntity);
        }
    }
}