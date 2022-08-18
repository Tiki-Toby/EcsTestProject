using System;

namespace XFlow.EcsLite
{
    public interface IAnyComponentListeners
    {
        void RemoveChanged(Object listener);
        void RemoveRemoved(Object listener);
    }
}