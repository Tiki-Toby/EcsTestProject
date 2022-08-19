using Leopotam.EcsLite;
using UnityEngine;

namespace GameEntities
{
    public class SyncTransformDataSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<TransformComponent>()
                .Inc<PositionComponent>()
                .End();

            var transformPool = world.GetPool<TransformComponent>();
            var positionPool = world.GetPool<PositionComponent>();
            var rotationPool = world.GetPool<RotationComponent>();

            foreach (var entity in filter)
            {
                Transform transform = transformPool.Get(entity).objectTransform;

                transform.position = positionPool.Get(entity).currentEntityPosition;
                
                if (rotationPool.Has(entity))
                {
                    transform.rotation = rotationPool.Get(entity).rotation;
                }
            }
        }
    }
}