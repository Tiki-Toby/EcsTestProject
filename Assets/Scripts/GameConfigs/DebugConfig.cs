using UnityEngine;

namespace GameConfigs
{
    [CreateAssetMenu(fileName = "DebugConfig", menuName = "Configs/DebugConfig", order = 5)]
    public class DebugConfig : ScriptableObject
    {
        [SerializeField] private bool isDebugMovingNeed;
        [SerializeField] private bool isDebugTriggerNeed;

        public bool IsDebugMovingNeed => isDebugMovingNeed;
        public bool IsDebugTriggerNeed => isDebugTriggerNeed;
    }
}