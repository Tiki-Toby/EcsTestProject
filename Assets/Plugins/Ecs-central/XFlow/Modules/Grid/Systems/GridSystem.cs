using System.Collections.Generic;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Grid.Components;
using XFlow.Modules.Grid.Other;
using Vector2Int = XFlow.Modules.Grid.Model.Vector2Int;

namespace XFlow.Modules.Grid.Systems
{
    public sealed class GridSystem : IEcsRunSystem, IEcsInitSystem
    {
        class Vector2IntEqualityComparer : IEqualityComparer<Model.Vector2Int>
        {
            public bool Equals(Vector2Int one, Vector2Int other)
            {
                return one.m_X == other.m_X && one.m_Y == other.m_Y;
            }

            public int GetHashCode(Vector2Int value) => value.m_X ^ (value.m_Y << 2);
        }
        
        
        private EcsWorld world;
        private EcsFilter filter;
        private EcsPool<PositionComponent> poolPosition;

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world
                .Filter<PositionComponent>()
                .End();
            poolPosition = world.GetPool<PositionComponent>();
            
            world.AddUnique<GridComponent>().GridMap = new Dictionary<Vector2Int, List<EcsPackedEntity>>(new Vector2IntEqualityComparer());
        }
        
        public void Run(EcsSystems systems)
        {
            var gridMap = world.GetUniqueRef<GridComponent>().GridMap;

            var values = gridMap.Values;

            foreach (var value in values)
            {
                value.Clear();
            }
        
            foreach (var entity in filter)
            {
                var positionComponent = poolPosition.Get(entity);
                var gridPos = GridExtensions.Vector3ToGridPosition(positionComponent.value);

                if (!gridMap.TryGetValue(gridPos, out var list))
                {
                    gridMap.Add(gridPos, new List<EcsPackedEntity>(8)
                    {
                        world.PackEntity(entity)
                    });
                }
                else
                {
                    list.Add(world.PackEntity(entity));
                }
            }
        }
    }
}