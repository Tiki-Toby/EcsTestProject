using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;

namespace XFlow.Modules.Box2D
{
    public static class Box2DModule
    {
        public static IEcsSystem[] CreateMainSystems(int positionIterations, int velocityIterations)
        {
            return new IEcsSystem[]
            {
                new Box2DInitSystem(), 
                new Box2DCreateBodiesSystem(),
                new Box2DUpdateInternalObjectsSystem(),
                //new Box2DCreateContactsSystem(),//add it yourself with Box2DDeleteContactsSystem
                new Box2DUpdateSystem(positionIterations, velocityIterations),
                new Box2DWriteBodiesToComponentsSystem()    
            };
        }
    }
}