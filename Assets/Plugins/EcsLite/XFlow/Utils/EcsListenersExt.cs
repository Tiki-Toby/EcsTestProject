using System;
using System.Collections.Generic;
using XFlow.EcsLite;

namespace XFlow.Utils
{
    public static class EcsListenersExt 
    {
        public static void AddChangedListener<T>(this int entity, EcsWorld world, EventsSystem<T>.IComponentChangedListener componentChangedListener) where T:struct
        {
            var pool = world.GetPool<EventsSystem<T>.ListenersComponent>();
            if (!pool.Has(entity))
                pool.Add(entity);
            ref var listeners = ref pool.InternalGetRef(entity).Changed;
            
            if (listeners == null)
                listeners = new List<EventsSystem<T>.IComponentChangedListener>();
            
            listeners.Add(componentChangedListener);
        }
        
        public static void AddRemovedListener<T>(this int entity, EcsWorld world, EventsSystem<T>.IComponentRemovedListener componentRemovedListener) where T:struct
        {
            var pool = world.GetPool<EventsSystem<T>.ListenersComponent>();
            
            if (!pool.Has(entity))
                pool.Add(entity);
            
            ref var listeners = ref pool.InternalGetRef(entity).Removed;
            
            if (listeners == null)
                listeners = new List<EventsSystem<T>.IComponentRemovedListener>();

            listeners.Add(componentRemovedListener);
        }
        
        [Obsolete("use DelChangedListener")]
        public static void RemoveChangedListener<T>(this int entity, EcsWorld world, EventsSystem<T>.IComponentChangedListener listener) where T:struct
        {
            DelChangedListener<T>(entity, world, listener);
        }
        
        [Obsolete("use DelRemovedListener")]
        public static void RemoveRemovedListener<T>(this int entity, EcsWorld world, EventsSystem<T>.IComponentRemovedListener listener) where T:struct
        {
            DelRemovedListener<T>(entity, world, listener);
        }
        
        public static void DelChangedListener<T>(this int entity, EcsWorld world, EventsSystem<T>.IComponentChangedListener listener) where T:struct
        {
            var pool = world.GetPool<EventsSystem<T>.ListenersComponent>();
            if (!pool.Has(entity))
                return;

            pool.InternalGetRef(entity).Changed.Remove(listener);
        }
        
        public static void DelRemovedListener<T>(this int entity, EcsWorld world, EventsSystem<T>.IComponentRemovedListener listener) where T:struct
        {
            var pool = world.GetPool<EventsSystem<T>.ListenersComponent>();
            if (!pool.Has(entity))
                return;

            pool.InternalGetRef(entity).Removed.Remove(listener);
        }
    }
}