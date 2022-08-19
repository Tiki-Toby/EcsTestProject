using UnityEngine;
using XFlow.EcsLite;

namespace GameEntities
{
    public class CameraFollowSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var transformPool = world.GetPool<TransformComponent>();
            var deltaTime = Time.deltaTime;

            int playerId = world.GetUnique<MainPlayerTag>().playerId;
            Transform transform = transformPool.Get(playerId).objectTransform;

            var cameraPool = world.GetPool<CameraComponent>();
            var cameraDataPool = world.GetPool<ActionCameraComponent>();

            var cameraEntity = world.GetUnique<MainCameraComponent>().cameraEntity;
            
            ref var cameraData = ref cameraDataPool.GetRef(cameraEntity);
            var targetPosition = transform.position + cameraData.FromPlayerOffset;

            Camera camera = cameraPool.Get(cameraEntity).Camera;
            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                targetPosition,
                cameraData.CameraVelocity * deltaTime);
        }
    }
}