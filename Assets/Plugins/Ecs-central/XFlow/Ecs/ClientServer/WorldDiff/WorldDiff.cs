using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.Ecs.ClientServer.WorldDiff.Internal;
using XFlow.EcsLite;
using XFlow.Utils;

namespace XFlow.Ecs.ClientServer.WorldDiff
{
    public class WorldDiff
    {
        public List<int> RemovedEntities = new List<int>();
        public List<int> CreatedEntities = new List<int>();
        public List<short> CreatedEntitiesGen = new List<short>();
        public List<ComponentDeltaGroup> Groups = new List<ComponentDeltaGroup>();
        
        //todo, multithreading? make static?
        private List<int> compareEntities = new List<int>();
        private List<int> existingEntities = new List<int>();
        private List<int> deletedComponentsFromEntities = new List<int>();
        
        public static WorldDiff BuildDiff(ComponentsCollection collection, EcsWorld worldFromA, EcsWorld worldToB, WorldDiff result = null)
        {
            Profiler.BeginSample("WorldDiff.BuildDiff");
            
            int entitiesCountA = 0;
            var entitiesA = worldFromA.GetRawEntities();
            entitiesCountA = worldFromA.GetAllocatedEntitiesCount();
            
            int entitiesCountB = 0;
            var entitiesB = worldToB.GetRawEntities();
            entitiesCountB = worldToB.GetAllocatedEntitiesCount();

            if (result == null)
                result = new WorldDiff();
            else
                result.Reset();
            
            
            List<int> removedEntities = result.RemovedEntities;
            List<int> createdEntities = result.CreatedEntities;
            List<short> createdEntitiesGen = result.CreatedEntitiesGen;

            var compareEntities = result.compareEntities;
            var existingEntities = result.existingEntities;
            
            
            int countAB = Math.Min(entitiesCountA, entitiesCountB);
            
            
            for (int entity = 0; entity < countAB; ++entity)
            {
                var genB = entitiesB[entity].Gen;
                if (genB > 0)
                {
                    var genA = entitiesA[entity].Gen;
                    //B alive
                    if (genA > 0)
                    {
                        if (genA != genB)
                        {
                            removedEntities.Add(entity);
                            createdEntities.Add(entity);
                            createdEntitiesGen.Add(genB);
                        }
                        else
                        {
                            compareEntities.Add(entity);
                        }
                    }
                    else
                    {
                        createdEntities.Add(entity);
                        createdEntitiesGen.Add(genB);
                    }
                    
                    existingEntities.Add(entity);
                }
                else
                {
                    var genA = entitiesA[entity].Gen;
                    if (genA > 0)
                    {
                        removedEntities.Add(entity);
                    }
                    else
                    {
                        //both dead
                    }
                }
            }

            for (int entity = countAB; entity < entitiesCountA; ++entity)
            {
                if (entitiesA[entity].Gen > 0)
                {
                    removedEntities.Add(entity);
                }
            }
            
            for (int entity = countAB; entity < entitiesCountB; ++entity)
            {
                var genB = entitiesB[entity].Gen;
                if (genB > 0)
                {
                    createdEntities.Add(entity);
                    createdEntitiesGen.Add(genB);
                    existingEntities.Add(entity);
                }
            }
            
            

            var allComponents = collection.Components;

            var deletedComponentsFromEntities = result.deletedComponentsFromEntities;
            
            for (int index = 0; index < allComponents.Count; ++index)
            {
                deletedComponentsFromEntities.Clear();
                var comp = allComponents[index];
                var type = comp.GetComponentType();
                
                IEcsPool poolFrom = worldFromA.GetPoolByType(type);
                IEcsPool poolTo = worldToB.GetPoolByType(type);

                 
                if (poolFrom == null && poolTo == null)
                {
                    continue;
                }

                if (poolFrom == null)
                {
                    //poolTo != null
                    //add all components poolFrom -> poolTo
                    var group = comp.AddAllEntities(poolTo, existingEntities);
                    if (group != null)
                        result.Groups.Add(group);

                    continue;
                    
                }
                
                if (poolTo == null)
                {
                    //poolFrom != null
                    //remove all components from poolFrom
                    
                    int total = worldFromA.GetAllocatedEntitiesCount();

                    var sparce = poolFrom.GetRawSparseItems();
                    for (int entity = 0; entity < total; ++entity)
                    {
                        if (!worldFromA.IsEntityAliveInternal(entity))
                            continue;
                        if (worldToB.IsEntityAliveInternal(entity) && sparce[entity] > 0)
                            deletedComponentsFromEntities.Add(entity);
                    }

                    if (deletedComponentsFromEntities.Count > 0)
                    {
                        var group = comp.GetOrCreateNewGroup();
                        group.RemovedFromEntities = deletedComponentsFromEntities;
                        result.Groups.Add(group);
                    }

                    continue;
                }
                
                var group1 = comp.CompareEntities( poolFrom, poolTo, compareEntities, createdEntities);
                if (group1 != null)
                    result.Groups.Add(group1);
            }
            
            Profiler.EndSample();
            return result;
        }
        
