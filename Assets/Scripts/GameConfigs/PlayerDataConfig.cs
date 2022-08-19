using UnityEngine;

namespace GameConfigs
{
    [CreateAssetMenu(fileName = "PlayerDataConfig", menuName = "Configs/PlayerDataConfig", order = 2)]
    public class PlayerDataConfig : ScriptableObject
    {
        [SerializeField] private float velocity;
        [SerializeField] private float density;
        [SerializeField] private float friction;

        public float Velocity => velocity;
        public float Density => density;
        public float Friction => friction;
    }
}