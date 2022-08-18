using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace XFlow.EcsLite {
    /*
    public struct DisableAutoDeleteComponent
    {
    
    }
    */
    
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public class EcsWorld
    {
        internal EntityData[] Entities;
        int _entitiesCount;
        int[] _recycledEntities;
        int _recycledEntitiesCount;
        IEcsPool[] _pools;
        int _poolsCount;
        readonly int _poolDenseSize;
        readonly int _poolRecycledSize;
        readonly Dictionary<Type, IEcsPool> _poolHashes;
        readonly Dictionary<int, EcsFilter> _hashedFilters;
        readonly List<EcsFilter> _allFilters;
        List<EcsFilter>[] _filtersByIncludedComponents;
        List<EcsFilter>[] _filtersByExcludedComponents;
        bool[] _enabledEvents;
        Mask[] _masks;
        int _masksCount;
        string _name;
        
        public readonly Dictionary<Type, IAnyComponentListeners> anyListeners = new Dictionary<Type, IAnyComponentListeners>(); 
        
        
        private short _defaultGen;

        bool _destroyed;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
        List<IEcsWorldEventListener> _eventListeners;
        
        
        public void AddEventListener (IEcsWorldEventListener listener) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (listener == null) { throw new Exception ("Listener is null."); }
#endif
            _eventListeners.Add (listener);
        }

        public void RemoveEventListener (IEcsWorldEventListener listener) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (listener == null) { throw new Exception ("Listener is null."); }
#endif
            _eventListeners.Remove (listener);
        }

        public void RaiseEntityChangeEvent (int entity) {
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnEntityChanged (entity);
            }
        }
