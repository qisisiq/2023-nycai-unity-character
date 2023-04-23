namespace RPGCharacterAnims.Lookups
{
	/// <summary>
	/// Enum to use with the "Weapon" parameter of the animator. To convert from a Weapon number,
	/// use WeaponExtensions.ToAnimatorWeapon.
	///
	/// Two-handed weapons have a 1:1 relationship with this enum, but all one-handed weapons use
	/// ARMED.
	/// </summary>
	public enum AnimatorWeapon
    {
        RELAX = -1,
        UNARMED = 0,
        TWOHANDSWORD = 1,
        TWOHANDSPEAR = 2,
        TWOHANDAXE = 3,
        TWOHANDBOW = 4,
        TWOHANDCROSSBOW = 5,
        STAFF = 6,
        ARMED = 7,
        RIFLE = 18,
    }
}