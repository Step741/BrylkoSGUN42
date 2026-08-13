public interface IWeapon
{
    bool CanShoot { get; }

    bool Shoot();

    void Reload();

    void Equip();

    void Unequip();
}