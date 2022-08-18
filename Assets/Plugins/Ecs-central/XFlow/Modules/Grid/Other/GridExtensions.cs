using System.Collections.Generic;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Grid.Components;
using Vector2Int = XFlow.Modules.Grid.Model.Vector2Int;

namespace XFlow.Modules.Grid.Other
{
    public static class GridExtensions
    {
        public delegate bool Predicate(int entity);
        
        public static Vector2Int Vector3ToGridPosition(Vector3 position) =>
            new Vector2Int
            {
                x = (int)position.x,
                y = (int)position.z
            };

        public static int GetNearestEntities(
            this EcsWorld world, int excludedEntity, Vector3 position, float radius, ref List<int> entities, Predicate predicate)
        {
            entities.Clear();
            
            var gridComponent = world.GetUnique<GridComponent>();
            var gridMap = gridComponent.GridMap;

            var poolPosition = world.GetPool<PositionComponent>();
            
            var startX = position.x - radius;
            var endX = position.x + radius;
            
            var startZ = position.z - radius;
            var endZ = position.z + radius;

            var sqrRadius = radius * radius;

            var gridStart = Vector3ToGridPosition(new Vector3(startX, 0, startZ));
            var gridEnd = Vector3ToGridPosition(new Vector3(endX, 0, endZ));

            for (int x = gridStart.x; x <= gridEnd.x; x++)
            {
                for (int y = gridStart.y; y <= gridEnd.y; y++)
                {
                    var gridPosition = new Vector2Int(x, y);
                    if(!gridMap.ContainsKey(gridPosition)) 
                        continue;

                    
                    List<EcsPackedEntity> cellEntities = gridMap[gridPosition];
                    foreach (var packedEntity in cellEntities)
                    {
                        int cellEntity;
                        if (!packedEntity.Unpack(world, out cellEntity))
                            continue;
                        
                        var dir = position - poolPosition.Get(cellEntity).value;
                        dir.y = 0;
                        if (dir.sqrMagnitude > sqrRadius)
                            continue;
                        if (!predicate(cellEntity))
                            continue;
                        if (excludedEntity != cellEntity)
                            entities.Add(cellEntity);
                    }
                }
            }


            entities.Sort((e1, e2) =>
            {
                var dir1 = position - poolPosition.Get(e1).value;
                var dir2 = position - poolPosition.Get(e2).value;
                return dir1.sqrMagnitude.CompareTo(dir2.sqrMagnitude);
            });
            

            return entities.Count;
        }
    }
}