using UnityEngine;
using XFlow.EcsLite;

namespace XFlow.UniLeo.Runtime.MonoHelpers
{
    public abstract class MonoProvider<T> : BaseMonoProvider, IConvertToEntity where T : struct
    {

        [SerializeField] protected T value;

        void IConvertToEntity.Convert(int entity, EcsWorld world)
        {
            var pool = world.GetPool<T>();
            if (pool.Has(entity))
            {
                pool.Del(entity);
            }

            pool.Add(entity) = value;
        }
    }
}
