using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Header("Camera")]
    [SerializeField]
    private Camera mainCamera;


    [Header("Player")]
    [SerializeField]
    private Transform playerTransform;


    [Header("Pickups")]
    [SerializeField]
    private PickupFactoryConfig pickupFactoryConfig;


    [Header("Audio")]
    [SerializeField]
    private SoundService soundService;

    [SerializeField]
    private UISoundLibrary uiSoundLibrary;


    public override void InstallBindings()
    {
        Container
            .BindInstance(mainCamera)
            .AsSingle();

        Container
            .Bind<PlayerTarget>()
            .AsSingle()
            .WithArguments(playerTransform);

        Container
            .Bind<EnemyStateMachine>()
            .AsTransient();

        Container
            .Bind<IEnemyFactory>()
            .To<EnemyFactory>()
            .AsSingle();


        Container
            .Bind<PickupFactoryConfig>()
            .FromInstance(pickupFactoryConfig)
            .AsSingle();

        Container
            .Bind<IPickupFactory>()
            .To<PickupFactory>()
            .AsSingle();

        Container
            .Bind<DropTable>()
            .AsTransient();

        Container
            .Bind<ISoundService>()
            .To<SoundService>()
            .FromInstance(soundService)
            .AsSingle();

        Container
            .BindInstance(uiSoundLibrary)
            .AsSingle();
    }
}