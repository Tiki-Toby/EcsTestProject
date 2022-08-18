using System;

namespace XFlow.Modules.Tick.Other
{
    [Serializable]
    public struct Tick
    {
        public int Value;// { get; set; }

        public Tick(int v)
        {
            Value = v;
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator >(Tick a, Tick b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(Tick a, Tick b)
        {
            return a.Value < b.Value;
        }

        public static bool operator <=(Tick a, Tick b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator >=(Tick a, Tick b)
        {
            return a.Value >= b.Value;
        }

        public static Tick operator +(Tick a, TickDelta b)
        {
            return new Tick(a.Value + b.Value);
        }
        public static Tick operator -(Tick a, TickDelta b)
        {
            return new Tick(a.Value - b.Value);
        }

        public static Tick operator +(Tick a, int b)
        {
            return new Tick(a.Value + b);
        }

        public static Tick operator -(Tick a, int b)
        {
            return new Tick(a.Value - b);
        }


        public static int operator - (Tick a, Tick b) {
            return a.Value - b.Value;
        }

        public static Tick operator - (Tick a) {
            return new Tick(-a.Value);
        }

        public static bool operator ==(Tick a, Tick b)
        {
            return a.Value == b.Value;
        }

        public static bool operator == (Tick a, int b)
        {
            return a.Value == b;
        }

        public static bool operator != (Tick a, int b)
        {
            return a.Value != b;
        }

        public static bool operator != (Tick lhs, Tick rhs) {
            return !(lhs == rhs);
        }
        
        public override bool Equals(object o)
        {
            if(o == null)
                return false;
            
            var second = (Tick)o;
            return second.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}