using System;

namespace XFlow.Net.ClientServer.Protocol
{
    [Serializable]
    public class WorldUpdateProto
    {
        public string difStr;
        public string difBinary;
        public int delay;
        public int LastClientTick;
        public int LastServerTick;
    }
}