#endif
        
        public AnyListener CreateAnyListener()
        {
            var listener = new AnyListener(this);
            return listener;
        }

        public AnyComponentListeners<T> GetAnyListeners<T>() where T:struct
        {
            AnyComponentListeners<T> instance;
            
            var key = typeof(T);
            if (anyListeners.TryGetValue(key, out IAnyComponentListeners lst))
                return lst as AnyComponentListeners<T>;
            
            instance = new AnyComponentListeners<T>();
            anyListeners.Add(key, instance);
            
            return instance;
        }

        /**
         * OnEntityDestroyed is fired before the entity  is deleted 
         */
        public List<IEcsEntityDestroyedListener> EntityDestroyedListeners { get; private set; } = new List<IEcsEntityDestroyedListener>();


        public string GetDebugName()
        {
            return _name;
        }

        public void SetDebugName(string name)
        {
            _name = name;
        }
        
        public void SetEventsEnabled<T>(bool enabled = true) where T:struct
        {
            _enabledEvents[GetPool<T>().GetId()] = enabled;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetUniqueRef<T>() where T : struct
        {
            return ref GetPool<T>().GetRef(0);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public T GetUnique<T> () where T : struct
        {
            return GetPool<T>().Get(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddUnique<T>() where T : struct
        {
            return ref GetPool<T>().Add(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddUnique<T>(T component) where T : struct
        {
            GetPool<T>().Add(0) = component;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReplaceUnique<T>(T component) where T : struct
        {
            GetPool<T>().Replace(0, component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetOrCreateUniqueRef<T>() where T : struct
        {
            return ref GetPool<T>().GetOrCreateRef(0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelUnique<T>() where T : struct
        {
            GetPool<T>().Del(0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasUnique<T>() where T : struct
        {
            return GetPool<T>().Has(0);
        }
        
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
        readonly List<int> _leakedEntities = new List<int> (512);

        internal bool CheckForLeakedEntities () {
            if (_leakedEntities.Count > 0) {
                for (int i = 0, iMax = _leakedEntities.Count; i < iMax; i++) {
                    ref var entityData = ref Entities[_leakedEntities[i]];
                    if (entityData.Gen > 0 && entityData.ComponentsCount == 0) {
                        return true;
                    }
                }
                _leakedEntities.Clear ();
            }
            return false;
        }
#endif

        public EcsWorld (in string name = "", in Config cfg = default)
        {
            _name = name;

            _defaultGen = cfg.DefaultGen > 0 ? cfg.DefaultGen : (short)1;
            // entities.
            var capacity = cfg.Entities > 0 ? cfg.Entities : Config.EntitiesDefault;
            Entities = new EntityData[capacity];
            capacity = cfg.RecycledEntities > 0 ? cfg.RecycledEntities : Config.RecycledEntitiesDefault;
            _recycledEntities = new int[capacity];
            _entitiesCount = 0;
            _recycledEntitiesCount = 0;
            // pools.
            capacity = cfg.Pools > 0 ? cfg.Pools : Config.PoolsDefault;
            _pools = new IEcsPool[capacity];
            _poolHashes = new Dictionary<Type, IEcsPool> (capacity);
            _filtersByIncludedComponents = new List<EcsFilter>[capacity];
            _filtersByExcludedComponents = new List<EcsFilter>[capacity];

            _poolDenseSize = cfg.PoolDenseSize > 0 ? cfg.PoolDenseSize : Config.PoolDenseSizeDefault;
            _poolRecycledSize = cfg.PoolRecycledSize > 0 ? cfg.PoolRecycledSize : Config.PoolRecycledSizeDefault;
            _enabledEvents = new bool[capacity];
            
            _poolsCount = 0;
            // filters.
            capacity = cfg.Filters > 0 ? cfg.Filters : Config.FiltersDefault;
            _hashedFilters = new Dictionary<int, EcsFilter> (capacity);
            _allFilters = new List<EcsFilter> (capacity);
            // masks.
            _masks = new Mask[64];
            _masksCount = 0;
            
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            _eventListeners = new List<IEcsWorldEventListener> (4);
#endif
            _destroyed = false;

            //create Zero pool
            //GetPool<DisableAutoDeleteComponent>();
            
            //reserved Unique
            //zero entity id reserved for UNIQUE components
            NewEntity();
        }

        public void SetDefaultGen(short v)
        {
            _defaultGen = v;
        }

        public override string ToString()
        {
            return $"EcsWorld({_name}, entities={GetAllEntitiesCount()}, pools={GetPoolsCount()})";
        }

        public void Destroy () {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (CheckForLeakedEntities ()) { throw new Exception ($"Empty entity detected before EcsWorld.Destroy()."); }
#endif
            _destroyed = true;
            for (var i = _entitiesCount - 1; i >= 0; i--) {
                ref var entityData = ref Entities[i];
                if (entityData.Gen > 0)
                    DelEntity (i);
                /*
                if (entityData.ComponentsCount > 0) {
                    DelEntity (i);
                }*/
            }
            _pools = Array.Empty<IEcsPool> ();
            _poolHashes.Clear ();
            _hashedFilters.Clear ();
            _allFilters.Clear ();
            _filtersByIncludedComponents = Array.Empty<List<EcsFilter>> ();
            _filtersByExcludedComponents = Array.Empty<List<EcsFilter>> ();
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (var ii = _eventListeners.Count - 1; ii >= 0; ii--) {
                _eventListeners[ii].OnWorldDestroyed (this);
            }
#endif
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsAlive () {
            return !_destroyed;
        }

        public int NewEntity () {
            int entity;
            if (_recycledEntitiesCount > 0) {
                entity = _recycledEntities[--_recycledEntitiesCount];
                ref var entityData = ref Entities[entity];
                entityData.Gen = (short) -entityData.Gen;
            } else {
                // new entity.
                if (_entitiesCount == Entities.Length) {
                    // resize entities and component pools.
                    var newSize = _entitiesCount << 1;
                    Array.Resize (ref Entities, newSize);
                    for (int i = 0, iMax = _poolsCount; i < iMax; i++) {
                        _pools[i].Resize (newSize);
                    }
                    for (int i = 0, iMax = _allFilters.Count; i < iMax; i++) {
                        _allFilters[i].ResizeSparseIndex (newSize);
                    }
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                    for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                        _eventListeners[ii].OnWorldResized (newSize);
                    }
#endif
                }
                entity = _entitiesCount++;
                Entities[entity].Gen = _defaultGen;
                
            }
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            _leakedEntities.Add (entity);
#endif
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnEntityCreated (entity);
            }
#endif

            //auto DelEntity workaround!!
            //GetPool<DisableAutoDeleteComponent>().Add(entity);
            //(_pools[0] as EcsPool<DisableAutoDeleteComponent>).Add(entity);
            
            return entity;
        }

        public void SortRecycledEntities()
        {
            Array.Sort(_recycledEntities, 0, _recycledEntitiesCount);
            Array.Reverse(_recycledEntities, 0, _recycledEntitiesCount);
        }

        public int InternalCalcComponentsCount(int entity)
        {
            var entityData = Entities[entity];

            int count = 0;
            for (int i = 0; i < _poolsCount; i++) 
            {
                if (_pools[i].Has (entity))
                {
                    count++;
                }
            }

            return count;
        }
        
        public void DelEntity (int entity) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (entity < 0 || entity >= _entitiesCount) { throw new Exception ("Cant touch destroyed entity."); }
#endif
            ref var entityData = ref Entities[entity];
            if (entityData.Gen < 0) {
                return;
            }
            
            for (int ii = 0, iMax = EntityDestroyedListeners.Count; ii < iMax; ii++) {
                EntityDestroyedListeners[ii].OnEntityWillBeDestroyed (this, entity);
            }
            
            
            // kill components.
            //if (entityData.ComponentsCount > 0) 
            {
                var idx = 0;
                while (entityData.ComponentsCount > 0 && idx < _poolsCount) {
                    for (; idx < _poolsCount; idx++) {
                        if (_pools[idx].Has (entity))
                        {
                            _pools[idx++].Del (entity);
                            break;
                        }
                    }
                }
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (entityData.ComponentsCount != 0)
                {
                    throw new Exception ($"Invalid components count on entity {entity} => {entityData.ComponentsCount}.");
                }
#endif
                //return;
            }
            
            entityData.Gen = (short) (entityData.Gen == short.MaxValue ? -1 : -(entityData.Gen + 1));
            if (_recycledEntitiesCount == _recycledEntities.Length) {
                Array.Resize (ref _recycledEntities, _recycledEntitiesCount << 1);
            }
            _recycledEntities[_recycledEntitiesCount++] = entity;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnEntityDestroyed (entity);
            }
#endif
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetComponentsCount (int entity) {
            return Entities[entity].ComponentsCount;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public short GetEntityGen (int entity) {
            return Entities[entity].Gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetAllocatedEntitiesCount () {
            return _entitiesCount;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetWorldSize () {
            return Entities.Length;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetPoolsCount () {
            return _poolsCount;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int GetEntitiesCount () {
            return _entitiesCount - _recycledEntitiesCount;
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EntityData[] GetRawEntities () {
            return Entities;
        }

        public EcsPool<T> GetPool<T> () where T : struct {
            var poolType = typeof (T);
            if (_poolHashes.TryGetValue (poolType, out var rawPool)) {
                return (EcsPool<T>) rawPool;
            }
            
            return AddPool<T>(_poolDenseSize);
        }
        
        public EcsPool<T> AddPool<T> ( int denseSize = -1, int recycledSize = -1) where T : struct {
            var poolType = typeof (T);
#if DEBUG
            if (_poolHashes.TryGetValue (poolType, out var rawPool))
            {
                throw new Exception("Pool already added");
            }
#endif

            if (denseSize < 0)
                denseSize = _poolDenseSize;
            if (recycledSize < 0)
                recycledSize = 512;
            
            var pool = new EcsPool<T> (this, _poolsCount, denseSize, Entities.Length, recycledSize);

            _poolHashes[poolType] = pool;
            if (_poolsCount == _pools.Length) {
                var newSize = _poolsCount << 1;
                Array.Resize (ref _pools, newSize);
                Array.Resize (ref _filtersByIncludedComponents, newSize);
                Array.Resize (ref _filtersByExcludedComponents, newSize);
                Array.Resize (ref _enabledEvents, newSize);
            }
            _pools[_poolsCount++] = pool;
            return pool;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public IEcsPool GetPoolById (int typeId) {
            return typeId >= 0 && typeId < _poolsCount ? _pools[typeId] : null;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public IEcsPool GetPoolByType (Type type) {
            return _poolHashes.TryGetValue (type, out var pool) ? pool : null;
        }
        
        
        public IEcsPool GetPoolByName (string name)
        {
            var keys = _poolHashes.Keys.ToArray();
            var key = Array.Find(keys, k => k.Name == name);
            if (key == null)
                return null;
            return GetPoolByType(key);
        }
        
        
        public IEcsPool GetOrCreatePoolByType (Type type) 
        {
            if (_poolHashes.TryGetValue (type, out var poolExisting))
                return poolExisting;
            
            MethodInfo method = typeof(EcsWorld).GetMethod(nameof(EcsWorld.AddPool));
            MethodInfo generic = method.MakeGenericMethod(type);
            var result = generic.Invoke(this, new object[]{-1, -1});

            return result as IEcsPool;
        }

        public int GetAllEntitiesCount()
        {
            return _entitiesCount - _recycledEntitiesCount;
        }
        
        public int GetAllEntities (ref int[] entities) {
            var count = _entitiesCount - _recycledEntitiesCount;
            if (entities == null || entities.Length < count) {
                entities = new int[count];
            }
            var id = 0;
            for (int i = 0, iMax = _entitiesCount; i < iMax; i++) {
                ref var entityData = ref Entities[i];
                // should we skip empty entities here?
                if (entityData.Gen > 0)// && entityData.ComponentsCount >= 0) 
                {
                    entities[id++] = i;
                }
            }
            return count;
        }

        public int GetAllPools (ref IEcsPool[] pools) {
            var count = _poolsCount;
            if (pools == null || pools.Length < count) {
                pools = new IEcsPool[count];
            }
            Array.Copy (_pools, 0, pools, 0, _poolsCount);
            return _poolsCount;
        }

        public int InternalGetAllPoolsRaw(out IEcsPool[] pools)
        {
            pools = _pools;
            return _poolsCount;
        }

        public void CopyFrom(EcsWorld srcWorld)
        {
            foreach (var filter in _allFilters)
            {
                filter.RemoveAllEntities();
            }

            for (int i = 0; i < _poolsCount; ++i)
            {
                _pools[i].InternalRemoveAllEntities();
            }


            void CopyArray<T>(T[] srcArray, ref T[] destArray, int srcCount) where T:struct
            {
                if (destArray.Length < srcCount)
                    destArray = new T[srcCount];
                Array.Copy(srcArray, destArray, srcCount);
            }

            CopyArray(srcWorld.Entities, ref Entities, srcWorld.Entities.Length);
            CopyArray(srcWorld._recycledEntities, ref _recycledEntities, srcWorld._recycledEntitiesCount);
            
            _entitiesCount = srcWorld._entitiesCount;
            _recycledEntitiesCount = srcWorld._recycledEntitiesCount;
            
            for (int i = 0; i < srcWorld._poolsCount; ++i)
            {
                var srcPool = srcWorld._pools[i];
                srcPool.CopyTo(this);
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void InternalChangeEntityGen(int entity, short gen)
        {
            Entities[entity].Gen = gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Mask Filter<T> () where T : struct {
            var mask = _masksCount > 0 ? _masks[--_masksCount] : new Mask (this);
            return mask.Inc<T> ();
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Mask FilterAdded<T> () where T : struct {
            var mask = _masksCount > 0 ? _masks[--_masksCount] : new Mask (this);
            return mask.IncAdded<T> ();
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Mask FilterChanged<T> () where T : struct {
            var mask = _masksCount > 0 ? _masks[--_masksCount] : new Mask (this);
            return mask.IncChanges<T> ();
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Mask FilterRemoved<T> () where T : struct {
            var mask = _masksCount > 0 ? _masks[--_masksCount] : new Mask (this);
            return mask.IncRemoved<T> ();
        }

        public int GetComponents (int entity, ref object[] list) {
            var itemsCount = Entities[entity].ComponentsCount;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new object[_pools.Length];
            }
            for (int i = 0, j = 0, iMax = _poolsCount; i < iMax; i++) {
                if (_pools[i].Has (entity)) {
                    list[j++] = _pools[i].GetRaw (entity);
                }
            }
            return itemsCount;
        }
        
        public int GetComponentsRead (int entity, ref object[] list) {
            var itemsCount = Entities[entity].ComponentsCount;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new object[_pools.Length];
            }
            for (int i = 0, j = 0, iMax = _poolsCount; i < iMax; i++) {
                if (_pools[i].Has (entity)) {
                    list[j++] = _pools[i].GetReadRaw (entity);
                }
            }
            return itemsCount;
        }
        
        public int GetComponents (int entity, ref Tuple<int, object>[] list) {
            var itemsCount = Entities[entity].ComponentsCount;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new Tuple<int, object>[itemsCount];
            }
            for (int i = 0, j = 0, iMax = _poolsCount; i < iMax; i++) {
                if (_pools[i].Has (entity)) {
                    list[j++] = new Tuple<int, object>(i, _pools[i].GetRaw (entity));
                }
            }
            return itemsCount;
        }

        public int GetComponentTypes (int entity, ref Type[] list) {
            var itemsCount = Entities[entity].ComponentsCount;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new Type[_pools.Length];
            }
            for (int i = 0, j = 0, iMax = _poolsCount; i < iMax; i++) {
                if (_pools[i].Has (entity)) {
                    list[j++] = _pools[i].GetComponentType ();
                }
            }
            return itemsCount;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsEntityAliveInternal (int entity) {
            return entity >= 0 && entity < _entitiesCount && Entities[entity].Gen > 0;
        }

        (EcsFilter, bool) GetFilterInternal (Mask mask, int capacity = 512) {
            var hash = mask.Hash;
            var exists = _hashedFilters.TryGetValue (hash, out var filter);
            if (exists) { return (filter, false); }
            filter = new EcsFilter (this, mask, capacity, Entities.Length);
            _hashedFilters[hash] = filter;
            _allFilters.Add (filter);
            // add to component dictionaries for fast compatibility scan.
            for (int i = 0, iMax = mask.IncludeCount; i < iMax; i++) {
                var list = _filtersByIncludedComponents[mask.Include[i]];
                if (list == null) {
                    list = new List<EcsFilter> (8);
                    _filtersByIncludedComponents[mask.Include[i]] = list;
                }
                list.Add (filter);
            }
            for (int i = 0, iMax = mask.ExcludeCount; i < iMax; i++) {
                var list = _filtersByExcludedComponents[mask.Exclude[i]];
                if (list == null) {
                    list = new List<EcsFilter> (8);
                    _filtersByExcludedComponents[mask.Exclude[i]] = list;
                }
                list.Add (filter);
            }
            // scan exist entities for compatibility with new filter.
            for (int i = 0, iMax = _entitiesCount; i < iMax; i++) {
                ref var entityData = ref Entities[i];
                if (entityData.ComponentsCount > 0 && IsMaskCompatible (mask, i)) {
                    filter.AddEntity (i);
                }
            }
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnFilterCreated (filter);
            }
#endif
            return (filter, true);
        }
        
        internal void OnEntityChange2<TChanged, TAdded, TRemoved>(int entity, int componentType, bool added, bool removed) 
            where TChanged:struct where TAdded:struct where TRemoved:struct
        {
            if (!_enabledEvents[componentType])
                return;

            if (added)
            {
                var poolRemoved = GetPool<TRemoved>();
                if (poolRemoved.Has(entity))
                {
                    poolRemoved.Del(entity);
                    // without AddedComponent! 
                }
                else
                {
                    //new component here
                    GetPool<TAdded>().Add(entity);    
                }

                GetPool<TChanged>().Add(entity);
            }
            else
            {
                if (removed)
                {
                    var poolAdded = GetPool<TAdded>();
                    if (poolAdded.Has(entity))
                    {
                        GetPool<TAdded>().Del(entity);
                        // without RemovedComponent
                    }
                    else
                    {
                        GetPool<TRemoved>().Add(entity);
                    }
                    GetPool<TChanged>().Del(entity);
                }
                else
                {
                    GetPool<TChanged>().GetOrCreateRef(entity);
                }
            }
        }

        public void OnEntityChangeInternal (int entity, int componentType, bool added) 
        {
            var includeList = _filtersByIncludedComponents[componentType];
            var excludeList = _filtersByExcludedComponents[componentType];
            if (added) {
                // add component.
                if (includeList != null) {
                    foreach (var filter in includeList) {
                        if (IsMaskCompatible (filter.GetMask (), entity)) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                            if (filter.SparseEntities[entity] > 0) { throw new Exception ("Entity already in filter."); }
#endif
                            filter.AddEntity (entity);
                        }
                    }
                }
                if (excludeList != null) {
                    foreach (var filter in excludeList) {
                        if (IsMaskCompatibleWithout (filter.GetMask (), entity, componentType)) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                            if (filter.SparseEntities[entity] == 0) { throw new Exception ("Entity not in filter."); }
#endif
                            filter.RemoveEntity (entity);
                        }
                    }
                }
            } else {
                // remove component.
                if (includeList != null) {
                    foreach (var filter in includeList) {
                        if (IsMaskCompatible (filter.GetMask (), entity)) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                            if (filter.SparseEntities[entity] == 0) { throw new Exception ("Entity not in filter."); }
#endif
                            filter.RemoveEntity (entity);
                        }
                    }
                }
                if (excludeList != null) {
                    foreach (var filter in excludeList) {
                        if (IsMaskCompatibleWithout (filter.GetMask (), entity, componentType)) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                            if (filter.SparseEntities[entity] > 0) { throw new Exception ("Entity already in filter."); }
#endif
                            filter.AddEntity (entity);
                        }
                    }
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        bool IsMaskCompatible (Mask filterMask, int entity) {
            for (int i = 0, iMax = filterMask.IncludeCount; i < iMax; i++) {
                if (!_pools[filterMask.Include[i]].Has (entity)) {
                    return false;
                }
            }
            for (int i = 0, iMax = filterMask.ExcludeCount; i < iMax; i++) {
                if (_pools[filterMask.Exclude[i]].Has (entity)) {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        bool IsMaskCompatibleWithout (Mask filterMask, int entity, int componentId) {
            for (int i = 0, iMax = filterMask.IncludeCount; i < iMax; i++) {
                var typeId = filterMask.Include[i];
                if (typeId == componentId || !_pools[typeId].Has (entity)) {
                    return false;
                }
            }
            for (int i = 0, iMax = filterMask.ExcludeCount; i < iMax; i++) {
                var typeId = filterMask.Exclude[i];
                if (typeId != componentId && _pools[typeId].Has (entity)) {
                    return false;
                }
            }
            return true;
        }

        public struct Config {
            public int Entities;
            public int RecycledEntities;
            public int Pools;
            public int Filters;
            public int PoolDenseSize;
            public int PoolRecycledSize;
            public short DefaultGen;

            internal const int EntitiesDefault = 512;
            internal const int RecycledEntitiesDefault = 512;
            internal const int PoolsDefault = 512;
            internal const int FiltersDefault = 512;
            internal const int PoolDenseSizeDefault = 512;
            internal const int PoolRecycledSizeDefault = 512;
            internal const short DefaultGenDefault = 1;
        }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
        public sealed class Mask {
            readonly EcsWorld _world;
            internal int[] Include;
            internal int[] Exclude;
            internal int IncludeCount;
            internal int ExcludeCount;
            internal int Hash;
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            bool _built;
#endif

            internal Mask (EcsWorld world) {
                _world = world;
                Include = new int[8];
                Exclude = new int[2];
                Reset ();
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            void Reset () {
                IncludeCount = 0;
                ExcludeCount = 0;
                Hash = 0;
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                _built = false;
#endif
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public Mask Inc<T> () where T : struct {
                var poolId = _world.GetPool<T> ().GetId ();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (_built) { throw new Exception ("Cant change built mask."); }
                if (Array.IndexOf (Include, poolId, 0, IncludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
                if (Array.IndexOf (Exclude, poolId, 0, ExcludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
#endif
                if (IncludeCount == Include.Length) { Array.Resize (ref Include, IncludeCount << 1); }
                Include[IncludeCount++] = poolId;
                return this;
            }
            
            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public Mask IncChanges<T> () where T : struct
            {
                Inc<EcsPool<T>.ChangedComponent>();
                return this;
            }
            
            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public Mask IncAdded<T> () where T : struct
            {
                Inc<EcsPool<T>.AddedComponent>();
                return this;
            }
            
            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public Mask IncRemoved<T> () where T : struct
            {
                Inc<EcsPool<T>.RemovedComponent>();
                return this;
            }

#if UNITY_2020_3_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public Mask Exc<T> () where T : struct {
                var poolId = _world.GetPool<T> ().GetId ();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (_built) { throw new Exception ("Cant change built mask."); }
                if (Array.IndexOf (Include, poolId, 0, IncludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
                if (Array.IndexOf (Exclude, poolId, 0, ExcludeCount) != -1) { throw new Exception ($"{typeof (T).Name} already in constraints list."); }
#endif
                if (ExcludeCount == Exclude.Length) { Array.Resize (ref Exclude, ExcludeCount << 1); }
                Exclude[ExcludeCount++] = poolId;
                return this;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public EcsFilter End (int capacity = 512) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                if (_built) { throw new Exception ("Cant change built mask."); }
                _built = true;
#endif
                Array.Sort (Include, 0, IncludeCount);
                Array.Sort (Exclude, 0, ExcludeCount);
                // calculate hash.
                Hash = IncludeCount + ExcludeCount;
                for (int i = 0, iMax = IncludeCount; i < iMax; i++) {
                    Hash = unchecked (Hash * 314159 + Include[i]);
                }
                for (int i = 0, iMax = ExcludeCount; i < iMax; i++) {
                    Hash = unchecked (Hash * 314159 - Exclude[i]);
                }
                var (filter, isNew) = _world.GetFilterInternal (this, capacity);
                if (!isNew) { Recycle (); }
                return filter;
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            void Recycle () {
                Reset ();
                if (_world._masksCount == _world._masks.Length) {
                    Array.Resize (ref _world._masks, _world._masksCount << 1);
                }
                _world._masks[_world._masksCount++] = this;
            }
        }

        public struct EntityData {
            public short Gen;
            public short ComponentsCount;
        }

        public int GetAllocMemorySizeInBytes()
        {
            var memorySize = 0;
            var entityDataMemorySize = Utils.Utils.GetArrayMemorySize(Entities);
            var recycledEntitiesMemorySize = Utils.Utils.GetArrayMemorySize(_recycledEntities);
            var enabledEventsMemorySize = Utils.Utils.GetArrayMemorySize(_enabledEvents);
            
            memorySize += entityDataMemorySize + recycledEntitiesMemorySize + enabledEventsMemorySize;
 
            foreach (var pool in _pools)
            {
                if (pool != null) 
                {
                    memorySize += pool.GetAllocMemorySizeInBytes();
                }
            }

            foreach (var filter in _allFilters)
            {
                memorySize += filter.GetAllocMemorySizeInBytes();
            }

            return memorySize;
        }
    }

#if DEBUG || LEOECSLITE_WORLD_EVENTS
    public interface IEcsWorldEventListener {
        void OnEntityCreated (int entity);
        void OnEntityChanged (int entity);
        void OnEntityDestroyed (int entity);
        void OnFilterCreated (EcsFilter filter);
        void OnWorldResized (int newSize);
        void OnWorldDestroyed (EcsWorld world);
    }
#endif
    
    public interface IEcsEntityDestroyedListener 
    {
        void OnEntityWillBeDestroyed (EcsWorld world, int entity);
    }
}