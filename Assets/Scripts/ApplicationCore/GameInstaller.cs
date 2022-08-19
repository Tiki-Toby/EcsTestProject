using GameConfigs;
using ECS.Server;
using UnityEngine;
using XFlow.EcsLite;
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

            Container.Bind<EcsWorld>().FromInstance(new EcsWorld()).AsCached();

            Container.Bind<EntityFactory>().FromNew().AsCached().NonLazy();
            Container.Bind<SceneManagerService>().FromNew().AsCached().NonLazy();
        }
    }
}