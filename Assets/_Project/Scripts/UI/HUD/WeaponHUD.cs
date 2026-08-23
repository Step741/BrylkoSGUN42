using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WeaponIconHUD : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField]
    private WeaponSwitcher weaponSwitcher;


    [Header("Weapon Icons")]
    [SerializeField]
    private Image pistolIcon;

    [SerializeField]
    private Image rifleIcon;

    [SerializeField]
    private Image shotgunIcon;

    [SerializeField]
    private Image grenadeLauncherIcon;

    [SerializeField]
    private Image railgunIcon;

    [SerializeField]
    private Image katanaIcon;


    [Header("Colors")]
    [SerializeField]
    private Color inactiveColor =
        new Color(1f, 1f, 1f, 0.35f);

    [SerializeField]
    private Color activeColor =
        Color.white;


    [Header("DOTween Animation")]
    [SerializeField]
    [Tooltip("Максимальное увеличение активной иконки.")]
    private float activeScale = 1.15f;

    [SerializeField]
    [Tooltip("Длительность увеличения иконки.")]
    private float punchDuration = 0.12f;

    [SerializeField]
    [Tooltip("Длительность возврата к обычному размеру.")]
    private float returnDuration = 0.15f;

    [SerializeField]
    [Tooltip("Длительность плавного изменения цвета.")]
    private float colorDuration = 0.15f;


    // =========================================================
    // UNITY
    // =========================================================

    private void OnEnable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.CurrentWeaponChanged +=
                OnWeaponChanged;
        }

        RefreshWeaponIcons(false);
    }


    private void Start()
    {
        RefreshWeaponIcons(false);
    }


    private void OnDisable()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.CurrentWeaponChanged -=
                OnWeaponChanged;
        }

        KillAllTweens();
    }


    // =========================================================
    // WEAPON CHANGED
    // =========================================================

    private void OnWeaponChanged(
        WeaponBase weapon)
    {
        UpdateIcons(weapon, true);
    }


    // =========================================================
    // REFRESH
    // =========================================================

    private void RefreshWeaponIcons(
        bool animate)
    {
        if (weaponSwitcher == null)
            return;

        UpdateIcons(
            weaponSwitcher.CurrentWeapon,
            animate
        );
    }


    // =========================================================
    // UPDATE ICONS
    // =========================================================

    private void UpdateIcons(
        WeaponBase currentWeapon,
        bool animate)
    {
        // Сначала выключаем выделение у всех.
        SetIconState(
            pistolIcon,
            false,
            animate
        );

        SetIconState(
            rifleIcon,
            false,
            animate
        );

        SetIconState(
            shotgunIcon,
            false,
            animate
        );

        SetIconState(
            grenadeLauncherIcon,
            false,
            animate
        );

        SetIconState(
            railgunIcon,
            false,
            animate
        );

        SetIconState(
            katanaIcon,
            false,
            animate
        );


        if (currentWeapon == null)
            return;


        // =====================================================
        // Определяем оружие по его реальному типу.
        // Порядок массива WeaponSwitcher больше не важен.
        // =====================================================

        if (currentWeapon is Pistol)
        {
            SetIconState(
                pistolIcon,
                true,
                animate
            );
        }
        else if (currentWeapon is Rifle)
        {
            SetIconState(
                rifleIcon,
                true,
                animate
            );
        }
        else if (currentWeapon is Shotgun)
        {
            SetIconState(
                shotgunIcon,
                true,
                animate
            );
        }
        else if (currentWeapon is GrenadeLauncher)
        {
            SetIconState(
                grenadeLauncherIcon,
                true,
                animate
            );
        }
        else if (currentWeapon is Railgun)
        {
            SetIconState(
                railgunIcon,
                true,
                animate
            );
        }
        else if (currentWeapon is Katana)
        {
            SetIconState(
                katanaIcon,
                true,
                animate
            );
        }
    }


    // =========================================================
    // ICON STATE
    // =========================================================

    private void SetIconState(
        Image icon,
        bool isActive,
        bool animate)
    {
        if (icon == null)
            return;


        // Останавливаем предыдущие анимации этой иконки.
        icon.DOKill();
        icon.transform.DOKill();


        // =====================================================
        // БЕЗ АНИМАЦИИ
        // Используется при загрузке HUD.
        // =====================================================

        if (!animate)
        {
            icon.color =
                isActive
                    ? activeColor
                    : inactiveColor;

            icon.transform.localScale =
                Vector3.one;

            return;
        }


        // =====================================================
        // НЕАКТИВНАЯ ИКОНКА
        // =====================================================

        if (!isActive)
        {
            icon
                .DOColor(
                    inactiveColor,
                    colorDuration
                )
                .SetEase(Ease.OutQuad);

            icon.transform
                .DOScale(
                    Vector3.one,
                    returnDuration
                )
                .SetEase(Ease.OutQuad);

            return;
        }


        // =====================================================
        // АКТИВНАЯ ИКОНКА
        // =====================================================

        Sequence sequence =
            DOTween.Sequence();

        // Сначала плавно меняем цвет.
        sequence.Join(
            icon
                .DOColor(
                    activeColor,
                    colorDuration
                )
                .SetEase(Ease.OutQuad)
        );

        // Увеличиваем.
        sequence.Append(
            icon.transform
                .DOScale(
                    Vector3.one * activeScale,
                    punchDuration
                )
                .SetEase(Ease.OutQuad)
        );

        // И мягко возвращаем обратно.
        sequence.Append(
            icon.transform
                .DOScale(
                    Vector3.one,
                    returnDuration
                )
                .SetEase(Ease.OutBack)
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void KillAllTweens()
    {
        KillIconTween(pistolIcon);
        KillIconTween(rifleIcon);
        KillIconTween(shotgunIcon);
        KillIconTween(grenadeLauncherIcon);
        KillIconTween(railgunIcon);
        KillIconTween(katanaIcon);
    }


    private void KillIconTween(
        Image icon)
    {
        if (icon == null)
            return;

        icon.DOKill();
        icon.transform.DOKill();
    }
}