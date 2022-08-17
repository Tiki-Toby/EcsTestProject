using GameEntities;
using Leopotam.EcsLite;
using UnityEngine;

namespace InputModule
{
    public class HandleInputRequestsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld inputWorld, world;
        private EcsFilter inputEntityFilter;
        
        public void Init(IEcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("Input");
            
            inputEntityFilter = inputWorld.Filter<InputComponent>().End();
        }
        
        public void Run(IEcsSystems systems)
        {
            var inputEntityIdPool = inputWorld.GetPool<InputEntityIdComponent>();
            
            var newTargetRequestPool = inputWorld.GetPool<InputTargetRequest>();
            var moveDirectionRequestPool = inputWorld.GetPool<InputDirectionRequest>();
            
            foreach (int i in inputEntityFilter)
            {
                if (newTargetRequestPool.Has(i))
                    SetTarget(inputEntityIdPool.Get(i).entity);
                
                if (moveDirectionRequestPool.Has(i))
                    SetMoveDirection(inputEntityIdPool.Get(i).entity, moveDirectionRequestPool.Get(i).MoveDirection);
            }
        }

        private void SetTarget(int inputEntity)
        {
            
        }
        
        private void SetMoveDirection(int playerId, Vector3 dir)
        {
            if (dir.sqrMagnitude > 0.1f)
            {
                inputWorld.GetPool<MoveToTargetComponent>().Del(playerId);
                var pool = world.GetPool<MovingEntityComponent>();
                if (!pool.Has(playerId))
                    pool.Add(playerId).direction = dir;
                else
                    pool.Get(playerId).direction = dir;
            }
            else
            {
                var pool = world.GetPool<MovingEntityComponent>();
                if (pool.Has(playerId))
                    pool.Del(playerId);
            }
        }
    }
}