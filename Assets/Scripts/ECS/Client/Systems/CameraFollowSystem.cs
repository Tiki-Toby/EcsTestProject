using UnityEngine;
using XFlow.EcsLite;

namespace ECS.Client
{
    public class CameraFollowSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var transformPool = world.GetPool<XFlow.Ecs.Client.Components.TransformComponent>();
            var deltaTime = Time.deltaTime;

            int playerId = world.GetUnique<MainPlayerTag>().playerId;
            Transform playerTransform = transformPool.Get(playerId).Transform;

            var cameraDataPool = world.GetPool<ActionCameraComponent>();

            var cameraEntity = world.GetUnique<MainCameraComponent>().cameraEntity;
            Transform cameraTransform = transformPool.Get(cameraEntity).Transform;
            
            ref var cameraData = ref cameraDataPool.GetRef(cameraEntity);
            var targetPosition = playerTransform.position + cameraData.FromPlayerOffset;

            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPosition,
                cameraData.CameraVelocity * deltaTime);
        }
    }
}