using GameEntities;
using Leopotam.EcsLite;
using UnityEngine;
using Utils;

namespace InputModule
{
    public abstract class ABaseInputController : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world, inputWorld;
        
        protected abstract bool IsTouchedSomeTargetPoint();
        protected abstract Vector3 GetTouchedPoint();
        protected abstract Vector3 GetInputMoveDirection();

        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("Input");
        }

        public void Run(IEcsSystems systems)
        {
            int playerId = world.GetUnique<MainPlayerTag>().playerId;

            if (IsTouchedSomeTargetPoint())
            {
                var newTargetInputRequest = new InputTargetRequest();
                newTargetInputRequest.target = GetTouchedPoint();
                
                CreateInputEntity(playerId, newTargetInputRequest);
            }
            
            Vector3 inputMoveDirection = GetInputMoveDirection();
            if (inputMoveDirection.sqrMagnitude > 0.01f)
            {
                var newDirectionInputRequest = new InputDirectionRequest();
                newDirectionInputRequest.MoveDirection = inputMoveDirection;
                CreateInputEntity(playerId, newDirectionInputRequest);
            }
        }
        
        private void CreateInputEntity(int playerId, IInputComponent inputRequest)
        {
            var inputEntity = inputWorld.NewEntity();
            
            inputWorld.GetPool<InputComponent>().Add(inputEntity);
            inputWorld.GetPool<InputEntityIdComponent>().Add(inputEntity).entity = playerId;
            
            var pool = inputWorld.GetPoolByType(inputRequest.GetType());
            pool.AddRaw(inputEntity, inputRequest);
        }
    }
}