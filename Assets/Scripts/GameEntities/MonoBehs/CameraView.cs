using UnityEngine;

namespace GameEntities
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        [SerializeField] private Vector3 fromPlayerOffset;
        [SerializeField] private float cameraVelocity;

        public Camera Camera => camera;
        public Vector3 FromPlayerOffset => fromPlayerOffset;
        public float CameraVelocity => cameraVelocity;
    }
}