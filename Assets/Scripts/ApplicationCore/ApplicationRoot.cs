using GameConfigs;
using ECS.Server;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using Zenject;

namespace ApplicationCore
{
    public class ApplicationRoot : MonoBehaviour
    {
        [Inject] private WorldView worldView;
        [Inject] private GameConfigsReceiver gameConfigs;
        [Inject] private SceneManagerService sceneManagerService;
        [Inject] private EcsWorld world;
        
        private EcsSystems systems;

        private ClientRootCore clientRootCore;
        private ServerRootCore serverRootCore;

        private void Start()
        {
            systems = new EcsSystems(world);
            
            world.AddUnique(new TickDeltaComponent
            {
                Value = new TickDelta((int)(1f/Time.fixedDeltaTime))
            });

            world.AddUnique(new TickComponent{Value = new Tick(0)});


            serverRootCore = new ServerRootCore(world, systems);
            clientRootCore = new ClientRootCore(world, systems, sceneManagerService);
            clientRootCore.CreateLevel(0);
            
            serverRootCore.Init();
            clientRootCore.Init();
        }

        private void Update()
        {
            serverRootCore.RunLogic(); 
            clientRootCore.RunView();
        }

        private void OnDestroy()
        {
            clientRootCore.Destroy();
            serverRootCore.Destroy();
        }
    }
}
