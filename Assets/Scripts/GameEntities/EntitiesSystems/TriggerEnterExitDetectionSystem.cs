using Leopotam.EcsLite;
using UnityEngine;
using Utils;

namespace GameEntities
{
    public class TriggerEnterExitDetectionSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
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
                positionPool.Get(world.GetUnique<MainPlayerTag>().playerId).currentEntityPosition;
            
            foreach (int buttonEntity in filter)
            {
                Vector3 buttonPosition = positionPool.Get(buttonEntity).currentEntityPosition;
                float buttonRadius = interactableRadiusPool.Get(buttonEntity).interactableRadius;

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