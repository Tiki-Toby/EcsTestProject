using System;
using XFlow.EcsLite;

namespace XFlow.Utils
{
    public static class EntityExt
    {
        public delegate void RefAction<T1>(ref T1 arg1);

        public static ref T EntityGetRefComponent<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return ref pool.GetRef(entity);
        }

        public static ref T EntityGetRef<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return ref pool.GetRef(entity);
        }

        public static T EntityGetComponent<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return pool.Get(entity);
        }

        public static T EntityGet<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return pool.Get(entity);
        }


        public static ref T EntityAddComponent<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return ref pool.Add(entity);
        }

        public static ref T EntityAdd<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return ref pool.Add(entity);
        }


        public static void EntityReplace<T>(this int entity, EcsWorld world, T data) where T : struct
        {
            var pool = world.GetPool<T>();
            pool.Replace(entity, data);
        }
        
        public static ref T EntityGetOrCreateRef<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return ref pool.GetOrCreateRef(entity);
        }

        public static bool EntityHasComponent<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return pool.Has(entity);
        }

        public static bool EntityHas<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return pool.Has(entity);
        }

        public static bool EntityTryGet<T>(this int entity, EcsWorld world, out T component) where T : struct
        {
            var pool = world.GetPool<T>();
            return pool.TryGet(entity, out component);
        }

        public static T? EntityGetNullable<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            return pool.GetNullable(entity);
        }

        public static void EntityDelComponent<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            pool.Del(entity);
        }

        public static void EntityDel<T>(this int entity, EcsWorld world) where T : struct
        {
            var pool = world.GetPool<T>();
            pool.Del(entity);
        }

        public static bool EntityWithRef<T>(this int entity, EcsWorld world, RefAction<T> action) where T : struct
        {
            var pool = world.GetPool<T>();
            if (!pool.Has(entity))
                return false;
            action(ref pool.GetRef(entity));
            return true;
        }

        public static bool EntityWith<T>(this int entity, EcsWorld world, Action<T> action) where T : struct
        {
            var pool = world.GetPool<T>();
            if (!pool.Has(entity))
                return false;
            action(pool.Get(entity));
            return true;
        }
    }
}