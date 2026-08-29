using UnityEngine;
using UnityEngine.Audio;

public class AudioSnapshotController : MonoBehaviour
{
    [Header("Audio Mixer Snapshots")]

    [SerializeField]
    private AudioMixerSnapshot outdoorSnapshot;

    [SerializeField]
    private AudioMixerSnapshot indoorSnapshot;


    [Header("Transition Settings")]

    [SerializeField]
    [Min(0f)]
    private float transitionTime = 0.5f;


    public static AudioSnapshotController Instance
    {
        get;
        private set;
    }


    private int activeIndoorZones;


    public bool IsIndoor =>
        activeIndoorZones > 0;


    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);

            return;
        }


        Instance = this;
    }


    private void Start()
    {
        activeIndoorZones = 0;

        SetOutdoorImmediate();
    }

    public void EnterIndoorZone()
    {
        activeIndoorZones++;


        if (activeIndoorZones == 1)
        {
            TransitionToIndoor();
        }
    }


    public void ExitIndoorZone()
    {
        activeIndoorZones--;


        if (activeIndoorZones < 0)
        {
            activeIndoorZones = 0;
        }


        if (activeIndoorZones == 0)
        {
            TransitionToOutdoor();
        }
    }

    public void SetIndoor()
    {
        activeIndoorZones = 1;

        TransitionToIndoor();
    }


    public void SetOutdoor()
    {
        activeIndoorZones = 0;

        TransitionToOutdoor();
    }


    private void TransitionToIndoor()
    {
        if (indoorSnapshot == null)
        {
            Debug.LogWarning(
                $"{name}: Indoor Snapshot is not assigned."
            );

            return;
        }


        indoorSnapshot.TransitionTo(
            transitionTime
        );
    }


    private void TransitionToOutdoor()
    {
        if (outdoorSnapshot == null)
        {
            Debug.LogWarning(
                $"{name}: Outdoor Snapshot is not assigned."
            );

            return;
        }


        outdoorSnapshot.TransitionTo(
            transitionTime
        );
    }

    public void SetIndoorImmediate()
    {
        activeIndoorZones = 1;


        if (indoorSnapshot == null)
            return;


        indoorSnapshot.TransitionTo(
            0f
        );
    }


    public void SetOutdoorImmediate()
    {
        activeIndoorZones = 0;


        if (outdoorSnapshot == null)
            return;


        outdoorSnapshot.TransitionTo(
            0f
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}