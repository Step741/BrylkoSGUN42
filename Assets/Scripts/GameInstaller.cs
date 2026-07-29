using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LayerMask collectibleLayer;

    public override void InstallBindings()
    {
        Container.BindInstance(_camera).AsSingle();
        Container.BindInstance(collectibleLayer).WithId("CollectibleLayer");
        Container.Bind<IItemFinder>().To<ItemFinder>().AsSingle();
    }
}
