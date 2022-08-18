using System;
using XFlow.EcsLite;

namespace XFlow.Utils
{
    public static class EcsWorldExt
    {
        public delegate void RefAction<T1>(ref T1 arg1);
        
        public static ref T EntityGetRef<T>(this EcsWorld world, int entity) where T:struct
        {
            var pool = world.GetPool<T>();
            return ref pool.GetRef(entity);
        }
        
        public static T EntityGet<T>(this EcsWorld world, int entity) where T:struct
        {
            var pool = world.GetPool<T>();
            return pool.Get(entity);
        }
        
        public static ref T EntityAdd<T>(this EcsWorld world, int entity) where T:struct
        {
            var pool = world.GetPool<T>();
            return ref pool.Add(entity);
        }
        
        public static void EntityReplace<T>(this EcsWorld world, int entity, T data) where T:struct
        {
            var pool = world.GetPool<T>();
            pool.Replace(entity, data);
        }
    
        public static bool EntityHas<T>(this EcsWorld world, int entity) where T:struct
        {
            var pool = world.GetPool<T>();
            return pool.Has(entity);
        }
        
        public static T? EntityGetNullable<T>(this EcsWorld world, int entity) where T:struct
        {
            var pool = world.GetPool<T>();
            return pool.GetNullable(entity);
        }
        
        public static void EntityDel<T>(this EcsWorld world, int entity) where T:struct
        {
            var pool = world.GetPool<T>();
            pool.Del(entity);
        }

        public static bool EntityWithRef<T>(this EcsWorld world, int entity, RefAction<T> action) where T : struct
        {
            var pool = world.GetPool<T>();
            if (!pool.Has(entity))
                return false;
            action(ref pool.GetRef(entity));
            return true;
        }
        
        public static bool EntityWith<T>(this EcsWorld world, int entity, Action<T> action) where T : struct
        {
            var pool = world.GetPool<T>();
            if (!pool.Has(entity))
                return false;
            action(pool.Get(entity));
            return true;
        }
    }
}