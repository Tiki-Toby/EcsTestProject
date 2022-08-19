using UnityEngine;

namespace ECS.Server
{
    public class DoorView : MonoBehaviour
    {
        [SerializeField] private int doorId;
        [SerializeField] private Transform doorTransform;

        public int DoorId => doorId;
        public Transform DoorTransform => doorTransform;
    }
}