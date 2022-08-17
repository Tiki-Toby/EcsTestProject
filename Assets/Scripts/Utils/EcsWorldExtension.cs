using System;
using Leopotam.EcsLite;

namespace Utils
{
    public static class EcsWorldExtension
    {
        public static ref T GetUnique<T>(this EcsWorld world) where T : struct
        {
            return ref world.GetPool<T>().Get(0);
        }

        public static ref T AddUnique<T>(this EcsWorld world) where T : struct
        {
            if (world.HasUnique<T>())
                return ref world.GetUnique<T>();
            else
                return ref world.GetPool<T>().Add(0);
        }
        
        public static ref T GetOrCreateUnique<T>(this EcsWorld world) where T : struct
        {
            if (world.HasUnique<T>())
                return ref world.GetUnique<T>();
            else
                return ref world.AddUnique<T>();
        }
        
        public static void DelUnique<T>(this EcsWorld world) where T : struct
        {
            world.GetPool<T>().Del(0);
        }
        
        public static bool HasUnique<T>(this EcsWorld world) where T : struct
        {
            return world.GetPool<T>().Has(0);
        }
        
        public static ref T GetFirstComponent<T>(this EcsWorld world) where T : struct
        {
            var filter = world.Filter<T>().End ();
            var pool = world.GetPool<T>();

            if (!pool.Has(filter.GetRawEntities()[0]))
                throw new Exception("don't set input data component");

            return ref pool.Get(filter.GetRawEntities()[0]);
        }
    }
}