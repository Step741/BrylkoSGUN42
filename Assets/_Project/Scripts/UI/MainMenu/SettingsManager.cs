using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]

    [SerializeField]
    private AudioMixer audioMixer;

    [Header("Volume Sliders")]

    [SerializeField]
    private Slider masterVolume;

    [SerializeField]
    private Slider musicVolume;

    [SerializeField]
    private Slider sfxVolume;

    [SerializeField]
    private Slider uiVolume;

    [SerializeField]
    private Slider ambientVolume;

    [Header("Mouse Settings")]

    [SerializeField]
    private Slider mouseSensitivity;

    [Header("Invert Y Axis")]

    [SerializeField]
    private Button invertYButton;

    [SerializeField]
    private TMP_Text invertYButtonText;

    private const string SettingsInitializedKey =
        "SettingsInitialized";

    private const string MasterVolumeKey =
        "MasterVolume";

    private const string MusicVolumeKey =
        "MusicVolume";

    private const string SFXVolumeKey =
        "SFXVolume";

    private const string UIVolumeKey =
        "UIVolume";

    private const string AmbientVolumeKey =
        "AmbientVolume";

    private const string MouseSensitivityKey =
        "MouseSensitivity";

    private const string MouseSensitivityVersionKey =
        "MouseSensitivityVersion";

    private const string InvertYAxisKey =
        "InvertYAxis";

    private const float DefaultMasterVolume =
        1f;

    private const float DefaultMusicVolume =
        0.6f;

    private const float DefaultSFXVolume =
        1f;

    private const float DefaultUIVolume =
        1f;

    private const float DefaultAmbientVolume =
        0.5f;

    private const float MinMouseSensitivity =
        0.1f;

    private const float MaxMouseSensitivity =
        5f;

    private const float DefaultMouseSensitivity =
        2f;

    private void Awake()
    {
        InitializeSettings();

        UpdateMouseSensitivitySettings();
    }


    private void Start()
    {
        LoadSettings();

        if (invertYButton != null)
        {
            invertYButton.onClick.AddListener(
                ToggleInvertYAxis
            );
        }
    }


    private void OnDestroy()
    {
        if (invertYButton != null)
        {
            invertYButton.onClick.RemoveListener(
                ToggleInvertYAxis
            );
        }
    }

    private void InitializeSettings()
    {
        bool settingsAlreadyExist =
            PlayerPrefs.GetInt(
                SettingsInitializedKey,
                0
            ) == 1;


        if (settingsAlreadyExist)
            return;

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            DefaultMasterVolume
        );


        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            DefaultMusicVolume
        );


        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            DefaultSFXVolume
        );


        PlayerPrefs.SetFloat(
            UIVolumeKey,
            DefaultUIVolume
        );


        PlayerPrefs.SetFloat(
            AmbientVolumeKey,
            DefaultAmbientVolume
        );

        PlayerPrefs.SetFloat(
            MouseSensitivityKey,
            DefaultMouseSensitivity
        );

        PlayerPrefs.SetInt(
            InvertYAxisKey,
            0
        );

        PlayerPrefs.SetInt(
            SettingsInitializedKey,
            1
        );


        PlayerPrefs.Save();
    }

    private void UpdateMouseSensitivitySettings()
    {
        int currentVersion =
            PlayerPrefs.GetInt(
                MouseSensitivityVersionKey,
                0
            );


        if (currentVersion < 1)
        {
            PlayerPrefs.SetFloat(
                MouseSensitivityKey,
                DefaultMouseSensitivity
            );

            PlayerPrefs.SetInt(
                MouseSensitivityVersionKey,
                1
            );

            PlayerPrefs.Save();
        }
    }

    public void SetMasterVolume(
        float volume)
    {
        SetMixerVolume(
            "MasterVolume",
            volume
        );

        SaveFloat(
            MasterVolumeKey,
            volume
        );
    }

    public void SetMusicVolume(
        float volume)
    {
        SetMixerVolume(
            "MusicVolume",
            volume
        );

        SaveFloat(
            MusicVolumeKey,
            volume
        );
    }

    public void SetSFXVolume(
        float volume)
    {
        SetMixerVolume(
            "SFXVolume",
            volume
        );

        SaveFloat(
            SFXVolumeKey,
            volume
        );
    }

    public void SetUIVolume(
        float volume)
    {
        SetMixerVolume(
            "UIVolume",
            volume
        );

        SaveFloat(
            UIVolumeKey,
            volume
        );
    }

    public void SetAmbientVolume(
        float volume)
    {
        SetMixerVolume(
            "AmbientVolume",
            volume
        );

        SaveFloat(
            AmbientVolumeKey,
            volume
        );
    }

    public void SetMouseSensitivity(
        float sensitivity)
    {
        sensitivity =
            Mathf.Clamp(
                sensitivity,
                MinMouseSensitivity,
                MaxMouseSensitivity
            );

        SaveFloat(
            MouseSensitivityKey,
            sensitivity
        );
    }


    public float GetMouseSensitivity()
    {
        return
            Mathf.Clamp(
                PlayerPrefs.GetFloat(
                    MouseSensitivityKey,
                    DefaultMouseSensitivity
                ),
                MinMouseSensitivity,
                MaxMouseSensitivity
            );
    }

    public void ToggleInvertYAxis()
    {
        bool currentValue =
            GetInvertYAxis();

        SetInvertYAxis(
            !currentValue
        );
    }


    public void SetInvertYAxis(
        bool value)
    {
        PlayerPrefs.SetInt(
            InvertYAxisKey,
            value
                ? 1
                : 0
        );

        PlayerPrefs.Save();

        UpdateInvertYButton(
            value
        );
    }


    public bool GetInvertYAxis()
    {
        return
            PlayerPrefs.GetInt(
                InvertYAxisKey,
                0
            ) == 1;
    }


    private void UpdateInvertYButton(
        bool value)
    {
        if (invertYButtonText == null)
            return;


        invertYButtonText.text =
            value
                ? "ON"
                : "OFF";
    }

    private void LoadSettings()
    {
        float master =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                DefaultMasterVolume
            );

        float music =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                DefaultMusicVolume
            );

        float sfx =
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                DefaultSFXVolume
            );

        float ui =
            PlayerPrefs.GetFloat(
                UIVolumeKey,
                DefaultUIVolume
            );

        float ambient =
            PlayerPrefs.GetFloat(
                AmbientVolumeKey,
                DefaultAmbientVolume
            );

        float sensitivity =
            GetMouseSensitivity();

        bool invertY =
            GetInvertYAxis();

        SetMixerVolume(
            "MasterVolume",
            master
        );

        SetMixerVolume(
            "MusicVolume",
            music
        );

        SetMixerVolume(
            "SFXVolume",
            sfx
        );

        SetMixerVolume(
            "UIVolume",
            ui
        );

        SetMixerVolume(
            "AmbientVolume",
            ambient
        );

        if (masterVolume != null)
        {
            masterVolume.SetValueWithoutNotify(
                master
            );
        }

        if (musicVolume != null)
        {
            musicVolume.SetValueWithoutNotify(
                music
            );
        }

        if (sfxVolume != null)
        {
            sfxVolume.SetValueWithoutNotify(
                sfx
            );
        }

        if (uiVolume != null)
        {
            uiVolume.SetValueWithoutNotify(
                ui
            );
        }

        if (ambientVolume != null)
        {
            ambientVolume.SetValueWithoutNotify(
                ambient
            );
        }

        if (mouseSensitivity != null)
        {
            mouseSensitivity.SetValueWithoutNotify(
                sensitivity
            );
        }

        UpdateInvertYButton(
            invertY
        );
    }

    private void SetMixerVolume(
        string parameterName,
        float volume)
    {
        if (audioMixer == null)
            return;


        float mixerValue;


        if (volume <= 0.0001f)
        {
            mixerValue =
                -80f;
        }
        else
        {
            mixerValue =
                Mathf.Log10(
                    volume
                ) * 20f;
        }


        audioMixer.SetFloat(
            parameterName,
            mixerValue
        );
    }

    private void SaveFloat(
        string key,
        float value)
    {
        PlayerPrefs.SetFloat(
            key,
            value
        );

        PlayerPrefs.Save();
    }
}