using System.Runtime.InteropServices;
using GameConfigs;
using GameEntities;
using InputModule;
using Leopotam.EcsLite;
using Zenject;

namespace ApplicationCore
{
    public class ClientRootCore
    {
        private EcsWorld inputWorld;
        private EcsSystems systems;
        
        private EcsSystems viewSystems;
        private SceneManagerService sceneManagerService;
        
        public ClientRootCore(EcsWorld world, EcsSystems systems, 
            WorldView worldView, GameConfigsReceiver gameConfigs)
        {
            this.systems = systems;

            inputWorld = new EcsWorld();
            systems.AddWorld(inputWorld, "Input");

            viewSystems = new EcsSystems(world);

            sceneManagerService = new SceneManagerService(world, worldView, gameConfigs);
            sceneManagerService.CreateEntitiesForLevel(0);

            AddInputSystems();
            AddViewSystems();
        }
        
        private void AddInputSystems()
        {
            systems.Add(new MouseInputController());
            systems.Add(new HandleInputRequestsSystem());
        }

        private void AddViewSystems()
        {
            viewSystems.Add(new SyncTransformDataSystem());
            viewSystems.Add(new CameraFollowSystem());
        }

        public void Init()
        {
            viewSystems.Init();
        }
        
        public void RunView()
        {
            viewSystems.Run();
        }

        public void Destroy()
        {
            if (viewSystems != null)
            {
                viewSystems.Destroy();
                viewSystems = null;
            }

            if (inputWorld != null)
            {
                inputWorld.Destroy();
                inputWorld = null;
            }
        }
    }
}