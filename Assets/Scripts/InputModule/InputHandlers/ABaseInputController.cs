using GameEntities;
using Leopotam.EcsLite;
using UnityEngine;
using Utils;

namespace InputModule
{
    public abstract class ABaseInputController : IEcsInitSystem, IEcsRunSystem
    {
        protected EcsWorld world;
        private EcsWorld inputWorld;
        
        protected abstract bool IsTouchedSomeTargetPoint();
        protected abstract Vector3 GetTouchedPoint();
        protected abstract Vector3 GetInputMoveDirection();

        public virtual void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("Input");
        }

        public void Run(IEcsSystems systems)
        {
            ClearOldInputEntities();
            int playerId = world.GetUnique<MainPlayerTag>().playerId;

            if (IsTouchedSomeTargetPoint())
            {
                var newTargetInputRequest = new InputTargetRequest();
                newTargetInputRequest.target = GetTouchedPoint();
                
                CreateInputEntity(playerId, newTargetInputRequest);
            }
            
            Vector3 inputMoveDirection = GetInputMoveDirection();
            if (inputMoveDirection.sqrMagnitude > 0f)
            {
                var newDirectionInputRequest = new InputDirectionRequest();
                newDirectionInputRequest.MoveDirection = inputMoveDirection;
                CreateInputEntity(playerId, newDirectionInputRequest);
            }
        }

        private void ClearOldInputEntities()
        {
            EcsFilter filter = world.Filter<InputComponent>().End();

            foreach (int i in filter)
            {
                inputWorld.DelEntity(i);
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