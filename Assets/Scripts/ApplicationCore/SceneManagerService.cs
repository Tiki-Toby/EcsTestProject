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

namespace ApplicationCore
{
    public class SceneManagerService
    {
        private readonly WorldView worldView;
        private readonly GameConfigsReceiver gameConfigs;
        
        private readonly Dictionary<GameObject, int> createdEntities;
        
        private readonly EcsWorld world;
        private readonly EntityFactory entityFactory;
        
        public SceneManagerService(EcsWorld world, WorldView worldView, GameConfigsReceiver gameConfigs)
        {
            this.world = world;
            entityFactory = new EntityFactory(this.world);

            createdEntities = new Dictionary<GameObject, int>();
            
            this.worldView = worldView;
            this.gameConfigs = gameConfigs;
        }

        public void CreateEntitiesForLevel(int index)
        {
            CreateCameraEntity(worldView.Camera,
                gameConfigs.CameraDataConfig.CameraVelocity,
                gameConfigs.CameraDataConfig.FromPlayerOffset);

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
            world.GetPool<TransformComponent>().Add(cameraEntity).Transform = camera.transform;
            
            world.AddUnique<MainCameraComponent>().cameraEntity = cameraEntity;
            
            ref var cameraComponent = ref world.GetPool<CameraComponent>().Add(cameraEntity);
            cameraComponent.Camera = camera;
            
            ref var actionCameraDataComponent = ref world.GetPool<ActionCameraComponent>().Add(cameraEntity);
            actionCameraDataComponent.FromPlayerOffset = offsetFromPlayer;
            actionCameraDataComponent.CameraVelocity = velocity;
            
            createdEntities.Add(cameraComponent.Camera.gameObject, cameraEntity);
        }
        
        private void CreatePlayerEntity(Transform playerTransformPrefab, Vector3 spawnPoint, float velocity)
        {
            Transform playerTransform = Object.Instantiate(playerTransformPrefab, spawnPoint, Quaternion.identity);
            
            int playerId = entityFactory.CreatePlayerEntity("Player", velocity);

            world.GetPool<TransformComponent>().Add(playerId).Transform = playerTransform;
            world.GetPool<PositionComponent>().GetRef(playerId).value = playerTransform.position;
            world.GetPool<RotationComponent>().GetRef(playerId).rotation = playerTransform.rotation;
            
            createdEntities.Add(playerTransform.gameObject, playerId);
        }

        private void CreateDoorEntity(Transform doorTransform,
            int doorId,
            float velocity, Vector3 offsetFromStartPosition)
        {
            if (!createdEntities.TryGetValue(doorTransform.gameObject, out int doorEntity))
            {
                doorEntity = entityFactory.CreateMovableGameObject(doorTransform.name, velocity);
                world.GetPool<TransformComponent>().Add(doorEntity).Transform = doorTransform;
                world.GetPool<PositionComponent>().GetRef(doorEntity).value = doorTransform.position;

                ref var doorComponent = ref world.GetPool<DoorComponent>().Add(doorEntity);
                doorComponent.doorId = doorId;
                doorComponent.moveDirecition = offsetFromStartPosition.normalized;
                doorComponent.finalPosition = doorTransform.position + offsetFromStartPosition;

                createdEntities.Add(doorTransform.gameObject, doorEntity);
            }
        }

        private void CreateDoorEntity(DoorButtonView doorButtonView, DoorView doorView, 
            float velocity, Vector3 offsetFromStartPosition)
        {
            if (!createdEntities.TryGetValue(doorView.gameObject, out int doorEntity))
            {
                doorEntity = entityFactory.CreateMovableGameObject(doorView.gameObject.name, velocity);
                Transform doorTransform = doorView.DoorTransform;
                world.GetPool<TransformComponent>().Add(doorEntity).Transform = doorTransform;
                world.GetPool<PositionComponent>().GetRef(doorEntity).value = doorTransform.position;
                world.GetPool<VelocityComponent>().GetRef(doorEntity).velocity = velocity;

                ref var doorComponent = ref world.GetPool<DoorComponent>().Add(doorEntity);
                doorComponent.doorId = doorView.DoorId;
                doorComponent.moveDirecition = offsetFromStartPosition.normalized;
                doorComponent.finalPosition = doorTransform.position + offsetFromStartPosition;

                createdEntities.Add(doorView.gameObject, doorEntity);
            }

            if (!createdEntities.TryGetValue(doorButtonView.gameObject, out int buttonEntity))
            {
                buttonEntity = entityFactory.CreateGameObject(doorButtonView.CircleCollider2D.name);
                world.GetPool<TransformComponent>().Add(buttonEntity).Transform =
                    doorButtonView.CircleCollider2D.transform;
                world.GetPool<PositionComponent>().GetRef(buttonEntity).value =
                    doorButtonView.CircleCollider2D.transform.position;
                world.GetPool<InteractableRadiusComponent>().Add(buttonEntity).interactableRadius =
                    doorButtonView.CircleCollider2D.radius;

                ref var buttonComponent = ref world.GetPool<DoorButtonComponent>().Add(buttonEntity);
                buttonComponent.buttonId = doorButtonView.ButtonId;
                buttonComponent.doorEntity = doorEntity;
                
                createdEntities.Add(doorButtonView.gameObject, doorEntity);
            }
        }

        private void AddCircleComponent(CircleCollider2D[] circleCollider2D)
        {
            foreach (CircleCollider2D collider in circleCollider2D)
            {
                if (!createdEntities.TryGetValue(collider.gameObject, out int entity))
                    entity = world.NewEntity();

                world.GetPool<RadiusComponent>().Add(entity).radius = collider.radius;
            }
        }

        private void CreateColliders(Collider2D[] collider2Ds)
        {
            foreach (Collider2D collider2D in collider2Ds)
            {
                if(collider2D.isTrigger)
                    continue;
                
                if (!createdEntities.TryGetValue(collider2D.gameObject, out int entity))
                    entity = world.NewEntity();
                
                ref var rigidbodyDefinitionComponent = ref world.GetPool<Box2DRigidbodyDefinitionComponent>().Add(entity);
                rigidbodyDefinitionComponent.BodyType = BodyType.Dynamic;
                rigidbodyDefinitionComponent.Bullet = true;
                rigidbodyDefinitionComponent.Density = 1;
                rigidbodyDefinitionComponent.LinearDamping = 1f;

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
}