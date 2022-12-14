using UnityEngine;

namespace ECS.Server
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        
        public Transform PlayerTransform => playerTransform;
    }
}