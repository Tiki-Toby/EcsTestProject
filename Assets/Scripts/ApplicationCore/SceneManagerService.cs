using System.Collections.Generic;
using System.Linq;
using GameConfigs;
using GameEntities;
using UnityEngine;
using XFlow.EcsLite;
using DoorButtonPair = GameConfigs.DoorButtonPair;

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

            Collider[] colliders = GameObject.FindObjectsOfType<Collider>();
        }

        private void CreateCameraEntity(Camera camera, 
            float velocity, Vector3 offsetFromPlayer)
        {
            int cameraEntity = entityFactory.CreateEmpty("Camera");
            world.GetPool<TransformComponent>().Add(cameraEntity).objectTransform = camera.transform;
            
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

            world.GetPool<TransformComponent>().Add(playerId).objectTransform = playerTransform;
            world.GetPool<PositionComponent>().GetRef(playerId).currentEntityPosition = playerTransform.position;
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
                world.GetPool<TransformComponent>().Add(doorEntity).objectTransform = doorTransform;
                world.GetPool<PositionComponent>().GetRef(doorEntity).currentEntityPosition = doorTransform.position;

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
                world.GetPool<TransformComponent>().Add(doorEntity).objectTransform = doorTransform;
                world.GetPool<PositionComponent>().GetRef(doorEntity).currentEntityPosition = doorTransform.position;
                world.GetPool<VelocityComponent>().GetRef(doorEntity).velocity = velocity;

                ref var doorComponent = ref world.GetPool<DoorComponent>().Add(doorEntity);
                doorComponent.doorId = doorView.DoorId;
                doorComponent.moveDirecition = offsetFromStartPosition.normalized;
                doorComponent.finalPosition = doorTransform.position + offsetFromStartPosition;

                createdEntities.Add(doorView.gameObject, doorEntity);
            }

            if (!createdEntities.TryGetValue(doorButtonView.gameObject, out int buttonEntity))
            {
                buttonEntity = entityFactory.CreateGameObject(doorButtonView.SphereCollider.name);
                world.GetPool<TransformComponent>().Add(buttonEntity).objectTransform =
                    doorButtonView.SphereCollider.transform;
                world.GetPool<PositionComponent>().GetRef(buttonEntity).currentEntityPosition =
                    doorButtonView.SphereCollider.transform.position;
                world.GetPool<InteractableRadiusComponent>().Add(buttonEntity).interactableRadius =
                    doorButtonView.SphereCollider.radius;

                ref var buttonComponent = ref world.GetPool<DoorButtonComponent>().Add(buttonEntity);
                buttonComponent.buttonId = doorButtonView.ButtonId;
                buttonComponent.doorEntity = doorEntity;
                
                createdEntities.Add(doorButtonView.gameObject, doorEntity);
            }
        }
    }
}