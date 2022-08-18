using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Grid.Systems;

namespace XFlow.Modules.Grid
{
    public static class GridModule
    {
        public static void AddSerializableComponents(ComponentsCollection collection)
        {
            //empty
        }

        public static IEcsSystem[] GetSystems()
        {
            return new IEcsSystem[]{new GridSystem()};
        }
    }
}