using GameConfigs;
using GameEntities;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace ApplicationCore
{
    public class ApplicationRoot : MonoBehaviour
    {
        [Inject] private WorldView worldView;
        [Inject] private GameConfigsReceiver gameConfigs;
        
        private EcsWorld world;
        private EcsSystems systems;

        private ClientRootCore clientRootCore;
        private ServerRootCore serverRootCore;

        private void Start()
        {
            world = new EcsWorld();
            systems = new EcsSystems(world);

            serverRootCore = new ServerRootCore(world, systems);
            clientRootCore = new ClientRootCore(world, systems, worldView, gameConfigs);
            
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
