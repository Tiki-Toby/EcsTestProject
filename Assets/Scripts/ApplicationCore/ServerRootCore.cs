using GameEntities;
using Leopotam.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;

namespace ApplicationCore
{
    public class ServerRootCore
    {
        private EcsWorld world;
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
            systems.Add(new MoveToTargetSystem());
            systems.Add(new TriggerEnterExitDetectionSystem());
            systems.Add(new DoorButtonPressingSystem());
            systems.Add(new MovementSystem());
            systems.Add(new GameProcessDebugSystem(false, true));
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