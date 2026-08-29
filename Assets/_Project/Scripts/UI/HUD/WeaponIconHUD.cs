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
    private float activeScale = 1.15f;

    [SerializeField]
    private float punchDuration = 0.12f;

    [SerializeField]
    private float returnDuration = 0.15f;

    [SerializeField]
    private float colorDuration = 0.15f;

    private Sequence pistolSequence;
    private Sequence rifleSequence;
    private Sequence shotgunSequence;
    private Sequence grenadeLauncherSequence;
    private Sequence railgunSequence;
    private Sequence katanaSequence;

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


    private void OnDestroy()
    {
        KillAllTweens();
    }

    private void OnWeaponChanged(
        WeaponBase weapon)
    {
        UpdateIcons(
            weapon,
            true
        );
    }

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

    private void UpdateIcons(
        WeaponBase currentWeapon,
        bool animate)
    {
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

    private void SetIconState(
        Image icon,
        bool isActive,
        bool animate)
    {
        if (icon == null)
            return;

        KillIconTween(icon);

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

        if (!isActive)
        {
            icon
                .DOColor(
                    inactiveColor,
                    colorDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
                .SetLink(
                    icon.gameObject
                );


            icon.transform
                .DOScale(
                    Vector3.one,
                    returnDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
                .SetLink(
                    icon.gameObject
                );

            return;
        }

        Sequence sequence =
            DOTween.Sequence()
                .SetLink(
                    icon.gameObject
                );

        sequence.Join(
            icon
                .DOColor(
                    activeColor,
                    colorDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );

        sequence.Append(
            icon.transform
                .DOScale(
                    Vector3.one * activeScale,
                    punchDuration
                )
                .SetEase(
                    Ease.OutQuad
                )
        );

        sequence.Append(
            icon.transform
                .DOScale(
                    Vector3.one,
                    returnDuration
                )
                .SetEase(
                    Ease.OutBack
                )
        );


        StoreSequence(
            icon,
            sequence
        );
    }

    private void StoreSequence(
        Image icon,
        Sequence sequence)
    {
        if (icon == pistolIcon)
        {
            pistolSequence = sequence;

            sequence.OnKill(
                () =>
                {
                    pistolSequence = null;
                }
            );
        }
        else if (icon == rifleIcon)
        {
            rifleSequence = sequence;

            sequence.OnKill(
                () =>
                {
                    rifleSequence = null;
                }
            );
        }
        else if (icon == shotgunIcon)
        {
            shotgunSequence = sequence;

            sequence.OnKill(
                () =>
                {
                    shotgunSequence = null;
                }
            );
        }
        else if (icon == grenadeLauncherIcon)
        {
            grenadeLauncherSequence = sequence;

            sequence.OnKill(
                () =>
                {
                    grenadeLauncherSequence = null;
                }
            );
        }
        else if (icon == railgunIcon)
        {
            railgunSequence = sequence;

            sequence.OnKill(
                () =>
                {
                    railgunSequence = null;
                }
            );
        }
        else if (icon == katanaIcon)
        {
            katanaSequence = sequence;

            sequence.OnKill(
                () =>
                {
                    katanaSequence = null;
                }
            );
        }
    }

    //CLEANUP
    private void KillAllTweens()
    {
        KillSequence(
            pistolSequence
        );

        pistolSequence = null;


        KillSequence(
            rifleSequence
        );

        rifleSequence = null;


        KillSequence(
            shotgunSequence
        );

        shotgunSequence = null;


        KillSequence(
            grenadeLauncherSequence
        );

        grenadeLauncherSequence = null;


        KillSequence(
            railgunSequence
        );

        railgunSequence = null;


        KillSequence(
            katanaSequence
        );

        katanaSequence = null;


        KillIconTween(
            pistolIcon
        );

        KillIconTween(
            rifleIcon
        );

        KillIconTween(
            shotgunIcon
        );

        KillIconTween(
            grenadeLauncherIcon
        );

        KillIconTween(
            railgunIcon
        );

        KillIconTween(
            katanaIcon
        );
    }


    private void KillSequence(
        Sequence sequence)
    {
        if (
            sequence != null &&
            sequence.IsActive()
        )
        {
            sequence.Kill();
        }
    }


    private void KillIconTween(
        Image icon)
    {
        if (icon == null)
            return;

        if (icon == pistolIcon)
        {
            KillSequence(
                pistolSequence
            );

            pistolSequence = null;
        }
        else if (icon == rifleIcon)
        {
            KillSequence(
                rifleSequence
            );

            rifleSequence = null;
        }
        else if (icon == shotgunIcon)
        {
            KillSequence(
                shotgunSequence
            );

            shotgunSequence = null;
        }
        else if (icon == grenadeLauncherIcon)
        {
            KillSequence(
                grenadeLauncherSequence
            );

            grenadeLauncherSequence = null;
        }
        else if (icon == railgunIcon)
        {
            KillSequence(
                railgunSequence
            );

            railgunSequence = null;
        }
        else if (icon == katanaIcon)
        {
            KillSequence(
                katanaSequence
            );

            katanaSequence = null;
        }


        icon.DOKill();
        icon.transform.DOKill();
    }
}