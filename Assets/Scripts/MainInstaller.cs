using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SingleController>().AsSingle();
        Container.BindInterfacesTo<MultiplayerController>().AsSingle();
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
