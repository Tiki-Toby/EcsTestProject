using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace XFlow.Ecs.ClientServer.WorldDiff
{
    public static class ComponentsCollectionUtils
    {
        public delegate bool CheckTypeDelegate(Type tp);

        public static  bool DefaultScanCheck(Type type)
        {
            var fullName = type.FullName;
            if (fullName == null)
                return false;
            //Debug.Log($"type {type.FullName} {type.IsPrimitive}");
            if (!fullName.Contains("ClientServer.Components.") &&
                !fullName.Contains("ClientServer.Ecs.Components."))
                return false;
            if (type.GetCustomAttribute<DontSerialize>() != null)
                return false;
            
            return true;
        }
        
        public static void AddComponents(ComponentsCollection pool, CheckTypeDelegate fn = null)
        {
            if (fn == null)
                fn = DefaultScanCheck;
            
            var serTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.GetName().Name;
                if (assemblyName.StartsWith("unity", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (assemblyName.StartsWith("system", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                
                //Debug.Log($"assembly: {assembly.FullName} {assembly.GetName().Name}");
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (!fn(type))
                            continue;
                        if (type.IsClass)
                            continue;
                        serTypes.Add(type);
                    }
                }
                catch(ReflectionTypeLoadException  ex)
                {
                    StringBuilder sb = new StringBuilder();
                    Debug.Log(ex.ToString());
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        Debug.Log(exSub);
                    }
                }
            }

            serTypes.Sort((a,b) => String.Compare(a.FullName, b.FullName, StringComparison.InvariantCultureIgnoreCase));
            
            var addComponentMethodName = nameof(ComponentsCollection.AddComponent);
            var componentPoolType = typeof(ComponentsCollection);
            var addComponentMethod = componentPoolType.GetMethod(addComponentMethodName);
            
            foreach (var type in serTypes)
            {
                try
                {
                    var genericAddComponentMethod = addComponentMethod.MakeGenericMethod(type);
                    genericAddComponentMethod.Invoke(pool, new object[] { -1, -1 });
                    //Debug.Log($"added type {type.FullName}");
                }
                catch (Exception ex) 
                {
                    int i = 0;
                }                
            }
            
            Debug.Log($"Added {serTypes.Count} components");
        }
    }
}