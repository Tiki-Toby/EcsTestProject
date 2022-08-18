using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace XFlow.Modules.Box2D.ClientServer.Api
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CollisionCallbackData
    {
        public IntPtr UserPtr;
        public IntPtr PhysicsWorld;
        public int EntityA;
        public int EntityB;
        public Vector2 VelA;
        public Vector2 VelB;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Vector2[] ContactPoints;
        public Int32 ContactPointCount;
        public Vector2 Normal;
    }
}