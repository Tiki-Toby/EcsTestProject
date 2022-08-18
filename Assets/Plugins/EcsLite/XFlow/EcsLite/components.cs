using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif


namespace XFlow.EcsLite {

    [AttributeUsage(AttributeTargets.Struct)]
    public class EmptyComponent : Attribute
    {
    }
    
    public interface IEcsPool {
        void Resize (int capacity);
        bool Has (int entity);
        void Del (int entity);
        void AddRaw (int entity, object dataRaw);
        object GetRaw (int entity);
        void SetRaw (int entity, object dataRaw);
        int GetId ();
        Type GetComponentType ();

        List<int> GetEntities(List<int> result = null);
        int[] GetRawSparseItems();

        bool InternalIsEmptyComponent();
        void InternalRemoveAllEntities();
        object GetReadRaw(int entity);

        int GetAllocMemorySizeInBytes();


        void CopyTo(EcsWorld destWorld);
    }

    public interface IEcsAutoReset<T> where T : struct {
        void AutoReset (ref T c);
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class EcsPool<T> : IEcsPool where T : struct {
        readonly Type _type;
        readonly EcsWorld _world;
        readonly int _id;
        readonly AutoResetHandler _autoReset;
        
        // 1-based index.
        int[] _sparseItems;
        T[] _denseItems;
        int _denseItemsCount;
        
        int[] _recycledItems;
        int _recycledItemsCount;

        private bool _emptyComponent; 

        [EmptyComponent]
        public struct ChangedComponent
        {
        }
        
        [EmptyComponent]
        public struct AddedComponent
        {
        }
        
        [EmptyComponent]
        public struct RemovedComponent
        {
        }

#if ENABLE_IL2CPP && !UNITY_EDITOR
        T _autoresetFakeInstance;
#endif

        public bool InternalIsEmptyComponent()
        {
            return _emptyComponent;
        }

        public void InternalRemoveAllEntities()
        {
            _denseItemsCount = 1;
            _recycledItemsCount = 0;
            Array.Clear(_sparseItems, 0, _sparseItems.Length);
        }
        
        internal EcsPool (EcsWorld world, int id, int denseCapacity, int sparseCapacity, int recycledCapacity) {
            _type = typeof (T);
            _world = world;
            _id = id;

            if (Utils.Utils.CheckIsEmptyComponent<T>())
            {
                _emptyComponent = true;
                denseCapacity = 1;
                recycledCapacity = 0;
            }

            
            _denseItems = new T[denseCapacity + 1];
            _sparseItems = new int[sparseCapacity];
            _denseItemsCount = 1;
            _recycledItems = new int[recycledCapacity];
            _recycledItemsCount = 0;

            var isAutoReset = typeof (IEcsAutoReset<T>).IsAssignableFrom (_type);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!isAutoReset && _type.GetInterface ("IEcsAutoReset`1") != null) {
                throw new Exception ($"IEcsAutoReset should have <{typeof (T).Name}> constraint for component \"{typeof (T).Name}\".");
            }
#endif
            if (isAutoReset) {
                var autoResetMethod = typeof (T).GetMethod (nameof (IEcsAutoReset<T>.AutoReset));
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (autoResetMethod == null) {
                    throw new Exception (
                        $"IEcsAutoReset<{typeof (T).Name}> explicit implementation not supported, use implicit instead.");
                }
#endif
                _autoReset = (AutoResetHandler) Delegate.CreateDelegate (
                    typeof (AutoResetHandler),
#if ENABLE_IL2CPP && !UNITY_EDITOR
                    _autoresetFakeInstance,
#else
                    null,
#endif
                    autoResetMethod);
            }
        }

#if UNITY_2020_3_OR_NEWER
        [UnityEngine.Scripting.Preserve]
#endif
        void ReflectionSupportHack () {
            _world.GetPool<T> ();
            _world.Filter<T> ().Exc<T> ().End ();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld () {
            return _world;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetId () {
            return _id;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Type GetComponentType () {
            return _type;
        }

        /*
         * Returns all entities with Component
         */
        public List<int> GetEntities(List<int> list = null)
        {
            if (list == null)
                list = new List<int>();
            else
                list.Clear();

            int total = _world.GetAllocatedEntitiesCount();
            for (int entity = 0; entity < total; ++entity)
            {
                if (!_world.IsEntityAliveInternal(entity))
                    continue;
                if (_sparseItems[entity] > 0)
                    list.Add(entity);
            }

            return list;
        }
        

        void IEcsPool.Resize (int capacity) {
            Array.Resize (ref _sparseItems, capacity);
        }

        object IEcsPool.GetRaw (int entity) {
            return GetRef (entity);
        }
        
        object IEcsPool.GetReadRaw (int entity) {
            return Get (entity);
        }

        void IEcsPool.SetRaw (int entity, object dataRaw) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (dataRaw == null || dataRaw.GetType () != _type) { throw new Exception ("Invalid component data, valid \"{typeof (T).Name}\" instance required."); }

            if (_sparseItems[entity] <= 0)
            {
                throw new Exception ($"Cant get \"{typeof (T).Name}\" component from #{entity}- not attached.");
            }
#endif
            _denseItems[_sparseItems[entity]] = (T) dataRaw;
            
            _world.OnEntityChange2<ChangedComponent, AddedComponent, RemovedComponent>(entity, _id, true, false);
        }

        void IEcsPool.AddRaw (int entity, object dataRaw) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (dataRaw == null || dataRaw.GetType() != _type)
            {
                throw new Exception ($"Invalid component data, valid '{typeof (T).Name}' instance required.");
            }
#endif
            ref var data = ref Add (entity);
            data = (T) dataRaw;
        }

        public T[] GetRawDenseItems () {
            return _denseItems;
        }

        public ref int GetRawDenseItemsCount () {
            return ref _denseItemsCount;
        }

        public int[] GetRawSparseItems () {
            return _sparseItems;
        }

        public int[] GetRawRecycledItems () {
            return _recycledItems;
        }

        public ref int GetRawRecycledItemsCount () {
            return ref _recycledItemsCount;
        }

        public ref T Add (int entity)
        {
            _world.Entities[entity].ComponentsCount++;//before InternalAdd, some listeners may use it
            
            ref var res = ref InternalAdd(entity);
            _world.OnEntityChange2<ChangedComponent, AddedComponent, RemovedComponent>(entity, _id, true, false);
            return ref res;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T InternalAdd(int entity)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }

            if (_sparseItems[entity] > 0)
            {
                throw new Exception ($"Component \"{typeof (T).Name}\" already attached to entity.");
            }
#endif
            int idx;
            if (_emptyComponent)
            {
                //all empty structs are identical and uses first element from _denseItems
                idx = 1;
                _denseItemsCount = 2;
            }
            else
            {
                if (_recycledItemsCount > 0)
                {
                    idx = _recycledItems[--_recycledItemsCount];
                }
                else
                {
                    idx = _denseItemsCount;
                    if (_denseItemsCount == _denseItems.Length) 
                    {
                        Array.Resize(ref _denseItems, _denseItemsCount << 1);
                    }

                    _denseItemsCount++;
                }
            }

            ref var componentRef = ref _denseItems[idx];
            if (_autoReset != null)
                _autoReset.Invoke(ref componentRef);
            else
                componentRef = default;
            

            _sparseItems[entity] = idx;
            _world.OnEntityChangeInternal (entity, _id, true);
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            _world.RaiseEntityChangeEvent (entity);
#endif

            return ref componentRef;
        }
        
