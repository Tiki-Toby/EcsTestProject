using GameEntities;
using Leopotam.EcsLite;

namespace ApplicationCore
{
    public class SceneManagerService
    {
        private EcsWorld _world;
        private EntityFactory _entityFactory;

        public SceneManagerService(EcsWorld world, WorldView worldView)
        {
            _world = world;
            _entityFactory = new EntityFactory(_world);

            CreateCameraEntity(worldView.CameraView);
            CreatePlayerEntity(worldView.PlayerView);
            CreateDoorEntities(worldView.DoorButtonViews, worldView.DoorViews);
        }

        private void CreateCameraEntity(CameraView cameraView)
        {
            int cameraId = _entityFactory.CreateEmpty("Camera");
            _world.GetPool<TransformComponent>().Add(cameraId).objectTransform = cameraView.Camera.transform;
            
            ref var cameraComponent = ref _world.GetPool<CameraComponent>().Add(cameraId);
            cameraComponent.Camera = cameraView.Camera;
            
            ref var actionCameraDataComponent = ref _world.GetPool<ActionCameraComponent>().Add(cameraId);
            actionCameraDataComponent.FromPlayerOffset = cameraView.FromPlayerOffset;
            actionCameraDataComponent.CameraVelocity = cameraView.CameraVelocity;
        }
        
        private void CreatePlayerEntity(PlayerView playerView)
        {
            int playerId = _entityFactory.CreatePlayerEntity(playerView.PlayerName, playerView.Velocity);

            _world.GetPool<TransformComponent>().Add(playerId).objectTransform = 
                playerView.PlayerTransform;

            _world.GetPool<PositionComponent>().Get(playerId).currentEntityPosition = 
                playerView.PlayerTransform.position;
            
            _world.GetPool<RotationComponent>().Get(playerId).rotation =
                playerView.PlayerTransform.rotation;
        }

        private void CreateDoorEntities(DoorButtonView[] doorButtonViews, DoorView[] doorViews)
        {
            for(int i = 0; i < doorButtonViews.Length; i++)
            {
                int doorEntity = _entityFactory.CreateMovableGameObject(doorViews[i].DoorTransform.name, 
                    doorViews[i].Velocity);
                _world.GetPool<TransformComponent>().Add(doorEntity).objectTransform = 
                    doorViews[i].DoorTransform;
                _world.GetPool<PositionComponent>().Get(doorEntity).currentEntityPosition = 
                    doorViews[i].DoorTransform.position;
                _world.GetPool<VelocityComponent>().Get(doorEntity).velocity = 
                    doorViews[i].Velocity;
                
                ref var doorComponent = ref _world.GetPool<DoorComponent>().Add(doorEntity);
                doorComponent.doorId = doorViews[i].DoorId;
                doorComponent.moveDirecition = doorViews[i].OffsetFromStartPosition.normalized;
                doorComponent.finalPosition = doorViews[i].DoorTransform.position + doorViews[i].OffsetFromStartPosition;

                int buttonId = _entityFactory.CreateGameObject(doorButtonViews[i].ButtonTransform.name);
                _world.GetPool<TransformComponent>().Add(buttonId).objectTransform = 
                    doorButtonViews[i].ButtonTransform;
                _world.GetPool<PositionComponent>().Get(buttonId).currentEntityPosition = 
                    doorButtonViews[i].ButtonTransform.position;
                
                ref var buttonComponent = ref _world.GetPool<DoorButtonComponent>().Add(buttonId);
                buttonComponent.buttonId = doorButtonViews[i].ButtonId;
                buttonComponent.doorEntity = doorEntity;
            }
        }
    }
}