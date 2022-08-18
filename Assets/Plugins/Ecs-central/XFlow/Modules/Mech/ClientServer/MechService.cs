using Fabros.EcsModules.Mech.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Fabros.EcsModules.Mech.ClientServer
{
    public class MechService
    {
        public int CreateMechEntity(EcsWorld world)
        {
            var entity = world.NewEntity();
            entity.EntityAdd<MechComponent>(world);
            entity.EntityAdd<PositionComponent>(world);
            entity.EntityAdd<Rotation2DComponent>(world).Angle = 1;
            //entity.EntityAdd<MechMovingComponent>(world);

            return entity;
        }
    }
}