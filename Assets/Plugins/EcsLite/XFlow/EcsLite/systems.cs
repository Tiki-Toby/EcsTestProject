using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace XFlow.EcsLite {
    public interface IEcsSystem { }

    public interface IEcsPreInitSystem : IEcsSystem {
        void PreInit (EcsSystems systems);
    }

    public interface IEcsInitSystem : IEcsSystem {
        void Init (EcsSystems systems);
    }

    public interface IEcsRunSystem : IEcsSystem {
        void Run (EcsSystems systems);
    }

    public interface IEcsDestroySystem : IEcsSystem {
        void Destroy (EcsSystems systems);
    }

    public interface IEcsPostDestroySystem : IEcsSystem {
        void PostDestroy (EcsSystems systems);
    }

    public interface IEcsWorldChangedSystem : IEcsSystem
    {
        void WorldChanged(EcsWorld world);
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public class EcsSystems {
        EcsWorld _defaultWorld;
        readonly Dictionary<string, EcsWorld> _worlds;
        readonly List<IEcsSystem> _allSystems;
        
        readonly object _shared;
        readonly Dictionary<Type, object> _sharedDict = new Dictionary<Type, object>();
        
        IEcsRunSystem[] _runSystems;
        int _runSystemsCount;
        private bool _initialized;

        public EcsSystems (EcsWorld defaultWorld, object shared = null) {
            _defaultWorld = defaultWorld;
            _shared = shared;
            _worlds = new Dictionary<string, EcsWorld> (32);
            _allSystems = new List<IEcsSystem> (128);
        }

        public Dictionary<string, EcsWorld> GetAllNamedWorlds () {
            return _worlds;
        }

        public int GetAllSystems (ref IEcsSystem[] list) {
            var itemsCount = _allSystems.Count;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new IEcsSystem[_allSystems.Capacity];
            }
            for (int i = 0, iMax = itemsCount; i < iMax; i++) {
                list[i] = _allSystems[i];
            }
            return itemsCount;
        }

        public int GetRunSystems (ref IEcsRunSystem[] list) {
            var itemsCount = _runSystemsCount;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new IEcsRunSystem[_runSystems.Length];
            }
            for (int i = 0, iMax = itemsCount; i < iMax; i++) {
                list[i] = _runSystems[i];
            }
            return itemsCount;
        }

        public T GetSystem<T>()
        {
            foreach (var system in _allSystems)
            {
                if (system is T instance)
                    return instance;
            }

            return default;
        }

        /*
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public T _GetSharedByKey<T> () where T : class
        {
            return _sharedDict[typeof(T)] as T;
        }
        
        public void SetSharedByKey<T> (T data) where T : class
        {
            _sharedDict[typeof(T)] = data;
        }*/
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public T GetShared<T> () where T : class {
            return _shared as T;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld (string name = null) {
            if (name == null) {
                return _defaultWorld;
            }
            _worlds.TryGetValue (name, out var world);
            return world;
        }

        public void Destroy () {
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                if (_allSystems[i] is IEcsDestroySystem destroySystem) {
                    destroySystem.Destroy (this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities ();
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {destroySystem.GetType ().Name}.Destroy()."); }
#endif
                }
            }
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                if (_allSystems[i] is IEcsPostDestroySystem postDestroySystem) {
                    postDestroySystem.PostDestroy (this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities ();
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {postDestroySystem.GetType ().Name}.PostDestroy()."); }
#endif
                }
            }
            _allSystems.Clear ();
            _runSystems = null;
        }

        public EcsSystems AddWorld (EcsWorld world, string name) {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (string.IsNullOrEmpty (name)) { throw new System.Exception ("World name cant be null or empty."); }
#endif
            if (world == null)
                throw new Exception($"AddWorld {name} is null");
            _worlds[name] = world;
            return this;
        }

        public EcsSystems Add (IEcsSystem system) {
            if (_initialized)
                throw new Exception("Systems already initialized");
            
            _allSystems.Add (system);
            if (system is IEcsRunSystem) {
                _runSystemsCount++;
            }
            return this;
        }
        
        public EcsSystems Add (IEcsSystem[] list) {
            foreach (IEcsSystem ecsSystem in list)
            {
                Add(ecsSystem);
            }
            
            return this;
        }

        public void Init ()
        {
            if (_initialized)
                throw new Exception("Systems already initialized");
            
            _initialized = true;
            Profiler.BeginSample("EcsSystems.Init");
            if (_runSystemsCount > 0) {
                _runSystems = new IEcsRunSystem[_runSystemsCount];
            }
            
            Profiler.BeginSample("EcsSystems.PreInit");
            
            foreach (var system in _allSystems) {
                if (system is IEcsPreInitSystem initSystem) {
                    Profiler.BeginSample($"EcsSystem.PreInit.{system.GetType().Name}");
                    initSystem.PreInit (this);
                    Profiler.EndSample();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities ();
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType ().Name}.PreInit()."); }
#endif
                }
            }
            
            Profiler.EndSample();
            
            Profiler.BeginSample("EcsSystems.Init1");
            var runIdx = 0;
            foreach (var system in _allSystems) {
                if (system is IEcsInitSystem initSystem) {
                    Profiler.BeginSample($"EcsSystem.Init.{system.GetType().Name}");
                    initSystem.Init (this);
                    Profiler.EndSample();
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities ();
                    if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType ().Name}.Init()."); }
#endif
                }
                if (system is IEcsRunSystem runSystem) {
                    _runSystems[runIdx++] = runSystem;
                }
            }
            Profiler.EndSample();
            
            if (_defaultWorld != null)
                DefaultWorldChanged();
            
            Profiler.EndSample();
        }

        private void DefaultWorldChanged()
        {
            Profiler.BeginSample("EcsSystems.DefaultWorldChanged");
            foreach (var system in _allSystems)
            {
                if (system is IEcsWorldChangedSystem worldChangedSystem)
                {
                    worldChangedSystem.WorldChanged(_defaultWorld);
                }
            }
            Profiler.EndSample();
        }
        
        public void ChangeDefaultWorld(EcsWorld world)
        {
            if (world == _defaultWorld)
                return;
            
            _defaultWorld = world;
            DefaultWorldChanged();
        }

        public void Run ()
        {
            Profiler.BeginSample("EcsSystems.Run");
            for (int i = 0, iMax = _runSystemsCount; i < iMax; i++) {
                var system = _runSystems[i];
                Profiler.BeginSample($"EcsSystem.Run.{system.GetType().Name}");
                system.Run (this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                Profiler.BeginSample("zzz");
                var worldName = CheckForLeakedEntities ();
                if (worldName != null) { throw new System.Exception ($"Empty entity detected in world \"{worldName}\" after {system.GetType ().Name}.Run()."); }
                Profiler.EndSample();
#endif
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
        public string CheckForLeakedEntities () {
            /*
            if (_defaultWorld != null && _defaultWorld.CheckForLeakedEntities ()) { return "default"; }
            foreach (var pair in _worlds) {
                if (pair.Value.CheckForLeakedEntities ()) {
                    return pair.Key;
                }
            }*/
            return null;
        }
#endif
    }
}