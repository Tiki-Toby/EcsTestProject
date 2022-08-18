using System;
using System.Collections.Generic;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff.Internal;
using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.WorldDiff
{
    public class ComponentsCollection
    {
        public List<IComponentCollection> Components { get; private set; } = new List<IComponentCollection>();
        
        private readonly Dictionary<string, IComponentCollection> componentsByName = new Dictionary<string, IComponentCollection>();
        private readonly Dictionary<Type, IComponentCollection> componentsByType = new Dictionary<Type, IComponentCollection>();

        public void SetupPools(EcsWorld world)
        {
            
        }

        public bool RemapOrder(string[] fullNames)
        {
            bool ok = true;
            if (fullNames.Length != Components.Count)
            {
                var str = $"not equal count of components {Components.Count} vs {fullNames.Length}";
                Debug.LogWarning(str);
                ok = false;
                //throw new Exception(str);
            }

            Components.Clear();
            foreach (var name in fullNames)
            {
                if (componentsByName.TryGetValue(name, out IComponentCollection pool))
                {
                    var cm = componentsByName[name];
                    cm.SetId(Components.Count);
                    Components.Add(cm);
                    continue;
                }

                var err = $"FullName '{name}' not found in components list";
                Debug.LogWarning(err);
                ok = false;
                //throw new Exception(err);
            }
            
            return ok;
        }

        public void AddComponent<T>(int denseSize = -1, int recycledSize = -1) where T:struct
        {
            var type = typeof(T);
            var component = new ComponentCollection<T>(Components.Count);

            if (!componentsByType.TryAdd(type, component))
                return;
            
            Components.Add(component);
            componentsByName.Add(type.FullName, component);
        }
        

        public bool ContainsCollection(string fullName)
        {
            return componentsByName.ContainsKey(fullName);
        }
        
        public bool ContainsCollection(Type type)
        {
            return componentsByType.ContainsKey(type);
        }
        
        public IComponentCollection GetComponent(string fullName)
        {
            return componentsByName[fullName];
        }
        
        public IComponentCollection GetComponent(int id)
        {
            return Components[id];
        }
        
        public IComponentCollection GetComponent(Type type)
        {
            if (componentsByType.TryGetValue(type, out IComponentCollection cc))
                return cc;
            return null;
        }
    }
}

