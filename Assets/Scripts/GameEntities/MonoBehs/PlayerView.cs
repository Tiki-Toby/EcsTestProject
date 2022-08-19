using UnityEngine;

namespace GameEntities
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        
        public Transform PlayerTransform => playerTransform;
    }
}