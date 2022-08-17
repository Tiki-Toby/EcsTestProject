using UnityEngine;

namespace GameEntities
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private string playerName;
        [SerializeField] private float velocity;
        
        public Transform PlayerTransform => playerTransform;
        public float Velocity => velocity;
        public string PlayerName => playerName;
    }
}