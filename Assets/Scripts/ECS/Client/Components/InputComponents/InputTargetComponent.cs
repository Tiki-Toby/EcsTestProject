using ECS.Client;
using UnityEngine;

namespace ECS.Client
{
    public struct InputTargetRequest : IInputComponent
    {
        public Vector3 target;
    }
}