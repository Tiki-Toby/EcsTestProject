using XFlow.EcsLite;

namespace ECS.Client
{
    public class ClearInputSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            EcsWorld inputWorld = systems.GetWorld("Input");
            EcsFilter filter = inputWorld.Filter<InputEntityTag>().End();

            foreach (int i in filter)
            {
                inputWorld.DelEntity(i);
            }
        }
    }
}