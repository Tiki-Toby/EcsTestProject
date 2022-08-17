using GameEntities;
using Leopotam.EcsLite;

namespace ApplicationCore
{
    public class ServerRootCore
    {
        private EcsWorld world, inputWorld;
        private EcsSystems systems;

        public ServerRootCore(EcsWorld world, EcsSystems systems)
        {
            this.world = world;
            this.systems = systems;

            AddSystems();
        }

        public void Init()
        {
            systems.Init();
        }
        
        private void AddSystems()
        {
            AddControlSystems();
        }

        private void AddControlSystems()
        {
            systems.Add(new MovementSystem());
            systems.Add(new GameProcessDebugSystem());
        }

        public void RunLogic()
        {
            systems.Run();
        }

        public void Destroy()
        {
            if (systems != null)
            {
                systems.Destroy();
                systems = null;
            }

            if (world != null)
            {
                world.Destroy();
                world = null;
            }
        }
    }
}