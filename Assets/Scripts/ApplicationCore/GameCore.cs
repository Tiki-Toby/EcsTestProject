using GameEntities;
using Leopotam.EcsLite;

namespace ApplicationCore
{
    public class GameCore
    {
        private EcsWorld world;
        private EcsSystems systems;

        private ClientRootCore clientRootCore;
        private ServerRootCore serverRootCore;

        public GameCore(WorldView worldView)
        {
            world = new EcsWorld();
            systems = new EcsSystems(world);

            serverRootCore = new ServerRootCore(world, systems);
            clientRootCore = new ClientRootCore(world, systems, worldView);
            
            serverRootCore.Init();
            clientRootCore.Init();
        }

        public void Run()
        {
            serverRootCore.RunLogic();
            clientRootCore.RunView();
        }

        public void Destroy()
        {
            clientRootCore.Destroy();
            serverRootCore.Destroy();
        }
    }
}
