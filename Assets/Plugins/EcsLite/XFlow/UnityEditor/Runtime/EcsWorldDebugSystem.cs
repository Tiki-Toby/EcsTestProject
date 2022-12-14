using System;
using System.Collections.Generic;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using Object = UnityEngine.Object;

namespace Flow.EcsLite.UnityEditor {
    public sealed class EcsWorldDebugSystem : IEcsPreInitSystem, IEcsRunSystem, IEcsWorldEventListener {
        readonly string _worldName;
        readonly GameObject _rootGO;
        readonly Transform _entitiesRoot;
        readonly bool _bakeComponentsInName;
        readonly string _entityNameFormat;
        EcsWorld _world;
        EcsEntityDebugView[] _entities;
        Dictionary<int, byte> _dirtyEntities;
        Type[] _typesCache;
        
        public delegate bool HasSpecialSubName(EcsWorld world, int entity, out string specialSubName);
        public HasSpecialSubName HasSpecialSubNameFunc;

        public EcsWorldDebugSystem (string worldName = null, bool bakeComponentsInName = true, string entityNameFormat = "X8") {
            _bakeComponentsInName = bakeComponentsInName;
            _worldName = worldName;
            _entityNameFormat = entityNameFormat;
            _rootGO = new GameObject (_worldName != null ? $"[ECS-WORLD {_worldName}]" : "[ECS-WORLD]");
            Object.DontDestroyOnLoad (_rootGO);
            _rootGO.hideFlags = HideFlags.NotEditable;
            _entitiesRoot = new GameObject ("Entities").transform;
            _entitiesRoot.gameObject.hideFlags = HideFlags.NotEditable;
            _entitiesRoot.SetParent (_rootGO.transform, false);
        }

        public void PreInit (EcsSystems systems) {
            _world = systems.GetWorld (_worldName);
            if (_world == null) { throw new Exception ("Cant find required world."); }
            _entities = new EcsEntityDebugView [_world.GetWorldSize ()];
            _dirtyEntities = new Dictionary<int, byte> (_entities.Length);
            _world.AddEventListener (this);
            var entities = Array.Empty<int> ();
            var entitiesCount = _world.GetAllEntities (ref entities);
            for (var i = 0; i < entitiesCount; i++) {
                OnEntityCreated (entities[i]);
            }
        }

        public void Run (EcsSystems systems) {
            var poolOwner = _world.GetPool<OwnerComponent>();
            
            foreach (var pair in _dirtyEntities) {
                var entity = pair.Key;
                var entityName = entity.ToString (_entityNameFormat);
                if (_world.GetEntityGen (entity) > 0) {
                    var count = _world.GetComponentTypes (entity, ref _typesCache);
                    
                    if (HasSpecialSubNameFunc != null &&
                        HasSpecialSubNameFunc(_world, entity, out string specialSubName))
                    {
                        entityName = $"[{specialSubName}] : {entityName}";
                    }

                    for (var i = 0; i < count; i++) {
                        entityName = $"{entityName}:{EditorExtensions.GetCleanGenericTypeName (_typesCache[i])}";
                    }
                    
                    var entityTransform = _entities[entity].transform;
                    if (poolOwner.Has(entity))
                    {
                        var ownerEntity = poolOwner.Get(entity).entity;
                        var ownerDebugViewTransform = _entities[ownerEntity].transform;
                        
                        if (entityTransform.parent != ownerDebugViewTransform)
                        {
                            entityTransform.SetParent(ownerDebugViewTransform);
                        }
                    }
                    else
                    {
                        if (entityTransform.parent != _entitiesRoot)
                        {
                            entityTransform.SetParent(_entitiesRoot);
                        }
                    }
                
                    _entities[entity].name = entityName;
                }
            }
            _dirtyEntities.Clear ();
        }

        public void OnEntityCreated (int entity) {
            if (!_entities[entity]) {
                var go = new GameObject ();
                go.transform.SetParent (_entitiesRoot, false);
                var entityObserver = go.AddComponent<EcsEntityDebugView> ();
                entityObserver.Entity = entity;
                entityObserver.World = _world;
                entityObserver.DebugSystem = this;
                _entities[entity] = entityObserver;
                if (_bakeComponentsInName) {
                    _dirtyEntities[entity] = 1;
                } else {
                    go.name = entity.ToString (_entityNameFormat);
                }
            }
            _entities[entity].gameObject.SetActive (true);
        }

        public void OnEntityDestroyed (int entity) {
            if (_entities[entity]) {
                _entities[entity].gameObject.SetActive (false);
            }
        }

        public void OnEntityChanged (int entity) {
            if (_bakeComponentsInName) {
                _dirtyEntities[entity] = 1;
            }
        }

        public void OnFilterCreated (EcsFilter filter) { }

        public void OnWorldResized (int newSize) {
            Array.Resize (ref _entities, newSize);
        }

        public void OnWorldDestroyed (EcsWorld world) {
            _world.RemoveEventListener (this);
            Object.Destroy (_rootGO);
        }

        public EcsEntityDebugView GetEntityView (int entity) {
            return entity >= 0 && entity < _entities.Length ? _entities[entity] : null;
        }
    }
}