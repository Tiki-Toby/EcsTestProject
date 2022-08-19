using ECS.Client;
using UnityEngine;

namespace ECS.Client
{
    public struct InputDirectionComponent : IInputComponent
    {
        public Vector3 MoveDirection;
    }
}