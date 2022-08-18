using System;
using System.Reflection;
using System.Runtime.InteropServices;
using XFlow.EcsLite;

namespace XFlow.Utils
{
    public static class Utils
    {
        public static int GetArrayMemorySize<TType>(TType[] array) where TType : struct
        {
            try
            {
                //если внутри струтуры положить class instance, то будет exception
                return Marshal.SizeOf<TType>() * array.Length;
            }
            catch (Exception e)
            {
                //но нам это не страшно вернем 0
                return 0;
            }
        }
        
        public static bool CheckIsEmptyComponent<T>() where T:struct
        {
            var type = typeof(T);
            //todo remove EmptyComponent component
            if (type.GetCustomAttribute<EmptyComponent>() != null)
            {
#if DEBUG
                if (type.GetFields().Length > 0)
                {
                    throw new Exception ($"Component <{typeof (T).Name}> marked as Empty but has fields");
                }
#endif

                return true;
            }

            return false;
        }
    }
}