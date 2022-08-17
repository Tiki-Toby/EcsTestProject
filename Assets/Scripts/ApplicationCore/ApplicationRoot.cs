using GameEntities;
using UnityEngine;

namespace ApplicationCore
{
    public class ApplicationRoot : MonoBehaviour
    {
        [SerializeField] private WorldView worldView;
        
        private GameCore gameCore;

        private void Awake()
        {
            gameCore = new GameCore(worldView);
        }

        private void Update()
        {
            gameCore.Run();
        }

        private void OnDestroy()
        {
            gameCore.Destroy();
        }
    }
}
