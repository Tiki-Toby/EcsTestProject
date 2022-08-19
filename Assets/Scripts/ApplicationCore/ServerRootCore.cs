using GameEntities;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;

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
            
            systems.Add(new Box2DInitSystem());
            systems.Add(new Box2DCreateBodiesSystem());
            systems.Add(new Box2DCreateContactsSystem());
            systems.Add(new Box2DUpdateInternalObjectsSystem());
            systems.Add(new Box2DUpdateSystem());
            systems.Add(new Box2DDeleteContactsSystem());
            
            systems.Add(new GameProcessDebugSystem(false, true));
            systems.Add(new TickSystem());
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