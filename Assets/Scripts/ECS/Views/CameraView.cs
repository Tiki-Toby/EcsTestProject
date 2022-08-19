using UnityEngine;

namespace ECS.Server
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private Camera camera;

        public Camera Camera => camera;
    }
}