        public void Replace(int entity, T data)
        {
            if (Has(entity))
                Del(entity);
            Add(entity) = data;
        }
        
        public ref T GetOrCreateRef(int entity)
        {
            if (Has(entity))
                return ref GetRef(entity);

            return ref Add(entity);
        }

        /*useful with .IncChanges */
        public void ReplaceIfChanged(int entity, T data)
        {
            if (Has(entity))
            {
                ref var current = ref InternalGetRef(entity);
                if (current.Equals(data))
                    return;

                GetRef(entity) = data;
                
                return;
            }

            Add(entity) = data;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T GetRef (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }

            if (_sparseItems[entity] == 0)
            {
                throw new Exception ($"Cant get \"{typeof (T).Name}\" component from #{entity}- not attached.");
            }
#endif
            _world.OnEntityChange2<ChangedComponent, AddedComponent, RemovedComponent>(entity, _id, false, false);
            
            return ref _denseItems[_sparseItems[entity]];
        }

        // Debug helper
        private static bool HasComponentsWithTypes(int entity, EcsWorld world, string[] typeNames)
        {
            var components = new object[1];
            world.GetComponentsRead(entity, ref components);

            int foundComponentsCount = 0;
            
            foreach (var component in components)
            {
                if (component != null && typeNames.Contains(component.GetType().Name))
                    foundComponentsCount++;
            }

            return typeNames.Length == foundComponentsCount;
        }

