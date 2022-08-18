using System;
using XFlow.EcsLite;

namespace XFlow.Utils
{
    public static class EcsDebug
    {
        /**
         * useful function for debugging
         */
        public static string e2s(this int entity, EcsWorld world)
        {
            var raw = world.GetRawEntities();
            
            var str = $"entity #{entity} gen={raw[entity].Gen}, ";
            object[] list_ = null;
            int count = world.GetComponents(entity, ref list_);

            var list = new object[count];
            Array.Copy(list_, list, count);
            
            Array.Sort(list, (a, b) =>
            {
                return String.Compare(a.GetType().Name, b.GetType().Name);
            });

            for (int i = 0; i < count; ++i)
            {
                var component = list[i];
                var tp = component.GetType();
                var fields = tp.GetFields();

                var componentStr = "";
                for (int f = 0; f < fields.Length; ++f)
                {
                    var name = fields[f];
                    var field = tp.GetField(name.Name);
                    var val = field.GetValue(component);
                    componentStr += $"{name.Name} = {val}, ";
                }
                if (componentStr.Length > 1)
                {
                    componentStr = componentStr.Remove(componentStr.Length - 2, 2);
                }
                str += $"{tp.Name}({componentStr}), ";
            }
            str = str.Remove(str.Length - 1);
            return str;
        }
        
        public static string e2s(EcsWorld world)
        {
            int[] entities = null;
            int count = world.GetAllEntities(ref entities);
            string str = "";
            for (int i = 0; i < count; ++i)
            {
                int entity = entities[i];
                str += $"{entity.e2s(world)}\n";
            }

            return str;
        }
    }
}