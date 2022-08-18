using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;

namespace XFlow.Modules.Box2D.ClientServer.Systems
{
    public class Box2DDeleteContactsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld world;
        private EcsFilter filterBeginContact;
        private EcsFilter filterEndContact;
        private EcsFilter filterPreSolve;
        private EcsFilter filterPostSolve;

        public Box2DDeleteContactsSystem()
        {
            
        }
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            
            filterBeginContact = world.Filter<Box2DBeginContactComponent>().End();
            filterEndContact = world.Filter<Box2DEndContactComponent>().End();
            filterPreSolve = world.Filter<Box2DPreSolveComponent>().End();
            filterPostSolve = world.Filter<Box2DPostSolveComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filterBeginContact)
            {
                world.DelEntity(entity);
            }
            foreach (var entity in filterEndContact)
            {
                world.DelEntity(entity);
            }
            foreach (var entity in filterPreSolve)
            {
                world.DelEntity(entity);
            }
            foreach (var entity in filterPostSolve)
            {
                world.DelEntity(entity);
            }
        }
    }
}