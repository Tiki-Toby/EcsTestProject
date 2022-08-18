using Fabros.EcsModules.Mech.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Fabros.EcsModules.Mech.Client.Systems
{
    public class MechAnimationSystem: IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<MechComponent>().End();//.Inc<MechMovingComponent>().End();
            foreach (var entity in filter)
            {
                var animator = entity.EntityGet<MechAnimatorComponent>(world).Animator;
                
                animator.SetBool("walking", entity.EntityHas<MechMovingComponent>(world));
            }
        }
    }
}