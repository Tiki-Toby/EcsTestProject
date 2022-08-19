using UnityEngine;

namespace GameConfigs
{
    [CreateAssetMenu(fileName = "GameConfigsPool", menuName = "Configs/GameConfigsPool", order = 1)]
    public class GameConfigsReceiver : ScriptableObject
    {
        [SerializeField] private PlayerDataConfig playerDataConfig;
        [SerializeField] private CameraDataConfig cameraDataConfig;
        [SerializeField] private SceneDataConfig[] sceneDataConfigs;
        [SerializeField] private DebugConfig debugConfig;

        public PlayerDataConfig PlayerDataConfig => playerDataConfig;
        public CameraDataConfig CameraDataConfig => cameraDataConfig;
        public DebugConfig DebugConfig => debugConfig;

        public SceneDataConfig GetSceneDataConfigByIndex(int index) => sceneDataConfigs[index];
    }
}