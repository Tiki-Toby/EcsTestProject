using System;

namespace XFlow.Net.ClientServer.Protocol
{
    [Serializable]
    public class Hello
    {
        public string Text;
        public string[] Components;
        public string InitialWorld;
    }
}