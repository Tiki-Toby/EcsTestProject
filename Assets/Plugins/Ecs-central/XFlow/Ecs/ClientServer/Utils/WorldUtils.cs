using System;
using System.Collections.Generic;
using UnityEngine.Profiling;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.Utils
{
    public static class WorldUtils
    {
        public static void CreateEntities(List<int> entities, List<short> gen,  EcsWorld world)
        {
            //var localEntities = new List<Tuple<int, object>[]>();
            List<int> tempEntities = new List<int>();

            var count = entities.Count;
            
            for (int i = 0; i < count; ++i)
            {
                var entity = entities[i];
                //it is precreated unique entity
                if (entity == 0)
                    continue;

                if (world.IsEntityAliveInternal(entity))
                {
                    //throw new Exception("entity exists without LocalEntityComponent");
                    /*
                    #if UNITY_EDITOR
                    //UnityEngine.Assertions.Assert.IsTrue(entity.EntityHas<LocalEntityComponent>(world));
                    if (!entity.EntityHas<LocalEntityComponent>(world))
                    {
                        throw new Exception("entity exists without LocalEntityComponent");
                    }
                    #endif
                    
                    var components = new Tuple<int, object>[world.GetComponentsCount(entity)]; 
                    world.GetComponents(entity, ref components);
                    localEntities.Add(components);
                    
                    
                    */
                    
                    world.DelEntity(entity);
                }
                
                while (true)
                {
                    var entityCopy = world.NewEntity();
                    if (entityCopy == entity)
                    {
                        world.InternalChangeEntityGen(entityCopy, gen[i]);
                        break;
                    }

                    //Assert.IsEqual(true, entityCopy < entity);
                    tempEntities.Add(entityCopy);

                    if (tempEntities.Count > 999)
                    {
                        throw new Exception("temp entities wtf");
                    }
                }
            }

            for (int i = 0; i < tempEntities.Count; ++i)
            {
                world.DelEntity(tempEntities[i]);
            }
            /*
            for (int i = 0; i < localEntities.Count; ++i)
            {
                var components = localEntities[i];
                var entity = world.NewEntity();
                
                for (int c = 0; c < components.Length; ++c)
                {
                    var component = components[c];
                    if (component != null)
                    {
                        var pool = world.GetPoolById(component.Item1);
                        pool.AddRaw(entity, component.Item2);
                    }
                }

                ref var localEntityComponent = ref poolLocalEntity.InternalGetRef(entity);
                localEntityComponent.Instance.Entity = entity;
            }*/
        }
        
        public static EcsWorld CopyWorld(ComponentsCollection collection, EcsWorld world)
        {
            Profiler.BeginSample("CopyWorld");
            var worldCopy = new EcsWorld(world.GetDebugName() + "_copy");
            collection.SetupPools(worldCopy);

            worldCopy.EntityDestroyedListeners.AddRange(world.EntityDestroyedListeners);
            
            var rawEntities = world.GetRawEntities();
            
            List<int> tempEntities = new List<int>();
            List<int> createdEntities = new List<int>();
            
            //skip 0 precreated unique entity
            createdEntities.Add(0);

            //var poolLocal = world.GetPool<LocalEntityComponent>();

            var allocatedEntitiesCount = world.GetAllocatedEntitiesCount();
            for (int entity = 1; entity < allocatedEntitiesCount; ++entity)
            {
                var gen = rawEntities[entity].Gen;
                if (gen <= 0)
                    continue;
                
                while (true)
                {
                    var entityCopy = worldCopy.NewEntity();
                    if (entityCopy == entity)
                    {
                        worldCopy.InternalChangeEntityGen(entity, gen);
                        createdEntities.Add(entity);
                        break;
                    }

                    tempEntities.Add(entityCopy);

                    if (tempEntities.Count > 999)
                    {
                        throw new Exception("temp entities wtf2");
                    }
                }
            }

            foreach (int entity in tempEntities)
            {
                worldCopy.DelEntity(entity);
            }
            
            
            
            var components = collection.Components;
            for (int i = 0; i < components.Count; ++i)
            {
                var comp = components[i];
                comp.CopyAllEntities(world, worldCopy, createdEntities);
            }
            
            Profiler.EndSample();

            return worldCopy;
        }
    }
}