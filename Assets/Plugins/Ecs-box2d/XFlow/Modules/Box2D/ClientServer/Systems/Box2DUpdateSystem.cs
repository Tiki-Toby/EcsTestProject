using UnityEngine;
using UnityEngine.Profiling;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    /**
     * ВАЖНО Box2DSystem должна быть в конце списка - после всех основных систем которые что-то двигают, толкают и тд
     * иначе может произойти рассинхрон на пару кадров
     */
    
    public class Box2DUpdateSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        public class Options
        {
            public int positionIterations = 6;
            public int velocityIterations = 2;
            public Vector2 graviti = new Vector2();
        }
        
        private EcsWorld world;
        
        
        private int positionIterations;
        private int velocityIterations;
        private Vector2 graviti;

        public Box2DUpdateSystem(Options options)
        {
            this.positionIterations = options.positionIterations;
            this.velocityIterations = options.velocityIterations;
            this.graviti = options.graviti;
        }
        
        public Box2DUpdateSystem(
            int positionIterations = 6,
            int velocityIterations = 2,
            Vector2 graviti = new Vector2())
        {
            this.positionIterations = positionIterations;
            this.velocityIterations = velocityIterations;
            this.graviti = graviti;
        }
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }
        
        public void Run(EcsSystems systems)
        {
            Profiler.BeginSample("Box2DUpdateSystem");
            var physicsWorld = world.GetUnique<Box2DWorldComponent>();
            var deltaTime = world.GetDeltaSeconds();
            
            Box2DApi.UpdateWorld(
                physicsWorld.WorldReference, 
                deltaTime, positionIterations, velocityIterations);
            Profiler.EndSample();
        }

        public void Destroy(EcsSystems systems)
        {
            var box2d = world.GetUnique<Box2DWorldComponent>();
            Box2DApi.DestroyWorld(box2d.WorldReference);
            world.DelUnique<Box2DWorldComponent>();

            var poolRefs = world.GetPool<Box2DBodyComponent>();
            var poolCreated = world.GetPool<Box2DBodyComponent>();
            
            poolRefs.GetEntities().ForEach(entity =>
            {
                poolRefs.Del(entity);
            });
            
            poolCreated.GetEntities().ForEach(entity =>
            {
                poolCreated.Del(entity);
            });
        }
    }
}
