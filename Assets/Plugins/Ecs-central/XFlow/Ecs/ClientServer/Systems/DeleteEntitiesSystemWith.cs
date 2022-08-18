using System;
using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.Systems
{
    public sealed class DeleteEntitiesSystemWith<T> : IEcsRunSystem, IEcsWorldChangedSystem where T : struct
    {
        private readonly Func<T, bool> canDeleteEntity;
        
        private EcsFilter filter;
        private EcsWorld world;
        private EcsPool<T> pool;
        
        public DeleteEntitiesSystemWith(Func<T, bool> canDeleteEntity)
        {
            this.canDeleteEntity = canDeleteEntity;
        }

        public DeleteEntitiesSystemWith()
        {
            canDeleteEntity = arg => true;
        }

        public void Run (EcsSystems systems)
        {
            foreach (var entity in filter) 
            {
                if (canDeleteEntity.Invoke(pool.Get(entity)))
                {
                    world.DelEntity(entity);
                }
            }
        }

        public void WorldChanged(EcsWorld world)
        {
            this.world = world;
            filter = world.Filter<T> ().End ();
            pool = world.GetPool<T>();
        }
    }
}