using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.Utils
{
    public interface IWorldLogger
    {
        void Log(EcsWorld world, string str);
    }
}