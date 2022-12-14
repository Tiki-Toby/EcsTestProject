using System.Collections.Generic;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Grid.Other;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace XFlow.Modules.Fire.ClientServer.Systems
{
    public class FireSystem : IEcsRunSystem
    {
        private List<int> entities = new List<int>();

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var filter = world
                .Filter<PositionComponent>()
                .Inc<FireComponent>().Exc<BurnedOutComponent>()
                .End();

            var poolPosition = world.GetPool<PositionComponent>();
            var poolFire = world.GetPool<FireComponent>();
            var poolFlammable = world.GetPool<FlammableComponent>();

            //var seconds = world.GetDeltaSeconds();

            var time = world.GetTime();

            foreach (var entity in filter)
            {
                var pos = poolPosition.Get(entity).value;
                var fireComponent = poolFire.Get(entity);


                if (fireComponent.endTime < time)
                {
                    if (fireComponent.destroyEntity)
                        entity.EntityAddComponent<BurnedOutComponent>(world);

                    //world.DelEntity(entity);
                    continue;
                }

                var dur = time - fireComponent.startTime;

                if (dur < 0.25f)
                    continue;

                world.GetNearestEntities(entity, pos, 1f, ref entities, entity => !poolFire.Has(entity) && poolFlammable.Has(entity));

                if (entities.Count == 0)
                    continue;

                var targetEntity = entities[0];
                var power = poolFlammable.Get(targetEntity).Power;

                targetEntity.EntityAddComponent<FireComponent>(world) = new FireComponent
                {
                    size = power,
                    startTime = world.GetTime(),
                    endTime = world.GetTime() + 5 * power,
                    destroyEntity = true
                };

                poolFlammable.Del(targetEntity);
            }
        }
    }
}