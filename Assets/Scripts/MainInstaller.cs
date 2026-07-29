using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField]
    private LayerMask collectibleLayer;

    public override void InstallBindings()
    {
        Container.Bind<SingleController>().AsSingle();
        Container.BindInterfacesTo<MultiplayerController>().AsSingle();

        Container.BindInstance(collectibleLayer).WithId("CollectibleLayer");
        Container.Bind<IItemFinder>().To<ItemFinder>().AsSingle();
    }
}

public class SingleController : IController
{

}

public class MultiplayerController : IController
{

}

public interface IController
{

}
