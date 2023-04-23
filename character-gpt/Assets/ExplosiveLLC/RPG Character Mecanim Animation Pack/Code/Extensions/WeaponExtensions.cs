using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims.Extensions
{
	public static class WeaponExtensions
	{
		/// <summary>
		/// Checks if the weapon is a right handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if right handed, false if not.</returns>
		public static bool IsRightHandedWeapon(this Weapon weapon)
		{
			return weapon == Weapon.RightSword || weapon == Weapon.RightMace || weapon == Weapon.RightDagger ||
				   weapon == Weapon.RightItem || weapon == Weapon.RightPistol || weapon == Weapon.RightSpear;
		}

		/// <summary>
		/// Checks if the weapon is a left handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if left handed, false if not.</returns>
		public static bool IsLeftHandedWeapon(this Weapon weapon)
		{
			return weapon == Weapon.LeftSword || weapon == Weapon.LeftMace || weapon == Weapon.LeftDagger ||
				   weapon == Weapon.LeftItem || weapon == Weapon.LeftPistol || weapon == Weapon.Shield;
		}

		/// <summary>
		/// Checks if the weapon is a 2 Handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 2 Handed, false if not.</returns>
		public static bool Is2HandedWeapon(this Weapon weapon)
		{
			return weapon == Weapon.Rifle || weapon == Weapon.TwoHandStaff || weapon == Weapon.TwoHandCrossbow ||
				   weapon == Weapon.TwoHandBow || weapon == Weapon.TwoHandAxe || weapon == Weapon.TwoHandSpear ||
				   weapon == Weapon.TwoHandSword;
		}

		/// <summary>
		/// Checks if the weapon is aimable.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if aimable, false if not.</returns>
		public static bool IsAimedWeapon(this Weapon weapon)
		{ return weapon == Weapon.Rifle || weapon == Weapon.TwoHandBow || weapon == Weapon.TwoHandCrossbow; }

		/// <summary>
		/// Checks if the weapon is equipped, i.e not Relaxing, or Unarmed.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True or false.</returns>
		public static bool HasEquippedWeapon(this Weapon weapon)
		{ return weapon != Weapon.Relax && weapon != Weapon.Unarmed; }

		/// <summary>
		/// Checks if the weapon is empty, i.e Relaxing, or Unarmed.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True or false.</returns>
		public static bool HasNoWeapon(this Weapon weapon)
		{ return weapon == Weapon.Relax || weapon == Weapon.Unarmed; }

		/// <summary>
		/// Checks if the weapon is a 1 Handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 1 Handed, false if not.</returns>
		public static bool Is1HandedWeapon(this Weapon weapon)
		{ return IsLeftHandedWeapon(weapon) || IsRightHandedWeapon(weapon); }

		/// <summary>
		/// Checks if the weapon is a castable weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check</param>
		/// <returns>True if castable, false if not</returns>
		public static bool IsCastableWeapon(this Weapon weapon)
		{
			return weapon != Weapon.Rifle && weapon != Weapon.TwoHandAxe && weapon != Weapon.TwoHandBow &&
				   weapon != Weapon.TwoHandCrossbow && weapon != Weapon.TwoHandSpear && weapon != Weapon.TwoHandSword;
		}

		/// <summary>
		/// Returns true if the weapon number can use IKHands.
		/// </summary>
		/// <param name="weapon">Weapon to test.</param>
		public static bool IsIKWeapon(this Weapon weapon)
		{
			return weapon == Weapon.TwoHandSword
				   || weapon == Weapon.TwoHandSpear
				   || weapon == Weapon.TwoHandAxe
				   || weapon == Weapon.TwoHandCrossbow
				   || weapon == Weapon.Rifle;
		}

		/// <summary>
		/// This converts the Weapon into AnimatorWeapon, which is used in the Animator component to determine the
		/// proper state to set the character into, because all 1 Handed weapons use the ARMED state. 2 Handed weapons,
		/// Unarmed, and Relax map directly from Weapon to AnimatorWeapon.
		/// </summary>
		/// <param name="weapon">Weapon to convert.</param>
		/// <returns></returns>
		public static AnimatorWeapon ToAnimatorWeapon(this Weapon weapon)
		{
			if (weapon == Weapon.Unarmed || weapon == Weapon.TwoHandAxe || weapon == Weapon.TwoHandBow
				|| weapon == Weapon.TwoHandCrossbow || weapon == Weapon.TwoHandSpear
				|| weapon == Weapon.TwoHandStaff  || weapon == Weapon.TwoHandSword || weapon == Weapon.Rifle)
			{ return ( AnimatorWeapon )weapon; }

			if (weapon == Weapon.Relax) { return AnimatorWeapon.RELAX; }

			return AnimatorWeapon.ARMED;
		}

		/// <summary>
		/// Checks if the animator weapon is a 1 Handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 1 Handed, false if not.</returns>
		public static bool Is1HandedAnimWeapon(this AnimatorWeapon weapon)
		{ return weapon == AnimatorWeapon.ARMED; }

		/// <summary>
		/// Checks if the animator weapon is a 2 Handed weapon.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 1 Handed, false if not.</returns>
		public static bool Is2HandedAnimWeapon(this AnimatorWeapon weapon)
		{
			return weapon == AnimatorWeapon.RIFLE || weapon == AnimatorWeapon.STAFF ||
				 weapon == AnimatorWeapon.TWOHANDAXE || weapon == AnimatorWeapon.TWOHANDBOW ||
				 weapon == AnimatorWeapon.TWOHANDSPEAR || weapon == AnimatorWeapon.TWOHANDSWORD ||
				 weapon == AnimatorWeapon.TWOHANDCROSSBOW;
		}

		/// <summary>
		/// Checks if the animator weapon is Unarmed or Relaxed.
		/// </summary>
		/// <param name="weapon">Weapon value to check.</param>
		/// <returns>True if 1 Handed, false if not.</returns>
		public static bool HasNoAnimWeapon(this AnimatorWeapon weapon)
		{ return weapon == AnimatorWeapon.UNARMED || weapon == AnimatorWeapon.RELAX; }
	}
}