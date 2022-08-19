using System.Collections.Generic;
using System.Linq;
using ECS.Client;
using ECS.Server;
using GameConfigs;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using Zenject;

namespace ApplicationCore
{
    public class SceneManagerService
    {
        private readonly WorldView worldView;
        private readonly GameConfigsReceiver gameConfigs;
        
        private readonly Dictionary<GameObject, int> createdEntities;
        
        private readonly EcsWorld world;
        private readonly EntityFactory entityFactory;
        
        public SceneManagerService([Inject] EcsWorld world, 
            [Inject] EntityFactory entityFactory,
            [Inject] WorldView worldView, 
            [Inject] GameConfigsReceiver gameConfigs)
        {
            this.world = world;
            this.entityFactory = entityFactory;

            createdEntities = new Dictionary<GameObject, int>();
            
            this.worldView = worldView;
            this.gameConfigs = gameConfigs;
            
            CreateCameraEntity(worldView.Camera,
                gameConfigs.CameraDataConfig.CameraVelocity,
                gameConfigs.CameraDataConfig.FromPlayerOffset);
        }

        public void CreateEntitiesForLevel(int index)
        {
            CreatePlayerEntity(worldView.PlayerTransformPrefab,
                gameConfigs.GetSceneDataConfigByIndex(index).GetRandomPlayerSpawnPoint(),
                gameConfigs.PlayerDataConfig.Velocity);

            foreach (DoorButtonPair doorButtonPair in gameConfigs.GetSceneDataConfigByIndex(index).DoorButtonPairs)
            {
                DoorView doorView = worldView.DoorViews.First(view => view.DoorId == doorButtonPair.DoorId);
                DoorButtonView doorButtonView = worldView.DoorButtonViews.First(view => view.ButtonId == doorButtonPair.ButtonId);
                CreateDoorEntity(doorButtonView, doorView, doorButtonPair.Velocity, doorButtonPair.OffsetFromStartPosition);
            }

            CircleCollider2D[] colliders = GameObject.FindObjectsOfType<CircleCollider2D>();
            CreateColliders(colliders);
        }

        private void CreateCameraEntity(Camera camera, 
            float velocity, Vector3 offsetFromPlayer)
        {
            int cameraEntity = entityFactory.CreateEmpty("Camera");
            createdEntities.Add(camera.gameObject, cameraEntity);
            
            world.GetPool<TransformComponent>().Add(cameraEntity).Transform = camera.transform;
            
            world.AddUnique<MainCameraComponent>().cameraEntity = cameraEntity;
            
            ref var cameraComponent = ref world.GetPool<CameraComponent>().Add(cameraEntity);
            cameraComponent.Camera = camera;
            
            ref var actionCameraDataComponent = ref world.GetPool<ActionCameraComponent>().Add(cameraEntity);
            actionCameraDataComponent.FromPlayerOffset = offsetFromPlayer;
            actionCameraDataComponent.CameraVelocity = velocity;
        }
        
        private void CreatePlayerEntity(Transform playerTransformPrefab, Vector3 spawnPoint, float velocity)
        {
            Transform playerTransform = Object.Instantiate(playerTransformPrefab, spawnPoint, Quaternion.identity);
            
            int playerId = entityFactory.CreatePlayerEntity("Player", velocity);
            createdEntities.Add(playerTransform.gameObject, playerId);

            world.GetPool<TransformComponent>().Add(playerId).Transform = playerTransform;
            world.GetPool<PositionComponent>().GetRef(playerId).value = playerTransform.position;
            world.GetPool<RotationComponent>().GetRef(playerId).rotation = playerTransform.rotation;

            CreateRigidbodyDefinition(playerTransform.GetComponentInChildren<CircleCollider2D>(),
                BodyType.Dynamic,
                gameConfigs.PlayerDataConfig.Density, gameConfigs.PlayerDataConfig.Friction);
        }

        private void CreateDoorEntity(DoorButtonView doorButtonView, DoorView doorView, 
            float velocity, Vector3 offsetFromStartPosition)
        {
            if (!createdEntities.TryGetValue(doorView.gameObject, out int doorEntity))
            {
                doorEntity = entityFactory.CreateMovableGameObject(doorView.gameObject.name, velocity);
                createdEntities.Add(doorView.gameObject, doorEntity);
                
                Transform doorTransform = doorView.DoorTransform;
                world.GetPool<TransformComponent>().Add(doorEntity).Transform = doorTransform;
                world.GetPool<PositionComponent>().GetRef(doorEntity).value = doorTransform.position;
                world.GetPool<VelocityComponent>().GetRef(doorEntity).velocity = velocity;

                ref var doorComponent = ref world.GetPool<DoorComponent>().Add(doorEntity);
                doorComponent.doorId = doorView.DoorId;
                doorComponent.moveDirecition = offsetFromStartPosition.normalized;
                doorComponent.finalPosition = doorTransform.position + offsetFromStartPosition;
            }

            if (!createdEntities.TryGetValue(doorButtonView.gameObject, out int buttonEntity))
            {
                buttonEntity = entityFactory.CreateGameObject(doorButtonView.CircleCollider2D.name);
                createdEntities.Add(doorButtonView.gameObject, doorEntity);
                
                world.GetPool<TransformComponent>().Add(buttonEntity).Transform =
                    doorButtonView.CircleCollider2D.transform;
                world.GetPool<PositionComponent>().GetRef(buttonEntity).value =
                    doorButtonView.CircleCollider2D.transform.position;
                world.GetPool<InteractableRadiusComponent>().Add(buttonEntity).interactableRadius =
                    doorButtonView.CircleCollider2D.radius;

                ref var buttonComponent = ref world.GetPool<DoorButtonComponent>().Add(buttonEntity);
                buttonComponent.buttonId = doorButtonView.ButtonId;
                buttonComponent.doorEntity = doorEntity;
            }
        }

        private void CreateColliders(Collider2D[] collider2Ds)
        {
            foreach (Collider2D collider2D in collider2Ds)
            {
                CreateRigidbodyDefinition(collider2D);
            }
        }

        private void CreateRigidbodyDefinition(Collider2D collider2D, BodyType bodyType = BodyType.Dynamic,
            float density = 1, float friction = 0)
        {
            if(collider2D.isTrigger)
                return;

            if (!createdEntities.TryGetValue(collider2D.gameObject, out int entity))
            {
                entity = world.NewEntity();
                world.GetPool<PositionComponent>().Add(entity);
                world.GetPool<RotationComponent>().Add(entity);
                world.GetPool<Rotation2DComponent>().Add(entity);
                world.GetPool<TransformComponent>().Add(entity).Transform = collider2D.transform;
            }
            else if(world.GetPool<Box2DRigidbodyDefinitionComponent>().Has(entity))
                return;
                
            ref var rigidbodyDefinitionComponent = ref world.GetPool<Box2DRigidbodyDefinitionComponent>().Add(entity);
            rigidbodyDefinitionComponent.BodyType = bodyType;
            rigidbodyDefinitionComponent.Bullet = true;
            rigidbodyDefinitionComponent.Density = density;
            rigidbodyDefinitionComponent.LinearDamping = friction;

            if (collider2D is CircleCollider2D)
            {
                ref var collider = ref world.GetPool<Box2DCircleColliderComponent>().Add(entity);
                float radius = ((CircleCollider2D)collider2D).radius;
                collider.Radius = radius;
            }
            else if (collider2D is BoxCollider2D)
            {
                ref var collider = ref world.GetPool<Box2DBoxColliderComponent>().Add(entity);
                collider.Size = ((BoxCollider2D)collider2D).size;
            }
        }
    }
}