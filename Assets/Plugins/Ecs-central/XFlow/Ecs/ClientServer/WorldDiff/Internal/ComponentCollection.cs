using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;
using XFlow.EcsLite;
using XFlow.Utils;

namespace XFlow.Ecs.ClientServer.WorldDiff.Internal
{
    public class ComponentCollection<T>: IComponentCollection where T:struct
    {
        private Type type;
        private int id;
        private List<ComponentDeltaGroup> groupsPool = new List<ComponentDeltaGroup>();
        private bool forceJsonComponent;
        private bool emptyComponent;

        private IntPtr memA;
        private IntPtr memB;
        private byte[] arrayA;
        private byte[] arrayB;
        private T[] compareItems;
        
        public ComponentCollection(int id)
        {
            type = typeof(T);
            this.id = id;
            forceJsonComponent = type.GetCustomAttribute<ForceJsonSerialize>() != null;

            emptyComponent = XFlow.Utils.Utils.CheckIsEmptyComponent<T>();

            if (!forceJsonComponent)
            {
                var sizeOne = Marshal.SizeOf<T>();// * 1000;
                memA = Marshal.AllocHGlobal(sizeOne);
                memB = Marshal.AllocHGlobal(sizeOne);
                arrayA = new byte[sizeOne];
                arrayB = new byte[sizeOne];
                //compareItems = new T[1000];
            }
        }


        public string GetFullName()
        {
            return type.FullName;
        }

        public Type GetComponentType()
        {
            return type;
        }

        public int GetId()
        {
            return id;
        }

        public void SetId(int id)
        {
            this.id = id;
        }

        static bool ByteArrayCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
        
        public ComponentDeltaGroup CompareEntities(IEcsPool poolFrom, IEcsPool poolTo, 
            List<int> compareEntities, List<int> createdEntities)
        {
            ComponentDeltaGroup group = GetOrCreateNewGroup();

            var poolA = poolFrom as EcsPool<T>;
            var poolB = poolTo as EcsPool<T>;
            



            List<int> changed = group.ChangedEntities;
            List<int> removed = group.RemovedFromEntities;
            
            List<T> data = group.Data as List<T>;


            
            if (forceJsonComponent)
            {
                var cmp = EqualityComparer<T>.Default;

                for (int i = 0; i < compareEntities.Count; ++i)
                {
                    var entity = compareEntities[i];
                    if (poolB.Has(entity))
                    {
                        if (poolA.Has(entity))
                        {
                            var a = poolA.Get(entity);
                            var b = poolB.Get(entity);

                            if (!cmp.Equals(a, b))
                            {
                                changed.Add(entity);
                                data.Add(b);
                            }
                        }
                        else
                        {
                            changed.Add(entity);
                            data.Add(poolB.Get(entity));
                        }
                    }
                    else
                    {
                        if (poolA.Has(entity))
                            removed.Add(entity);
                    }
                }
            }
            else
            {
                int sizeOne = arrayA.Length;
                for (int i = 0; i < compareEntities.Count; ++i)
                {
                    var entity = compareEntities[i];
                    if (poolB.Has(entity))
                    {
                        if (poolA.Has(entity))
                        {
                            var a = poolA.Get(entity);
                            var b = poolB.Get(entity);

#if true
                            MemoryMarshal.Write(arrayA, ref a);
                            MemoryMarshal.Write(arrayB, ref b);

                            if (!ByteArrayCompare(arrayA, arrayB))
                            {
                                changed.Add(entity);
                                data.Add(b);
                            }
#elif false 
                            var spanA = MemoryMarshal.CreateReadOnlySpan(ref a, 1);
                            var spanB = MemoryMarshal.CreateReadOnlySpan(ref b, 1);
                            var qA = MemoryMarshal.AsBytes(spanA);
                            var qB = MemoryMarshal.AsBytes(spanB);
                            if (!ByteArrayCompare(qA, qB))
                            {
                                changed.Add(entity);
                                data.Add(b);
                            }
#else
                            Marshal.StructureToPtr<T>(a, memA, true);
                            Marshal.StructureToPtr<T>(b, memB, true);
                            Marshal.Copy(memA, arrayA, 0, sizeOne);
                            Marshal.Copy(memB, arrayB, 0, sizeOne);

                            if (!ByteArrayCompare(arrayA, arrayB))
                            {
                                changed.Add(entity);
                                data.Add(b);
                            }
#endif
                        }
                        else
                        {
                            changed.Add(entity);
                            data.Add(poolB.Get(entity));
                        }
                    }
                    else
                    {
                        if (poolA.Has(entity))
                            removed.Add(entity);
                    }
                }
            }
            

            for (int i = 0; i < createdEntities.Count; ++i)
            {
                var entity = createdEntities[i];
                if (poolB.Has(entity))
                {
                    changed.Add(entity);
                    data.Add(poolB.Get(entity));
                }
            }

            if (group.isEmpty())
            {
                PutGroupBackToPool(group);
                return null;
            }

            if (emptyComponent)
                data.Clear();

            return group;
        }

