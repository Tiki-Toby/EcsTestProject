using UnityEngine;

namespace GameConfigs
{
    [CreateAssetMenu(fileName = "PlayerDataConfig", menuName = "Configs/PlayerDataConfig", order = 2)]
    public class PlayerDataConfig : ScriptableObject
    {
        [SerializeField] private float velocity;

        public float Velocity => velocity;
    }
}