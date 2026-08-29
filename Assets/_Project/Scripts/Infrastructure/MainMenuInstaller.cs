using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [Header("Audio")]

    [SerializeField]
    private SoundService soundService;

    [SerializeField]
    private UISoundLibrary uiSoundLibrary;


    public override void InstallBindings()
    {
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