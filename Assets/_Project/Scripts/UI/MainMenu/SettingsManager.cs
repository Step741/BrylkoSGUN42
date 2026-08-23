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


    // =========================
    // PLAYER PREFS KEYS
    // =========================

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


    // =========================
    // MOUSE SENSITIVITY SETTINGS
    // =========================

    private const float MinMouseSensitivity =
        0.1f;

    private const float MaxMouseSensitivity =
        5f;

    private const float DefaultMouseSensitivity =
        2f;


    // =========================
    // UNITY
    // =========================

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


    // =========================
    // FIRST RUN
    // =========================

    private void InitializeSettings()
    {
        bool settingsAlreadyExist =
            PlayerPrefs.GetInt(
                SettingsInitializedKey,
                0
            ) == 1;


        if (settingsAlreadyExist)
            return;


        // -------------------------
        // AUDIO DEFAULTS
        // -------------------------

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            1f
        );

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            1f
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            1f
        );

        PlayerPrefs.SetFloat(
            UIVolumeKey,
            1f
        );

        PlayerPrefs.SetFloat(
            AmbientVolumeKey,
            1f
        );


        // -------------------------
        // MOUSE DEFAULTS
        // -------------------------

        PlayerPrefs.SetFloat(
            MouseSensitivityKey,
            DefaultMouseSensitivity
        );

        PlayerPrefs.SetInt(
            InvertYAxisKey,
            0
        );


        // -------------------------
        // INITIALIZATION FLAG
        // -------------------------

        PlayerPrefs.SetInt(
            SettingsInitializedKey,
            1
        );

        PlayerPrefs.Save();
    }


    // =========================
    // UPDATE OLD MOUSE SETTINGS
    // =========================

    private void UpdateMouseSensitivitySettings()
    {
        int currentVersion =
            PlayerPrefs.GetInt(
                MouseSensitivityVersionKey,
                0
            );


        // Одноразово заменяем старое
        // сохранённое значение чувствительности
        // на новое значение по умолчанию.
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


    // =========================
    // VOLUME
    // =========================

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


    // =========================
    // MOUSE SENSITIVITY
    // =========================

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

        Debug.Log(
        "SAVED MOUSE SENSITIVITY: " +
        PlayerPrefs.GetFloat(
            MouseSensitivityKey
        )
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


    // =========================
    // INVERT Y AXIS
    // =========================

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
        return PlayerPrefs.GetInt(
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


    // =========================
    // LOAD SETTINGS
    // =========================

    private void LoadSettings()
    {
        float master =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        float music =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                1f
            );

        float sfx =
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                1f
            );

        float ui =
            PlayerPrefs.GetFloat(
                UIVolumeKey,
                1f
            );

        float ambient =
            PlayerPrefs.GetFloat(
                AmbientVolumeKey,
                1f
            );


        float sensitivity =
            GetMouseSensitivity();


        bool invertY =
            GetInvertYAxis();


        // -------------------------
        // APPLY AUDIO
        // -------------------------

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


        // -------------------------
        // APPLY UI
        // -------------------------

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


    // =========================
    // MIXER VOLUME
    // =========================

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


    // =========================
    // SAVE FLOAT
    // =========================

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