using System;

namespace RPGCharacterAnims.Lookups
{
    public class WeaponGroupings
    {
        public static Weapon[] LeftHandedWeapons = new Weapon[] {
            Weapon.Shield,
            Weapon.LeftSword,
            Weapon.LeftMace,
            Weapon.LeftDagger,
            Weapon.LeftItem,
            Weapon.LeftPistol,
        };

        public static Weapon[] RightHandedWeapons = new Weapon[] {
            Weapon.Unarmed,
            Weapon.RightSword,
            Weapon.RightMace,
            Weapon.RightDagger,
            Weapon.RightItem,
            Weapon.RightPistol,
            Weapon.RightSpear,
        };

        public static Weapon[] TwoHandedWeapons = new Weapon[] {
            Weapon.TwoHandSword,
            Weapon.TwoHandSpear,
            Weapon.TwoHandAxe,
            Weapon.TwoHandStaff,
            Weapon.TwoHandBow,
            Weapon.TwoHandCrossbow,
            Weapon.Rifle,
        };

        public static Tuple<Weapon, Weapon>[] LeftRightWeaponPairs = new Tuple<Weapon, Weapon>[] {
            Tuple.Create(Weapon.LeftSword, Weapon.RightSword),
            Tuple.Create(Weapon.LeftMace, Weapon.RightMace),
            Tuple.Create(Weapon.LeftDagger, Weapon.RightDagger),
            Tuple.Create(Weapon.LeftItem, Weapon.RightItem),
            Tuple.Create(Weapon.LeftPistol, Weapon.RightPistol),
            Tuple.Create(Weapon.Shield, Weapon.RightSpear),
        };
    }
}