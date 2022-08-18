using UnityEngine;

namespace XFlow.Net.ClientServer.Ecs.Components.Input.proto
{
    public struct InputShotComponent:IInputComponent
    {
        public Vector3 dir;
        public Vector3 pos;
    }
}