        public void ApplyChanges(EcsWorld world)
        {
            Profiler.BeginSample("WorldDiff.ApplyChanges");
            
            foreach (var diffRemovedEntity in RemovedEntities)
            {
                world.DelEntity(diffRemovedEntity);
            }

            world.SortRecycledEntities();
            
            WorldUtils.CreateEntities(CreatedEntities, CreatedEntitiesGen, world);

            for (int i = 0; i < Groups.Count; ++i)
            {
                var group = Groups[i];
                group.ApplyChanges(world);
            }
            
            world.SortRecycledEntities();
            Profiler.EndSample();
        }

        
        private void Reset()
        {
            CreatedEntities.Clear();
            RemovedEntities.Clear();
            CreatedEntitiesGen.Clear();
            
            foreach (var group in Groups)
            {
                group.Component.PutGroupBackToPool(group);
            }
            
            Groups.Clear();
            
            compareEntities.Clear();
            existingEntities.Clear();
            deletedComponentsFromEntities.Clear();
        }

        
        public byte[] ToByteArray(bool useFullNames, HGlobalWriter writer = null)
        {
            if (writer == null)
                writer = new HGlobalWriter();
            WriteBinary(useFullNames, writer);
            var res = writer.CopyToByteArray();
            return res;
        }

        public string ToBase64String()
        {
            var data = ToByteArray(true);
            return Convert.ToBase64String(data);
        }

        public string ToJsonString(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(ToJsonSerializable(), prettyPrint);
        }
        
        public WorldDiffJsonSerializable ToJsonSerializable()
        {
            var data = new WorldDiffJsonSerializable();
            data.CreatedEntities = CreatedEntities.ToArray();
            data.RemovedEntities = RemovedEntities.ToArray();
            data.CreatedEntitiesGen = CreatedEntitiesGen.ToArray();
            data.Groups = new ComponentDeltaGroupSerializable[Groups.Count];
            for (var i = 0; i < Groups.Count; i++)
            {
                data.Groups[i] = Groups[i].BuildSerializable();
            }
            return data;
        }
        
        public void WriteBinary(bool useFullNames, HGlobalWriter writer)
        {
            writer.Reset();
            writer.WriteByte(useFullNames ? (byte)1 : (byte)0);//todo, optimize? remove?
            writer.WriteInt32Array(RemovedEntities, true);
            writer.WriteInt32Array(CreatedEntities, true);
            writer.WriteInt16Array(CreatedEntitiesGen, true);

            writer.WriteInt32(Groups.Count);
            foreach (var group in Groups)
            {
                if (useFullNames)
                    writer.WriteString(group.Component.GetComponentType().FullName);
                else    
                    writer.WriteInt32(group.Component.GetId());
                group.Component.Write(writer, group);
                
                writer.WriteInt32Array(group.ChangedEntities, true);
                writer.WriteInt32Array(group.RemovedFromEntities, true);
            }
        }
        
        public static WorldDiff FromJsonString(ComponentsCollection collection, String str)
        {
            if (str == null)
                throw new Exception("json string is null");
            var ser = JsonUtility.FromJson<WorldDiffJsonSerializable>(str);
            return FromJsonSerializable(collection, ser);
        }

        public static WorldDiff FromJsonSerializable(ComponentsCollection collection, WorldDiffJsonSerializable serializable)
        {
            var dif = new WorldDiff();
            if (serializable.RemovedEntities != null)
                dif.RemovedEntities.AddRange(serializable.RemovedEntities);
            if (serializable.CreatedEntities != null)
            {
                dif.CreatedEntities.AddRange(serializable.CreatedEntities);
                dif.CreatedEntitiesGen.AddRange(serializable.CreatedEntitiesGen);
            }

            if (serializable.Groups == null)
                return dif;
            
            for (var i = 0; i < serializable.Groups.Length; i++)
            {
                var groupSerializable = serializable.Groups[i];

                IComponentCollection component = collection.GetComponent(groupSerializable.FullComponentName);

                var group = component.GetOrCreateNewGroup();
                if (groupSerializable.ChangedEntities != null)
                    group.ChangedEntities.AddRange(groupSerializable.ChangedEntities);
                if (groupSerializable.RemovedFromEntities != null)
                    group.RemovedFromEntities.AddRange(groupSerializable.RemovedFromEntities);
                if (groupSerializable.Json != null)
                    component.AddFromJson(groupSerializable.Json, group);
                
                dif.Groups.Add(group);
            }
            
            return dif;
        }
        
        public static WorldDiff FromByteArray(ComponentsCollection collection, byte[] array, WorldDiff result = null)
        {
            var reader = new HGlobalReader(array);

            byte header = reader.ReadByte();
            var useFullNames = header == 1;

            if (result == null)
                result = new WorldDiff();
            else
            {
                result.Groups.Clear();
            }

            reader.ReadInt32Array(result.RemovedEntities);
            reader.ReadInt32Array(result.CreatedEntities);
            reader.ReadInt16Array(result.CreatedEntitiesGen);

            int groupsCount = reader.ReadInt32();

            for (int i = 0; i < groupsCount; ++i)
            {
                IComponentCollection component = null;
                if (useFullNames)
                {
                    var fullName = reader.ReadString();
                    component = collection.GetComponent(fullName);
                }
                else
                {
                    var id = reader.ReadInt32();
                    component = collection.GetComponent(id);
                }

                var group = component.ReadComponents(reader);
                
                reader.ReadInt32Array(group.ChangedEntities);
                reader.ReadInt32Array(group.RemovedFromEntities);

                result.Groups.Add(group);
            }

            return result;
        }
    }
}