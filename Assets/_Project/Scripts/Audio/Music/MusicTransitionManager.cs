public static class MusicTransitionManager
{
    public static bool IsCrossfading { get; private set; }

    public static bool IsReturningToMenu { get; private set; }

    public static void StartGameTransition(
        float fadeDuration = 1.5f
    )
    {
        IsCrossfading = true;
        IsReturningToMenu = false;

        if (MainMenuMusic.Instance != null)
        {
            MainMenuMusic.Instance
                .FadeOutAndDestroy(fadeDuration);
        }
    }

    public static void StartMenuTransition(
        GameMusic gameMusic,
        float fadeDuration = 1.5f
    )
    {
        IsCrossfading = true;
        IsReturningToMenu = true;

        if (gameMusic != null)
        {
            gameMusic
                .FadeOutAndDestroy(fadeDuration);
        }
    }

    public static void CompleteTransition()
    {
        IsCrossfading = false;
        IsReturningToMenu = false;
    }
}