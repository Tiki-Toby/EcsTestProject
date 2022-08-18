using XFlow.EcsLite;
using XFlow.Modules.Fire.ClientServer.Components;

namespace XFlow.Modules.Fire.Client.Systems
{
    public class FireBurnedOutViewSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<BurnedOutComponent>();
        }
    }
}