using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace ECS.Server
{
    public class TriggerEnterExitDetectionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter buttonFilter = world
                .Filter<DoorButtonComponent>()
                .Inc<PositionComponent>()
                .Inc<InteractableRadiusComponent>()
                .End();

            var positionPool = world.GetPool<PositionComponent>();
            var interactableRadiusPool = world.GetPool<InteractableRadiusComponent>();

            var triggeredEnterPool = world.GetPool<TriggerEnterTag>();
            var triggeredExitPool = world.GetPool<TriggerExitTag>();

            EcsFilter playerFilter = world
                .Filter<PlayerTag>()
                .Inc<PositionComponent>()
                .End();
            
            foreach (int playerEntity in playerFilter)
            {
                Vector3 playerPosition = positionPool.GetRef(playerEntity).value;
                
                foreach (int buttonEntity in buttonFilter)
                {
                    Vector3 buttonPosition = positionPool.GetRef(buttonEntity).value;
                    float buttonRadius = interactableRadiusPool.GetRef(buttonEntity).interactableRadius;

                    if ((playerPosition - buttonPosition).sqrMagnitude <= buttonRadius)
                    {
                        if(!triggeredEnterPool.Has(buttonEntity))
                            triggeredEnterPool.Add(buttonEntity);
                    }
                    else if(triggeredEnterPool.Has(buttonEntity) && !triggeredExitPool.Has(buttonEntity))
                        triggeredExitPool.Add(buttonEntity);
                }
            }
        }
    }
}