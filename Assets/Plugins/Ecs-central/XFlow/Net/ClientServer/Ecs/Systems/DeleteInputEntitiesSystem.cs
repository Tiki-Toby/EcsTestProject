using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components.Input;

namespace XFlow.Net.ClientServer.Ecs.Systems
{
    public class DeleteInputEntitiesSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputComponent>()
                .Exc<InputTickComponent>().End();

            foreach (var inputEntity in filter)
            {
                inputWorld.DelEntity(inputEntity);
            }
        }
    }
}