using UnityEngine;
using XFlow.EcsLite;

namespace GameEntities
{
    public class EntityFactory
    {
        private readonly EcsWorld world;

        public EntityFactory(EcsWorld world)
        {
            this.world = world;
        }

        public int CreateEmpty(string name)
        {
            int newEntity = world.NewEntity();
            
            world.GetPool<EntityDataComponent>().Add(newEntity).name = name;
            Debug.Log($"Create entity with name: {name}");
            return newEntity;
        }

        public int CreateGameObject(string name)
        {
            int newEntity = CreateEmpty(name);
            
            world.GetPool<PositionComponent>().Add(newEntity).currentEntityPosition = Vector3.zero;

            return newEntity;
        }

        public int CreateMovableGameObject(string name, float velocity)
        {
            int newEntity = CreateGameObject(name);
            
            world.GetPool<VelocityComponent>().Add(newEntity).velocity = velocity;

            return newEntity;
        }

        public int CreateGameBallEntity(string name, float velocity)
        {
            int newEntity = CreateMovableGameObject(name, velocity);
            
            world.GetPool<RotationComponent>().Add(newEntity).rotation = Quaternion.identity;

            return newEntity;
        }

        public int  CreatePlayerEntity(string name, float velocity)
        {
            int newEntity = CreateGameBallEntity(name, velocity);
            world.AddUnique<MainPlayerTag>().playerId = newEntity;

            return newEntity;
        }
    }
}