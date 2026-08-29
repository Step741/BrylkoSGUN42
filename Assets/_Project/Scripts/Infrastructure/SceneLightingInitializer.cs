using UnityEngine;

public class SceneLightingInitializer : MonoBehaviour
{
    private void Start()
    {
        DynamicGI.UpdateEnvironment();
    }
}