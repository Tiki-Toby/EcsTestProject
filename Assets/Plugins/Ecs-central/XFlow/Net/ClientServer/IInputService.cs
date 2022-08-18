using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace XFlow.Net.ClientServer
{
    public interface IInputService
    {
        public void Input(EcsWorld inputWorld, int playerId, int tick, IInputComponent input);
    }
}