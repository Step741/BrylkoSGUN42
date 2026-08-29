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
    private float ammoPunchScale = 0.08f;

    [SerializeField]
    private float ammoPunchDuration = 0.18f;

    [Header("Railgun Heat DOTween Animation")]

    [SerializeField]
    private float heatAnimationDuration = 0.15f;

    [SerializeField]
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

    private float lastHeat = -1f;

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
            return;


        if (ammoText == null)
            return;


        if (railgunHeatBar == null)
            return;


        if (
            railgunHeatRoot == null &&
            railgunHeatBar != null
        )
        {
            railgunHeatRoot =
                railgunHeatBar.gameObject;
        }


        railgunHeatBar.minValue = 0f;

        railgunHeatBar.maxValue = 1f;


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

    private void OnDestroy()
    {
        KillAllTweens();
    }

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

        if (currentWeapon is Railgun railgun)
        {
            currentRailgun =
                railgun;


            HideAmmoText();

            ShowRailgunHeat();


            float currentHeat =
                currentRailgun.HeatNormalized;


            lastHeat =
                currentHeat;


            SetRailgunHeatInstant(
                currentHeat
            );


            return;
        }

        if (currentWeapon is Katana)
        {
            HideAmmoText();

            HideRailgunHeat();

            return;
        }

        ShowAmmoText();

        HideRailgunHeat();


        currentWeapon.AmmoChanged +=
            OnAmmoChanged;


        UpdateAmmoUI(
            currentWeapon.CurrentAmmo,
            currentWeapon.ReserveAmmo,
            false
        );
    }

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

        ammoText.transform.DOKill();

        ammoText.transform.localScale =
            Vector3.one;

        ammoText.text =
            $"{currentAmmo} / {reserveAmmo}";

        if (!animate)
            return;

        ammoText.transform
            .DOPunchScale(
                Vector3.one *
                ammoPunchScale,

                ammoPunchDuration,

                1,

                0.5f
            )
            .SetLink(
                ammoText.gameObject
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

    private void AnimateRailgunHeat(
        float targetHeat)
    {
        if (railgunHeatBar == null)
            return;


        Color targetColor =
            GetHeatColor(
                targetHeat
            );


        railgunHeatBar.DOKill();


        railgunHeatBar
            .DOValue(
                targetHeat,
                heatAnimationDuration
            )
            .SetEase(
                Ease.OutQuad
            )
            .SetLink(
                railgunHeatBar.gameObject
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
                )
                .SetLink(
                    railgunHeatFill.gameObject
                );
        }
    }

    private Color GetHeatColor(
        float heatNormalized)
    {
        if (heatNormalized <= 0.5f)
        {
            return lowHeatColor;
        }


        if (heatNormalized <= 0.75f)
        {
            return mediumHeatColor;
        }


        return highHeatColor;
    }

    //CLEANUP
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