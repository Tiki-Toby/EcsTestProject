using UnityEngine;
using XFlow.EcsLite;

namespace ECS.Client
{
    public abstract class ABaseInputController : IEcsInitSystem, IEcsRunSystem
    {
        protected EcsWorld world;
        private EcsWorld inputWorld;
        
        protected abstract bool IsTouchedSomeTargetPoint();
        protected abstract Vector3 GetTouchedPoint();
        protected abstract Vector3 GetInputMoveDirection();

        public virtual void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("Input");
        }

        public void Run(EcsSystems systems)
        {
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
                var newDirectionInputRequest = new InputDirectionComponent();
                newDirectionInputRequest.MoveDirection = inputMoveDirection;
                CreateInputEntity(playerId, newDirectionInputRequest);
            }
        }
        
        private void CreateInputEntity(int playerId, IInputComponent inputRequest)
        {
            var inputEntity = inputWorld.NewEntity();
            
            inputWorld.GetPool<InputEntityTag>().Add(inputEntity);
            inputWorld.GetPool<InputEntityIdComponent>().Add(inputEntity).entity = playerId;
            
            var pool = inputWorld.GetPoolByType(inputRequest.GetType());
            pool.AddRaw(inputEntity, inputRequest);
        }
    }
}