using UnityEngine;

namespace GameEntities
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private Camera camera;

        public Camera Camera => camera;
    }
}