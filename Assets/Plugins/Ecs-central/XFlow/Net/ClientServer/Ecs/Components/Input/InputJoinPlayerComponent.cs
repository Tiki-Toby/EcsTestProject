using System;

namespace XFlow.Net.ClientServer.Ecs.Components.Input
{
    [Serializable]
    public struct InputJoinPlayerComponent
    {
        public bool leave;
        public int playerID;
    }
}