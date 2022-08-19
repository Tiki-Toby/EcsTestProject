using UnityEngine;
using XFlow.EcsLite;

namespace GameEntities
{
    public class TriggerEnterExitDetectionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            EcsFilter filter = world
                .Filter<DoorButtonComponent>()
                .Inc<PositionComponent>()
                .Inc<InteractableRadiusComponent>()
                .End();

            var positionPool = world.GetPool<PositionComponent>();
            var interactableRadiusPool = world.GetPool<InteractableRadiusComponent>();

            var triggeredEnterPool = world.GetPool<TriggerEnterTag>();
            var triggeredExitPool = world.GetPool<TriggerExitTag>();

            Vector3 playerPosition = 
                positionPool.GetRef(world.GetUnique<MainPlayerTag>().playerId).currentEntityPosition;
            
            foreach (int buttonEntity in filter)
            {
                Vector3 buttonPosition = positionPool.GetRef(buttonEntity).currentEntityPosition;
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