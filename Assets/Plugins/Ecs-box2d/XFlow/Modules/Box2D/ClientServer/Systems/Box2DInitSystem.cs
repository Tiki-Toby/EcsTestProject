using UnityEngine;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    public class Box2DInitSystem : IEcsInitSystem
    {
        private EcsWorld world;

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            if (!world.HasUnique<Box2DWorldComponent>())
            {
                var worldReference = Box2DApi.CreateWorld(new Vector2(0, 0));
                world.AddUnique<Box2DWorldComponent>().WorldReference = worldReference;
                world.Log($"create box2 {worldReference.ToInt64():X8}");
            }
        } 
    }
}