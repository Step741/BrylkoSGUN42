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
        // ==========================================
        // CAMERA
        // ==========================================

        Container
            .BindInstance(mainCamera)
            .AsSingle();


        // ==========================================
        // PLAYER TARGET
        // ==========================================

        Container
            .Bind<PlayerTarget>()
            .AsSingle()
            .WithArguments(playerTransform);


        // ==========================================
        // ENEMY AI
        // ==========================================

        Container
            .Bind<EnemyStateMachine>()
            .AsTransient();


        // ==========================================
        // ENEMY FACTORY
        // ==========================================

        Container
            .Bind<IEnemyFactory>()
            .To<EnemyFactory>()
            .AsSingle();


        // ==========================================
        // PICKUP FACTORY
        // ==========================================

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


        // ==========================================
        // SOUND SERVICE
        // ==========================================

        Container
            .Bind<ISoundService>()
            .To<SoundService>()
            .FromInstance(soundService)
            .AsSingle();


        // ==========================================
        // UI SOUND LIBRARY
        // ==========================================

        Container
            .BindInstance(uiSoundLibrary)
            .AsSingle();
    }
}