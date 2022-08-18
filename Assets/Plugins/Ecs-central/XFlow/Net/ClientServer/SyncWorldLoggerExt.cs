using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;

namespace XFlow.Net.ClientServer
{
    public static class SyncWorldLoggerExt
    {
        public static SyncWorldLogger GetSyncLogger(this EcsWorld world)
        {
            return WorldLoggerExt.logger as SyncWorldLogger;
        }
    }
}