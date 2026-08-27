using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;

public class AmmoHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;

    [SerializeField]
    private TMP_Text ammoText;


    [Header("Railgun Heat")]
    [SerializeField]
    private Slider railgunHeatBar;

    [SerializeField]
    private GameObject railgunHeatRoot;


    [Header("Ammo DOTween Animation")]
    [SerializeField]
    [Tooltip("Сила лёгкого эффекта при изменении патронов.")]
    private float ammoPunchScale = 0.08f;

    [SerializeField]
    [Tooltip("Длительность анимации патронов.")]
    private float ammoPunchDuration = 0.18f;


    [Header("Railgun Heat DOTween Animation")]
    [SerializeField]
    [Tooltip("Длительность анимации изменения перегрева.")]
    private float heatAnimationDuration = 0.15f;

    [SerializeField]
    [Tooltip("Длительность плавной смены цвета перегрева.")]
    private float heatColorDuration = 0.12f;

    [SerializeField]
    private Color lowHeatColor = Color.green;

    [SerializeField]
    private Color mediumHeatColor = Color.yellow;

    [SerializeField]
    private Color highHeatColor = Color.red;


    private WeaponBase currentWeapon;
    private Railgun currentRailgun;

    private Image railgunHeatFill;


    // Последнее значение, которое уже было
    // передано в HUD.
    private float lastHeat = -1f;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (
            railgunHeatBar != null &&
            railgunHeatBar.fillRect != null
        )
        {
            railgunHeatFill =
                railgunHeatBar
                    .fillRect
                    .GetComponent<Image>();
        }
    }


    private void OnEnable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.CurrentWeaponChanged +=
                OnCurrentWeaponChanged;
        }
    }


    private void Start()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogError(
                "AmmoHUD: WeaponSwitcher reference is missing."
            );

            return;
        }


        if (ammoText == null)
        {
            Debug.LogError(
                "AmmoHUD: Ammo Text reference is missing."
            );

            return;
        }


        if (railgunHeatBar == null)
        {
            Debug.LogError(
                "AmmoHUD: Railgun Heat Bar reference is missing."
            );

            return;
        }


        if (
            railgunHeatRoot == null &&
            railgunHeatBar != null
        )
        {
            railgunHeatRoot =
                railgunHeatBar.gameObject;
        }


        if (railgunHeatBar != null)
        {
            railgunHeatBar.minValue = 0f;
            railgunHeatBar.maxValue = 1f;
        }


        OnCurrentWeaponChanged(
            weaponSwitcher.CurrentWeapon
        );
    }


    private void OnDisable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.CurrentWeaponChanged -=
                OnCurrentWeaponChanged;
        }


        UnsubscribeFromWeapon();
        UnsubscribeFromRailgun();

        KillAllTweens();

        HideRailgunHeat();
    }


    // =========================================================
    // UPDATE
    // Только проверяем изменение HeatNormalized.
    // DOTween не запускается каждый кадр.
    // =========================================================

    private void Update()
    {
        if (currentRailgun == null)
            return;


        float currentHeat =
            currentRailgun.HeatNormalized;


        if (
            Mathf.Approximately(
                currentHeat,
                lastHeat
            )
        )
        {
            return;
        }


        lastHeat =
            currentHeat;


        AnimateRailgunHeat(
            currentHeat
        );
    }


    // =========================================================
    // WEAPON CHANGED
    // =========================================================

    private void OnCurrentWeaponChanged(
        WeaponBase newWeapon)
    {
        UnsubscribeFromWeapon();
        UnsubscribeFromRailgun();

        KillAllTweens();

        currentWeapon =
            newWeapon;


        if (currentWeapon == null)
        {
            ClearAmmoUI();

            HideAmmoText();
            HideRailgunHeat();

            return;
        }


        // =====================================================
        // РЕЛЬСОТРОН
        // =====================================================

        if (currentWeapon is Railgun railgun)
        {
            currentRailgun =
                railgun;


            HideAmmoText();
            ShowRailgunHeat();


            // Получаем текущее значение
            // без стартовой анимации.
            float currentHeat =
                currentRailgun.HeatNormalized;


            lastHeat =
                currentHeat;


            SetRailgunHeatInstant(
                currentHeat
            );


            return;
        }


        // =====================================================
        // KATANA
        // =====================================================

        if (currentWeapon is Katana)
        {
            HideAmmoText();
            HideRailgunHeat();

            return;
        }


        // =====================================================
        // ОБЫЧНОЕ ОРУЖИЕ
        // =====================================================

        ShowAmmoText();
        HideRailgunHeat();


        currentWeapon.AmmoChanged +=
            OnAmmoChanged;


        // Первичное отображение без анимации.
        UpdateAmmoUI(
            currentWeapon.CurrentAmmo,
            currentWeapon.ReserveAmmo,
            false
        );
    }


    // =========================================================
    // UNSUBSCRIBE
    // =========================================================

    private void UnsubscribeFromWeapon()
    {
        if (currentWeapon == null)
            return;


        currentWeapon.AmmoChanged -=
            OnAmmoChanged;


        currentWeapon = null;
    }


    private void UnsubscribeFromRailgun()
    {
        currentRailgun = null;

        lastHeat = -1f;
    }


    // =========================================================
    // AMMO
    // =========================================================

    private void OnAmmoChanged(
        int currentAmmo,
        int reserveAmmo)
    {
        UpdateAmmoUI(
            currentAmmo,
            reserveAmmo,
            true
        );
    }


    private void UpdateAmmoUI(
        int currentAmmo,
        int reserveAmmo,
        bool animate)
    {
        if (ammoText == null)
            return;


        ammoText.text =
            $"{currentAmmo} / {reserveAmmo}";


        if (!animate)
        {
            ammoText.transform.localScale =
                Vector3.one;

            return;
        }


        // Останавливаем предыдущую анимацию,
        // если игрок быстро стреляет.
        ammoText.transform.DOKill();


        ammoText.transform
            .DOPunchScale(
                Vector3.one * ammoPunchScale,
                ammoPunchDuration,
                1,
                0.5f
            );
    }


    private void ClearAmmoUI()
    {
        if (ammoText == null)
            return;


        ammoText.transform.DOKill();

        ammoText.transform.localScale =
            Vector3.one;


        ammoText.text =
            "— / —";
    }


    // =========================================================
    // AMMO TEXT
    // =========================================================

    private void ShowAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(
                true
            );
        }
    }


    private void HideAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.transform.DOKill();

            ammoText.transform.localScale =
                Vector3.one;


            ammoText.gameObject.SetActive(
                false
            );
        }
    }


    // =========================================================
    // RAILGUN HEAT
    // =========================================================

    private void ShowRailgunHeat()
    {
        if (railgunHeatRoot != null)
        {
            railgunHeatRoot.SetActive(
                true
            );
        }
    }


    private void HideRailgunHeat()
    {
        if (railgunHeatRoot != null)
        {
            railgunHeatRoot.SetActive(
                false
            );
        }
    }


    // =========================================================
    // SET HEAT INSTANT
    // Используется только при переключении
    // на рейлган.
    // =========================================================

    private void SetRailgunHeatInstant(
        float heat)
    {
        if (railgunHeatBar == null)
            return;


        railgunHeatBar.DOKill();

        if (railgunHeatFill != null)
        {
            railgunHeatFill.DOKill();
        }


        railgunHeatBar.value =
            heat;


        if (railgunHeatFill != null)
        {
            railgunHeatFill.color =
                GetHeatColor(
                    heat
                );
        }
    }


    // =========================================================
    // DOTWEEN RAILGUN HEAT
    // =========================================================

    private void AnimateRailgunHeat(
        float targetHeat)
    {
        if (railgunHeatBar == null)
            return;


        Color targetColor =
            GetHeatColor(
                targetHeat
            );


        // Убиваем предыдущую анимацию,
        // чтобы при быстром изменении
        // перегрева tween'ы не наслаивались.
        railgunHeatBar.DOKill();


        railgunHeatBar
            .DOValue(
                targetHeat,
                heatAnimationDuration
            )
            .SetEase(
                Ease.OutQuad
            );


        if (railgunHeatFill != null)
        {
            railgunHeatFill.DOKill();


            railgunHeatFill
                .DOColor(
                    targetColor,
                    heatColorDuration
                )
                .SetEase(
                    Ease.OutQuad
                );
        }
    }


    // =========================================================
    // HEAT COLOR
    // =========================================================

    private Color GetHeatColor(
        float heatNormalized)
    {
        // 0–2 выстрела
        if (heatNormalized <= 0.5f)
        {
            return lowHeatColor;
        }


        // 3-й выстрел
        if (heatNormalized <= 0.75f)
        {
            return mediumHeatColor;
        }


        // 4-й выстрел и критический перегрев
        return highHeatColor;
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillAllTweens()
    {
        if (ammoText != null)
        {
            ammoText.transform.DOKill();

            ammoText.transform.localScale =
                Vector3.one;
        }


        if (railgunHeatBar != null)
        {
            railgunHeatBar.DOKill();
        }


        if (railgunHeatFill != null)
        {
            railgunHeatFill.DOKill();
        }
    }
}