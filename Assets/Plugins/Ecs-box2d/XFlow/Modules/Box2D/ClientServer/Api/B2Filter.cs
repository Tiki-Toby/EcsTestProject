using System;

namespace XFlow.Modules.Box2D.ClientServer.Api
{
    public struct B2Filter
    {
        public UInt16 CategoryBits;
        public UInt16 MaskBits;
        public Int16 GroupIndex;
    };
}