using UnityEngine;

namespace GameConfigs
{
    [CreateAssetMenu(fileName = "CameraDataConfig", menuName = "Configs/CameraDataConfig", order = 3)]
    public class CameraDataConfig : ScriptableObject
    {
        [SerializeField] private Vector3 fromPlayerOffset;
        [SerializeField] private float cameraVelocity;
        
        public Vector3 FromPlayerOffset => fromPlayerOffset;
        public float CameraVelocity => cameraVelocity;
        
    }
}