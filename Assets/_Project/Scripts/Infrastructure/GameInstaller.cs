using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform playerTransform;

    [Header("Pickups")]
    [SerializeField]
    private PickupFactoryConfig pickupFactoryConfig;


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
    }
}