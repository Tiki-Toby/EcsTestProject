using ECS.Server;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace ECS.Client
{
    public class SyncTransformDataSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<XFlow.Ecs.Client.Components.TransformComponent>()
                .Inc<PositionComponent>()
                .End();

            var transformPool = world.GetPool<XFlow.Ecs.Client.Components.TransformComponent>();
            var positionPool = world.GetPool<PositionComponent>();
            var rotationPool = world.GetPool<RotationComponent>();

            foreach (var entity in filter)
            {
                Transform transform = transformPool.Get(entity).Transform;

                transform.position = positionPool.Get(entity).value;
                
                if (rotationPool.Has(entity))
                {
                    transform.rotation = rotationPool.Get(entity).rotation;
                }
            }
        }
    }
}