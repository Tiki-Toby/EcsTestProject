using System;

namespace XFlow.Modules.Tick.Other
{
    [Serializable]
    public struct TickDelta
    {
        public int Value;
        public float Seconds;

        public TickDelta(int ticksPerSecond)
        {
            Value = 1;
            //Assert.AreEqual(value * serverTickrate, clientTickrate);

            Seconds = 1f/ticksPerSecond;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}