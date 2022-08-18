using System;
using System.Collections.Generic;

namespace XFlow.EcsLite
{
    public class AnyListener
    {
        private EcsWorld world;
        private Dictionary<Type, Object> registeredChangedListeners = new Dictionary<Type, object>();
        private Dictionary<Type, Object> registeredRemovedListeners = new Dictionary<Type, object>();

        public AnyListener(EcsWorld world)
        {
            this.world = world;
        }
  
        public void SetAnyChangedListener<T>(EventsSystem<T>.IAnyComponentChangedListener listener) where T : struct
        {
            var instance = world.GetAnyListeners<T>();
            if (instance.Changed == null)
                instance.Changed = new List<EventsSystem<T>.IAnyComponentChangedListener>();
            
            instance.Changed.Add(listener);
            registeredChangedListeners.Add(typeof(T), listener);
        }
        
        public void SetAnyRemovedListener<T>(EventsSystem<T>.IAnyComponentRemovedListener listener) where T : struct
        {
            var instance = world.GetAnyListeners<T>();
            if (instance.Removed == null)
                instance.Removed = new List<EventsSystem<T>.IAnyComponentRemovedListener>();
            
            instance.Removed.Add(listener);
            registeredRemovedListeners.Add(typeof(T), listener);
        }
        
        public void DelChangedListener<T>(EventsSystem<T>.IAnyComponentChangedListener listener) where T : struct
        {
            var instance = world.GetAnyListeners<T>();
            if (instance.Changed == null)
                return;

            instance.Changed.Remove(listener);
            registeredChangedListeners.Remove(typeof(T));
        }
        
        public void DelRemovedListener<T>(EventsSystem<T>.IAnyComponentRemovedListener listener) where T : struct
        {
            var instance = world.GetAnyListeners<T>();
            if (instance.Removed == null)
                return;

            instance.Removed.Remove(listener);
            registeredRemovedListeners.Remove(typeof(T));
        }

        public void Destroy()
        {
            foreach (var keyValuePair in registeredChangedListeners)
            {
                var key = keyValuePair.Key;
                var listener = keyValuePair.Value;
                world.anyListeners[key].RemoveChanged(listener);
            }
            
            foreach (var keyValuePair in registeredRemovedListeners)
            {
                var key = keyValuePair.Key;
                var listener = keyValuePair.Value;
                world.anyListeners[key].RemoveRemoved(listener);
            }
        }
    }
}