using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private Camera mainCamera;

    public override void InstallBindings()
    {
        Container.BindInstance(mainCamera).AsSingle();
    }
}