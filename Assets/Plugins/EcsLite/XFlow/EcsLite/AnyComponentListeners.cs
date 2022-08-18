using System;
using System.Collections.Generic;

namespace XFlow.EcsLite
{
    public class AnyComponentListeners<T>:IAnyComponentListeners where T:struct
    {
        public List<EventsSystem<T>.IAnyComponentChangedListener> Changed;
        public List<EventsSystem<T>.IAnyComponentRemovedListener> Removed;

        public void RemoveChanged(Object listener)
        {
            Changed.Remove(listener as EventsSystem<T>.IAnyComponentChangedListener);
        }
    
        public void RemoveRemoved(Object listener)
        {
            Removed.Remove(listener as EventsSystem<T>.IAnyComponentRemovedListener);
        }
    }
}