        /*
         * should not be used 
         */
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ref T InternalGetRef (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal(entity))
            {
                throw new Exception ($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }

            if (_sparseItems[entity] == 0)
            {
                throw new Exception ($"Not attached #{entity}. {typeof(T)}");
            }
#endif
            
            return ref _denseItems[_sparseItems[entity]];
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public T Get (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal (entity))
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }

            if (_sparseItems[entity] == 0)
            {
                throw new Exception ($"Not attached #{entity}. {typeof(T)}");
            }
#endif
            return _denseItems[_sparseItems[entity]];
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public T? GetNullable (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal (entity))
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }
#endif
            if (_sparseItems[entity] == 0)
                return null;
            
            return _denseItems[_sparseItems[entity]];
        }

        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal (entity)) 
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }
#endif
            return _sparseItems[entity] > 0;
        }
        
        
        public bool TryGet(int entity, out T component)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal (entity)) 
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }
#endif
            int lookup = _sparseItems[entity];
            if (lookup == 0)
            {
                component = default;
                return false;
            }

            component = _denseItems[_sparseItems[entity]];
            return true;
        }

        public void Del (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (!_world.IsEntityAliveInternal (entity))
            {
                throw new Exception($"Cant touch destroyed entity #{entity}. {typeof(T)}");
            }
#endif
            ref var sparseData = ref _sparseItems[entity];
            if (sparseData > 0) {

                _world.OnEntityChangeInternal (entity, _id, false);
                
                if (!_emptyComponent)
                {
                    if (_recycledItemsCount == _recycledItems.Length) {
                        Array.Resize (ref _recycledItems, _recycledItemsCount << 1);
                    }
                    _recycledItems[_recycledItemsCount++] = sparseData;
                    /*
                    if (_autoReset != null) {
                        _autoReset.Invoke (ref _denseItems[sparseData]);
                    } else {
                        
                    }*/
                    
                    _denseItems[sparseData] = default;
                }
                
                sparseData = 0;
                ref var entityData = ref _world.Entities[entity];
                entityData.ComponentsCount--;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                _world.RaiseEntityChangeEvent (entity);
#endif
                _world.OnEntityChange2<ChangedComponent, AddedComponent, RemovedComponent>(entity, _id, false, true);
                
                /*
                if (entityData.ComponentsCount == 0) {
                    _world.DelEntity (entity);
                }*/
            }
        }

        delegate void AutoResetHandler (ref T component);

        public int GetAllocMemorySizeInBytes()
        {
            var memorySize = 0;
            var denseItemsMemorySize = Utils.Utils.GetArrayMemorySize(_denseItems);
            var sparseItemsMemorySize = Utils.Utils.GetArrayMemorySize(_sparseItems);
            var recycledItemsMemorySize = Utils.Utils.GetArrayMemorySize(_recycledItems);
            memorySize += denseItemsMemorySize + sparseItemsMemorySize + recycledItemsMemorySize;
            
            return memorySize;
        }


        public void CopyTo(EcsWorld destWorld)
        {
            var destPool = destWorld.GetPool<T>();
            int entities = _world.GetAllocatedEntitiesCount();
            for (int entity = 0; entity < entities; ++entity)
            {
                // HAS = return _sparseItems[entity] > 0;

                var lookup = _sparseItems[entity];
                if (lookup <= 0)
                    continue;
                
                
                destPool.InternalAdd(entity) = _denseItems[lookup];
                //destWorld.GetPool<ChangedComponent>().Remo
            }
        }
    }
}