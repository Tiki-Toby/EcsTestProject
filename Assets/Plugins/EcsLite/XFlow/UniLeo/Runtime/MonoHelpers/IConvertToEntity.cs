using XFlow.EcsLite;

namespace XFlow.UniLeo.Runtime.MonoHelpers
{
    public interface IConvertToEntity
    {
        void Convert(int entity, EcsWorld world);
    }
}
