using System;

namespace XFlow.Net.ClientServer.Protocol
{
    /* это протокол для прототипа, для простоты в нем нет оптимизаций, содержит лишние поля и прочий мусор */
    [Serializable]
    public class Packet
    {
        public int playerID;

        public WorldUpdateProto WorldUpdate;
        public bool hasWorldUpdate;
        public bool hasWelcomeFromServer;

        public Hello hello;
        public bool hasHello;
    }
}