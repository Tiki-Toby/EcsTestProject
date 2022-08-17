using System;
using Leopotam.EcsLite;
using Utils;

namespace InputModule
{
    public class InputSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            /*EcsWorld world = systems.GetWorld();

            ref var inputData = ref world.GetUnique<InputDataComponent>();
            
            var filter = world.Filter<InputDirectionRequest>().End ();
            var directionPool = world.GetPool<InputDirectionRequest>();

            foreach (var i in filter)
            {
                ref var directionComponent = ref directionPool.Get(i);
                ref var direction = ref directionComponent.MoveDirection;

                direction.x = inputData.InputMoveDirection.x;
                direction.z = inputData.InputMoveDirection.z;
            }*/
        }
    }
}