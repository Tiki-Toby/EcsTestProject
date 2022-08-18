using XFlow.EcsLite;

namespace XFlow.Ecs.ClientServer.Utils
{
    public static class WorldLoggerExt
    {
        public static IWorldLogger logger;
        public static void Log(this EcsWorld world, string str)
        {
            logger?.Log(world, str);
        }
        
#if UNITY_EDITOR //unity editor domain reload feature
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void _reset()
        {
            logger = null;
        }
#endif
    }
}