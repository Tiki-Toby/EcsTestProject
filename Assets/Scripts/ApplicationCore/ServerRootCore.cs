using ECS.Server;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Tick.ClientServer.Systems;

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
            systems.Add(new TickSystem());
            
            systems.Add(new MoveToTargetSystem());
            systems.Add(new TriggerEnterExitDetectionSystem());
            systems.Add(new DoorButtonPressingSystem());
            systems.Add(new MovementSystem());
            systems.Add(new PhysicsMovementSystem());
            systems.Add(new RotateSpheresSystem());
            
            systems.Add(new Box2DInitSystem());
            systems.Add(new CreatePhysicsBodySystem());
            
            systems.Add(new Box2DCreateBodiesSystem());
            systems.Add(new Box2DCreateContactsSystem());
            systems.Add(new Box2DUpdateInternalObjectsSystem());
            systems.Add(new Box2DUpdateSystem());
            systems.Add(new Box2DDeleteContactsSystem());
            systems.Add(new Box2DWriteBodiesToComponentsSystem());
            
            systems.Add(new GameProcessDebugSystem(false, false));
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