using System;
using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.Systems
{
    public sealed class DeleteComponentsHereWith<T> : IEcsRunSystem, IEcsWorldChangedSystem where T : struct
    {
        private EcsWorld world;
        private EcsFilter filter;
        
        private Func<T, bool> canDeleteComponent;

        public DeleteComponentsHereWith(Func<T, bool> deleteCondition)
        {
            canDeleteComponent = deleteCondition;
        }

        public void Run(EcsSystems systems)
        {
            var pool = world.GetPool<T>();

            foreach (var entity in filter)
            {
                if (canDeleteComponent(pool.Get(entity)))
                {
                    pool.Del(entity);
                }
            }
        }
        
        public void WorldChanged(EcsWorld world)
        {
            this.world = world;
            filter = world.Filter<T>().End();
        }
    }
}