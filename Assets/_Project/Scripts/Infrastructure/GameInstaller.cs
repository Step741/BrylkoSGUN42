using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform playerTransform;

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
    }
}