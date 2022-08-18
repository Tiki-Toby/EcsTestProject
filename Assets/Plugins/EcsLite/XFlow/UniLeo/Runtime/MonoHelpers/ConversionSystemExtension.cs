using XFlow.EcsLite;
using XFlow.UniLeo.Runtime.World.Systems;

namespace XFlow.UniLeo.Runtime.MonoHelpers
{
    public static class ConversionSystemExtension
    {
        public static EcsSystems ConvertScene(this EcsSystems ecsSystems)
        {
            ecsSystems.Add(new WorldInitSystem());
            return ecsSystems;
        }
    }
}
