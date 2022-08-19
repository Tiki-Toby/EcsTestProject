using UnityEngine;

namespace ECS.Server
{
    public struct DoorComponent
    {
        public int doorId;
        public Vector3 moveDirecition;
        public Vector3 finalPosition;
    }
}