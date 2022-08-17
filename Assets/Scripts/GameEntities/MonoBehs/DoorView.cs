using UnityEngine;

namespace GameEntities
{
    public class DoorView : MonoBehaviour
    {
        [SerializeField] private int doorId;
        [SerializeField] private Vector3 offsetFromStartPosition;
        [SerializeField] private float velocity;
        [SerializeField] private Transform doorTransform;

        public int DoorId => doorId;
        public Vector3 OffsetFromStartPosition => offsetFromStartPosition;
        public float Velocity => velocity;
        public Transform DoorTransform => doorTransform;
    }
}