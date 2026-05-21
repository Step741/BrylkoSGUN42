using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private Camera _camera;

    public override void InstallBindings()
    {
        Container.BindInstance(_camera).AsSingle();
    }
}
