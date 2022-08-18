using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Fire.Client.Components;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Utils;

namespace XFlow.Modules.Fire.Client.Systems
{
    public class FireViewSystem : IEcsRunSystem
    {
        private ParticleSystem particles;
        
        public FireViewSystem(ParticleSystem particles)
        {
            this.particles = particles;
        }
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var poolFire = world.GetPool<FireComponent>();
            var poolFireView = world.GetPool<FireViewComponent>();


            var filter = world.FilterAdded<FireComponent>().End();
            foreach (var entity in filter)
            {
                var fireComponent = poolFire.Get(entity);

                var ps = Object.Instantiate(particles);
                ps.transform.position = entity.EntityGetComponent<PositionComponent>(world).value;
                ps.name = entity.ToString();
                ps.transform.localScale = Vector3.one * fireComponent.size;
                poolFireView.Add(entity).view = ps;
            }

            var filterBurned = world.Filter<BurnedOutComponent>().Inc<FireViewComponent>().End();
            foreach (var entity in filterBurned)
            {
                Object.Destroy(poolFireView.Get(entity).view.gameObject);
                poolFireView.Del(entity);

                entity.EntityWith<TransformComponent>(world, component =>
                {
                    Object.Destroy(component.Transform.gameObject);
                });
            }
        }
    }
}