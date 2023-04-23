using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims.Actions
{
	public class SwitchWeaponContext
    {
        public string type;
        public string side;

        // "back" or "hips".
        public string sheathLocation;

        public Weapon rightWeapon;
        public Weapon leftWeapon;

        public SwitchWeaponContext()
        {
            this.type = "Instant";
            this.side = "None";
            this.sheathLocation = "Back";
            this.rightWeapon = Weapon.Relax;
            this.leftWeapon = Weapon.Relax;
        }

        public SwitchWeaponContext(string type, string side, string sheathLocation = "Back", Weapon rightWeapon = Weapon.Relax, Weapon leftWeapon = Weapon.Relax)
        {
            this.type = type;
            this.side = side;
            this.sheathLocation = sheathLocation;
            this.rightWeapon = rightWeapon;
            this.leftWeapon = leftWeapon;
        }

        public void LowercaseStrings()
        {
            type = type.ToLower();
            side = side.ToLower();
            sheathLocation = sheathLocation.ToLower();
        }
    }

    public class SwitchWeapon : BaseActionHandler<SwitchWeaponContext>
    {
        public override bool CanStartAction(RPGCharacterController controller)
        { return !IsActive() && !controller.isCasting; }

        public override bool CanEndAction(RPGCharacterController controller)
        { return IsActive(); }

        protected override void _StartAction(RPGCharacterController controller, SwitchWeaponContext context)
		{
			RPGCharacterWeaponController weaponController = controller.GetComponent<RPGCharacterWeaponController>();

			if (weaponController == null) {
				EndAction(controller);
				return;
			}

			context.LowercaseStrings();

			// Debug IncomingWeaponContext.
			if (weaponController != null) {
				if (weaponController.debugIncomingWeaponContext) { DebugIncomingContext(context); }
			}

			bool changeRight = false;
			bool sheathRight = false;
			bool unsheathRight = false;
			Weapon fromRightWeapon = controller.rightWeapon;
			Weapon toRightWeapon = context.rightWeapon;

			bool changeLeft = false;
			bool sheathLeft = false;
			bool unsheathLeft = false;
			Weapon fromLeftWeapon = controller.leftWeapon;
			Weapon toLeftWeapon = context.leftWeapon;

			bool dualWielding = fromRightWeapon.Is1HandedWeapon() && fromLeftWeapon.Is1HandedWeapon();
			bool dualUnsheath = context.side == "dual";
			bool dualSheath = false;

			AnimatorWeapon toAnimatorWeapon = 0;

			// Filter which side is changing.
			switch (context.side) {
				case "none":
				case "right":
					changeRight = true;
					if (toRightWeapon.Is2HandedWeapon() && !fromLeftWeapon.HasNoWeapon()) {
						changeLeft = true;
						toLeftWeapon = Weapon.Unarmed;
						dualSheath = dualWielding;
					}
					break;
				case "left":
					changeLeft = true;
					if (fromRightWeapon.Is2HandedWeapon()) {
						changeRight = true;
						toRightWeapon = Weapon.Unarmed;
					}
					break;
				case "dual":
					changeLeft = true;
					changeRight = true;
					dualSheath = dualWielding;
					break;
				case "both":
					changeLeft = true;
					changeRight = true;
					break;
			}

			// Set sheath location.
			switch (context.sheathLocation) {
				case "back":
					controller.animator.SetInteger(AnimationParameters.SheathLocation, 0);
					break;
				case "hips":
					controller.animator.SetInteger(AnimationParameters.SheathLocation, 1);
					break;
			}

			// If relaxing, just use the Relax action.
			if (context.type == "relax") {
				controller.StartAction(HandlerTypes.Relax);
				return;
			}

			// Force Unarmed if sheathing weapons.
			if (context.type == "sheath") {
				if (context.side == "left" || context.side == "dual" || context.side == "both")
				{ toLeftWeapon = Weapon.Unarmed; }
				else { toLeftWeapon = fromLeftWeapon; }

				if (context.side == "none" || context.side == "right" || context.side == "dual" || context.side == "both")
				{ toRightWeapon = Weapon.Unarmed; }
				else { toRightWeapon = fromRightWeapon; }
			}

			toAnimatorWeapon = AnimationData.ConvertToAnimatorWeapon(controller.leftWeapon, controller.rightWeapon);

			// If switching weapons in Armed state or from Dpad.
			if (context.type == "switch") {
				sheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !fromLeftWeapon.HasNoWeapon();
				sheathRight = changeRight && fromRightWeapon != toRightWeapon && !fromRightWeapon.HasNoWeapon();
				unsheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !toLeftWeapon.HasNoWeapon();
				unsheathRight = changeRight && fromRightWeapon != toRightWeapon && !toRightWeapon.HasNoWeapon();

				// If pulling a weapon from the same side, only play 1 weapon switch animation.
				if ((toAnimatorWeapon == AnimatorWeapon.ARMED && toLeftWeapon != Weapon.Shield
					&& fromLeftWeapon != Weapon.Shield && weaponController.singleAnimSwitch)
					&& (!dualSheath && !dualUnsheath)) {
					Debug.Log("Pulling a weapon from the same side, only play 1 weapon switch animation.");
					if (changeLeft) {
						if (toLeftWeapon.Is1HandedWeapon()) {
							weaponController.SheathAndUnSheath(controller.leftWeapon, toLeftWeapon, Side.Left);
							EndSwitch(controller, weaponController, changeRight, toRightWeapon, changeLeft, toLeftWeapon);
							return;
						}
					}
					else if (changeRight) {
						if (toRightWeapon.Is1HandedWeapon()) {
							weaponController.SheathAndUnSheath(controller.rightWeapon, toRightWeapon, Side.Right);
							EndSwitch(controller, weaponController, changeRight, toRightWeapon, changeLeft, toLeftWeapon);
							return;
						}
					}

				}
				// Dual sheathing and unsheathing 2 weapons, only play 1 weapon switch animation.
				else if (toAnimatorWeapon == AnimatorWeapon.ARMED && dualUnsheath
					&& toLeftWeapon != Weapon.Shield && weaponController.singleAnimSwitchDual) {
					Debug.Log("Dual sheathing and unsheathing 2 weapons, only play 1 weapon switch animation.");
					if (changeLeft) {
						if (toLeftWeapon.Is1HandedWeapon()) {
							weaponController.DualSheathAndUnSheath(controller.leftWeapon, toLeftWeapon, Side.Dual);
							EndSwitch(controller, weaponController, changeRight, toRightWeapon, changeLeft, toLeftWeapon);
							return;
						}
					}
					else if (changeRight) {
						if (toRightWeapon.Is1HandedWeapon()) {
							weaponController.DualSheathAndUnSheath(controller.rightWeapon, toRightWeapon, Side.Dual);
							EndSwitch(controller, weaponController, changeRight, toRightWeapon, changeLeft, toLeftWeapon);
							return;
						}
					}
				}
			}

			// Sheath weapons first if our starting weapon is different from our desired weapon and we're
			// not starting from an Unarmed position.
			if (context.type == "sheath" || context.type == "switch") {
				sheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !fromLeftWeapon.HasNoWeapon();
				sheathRight = changeRight && fromRightWeapon != toRightWeapon && !fromRightWeapon.HasNoWeapon();
			}

			// Unsheath a weapon if our starting weapon is different from our desired weapon and we're
			// not ending on an Unarmed position.
			if (context.type == "unsheath" || context.type == "switch") {
				unsheathLeft = changeLeft && fromLeftWeapon != toLeftWeapon && !toLeftWeapon.HasNoWeapon();
				unsheathRight = changeRight && fromRightWeapon != toRightWeapon && !toRightWeapon.HasNoWeapon();

				// If you're switching from the relaxed state, you can "unsheath" your fists.
				if ((controller.isRelaxed || controller.hasNoWeapon)
					&& (toLeftWeapon == Weapon.Unarmed || toRightWeapon == Weapon.Unarmed)) {
					fromLeftWeapon = Weapon.Relax;
					fromRightWeapon = Weapon.Relax;
					sheathLeft = false;
					sheathRight = false;
					unsheathLeft = false;
					unsheathRight = true;
					dualSheath = false;
					dualUnsheath = false;
				}
			}

			// Debug SwitchWeaponContext.
			if (weaponController != null) {
				if (weaponController.debugSwitchWeaponContext) {
					DebugSwitchWeapon(weaponController, context, changeRight, changeLeft, sheathRight, sheathLeft, unsheathRight, unsheathLeft,
						fromRightWeapon, toRightWeapon, fromLeftWeapon, toLeftWeapon, dualWielding, dualUnsheath, dualSheath);
				}
			}

			///
			/// Actually make changes to the weapon controller.
			///

			// If Instant Switch.
			if (context.type == "instant") {

				Debug.Log("Instant Switch");

				if (toLeftWeapon == Weapon.Unarmed || toLeftWeapon == Weapon.Relax) {
					weaponController.InstantWeaponSwitch(toLeftWeapon);
					weaponController.InstantWeaponSwitch(toRightWeapon);
				}
				else {
					weaponController.InstantWeaponSwitch(toRightWeapon);
					weaponController.InstantWeaponSwitch(toLeftWeapon);
				}
			}
			// Non-instant weapon switch.
			else {

				// SHEATHING

				// Sheath weapons first if that's necessary.
				if (dualSheath && (sheathRight || sheathLeft)) {

					// Dual sheathing requires at most one call.
					Debug.Log("Dual Sheathing");
					weaponController.SheathWeapon(fromRightWeapon, Weapon.Unarmed, dualSheath);
				}
				else {
					if (sheathLeft) {
						Debug.Log("Sheath Left (dual: " + dualSheath + "): " + "fromLeftAnim: "
							+ fromLeftWeapon + " > " + "toLeftAnim: " + toLeftWeapon);
						weaponController.SheathWeapon(fromLeftWeapon, toLeftWeapon, dualSheath);
					}
					if (sheathRight) {
						Debug.Log("Sheath Right (dual: " + dualSheath + "): " + "fromRightAnim: "
							+ fromRightWeapon + " > " + "toRightAnim: " + toRightWeapon);
						weaponController.SheathWeapon(fromRightWeapon, toRightWeapon, dualSheath);
					}
				}

				// UNSHEATHING

				// Finally, unsheath the desired weapons!
				if (dualUnsheath && (unsheathRight || unsheathLeft)) {
					Debug.Log("Dual Unsheathing");

					// Dual unsheathing requires at most one call.
					weaponController.UnsheathWeapon(toRightWeapon, dualUnsheath);
				}
				else {
					if (unsheathLeft) {
						Debug.Log("Unsheath Left: " + toLeftWeapon);
						weaponController.UnsheathWeapon(toLeftWeapon, dualUnsheath);
					}
					else if (unsheathRight) {
						Debug.Log("Unsheath Right (dual: " + dualUnsheath + "): " + toRightWeapon);
						weaponController.UnsheathWeapon(toRightWeapon, dualUnsheath);
					}
				}
			}

			EndSwitch(controller, weaponController, changeRight, toRightWeapon, changeLeft, toLeftWeapon);
		}

		/// <summary>
		/// Updates the weapons in character controller through callback, syncs the weapon visibility, and then ends the action.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="weaponController">RPGCharacterWeaponController.</param>
		/// <param name="changeRight">If rightWeapon changed.</param>
		/// <param name="toRightWeapon">New rightWeapon number.</param>
		/// <param name="changeLeft">If leftWeapon changed.</param>
		/// <param name="toLeftWeapon">New leftWeapon number.</param>
		private void EndSwitch(RPGCharacterController controller, RPGCharacterWeaponController weaponController, bool changeRight, Weapon toRightWeapon, bool changeLeft, Weapon toLeftWeapon)
		{
			// This callback will update the weapons in character controller after all other
			// coroutines finish.
			weaponController.AddCallback(() => {
				if (changeLeft) { controller.leftWeapon = toLeftWeapon; }
				if (changeRight) { controller.rightWeapon = toRightWeapon; }

				// Turn off the isWeaponSwitching flag and sync weapon object visibility.
				weaponController.SyncWeaponVisibility();
				controller.EndAction(HandlerTypes.Relax);
				EndAction(controller);
			});
		}

		private static void DebugIncomingContext(SwitchWeaponContext context)
		{
			Debug.Log("===IncomingWeaponContext===");
			Debug.Log($"type:{context.type}   side:{context.side}   sheathlocation:{context.sheathLocation}  " +
					$"rightWeapon:{context.rightWeapon}    leftWeapon:{context.leftWeapon}");
		}

		private static void DebugSwitchWeapon(RPGCharacterWeaponController weaponController, SwitchWeaponContext context, bool changeRight, bool changeLeft, bool sheathRight,
			bool sheathLeft, bool unsheathRight, bool unsheathLeft, Weapon fromRightWeapon, Weapon toRightWeapon, Weapon fromLeftWeapon,
			Weapon toLeftWeapon, bool dualWielding, bool dualUnsheath, bool dualSheath)
		{
			Debug.Log("===SwitchWeaponContext===");
			Debug.Log($"leftWeapon:{context.leftWeapon}   rightWeapon:{context.rightWeapon}   " +
				$"side:{context.side}    type:{context.type}    sheathlocation:{context.sheathLocation}  " +
				$"changeLeft:{changeLeft}    changeRight:{changeRight}    sheathRight:{sheathRight}    sheathLeft:{sheathLeft}");
			Debug.Log($"fromRightWeapon:{fromRightWeapon}   toRightWeapon:{toRightWeapon}   " +
				$"fromLeftWeapon:{fromLeftWeapon}    toLeftWeapon:{toLeftWeapon}    dualWielding:{dualWielding}  " +
				$"dualUnsheath:{dualUnsheath}    dualSheath:{dualSheath}");
		}

		protected override void _EndAction(RPGCharacterController controller)
        {
        }
    }
}