        public ComponentDeltaGroup AddAllEntities(IEcsPool poolTo, List<int> entities)
        {
            ComponentDeltaGroup group = GetOrCreateNewGroup();
            
            var pool = poolTo as EcsPool<T>;
            
            List<int> changed = group.ChangedEntities;
            List<T> data = group.Data as List<T>;
            
            for (int i = 0; i < entities.Count; ++i)
            {
                var entity = entities[i];
                if (pool.Has(entity))
                {
                    changed.Add(entity);
                    data.Add(pool.Get(entity));
                }
            }
            
            if (group.isEmpty())
            {
                PutGroupBackToPool(group);
                return null;
            }
            
            if (emptyComponent)
                data.Clear();

            return group;
        }

        
        public void CopyAllEntities(EcsWorld worldFrom, EcsWorld worldTo, List<int> entities)
        {
            var poolFrom = worldFrom.GetPoolByType(type) as EcsPool<T>;
            if (poolFrom == null)
                return;//nothing to copy
            
            var poolTo = worldTo.GetPool<T>();
            var entitiesCount = entities.Count;
            for (int i = 0; i < entitiesCount; ++i)
            {
                int entity = entities[i]; 
                if (!poolFrom.Has(entity))
                    continue;
                    
                var component = poolFrom.Get(entity);
                poolTo.Add(entity) = component;
            }
        }

        public void ApplyDiff(EcsWorld world, List<int> entities,
            object componentsData, List<int> removedEntities)
        {
            var pool = world.GetPool<T>();
            if (entities?.Count > 0)
            {
                if (emptyComponent)
                {
                    for (int i = 0; i < entities.Count; ++i)
                    {
                        var entity = entities[i];
                        pool.GetOrCreateRef(entity);
                    }  
                }
                else
                {
                    //throw new Exception("aaa");

                    var components = componentsData as List<T>;
                    
                    for (int i = 0; i < entities.Count; ++i)
                    {
                        var entity = entities[i];
                        var component = components[i];
                        pool.Replace(entity, component);
                    }    
                }
            }

            if (removedEntities != null)
            {
                for (int i = 0; i < removedEntities.Count; ++i)
                {
                    var entity = removedEntities[i];
                    pool.Del(entity);
                }
            }
        }

        public void Write(HGlobalWriter writer, ComponentDeltaGroup group)
        {
            var array = group.Data as List<T>;
            if (forceJsonComponent)
            {
                writer.WriteInt32(array.Count);
                foreach (var item in array)
                    writer.WriteString(JsonUtility.ToJson(item));
            }
            else
                writer.WriteListT(array, true);
        }

        public void WriteSingleComponentWithId(HGlobalWriter writer, object data)
        {
            writer.WriteInt32(id);
            if (forceJsonComponent)
            {
                var str = JsonUtility.ToJson((T)data);
                writer.WriteString(str);
                return;   
            }
            writer.WriteSingleT((T)data);
        }

        public ComponentDeltaGroup ReadComponents(HGlobalReader reader)
        {
            var group = GetOrCreateNewGroup();

            var components = group.Data as List<T>;

            if (forceJsonComponent)
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                    components.Add(JsonUtility.FromJson<T>(reader.ReadString()));
            }
            else
                reader.ReadStructures(components);

            return group;
        }

        public object ReadSingleComponent(HGlobalReader reader)
        {
            if (forceJsonComponent)
            {
                var str = reader.ReadString();
                return JsonUtility.FromJson<T>(str);
            }
            T data = reader.ReadStructure<T>();
            return data;
        }

        public ComponentDeltaGroup GetOrCreateNewGroup()
        {
            ComponentDeltaGroup group = null;
            if (groupsPool.Count > 0)
            {
                group = groupsPool[groupsPool.Count - 1];
                
                group.Component = this;
                group.ChangedEntities.Clear();
                group.RemovedFromEntities.Clear();
                (group.Data as List<T>).Clear();
                
                groupsPool.RemoveAt(groupsPool.Count - 1);
                return group;
            }
            
            group = new ComponentDeltaGroup();
            group.Data = new List<T>();
            group.Component = this;
            group.ChangedEntities = new List<int>();
            group.RemovedFromEntities = new List<int>();    
            
            return group;
        }

        public object ListToArray(object lst)
        {
            return (lst as List<T>).ToArray();
        }

        [Serializable]
        struct Json
        {
            public T[] v;
        }
        
        
        public string ListToJson(object lst)
        {
            if (emptyComponent)
                return "";

            if (!type.IsSerializable)
               throw new Exception($"Component of type {type} isn't Serializable to JSON");
            
            var items = lst as List<T>;
            return JsonUtility.ToJson(new Json {v = items.ToArray()});
        }


        public void AddFromJson(string str, ComponentDeltaGroup group)
        {
            if (emptyComponent)
                return;

            if (!type.IsSerializable)
                throw new Exception($"Component of type {type} isn't Serializable to JSON");
            
            var js = JsonUtility.FromJson<Json>(str);
            var lst = group.Data as List<T>;
            lst.Clear();
            lst.AddRange(js.v);
        }

        public void PutGroupBackToPool(ComponentDeltaGroup group)
        {
            groupsPool.Add(group);
        }
    }

}