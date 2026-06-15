using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Controls controls = new Controls();

        Container.BindInstance(controls).AsSingle();
    }
}
