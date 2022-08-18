using System.Collections.Generic;

namespace XFlow.EcsLite
{
    public class AlwaysNull<T> {}

    public class EventsSystem<T>:IEcsPreInitSystem, IEcsInitSystem, IEcsRunSystem where T: struct
    {
        private EcsWorld world;
    
        public interface IComponentChangedListener
        {
            void OnComponentChanged(EcsWorld world, int entity, T data, bool newComponent);
        }
    
        public interface IComponentRemovedListener
        {
            void OnComponentRemoved(EcsWorld world, int entity, AlwaysNull<T> alwaysNull);
        }

        public interface IAnyComponentChangedListener
        {
            void OnAnyComponentChanged(EcsWorld world, int entity, T data, bool added);
        }
    
        public interface IAnyComponentRemovedListener
        {
            void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<T> alwaysNull);
        }
    
        public struct ListenersComponent
        {
            public List<IComponentChangedListener> Changed;
            public List<IComponentRemovedListener> Removed;
        }
    
    
        private readonly List<IComponentChangedListener> ListenersBuffer = new List<IComponentChangedListener>(32);
        private readonly List<IComponentRemovedListener> ListenersRemovedBuffer = new List<IComponentRemovedListener>(32);
    
        private readonly List<IAnyComponentChangedListener> AnyListenersBuffer = new List<IAnyComponentChangedListener>(32);
        private readonly List<IAnyComponentRemovedListener> AnyListenersRemovedBuffer = new List<IAnyComponentRemovedListener>(32);


        /*
    public EventsSystem(bool added = true, bool removed = true, bool changed = true)
    {
        
    }*/
    
        private EcsFilter filterChanges;
        private EcsFilter filterRemoved;
    
        private EcsPool<T> poolComponent;
        private EcsPool<EcsPool<T>.ChangedComponent> poolChanged;
        private EcsPool<EcsPool<T>.AddedComponent> poolAdded;
        private EcsPool<EcsPool<T>.RemovedComponent> poolRemoved;
    
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filterChanges = world.Filter<T>().IncChanges<T>().End();
            filterRemoved = world.FilterRemoved<T>().End();
        
            poolComponent = world.GetPool<T>();
            poolChanged = world.GetPool<EcsPool<T>.ChangedComponent>();
            poolAdded = world.GetPool<EcsPool<T>.AddedComponent>();
        
            poolRemoved = world.GetPool<EcsPool<T>.RemovedComponent>();
        }
    
        public void Run(EcsSystems systems)
        {
            bool hasChangedComponents = filterChanges.GetEntitiesCount() != 0;
            bool hasRemovedComponents = filterRemoved.GetEntitiesCount() != 0;

            if (!hasChangedComponents && !hasRemovedComponents)
                return;//nothing changed


            //collect all AnyListeners
            AnyListenersRemovedBuffer.Clear();
            AnyListenersBuffer.Clear();
    
            var type = typeof(T);

            if (hasChangedComponents)
            {
                if (world.anyListeners.TryGetValue(type, out IAnyComponentListeners ls))
                {
                    var listeners = ls as AnyComponentListeners<T>;
                    if (listeners.Changed != null)
                        AnyListenersBuffer.AddRange(listeners.Changed);
                }
            }
        
            if (hasRemovedComponents)
            {
                if (world.anyListeners.TryGetValue(type, out IAnyComponentListeners ls))
                {
                    var listeners = ls as AnyComponentListeners<T>;
                    if (listeners.Removed != null)
                        AnyListenersRemovedBuffer.AddRange(listeners.Removed);
                }
            }


            EcsPool<ListenersComponent> poolListeners = world.GetPool<ListenersComponent>();
        
            if (hasChangedComponents)
            {

                foreach (int entity in filterChanges)
                {
                    ListenersBuffer.Clear();

                    if (poolListeners.Has(entity))
                    {
                        List<IComponentChangedListener> lst = poolListeners.InternalGetRef(entity).Changed;
                        if (lst != null)
                            ListenersBuffer.AddRange(lst);
                    }

                    bool added = poolAdded.Has(entity);
                
                    T component = poolComponent.InternalGetRef(entity);
                    foreach (IComponentChangedListener listener in ListenersBuffer)
                    {
                        listener.OnComponentChanged(world, entity, component, added);
                    }

                    foreach (IAnyComponentChangedListener listener in AnyListenersBuffer)
                    {
                        listener.OnAnyComponentChanged(world, entity, component, added);
                    }


                    poolChanged.Del(entity);
                    poolAdded.Del(entity);
                }
            
                ListenersBuffer.Clear();
            }


            if (hasRemovedComponents)
            {
                foreach (int entity in filterRemoved)
                {
                    ListenersRemovedBuffer.Clear();

                    if (poolListeners.Has(entity))
                    {
                        List<IComponentRemovedListener> lst = poolListeners.InternalGetRef(entity).Removed;
                        if (lst != null)
                            ListenersRemovedBuffer.AddRange(lst);
                    }

                    foreach (IComponentRemovedListener listener in ListenersRemovedBuffer)
                    {
                        listener.OnComponentRemoved(world, entity, null);
                    }

                    foreach (IAnyComponentRemovedListener listener in AnyListenersRemovedBuffer)
                    {
                        listener.OnAnyComponentRemoved(world, entity, null);
                    }
                
                    poolRemoved.Del(entity);
                }
            
                ListenersRemovedBuffer.Clear();
            }
        }

        public void PreInit(EcsSystems systems)
        {
            systems.GetWorld().SetEventsEnabled<T>();
        }
    }
}