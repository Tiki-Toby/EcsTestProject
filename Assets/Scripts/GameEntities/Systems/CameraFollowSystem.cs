using Leopotam.EcsLite;
using UnityEngine;
using Utils;
using Zenject;

namespace GameEntities
{
    public class CameraFollowSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var transformPool = world.GetPool<TransformComponent>();
            var deltaTime = Time.deltaTime;
            
            int playerId = world.GetUnique<MainPlayerTag>().playerId;
            Transform transform = transformPool.Get(playerId).objectTransform;
            
            var cameraPool = world.GetPool<CameraComponent>();
            var cameraDataPool = world.GetPool<ActionCameraComponent>();
            var filter = world.Filter<CameraComponent>().Inc<ActionCameraComponent>().End();
            
            foreach (int i in filter)
            {
                ref var cameraData = ref cameraDataPool.Get(i);
                var targetPosition = transform.position + cameraData.FromPlayerOffset;
                
                Camera camera = cameraPool.Get(i).Camera;
                camera.transform.position = Vector3.Lerp(
                    camera.transform.position,
                    targetPosition,
                    cameraData.CameraVelocity * deltaTime);
            }
        }
    }
}