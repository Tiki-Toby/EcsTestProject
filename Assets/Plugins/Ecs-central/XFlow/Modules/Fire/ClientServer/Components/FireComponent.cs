using System;

namespace XFlow.Modules.Fire.ClientServer.Components
{
    [Serializable]
    public struct FireComponent
    {
        public float size;
        public float startTime;
        public float endTime;
        public bool destroyEntity;
    }
}