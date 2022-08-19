using GameConfigs;
using GameEntities;
using UnityEngine;
using Zenject;

namespace ApplicationCore
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameConfigsReceiver gameConfigsReceiver;
        [SerializeField] private WorldView worldView;

        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromComponentsOn(worldView.Camera.gameObject).AsSingle();

            Container.Bind<GameConfigsReceiver>().FromInstance(gameConfigsReceiver).AsCached().NonLazy();
            Container.Bind<WorldView>().FromInstance(worldView).AsCached().NonLazy();
        }
    }
}