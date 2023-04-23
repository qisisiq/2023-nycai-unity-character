using System.Collections;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
    public class RPGCharacterWeaponController : MonoBehaviour
    {
		// Components.
        private RPGCharacterController rpgCharacterController;
        private Animator animator;
        private CoroutineQueue coroQueue;

		// Weapon Parameters.
		public bool singleAnimSwitch = true;
		public bool singleAnimSwitchDual = true;

		[Header("Debug Options")]
		public bool debugWalkthrough = true;
		public bool debugIncomingWeaponContext = true;
		public bool debugSwitchWeaponContext = true;
		public bool debugDoWeaponSwitch = true;
		public bool debugWeaponVisibility = true;
		public bool debugSetAnimator = true;

		[HideInInspector] bool isWeaponSwitching = false;
        [HideInInspector] bool dualSwitch;

		[Header("Weapon Models")]
		public GameObject twoHandAxe;
        public GameObject twoHandSword;
        public GameObject twoHandSpear;
        public GameObject twoHandBow;
        public GameObject twoHandCrossbow;
        public GameObject staff;
        public GameObject swordL;
        public GameObject swordR;
        public GameObject maceL;
        public GameObject maceR;
        public GameObject daggerL;
        public GameObject daggerR;
        public GameObject itemL;
        public GameObject itemR;
        public GameObject shield;
        public GameObject pistolL;
        public GameObject pistolR;
        public GameObject rifle;
        public GameObject spear;

        private void Awake()
        {
            coroQueue = new CoroutineQueue(1, StartCoroutine);
            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterController.SetHandler(HandlerTypes.SwitchWeapon, new SwitchWeapon());
            rpgCharacterController.SetHandler(HandlerTypes.Relax, new Relax());

            // Find the Animator component.
            animator = GetComponentInChildren<Animator>();

			// Character starts in Unarmed so hide all weapons.
            StartCoroutine(_HideAllWeapons(false, false));
        }

        private void Start()
        {
            // Listen for the animator's weapon switch event.
            var animatorEvents = animator.gameObject.GetComponent<RPGCharacterAnimatorEvents>();
            animatorEvents.OnWeaponSwitch.AddListener(WeaponSwitch);

            // Hide all weapons when the swim action begins.
            var swimHandler = rpgCharacterController.GetHandler(HandlerTypes.Swim);
            swimHandler.AddStartListener(HideAllWeapons);

			// Hide all weapons when the Crawl action begins.
			var crawlHandler = rpgCharacterController.GetHandler(HandlerTypes.Crawl);
			crawlHandler.AddStartListener(HideAllWeapons);

			// Hide all weapons when the ClimbLadder action begins.
			var climbLadderHandler = rpgCharacterController.GetHandler(HandlerTypes.ClimbLadder);
			climbLadderHandler.AddStartListener(HideAllWeapons);
		}

        /// <summary>
        /// Add a callback to the coroutine queue to be executed in sequence.
        /// </summary>
        /// <param name="callback">The action to call.</param>
        public void AddCallback(System.Action callback)
        { coroQueue.RunCallback(callback); }

        /// <summary>
        /// Queue a command to unsheath a weapon.
        /// </summary>
        /// <param name="weapon">Weapon to unsheath.</param>
        /// <param name="dual">Whether to unsheath the same weapon in the other hand.</param>
        public void UnsheathWeapon(Weapon weapon, bool dual)
        {
			if (debugWalkthrough) { Debug.Log("UnsheathWeapon:" + weapon + " dual:" + dual); }
            if (dual) { coroQueue.RunCallback(() => { dualSwitch = true; }); }
			coroQueue.Run(_UnSheathWeapon(weapon));
			if (dual) { coroQueue.RunCallback(() => { dualSwitch = false; }); }
        }

        /// <summary>
        /// Async method to unsheath a weapon.
        /// </summary>
        /// <param name="weapon">Weapon to unsheath.</param>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        private IEnumerator _UnSheathWeapon(Weapon weapon)
        {
			if (!dualSwitch) {
				if (debugWalkthrough) { Debug.Log($"UnsheathWeapon - weapon:{weapon} dualSwitch:{dualSwitch}"); }
			}

            isWeaponSwitching = true;

			var currentAnimatorWeapon = ( AnimatorWeapon )animator.GetInteger(AnimationParameters.Weapon);
			var currentWeaponType = (Weapon) animator.GetInteger(AnimationParameters.Weapon);

            // If Dual Switching.
            if (dualSwitch) {
				yield return _DualUnSheath(weapon);
				yield break;
            }

            // Switching to Unarmed from Relax.
            if (weapon == Weapon.Unarmed) {
				if (debugWalkthrough) { Debug.Log("Switching to Unarmed from Relax."); }
                DoWeaponSwitch(( int )AnimatorWeapon.RELAX, Weapon.Relax, AnimatorWeapon.RELAX, Side.None, false);

				// Wait for WeaponSwitch() to happen then update Animator.
				yield return new WaitForSeconds(0.1f);
				SetWeaponWithDebug(AnimatorWeapon.UNARMED, -2, Weapon.Unarmed, Weapon.Unarmed, Side.None);
            }

            // Switching to 2Handed weapon.
            else if (weapon.Is2HandedWeapon()) {
				if (debugWalkthrough) { Debug.Log($"Switching to 2Handed Weapon:{weapon}"); }

				// Switching from 2Handed weapon.
				if (currentWeaponType.Is2HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching from 2Handed weapon."); }
					DoWeaponSwitch(( int )AnimatorWeapon.UNARMED, weapon, weapon.ToAnimatorWeapon(), Side.Unchanged, false);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.75f);
					SetWeaponWithDebug(weapon.ToAnimatorWeapon(), -2, currentWeaponType, Weapon.Unarmed, Side.Unchanged);
				}
				else {
                    DoWeaponSwitch(( int )currentWeaponType, weapon, weapon.ToAnimatorWeapon(), Side.Unchanged, false);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.75f);
					SetWeaponWithDebug(weapon.ToAnimatorWeapon(), -2, weapon, Weapon.Unarmed, Side.Unchanged);
				}
			}

            // Switching to 1Handed weapon.
            else {
				if (debugWalkthrough) { Debug.Log("Switching to 1Handed weapon."); }

                // Left hand weapons.
                if (weapon.IsLeftHandedWeapon()) {

					if (debugWalkthrough) { Debug.Log("Left Handed weapon."); }

					// If Unsheathing Shield.
					if (weapon == Weapon.Shield) {
						if (debugWalkthrough) { Debug.Log("Unsheathing Shield."); }
						animator.SetInteger("LeftWeapon", 7);
					}
					DoWeaponSwitch(( int )AnimatorWeapon.ARMED, weapon, currentAnimatorWeapon, Side.Left, false);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, weapon, rpgCharacterController.rightWeapon, Side.Left);
				}

				// Right hand weapons.
				else if (weapon.IsRightHandedWeapon()) {

					if (debugWalkthrough) { Debug.Log("Right Handed weapon."); }

					DoWeaponSwitch(( int )AnimatorWeapon.ARMED, weapon, currentAnimatorWeapon, Side.Right, false);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, rpgCharacterController.leftWeapon, weapon, Side.Right);
				}
			}
        }

		/// <summary>
		/// Async method to unsheath both weapons at once. This is called by _UnSheath when dualSwitch is
		/// set to true. To do this all at once, just use the Unsheath method.
		/// </summary>
		/// <param name="weapon">Weapon number to unsheath.</param>
		/// <returns>IEnumerator for use with StartCoroutine.</returns>
		public IEnumerator _DualUnSheath(Weapon weapon)
		{
			if (debugWalkthrough) { Debug.Log($"_DualUnSheath:{weapon}"); }
			var currentAnimatorWeapon = ( AnimatorWeapon )animator.GetInteger(AnimationParameters.Weapon);

			DoWeaponSwitch(( int )AnimatorWeapon.ARMED, weapon, currentAnimatorWeapon, Side.Dual, false);

			yield return new WaitForSeconds(0.55f);
			SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, (Weapon)weapon -1, weapon, Side.Dual);
		}

		/// <summary>
		/// Queue a command to sheath the current weapon and switch to a new one.
		/// </summary>
		/// <param name="fromWeapon">Which weapon to sheath.</param>
		/// <param name="toWeapon">Target weapon if immediately unsheathing new weapon.</param>
		/// <param name="dual">Whether to sheath both weapons at once.</param>
		public void SheathWeapon(Weapon fromWeapon, Weapon toWeapon, bool dual)
        {
			if (debugWalkthrough) { Debug.Log($"SheathWeapon - fromWeapon:{fromWeapon}  toWeapon:{toWeapon} dual:{dual}"); }
            if (dual) { coroQueue.RunCallback(() => { dualSwitch = true; }); }
            coroQueue.Run(_SheathWeapon(fromWeapon, toWeapon));
            if (dual) { coroQueue.RunCallback(() => { dualSwitch = false; }); }
        }

        /// <summary>
        /// Async method to sheath the current weapon and switch to a new one.
        /// </summary>
        /// <param name="weaponToSheath">Which weapon to sheath.</param>
        /// <param name="weaponToUnsheath">Target weapon if immediately unsheathing a new weapon.</param>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        public IEnumerator _SheathWeapon(Weapon weaponToSheath, Weapon weaponToUnsheath)
        {
			if (!dualSwitch) {
				if (debugWalkthrough) { Debug.Log($"Sheath Weapon - weaponToSheath:{weaponToSheath}  weaponToUnsheath:{weaponToUnsheath}"); }
			}

            var currentWeaponType = (Weapon) animator.GetInteger(AnimationParameters.Weapon);
			var currentAnimatorWeapon = ( AnimatorWeapon )animator.GetInteger(AnimationParameters.Weapon);

			// Reset for animation events.
			isWeaponSwitching = true;

            // Use Dual switch.
            if (dualSwitch) {
                yield return _DualSheath(weaponToSheath, weaponToUnsheath);
                yield break;
            }

            // Set Side for 1Handed switching.
            if (weaponToSheath.IsLeftHandedWeapon()) { animator.SetSide(Side.Left); }
			else if (weaponToSheath.IsRightHandedWeapon()) { animator.SetSide(Side.Right); }

            // Putting away a weapon.
            if (weaponToUnsheath.HasNoWeapon()) {
				if (debugWalkthrough) { Debug.Log("Putting away a weapon."); }

				// Have at least 1 weapon.
				if (rpgCharacterController.rightWeapon.HasEquippedWeapon()
					|| rpgCharacterController.leftWeapon.HasEquippedWeapon()) {

                    // Sheath 1Handed weapon.
                    if (weaponToSheath.Is1HandedWeapon()) {

						// Switching to Unarmed or Relax.
						if (weaponToUnsheath.HasNoWeapon()) {
							if (rpgCharacterController.rightWeapon.HasEquippedWeapon()
								&& rpgCharacterController.leftWeapon.HasEquippedWeapon()) {
								if (debugWalkthrough) { Debug.Log("Sheathing 1 of 2 weapons."); }
								DoWeaponSwitch(( int )AnimatorWeapon.ARMED, weaponToSheath, AnimatorWeapon.ARMED, Side.Unchanged, true);
							}
							else {
								if (debugWalkthrough) { Debug.Log("Sheathing only 1Handed weapon."); }
								DoWeaponSwitch(( int )weaponToUnsheath, weaponToSheath, AnimatorWeapon.ARMED, Side.Unchanged, true);
							}
						}
						// Wait for WeaponSwitch() to happen, then update Animator.
						yield return new WaitForSeconds(0.55f);

						// Sheathing from Dual weapons.
						if (rpgCharacterController.hasDualWeapons) {

							// Set Left Weapon.
							if (weaponToSheath.IsLeftHandedWeapon())
							{ SetWeaponWithDebug(AnimatorWeapon.ARMED, -2, Weapon.Unarmed, rpgCharacterController.rightWeapon, Side.Right); }

							// Set Right Weapon.
							else if (weaponToSheath.IsRightHandedWeapon())
							{ SetWeaponWithDebug(AnimatorWeapon.ARMED, -2, rpgCharacterController.leftWeapon, Weapon.Unarmed, Side.Left); }
						}
						// Just switching 1 weapon.
						else { SetWeaponWithDebug(weaponToUnsheath.ToAnimatorWeapon(), -2, Weapon.Unarmed, Weapon.Unarmed, Side.None); }
					}
                    // Sheath 2Handed weapon.
                    else {
						if (debugWalkthrough) { Debug.Log("Sheath 2Handed weapon."); }
						DoWeaponSwitch(( int )weaponToUnsheath, weaponToSheath, currentAnimatorWeapon, Side.Unchanged, true);

						// Wait for WeaponSwitch() to happen then update Animator.
						yield return new WaitForSeconds(0.55f);
						SetWeaponWithDebug(weaponToUnsheath.ToAnimatorWeapon(), -2, Weapon.Unarmed, Weapon.Unarmed, Side.Unchanged);
					}
				}
                // Unarmed, switching to Relax.
                else {
					if (debugWalkthrough) { Debug.Log("Unarmed, switching to Relax."); }
					DoWeaponSwitch(( int )weaponToUnsheath, weaponToSheath, currentAnimatorWeapon, Side.None, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.1f);
					SetWeaponWithDebug(AnimatorWeapon.RELAX, -2, Weapon.Relax, Weapon.Relax, Side.None);
				}
			}
            // Switching to 2Handed Weapon.
            else if (weaponToUnsheath.Is2HandedWeapon()) {
				if (debugWalkthrough) { Debug.Log("Switching to 2Handed Weapon."); }

				// Switching from 1Handed Weapon(s).
				if (currentWeaponType.Is1HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switch from 1Handed Weapon."); }
                    DoWeaponSwitch(( int )AnimatorWeapon.UNARMED, weaponToSheath, AnimatorWeapon.ARMED, Side.Unchanged, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					SetWeaponWithDebug(AnimatorWeapon.UNARMED, -2, Weapon.Unarmed, Weapon.Unarmed, Side.Unchanged);
                }

                // Switching from 2Handed Weapon.
                else {
					if (debugWalkthrough) { Debug.Log("Switching from 2Handed Weapon."); }
					DoWeaponSwitch(( int )AnimatorWeapon.UNARMED, weaponToSheath, currentAnimatorWeapon, Side.Unchanged, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					SetWeaponWithDebug(AnimatorWeapon.UNARMED, -2, Weapon.Unarmed, Weapon.Unarmed, Side.Unchanged);
				}
			}
            // Switching to 1Handed weapons.
            else {
                // Switching from 2Handed weapons, go to Unarmed before next switch.
                if (currentWeaponType.Is2HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching from 2Handed weapons, go to Unarmed before next switch."); }
					DoWeaponSwitch((int)AnimatorWeapon.UNARMED, weaponToSheath, currentAnimatorWeapon, Side.None, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					SetWeaponWithDebug(AnimatorWeapon.UNARMED, -2, Weapon.Unarmed, Weapon.Unarmed, Side.None);
                }

				// Switching from 1Handed weapon.
				if (currentWeaponType.Is1HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching from 1Handed weapon."); }
					DoWeaponSwitch((int)AnimatorWeapon.ARMED, weaponToSheath, AnimatorWeapon.ARMED, Side.Unchanged, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					if (weaponToSheath.IsLeftHandedWeapon())
					{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int)AnimatorWeapon.ARMED, Weapon.Unarmed, rpgCharacterController.rightWeapon, Side.Unchanged); }
					else if (weaponToSheath.IsRightHandedWeapon())
					{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, rpgCharacterController.leftWeapon, Weapon.Unarmed, Side.Unchanged); }
				}
			}
        }

		/// <summary>
		/// Async method sheath both weapons at once. This is called by _Sheath when dualSwitch is
		/// set to true. To do this all at once, just use the Sheath method.
		/// </summary>
		/// <param name="weaponToSheath">Weapon number to unsheath.</param>
		/// <returns>IEnumerator for use with StartCoroutine.</returns>
		public IEnumerator _DualSheath(Weapon weaponToSheath, Weapon weaponToUnsheath)
		{
			if (debugWalkthrough) { Debug.Log($"DualSheath - weaponToSheath:{weaponToSheath}  weaponToUnsheath:{weaponToUnsheath}"); }

			// If switching to Relax from Unarmed.
			if (weaponToSheath == Weapon.Unarmed && weaponToUnsheath == Weapon.Relax) {
				if (debugWalkthrough) { Debug.Log("Switching to Relax from Unarmed."); }
				DoWeaponSwitch(-1, Weapon.Relax, AnimatorWeapon.RELAX, Side.Unchanged, true);

				// Wait for WeaponSwitch() to happen then update Animator.
				yield return new WaitForSeconds(0.55f);
				SetWeaponWithDebug(AnimatorWeapon.RELAX, (int)AnimatorWeapon.RELAX, Weapon.Unarmed, Weapon.Unarmed, Side.None);
			}
			// Sheath 1Handed weapons.
			else if (weaponToSheath.Is1HandedWeapon()) {
				if (debugWalkthrough) { Debug.Log("Sheath 1Handed weapons."); }

				// If switching to 2Handed weapon, goto Unarmed.
				if (weaponToUnsheath.Is2HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching to 2Handed weapon, goto Unarmed."); }
					DoWeaponSwitch(0, weaponToSheath, AnimatorWeapon.ARMED, Side.Dual, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					StartCoroutine(_HideAllWeapons(false, false));
					SetWeaponWithDebug(AnimatorWeapon.UNARMED, -2, Weapon.Unarmed, Weapon.Unarmed, Side.None);
				}
				// Switching to other 1Handed weapons.
				else if (weaponToUnsheath.Is1HandedWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching to other 1Handed weapons."); }
					DoWeaponSwitch((int)AnimatorWeapon.ARMED, weaponToSheath, AnimatorWeapon.ARMED, Side.Dual, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					StartCoroutine(_HideAllWeapons(false, false));
					SetWeaponWithDebug(AnimatorWeapon.UNARMED, ( int )AnimatorWeapon.UNARMED, Weapon.Unarmed, Weapon.Unarmed, Side.Dual);
				}
				// Switching to Unarmed/Relax.
				else if (weaponToUnsheath.HasNoWeapon()) {
					if (debugWalkthrough) { Debug.Log("Switching to Unarmed/Relax."); }
					DoWeaponSwitch(( int )weaponToUnsheath, weaponToSheath, AnimatorWeapon.ARMED, Side.Dual, true);

					// Wait for WeaponSwitch() to happen then update Animator.
					yield return new WaitForSeconds(0.55f);
					StartCoroutine(_HideAllWeapons(false, false));
					SetWeaponWithDebug(weaponToUnsheath.ToAnimatorWeapon(), -2, Weapon.Unarmed, Weapon.Unarmed, Side.None);
				}
			}
		}

		/// <summary>
		/// Sheaths a 1Handed weapon and Unsheaths a new one in 1 animation.
		/// </summary>
		/// <param name="weaponFrom">Existing weapon switching from.</param>
		/// <param name="weaponTo">Weapon switching to.</param>
		/// <param name="side">Weapon side.</param>
		public void SheathAndUnSheath(Weapon weaponFrom, Weapon weaponTo, Side side)
		{ StartCoroutine(_SheathAndUnSheath(weaponFrom, weaponTo, side)); }

		private IEnumerator _SheathAndUnSheath(Weapon weaponFrom, Weapon weaponTo, Side side)
		{
			if (debugWalkthrough) { Debug.Log($"SheathAndUnSheath - weaponFrom:{weaponFrom}  weaponTo:{weaponTo}  side:{ side}"); }

			var currentWeaponType = animator.GetInteger(AnimationParameters.Weapon);

			// Reset for animation events.
			isWeaponSwitching = true;

			DoWeaponSwitch(( int )AnimatorWeapon.ARMED, weaponTo, AnimatorWeapon.ARMED, side, true);

			// Wait for WeaponSwitch() to happen then update Animator.
			yield return new WaitForSeconds(0.5f);
			if (side == Side.Left)
			{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, weaponTo, rpgCharacterController.rightWeapon, side); }
			if (side == Side.Right)
			{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, rpgCharacterController.leftWeapon, weaponTo, side); }
		}

		/// <summary>
		/// Sheaths both 1Handed weapons and Unsheaths new ones in 1 animation.
		/// </summary>
		/// <param name="weaponFrom">Existing weapon switching from.</param>
		/// <param name="weaponTo">Weapon switching to.</param>
		/// <param name="side">Weapon side.</param>
		public void DualSheathAndUnSheath(Weapon weaponFrom, Weapon weaponTo, Side side)
		{ StartCoroutine(_DualSheathAndUnSheath(weaponFrom, weaponTo, side)); }

		private IEnumerator _DualSheathAndUnSheath(Weapon weaponFrom, Weapon weaponTo, Side side)
		{
			if (debugWalkthrough) { Debug.Log($"DualSheathAndUnSheath - weaponFrom:{weaponFrom}  weaponTo:{weaponTo}"); }

			var currentWeaponType = animator.GetInteger(AnimationParameters.Weapon);

			// Reset for animation events.
			isWeaponSwitching = true;

			DoWeaponSwitch(( int )AnimatorWeapon.ARMED, weaponTo, AnimatorWeapon.ARMED, side, true);

			// Wait for WeaponSwitch() to happen then update Animator.
			yield return new WaitForSeconds(0.55f);
			if (side == Side.Left)
			{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, weaponTo, rpgCharacterController.rightWeapon, side); }
			if (side == Side.Right)
			{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, rpgCharacterController.leftWeapon, weaponTo, side); }
			if (side == Side.Dual)
			{ SetWeaponWithDebug(AnimatorWeapon.ARMED, ( int )AnimatorWeapon.ARMED, weaponTo, weaponTo + 1, side); }
		}

        /// <summary>
        /// Switch to the weapon number instantly.
        /// </summary>
        /// <param name="weapon">Weapon to switch to.</param>
        public void InstantWeaponSwitch(Weapon weapon)
        { coroQueue.Run(_InstantWeaponSwitch(weapon)); }

		/// <summary>
		/// Async method to instant weapon switch.
		/// </summary>
		/// <param name="weaponNumber">Weapon number to switch to.</param>
		/// <returns>IEnumerator for use with StartCoroutine.</returns>
		/// /// <summary>
		public IEnumerator _InstantWeaponSwitch(Weapon weapon)
		{
			if (debugWalkthrough) { Debug.Log($"_InstantWeaponSwitch:{weapon}"); }
			animator.SetAnimatorTrigger(AnimatorTrigger.InstantSwitchTrigger);
			rpgCharacterController.SetIKOff();
			StartCoroutine(_HideAllWeapons(false, false));

			// 1Handed.
			if (weapon.Is1HandedWeapon()) {

				// Dual weapons.
				if (dualSwitch) {
					if (debugWalkthrough) { Debug.Log($"InstantSwitch DualWeapons:{weapon}"); }
					animator.SetInteger(AnimationParameters.Weapon, ( int )AnimatorWeapon.ARMED);
					StartCoroutine(_WeaponVisibility(weapon, true, true));
					animator.SetInteger(AnimationParameters.Side, ( int )Side.Dual);
				}
				// Single weapon.
				else {
					if (debugWalkthrough) { Debug.Log($"InstantSwitch to 1HandedWeapon:{weapon}"); }
					animator.SetInteger(AnimationParameters.Weapon, 7);
					animator.SetInteger(AnimationParameters.WeaponSwitch, 7);

					// Right weapon.
					if (weapon.IsRightHandedWeapon()) {
						if (debugWalkthrough) { Debug.Log($"Right Weapon:{weapon}"); }
						animator.SetInteger(AnimationParameters.RightWeapon, ( int )weapon);
						rpgCharacterController.rightWeapon = weapon;
						StartCoroutine(_WeaponVisibility(weapon, true, false));
						if (rpgCharacterController.hasLeftWeapon)
						{ animator.SetInteger(AnimationParameters.Side, ( int )Side.Dual); }
						else { animator.SetInteger(AnimationParameters.Side, ( int )Side.Right); }
					}
					// Left weapon.
					else if (weapon.IsLeftHandedWeapon()) {
						if (debugWalkthrough) { Debug.Log($"Left Weapon:{weapon}"); }
						animator.SetInteger(AnimationParameters.LeftWeapon, ( int )weapon);
						rpgCharacterController.leftWeapon = weapon;
						StartCoroutine(_WeaponVisibility(weapon, true, false));
						if (rpgCharacterController.hasRightWeapon)
						{ animator.SetInteger(AnimationParameters.Side, ( int )Side.Dual); }
						else { animator.SetInteger(AnimationParameters.Side, ( int )Side.Left); }
					}
				}
			}
			// 2Handed.
			else if (weapon.Is2HandedWeapon()) {
				if (debugWalkthrough) { Debug.Log($"InstantSwitch to 2HandedWeapon - weapon:{weapon}"); }

				// 2Handed weapons map directly to AnimatorWeapon.
				animator.SetInteger(AnimationParameters.Weapon, ( int )weapon);
				rpgCharacterController.rightWeapon = weapon;
				rpgCharacterController.leftWeapon = Weapon.Unarmed;
				animator.SetInteger(AnimationParameters.LeftWeapon, 0);
				animator.SetInteger(AnimationParameters.RightWeapon, ( int )weapon);
				StartCoroutine(_HideAllWeapons(false, false));
				StartCoroutine(_WeaponVisibility(weapon, true, false));

				if (weapon.IsIKWeapon())
				{ rpgCharacterController.SetIKOn(( Weapon )animator.GetInteger(AnimationParameters.Weapon)); }
			}
			// Switching to Unarmed or Relax which map directly.
			else {
				if (weapon == Weapon.Unarmed) {
					rpgCharacterController.rightWeapon = Weapon.Unarmed;
					rpgCharacterController.leftWeapon = Weapon.Unarmed;
					animator.SetInteger(AnimationParameters.Weapon, 0);
				}
				else {
					rpgCharacterController.rightWeapon = Weapon.Relax;
					rpgCharacterController.leftWeapon = Weapon.Relax;
					animator.SetInteger(AnimationParameters.Weapon, -1);
				}
				animator.SetInteger(AnimationParameters.LeftWeapon, 0);
				animator.SetInteger(AnimationParameters.RightWeapon, 0);
				animator.SetInteger(AnimationParameters.Side, 0);
			}
			yield return null;
		}

		/// <summary>
		/// Performs the weapon switch by setting Animator parameters then triggering Sheath or Unsheath animation.
		/// </summary>
		/// <param name="weaponSwitch">AnimatorWeapon switching from.</param>
		/// <param name="weapon">The weapon switching to.</param>
		/// <param name="animatorWeapon">Weapon enum switching to.</param>
		/// <param name="side">Weapon side. -1=Leave existing, 1=Left, 2=Right, 3=Dual</param>
		/// <param name="sheath">"sheath" or "unsheath".</param>
		private void DoWeaponSwitch(int weaponSwitch, Weapon weapon, AnimatorWeapon animatorWeapon, Side side, bool sheath)
		{
			if (debugDoWeaponSwitch)
			{ Debug.Log($"DO WEAPON SWITCH - weaponSwitch:{weaponSwitch}  weapon:{weapon}  animatorWeapon:{animatorWeapon}  side:{side}  sheath:{sheath}"); }

			// Lock character movement for switch unless has moving sheath/unsheath anims.
			rpgCharacterController.Lock(!rpgCharacterController.isMoving, true, true, 0f, 1f);

			// Set WeaponSwitch and Weapon.
			animator.SetInteger(AnimationParameters.WeaponSwitch, weaponSwitch);
			animator.SetInteger(AnimationParameters.Weapon, ( int )animatorWeapon);

			// Set LeftRight if applicable.
			if (side != Side.Unchanged) { animator.SetInteger(AnimationParameters.Side, ( int )side); }

			// Sheath.
			if (sheath) {
				animator.SetAnimatorTrigger(AnimatorTrigger.WeaponSheathTrigger);
				StartCoroutine(_WeaponVisibility(weapon, false, dualSwitch));

				// If using IKHands, trigger IK blend.
				if (rpgCharacterController.ikHands != null)
				{ rpgCharacterController.ikHands.BlendIK(false, 0f, 0.2f, weapon); }

			}
			// Unsheath.
			else {
				animator.SetAnimatorTrigger(AnimatorTrigger.WeaponUnsheathTrigger);
				StartCoroutine(_WeaponVisibility(weapon, true, dualSwitch));

				// If using IKHands and IK weapon, trigger IK blend.
				if (rpgCharacterController.ikHands != null && weapon.IsIKWeapon())
				{ rpgCharacterController.ikHands.BlendIK(true, 0.75f, 1, weapon); }
			}
		}

		/// <summary>
		/// Sets the animation state for weapons with debug option.
		/// </summary>
		/// <param name="animatorWeapon">Animator weapon number. Use AnimationData's AnimatorWeapon enum.</param>
		/// <param name="weaponSwitch">Weapon switch. -2 leaves parameter unchanged.</param>
		/// <param name="leftWeapon">Left weapon number. Use Weapon.cs enum.</param>
		/// <param name="rightWeapon">Right weapon number. Use Weapon.cs enum.</param>
		/// <param name="weaponSide">Weapon side: 0-None, 1-Left, 2-Right, 3-Dual.</param>
		private void SetWeaponWithDebug(AnimatorWeapon animatorWeapon, int weaponSwitch, Weapon leftWeapon, Weapon rightWeapon, Side weaponSide)
		{
			if (debugSetAnimator) { Debug.Log($"SET ANIMATOR - Weapon:{animatorWeapon}  WeaponSwitch:{weaponSwitch}  Lweapon:{leftWeapon}  Rweapon:{rightWeapon}  Weaponside:{weaponSide}"); }

			animator.SetWeapons(animatorWeapon, weaponSwitch, leftWeapon, rightWeapon, weaponSide);
		}

		/// <summary>
		/// Callback to use with Animator's WeaponSwitch event.
		/// </summary>
		public void WeaponSwitch()
        {
            if (isWeaponSwitching) { isWeaponSwitching = false; }
        }

        /// <summary>
        /// Helper method used by other weapon visibility methods to safely set a weapon object's visibility.
        /// This will work even if the object is not set in the component parameters.
        /// </summary>
        /// <param name="weaponObject">Weapon to update.</param>
        /// <param name="visibility">Visibility status.</param>
        public void SafeSetVisibility(GameObject weaponObject, bool visibility)
        {
            if (weaponObject != null) { weaponObject.SetActive(visibility); }
        }

        /// <summary>
        /// Hide all weapon objects and set the animator and the character controller to the unarmed state.
        /// </summary>
        public void HideAllWeapons()
        { StartCoroutine(_HideAllWeapons(false, true)); }

        /// <summary>
        /// Async method to all weapon objects and set the animator and the character controller to the unarmed state.
        /// </summary>
        /// <param name="timed">Whether to wait until a period of time to hide the weapon.</param>
        /// <param name="resetToUnarmed">Whether to reset the animator and the character controller to the unarmed state.</param>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        public IEnumerator _HideAllWeapons(bool timed, bool resetToUnarmed)
        {
            if (timed) { while (!isWeaponSwitching) { yield return null; } }

            // Reset to Unarmed if not in Relax.
            if (resetToUnarmed && !rpgCharacterController.isRelaxed) {
                animator.SetInteger(AnimationParameters.Weapon, 0);
                rpgCharacterController.rightWeapon = Weapon.Unarmed;
                rpgCharacterController.leftWeapon = Weapon.Unarmed;
                StartCoroutine(_WeaponVisibility(rpgCharacterController.leftWeapon, false, true));
                animator.SetInteger(AnimationParameters.RightWeapon, 0);
                animator.SetInteger(AnimationParameters.LeftWeapon, 0);
                animator.SetSide(Side.None);
            }
            SafeSetVisibility(twoHandAxe, false);
            SafeSetVisibility(twoHandBow, false);
            SafeSetVisibility(twoHandCrossbow, false);
            SafeSetVisibility(twoHandSpear, false);
            SafeSetVisibility(twoHandSword, false);
            SafeSetVisibility(staff, false);
            SafeSetVisibility(swordL, false);
            SafeSetVisibility(swordR, false);
            SafeSetVisibility(maceL, false);
            SafeSetVisibility(maceR, false);
            SafeSetVisibility(daggerL, false);
            SafeSetVisibility(daggerR, false);
            SafeSetVisibility(itemL, false);
            SafeSetVisibility(itemR, false);
            SafeSetVisibility(shield, false);
            SafeSetVisibility(pistolL, false);
            SafeSetVisibility(pistolR, false);
            SafeSetVisibility(rifle, false);
            SafeSetVisibility(spear, false);
        }

        /// <summary>
        /// Set a single weapon's visibility.
        /// </summary>
        /// <param name="weaponNumber">Weapon object to set.</param>
        /// <param name="visible">Whether to set it visible or not.</param>
        /// <param name="dual">Whether to update the same weapon in the other hand as well.</param>
        public IEnumerator _WeaponVisibility(Weapon weaponNumber, bool visible, bool dual)
        {
			if (debugWeaponVisibility) { Debug.Log($"WeaponVisibility:{weaponNumber}   Visible:{visible}   Dual:{dual}"); }

			while (isWeaponSwitching) { yield return null; }
            var weaponType = (Weapon)weaponNumber;
			switch (weaponType) {
				case Weapon.TwoHandSword: SafeSetVisibility(twoHandSword, visible); break;
				case Weapon.TwoHandSpear: SafeSetVisibility(twoHandSpear, visible); break;
				case Weapon.TwoHandAxe: SafeSetVisibility(twoHandAxe, visible); break;
				case Weapon.TwoHandBow: SafeSetVisibility(twoHandBow, visible); break;
				case Weapon.TwoHandCrossbow: SafeSetVisibility(twoHandCrossbow, visible); break;
				case Weapon.TwoHandStaff: SafeSetVisibility(staff, visible); break;
				case Weapon.Shield: SafeSetVisibility(shield, visible); break;
				case Weapon.Rifle: SafeSetVisibility(rifle, visible); break;
				case Weapon.RightSpear: SafeSetVisibility(spear, visible); break;
				case Weapon.LeftSword: {
					SafeSetVisibility(swordL, visible);
					if (dual) { SafeSetVisibility(swordR, visible); }
					break;
				}
				case Weapon.RightSword: {
					SafeSetVisibility(swordR, visible);
					if (dual) { SafeSetVisibility(swordL, visible); }
					break;
				}
				case Weapon.LeftMace: {
					SafeSetVisibility(maceL, visible);
					if (dual) { SafeSetVisibility(maceR, visible); }
					break;
				}
				case Weapon.RightMace: {
					SafeSetVisibility(maceR, visible);
					if (dual) { SafeSetVisibility(maceL, visible); }
					break;
				}
				case Weapon.LeftDagger: {
					SafeSetVisibility(daggerL, visible);
					if (dual) { SafeSetVisibility(daggerR, visible); }
					break;
				}
				case Weapon.RightDagger: {
					SafeSetVisibility(daggerR, visible);
					if (dual) { SafeSetVisibility(daggerL, visible); }
					break;
				}
				case Weapon.LeftItem: {
					SafeSetVisibility(itemL, visible);
					if (dual) { SafeSetVisibility(itemR, visible); }
					break;
				}
				case Weapon.RightItem: {
					SafeSetVisibility(itemR, visible);
					if (dual) { SafeSetVisibility(itemL, visible); }
					break;
				}
				case Weapon.LeftPistol: {
					SafeSetVisibility(pistolL, visible);
					if (dual) { SafeSetVisibility(pistolR, visible); }
					break;
				}
				case Weapon.RightPistol: {
					SafeSetVisibility(pistolR, visible);
					if (dual) { SafeSetVisibility(pistolL, visible); }
					break;
				}
			}
            yield return null;
        }

        /// <summary>
        /// Sync weapon object visibility to the current weapons in the RPGCharacterController.
        /// </summary>
        public void SyncWeaponVisibility()
        { coroQueue.Run(_SyncWeaponVisibility()); }

        /// <summary>
        /// Async method to sync weapon object visiblity to the current weapons in RPGCharacterController.
        /// This will wait for weapon switching to finish. If your aim is to force this update, call WeaponSwitch
        /// first. This will stop the _HideAllWeapons and _WeaponVisibility coroutines.
        /// </summary>
        /// <returns>IEnumerator for use with.</returns>
        private IEnumerator _SyncWeaponVisibility()
        {
            while (isWeaponSwitching && !(rpgCharacterController.canAction && rpgCharacterController.canMove))
			{ yield return null; }

            StopCoroutine(nameof(_HideAllWeapons));
            StopCoroutine(nameof(_WeaponVisibility));

            SafeSetVisibility(twoHandAxe, false);
            SafeSetVisibility(twoHandBow, false);
            SafeSetVisibility(twoHandCrossbow, false);
            SafeSetVisibility(twoHandSpear, false);
            SafeSetVisibility(twoHandSword, false);
            SafeSetVisibility(staff, false);
            SafeSetVisibility(swordL, false);
            SafeSetVisibility(swordR, false);
            SafeSetVisibility(maceL, false);
            SafeSetVisibility(maceR, false);
            SafeSetVisibility(daggerL, false);
            SafeSetVisibility(daggerR, false);
            SafeSetVisibility(itemL, false);
            SafeSetVisibility(itemR, false);
            SafeSetVisibility(shield, false);
            SafeSetVisibility(pistolL, false);
            SafeSetVisibility(pistolR, false);
            SafeSetVisibility(rifle, false);
            SafeSetVisibility(spear, false);

            var leftWeaponType = (Weapon)rpgCharacterController.leftWeapon;
            switch (leftWeaponType) {
                case Weapon.Shield: SafeSetVisibility(shield, true); break;
                case Weapon.LeftSword: SafeSetVisibility(swordL, true); break;
                case Weapon.LeftMace: SafeSetVisibility(maceL, true); break;
                case Weapon.LeftDagger: SafeSetVisibility(daggerL, true); break;
                case Weapon.LeftItem: SafeSetVisibility(itemL, true); break;
                case Weapon.LeftPistol: SafeSetVisibility(pistolL, true); break;
            }

            var rightWeaponType = (Weapon)rpgCharacterController.rightWeapon;
            switch (rightWeaponType) {
                case Weapon.TwoHandSword: SafeSetVisibility(twoHandSword, true); break;
                case Weapon.TwoHandSpear: SafeSetVisibility(twoHandSpear, true); break;
                case Weapon.TwoHandAxe: SafeSetVisibility(twoHandAxe, true); break;
                case Weapon.TwoHandBow: SafeSetVisibility(twoHandBow, true); break;
                case Weapon.TwoHandCrossbow: SafeSetVisibility(twoHandCrossbow, true); break;
                case Weapon.TwoHandStaff: SafeSetVisibility(staff, true); break;
                case Weapon.RightSword: SafeSetVisibility(swordR, true); break;
                case Weapon.RightMace: SafeSetVisibility(maceR, true); break;
                case Weapon.RightDagger: SafeSetVisibility(daggerR, true); break;
                case Weapon.RightItem: SafeSetVisibility(itemR, true); break;
                case Weapon.RightPistol: SafeSetVisibility(pistolR, true); break;
                case Weapon.Rifle: SafeSetVisibility(rifle, true); break;
                case Weapon.RightSpear: SafeSetVisibility(spear, true); break;
            }
        }
    }
}