using System;
using UnityEngine;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
	[HelpURL("https://docs.unity3d.com/Manual/class-InputManager.html")]

	public class RPGCharacterInputController : MonoBehaviour
    {
        RPGCharacterController rpgCharacterController;

        // Inputs.
        private float inputHorizontal = 0;
        private float inputVertical = 0;
        private bool inputJump;
        private bool inputLightHit;
        private bool inputDeath;
        private bool inputAttackL;
        private bool inputAttackR;
        private bool inputCastL;
        private bool inputCastR;
        private float inputSwitchUpDown;
        private float inputSwitchLeftRight;
        private float inputAimBlock;
        private bool inputAiming;
        private bool inputFace;
        private float inputFacingHorizontal;
        private float inputFacingVertical;
        private bool inputRoll;
        private bool inputShield;
        private bool inputRelax;

        // Variables.
        private Vector3 moveInput;
        private bool isJumpHeld;
        private Vector3 currentAim;
        private float bowPull;
        private bool blockToggle;
        private float inputPauseTimeout = 0;
        private bool inputPaused = false;

        private void Awake()
        {
            rpgCharacterController = GetComponent<RPGCharacterController>();
            currentAim = Vector3.zero;
        }

        private void Update()
        {
			// Pause input for other external input.
			if (inputPaused) {
				if (Time.time > inputPauseTimeout) { inputPaused = false; }
				else { return; }
			}

			if (!inputPaused) { Inputs(); }

            Moving();
			Jumping();
			Damage();
            SwitchWeapons();
            Strafing();
            Facing();

			if (!rpgCharacterController.HandlerExists("Relax")) { return; }
			if (rpgCharacterController.IsActive("Relax")) { return; }

			// Actions that can't be performed in Relax state.
			Blocking();
            Aiming();
            Rolling();
            Attacking();
        }

        /// <summary>
        /// Pause input for a number of seconds.
        /// </summary>
        /// <param name="timeout">The amount of time in seconds to ignore input.</param>
        public void PauseInput(float timeout)
        {
            inputPaused = true;
            inputPauseTimeout = Time.time + timeout;
        }

        /// <summary>
        /// Input abstraction for easier asset updates using outside control schemes.
        /// </summary>
        private void Inputs()
        {
	        try {
		        inputJump = Input.GetButtonDown("Jump");
		        isJumpHeld = Input.GetButton("Jump");
		        inputLightHit = Input.GetButtonDown("LightHit");
		        inputDeath = Input.GetButtonDown("Death");
		        inputAttackL = Input.GetButtonDown("AttackL");
		        inputAttackR = Input.GetButtonDown("AttackR");
		        inputCastL = Input.GetButtonDown("CastL");
		        inputCastR = Input.GetButtonDown("CastR");
		        inputSwitchUpDown = Input.GetAxisRaw("SwitchUpDown");
		        inputSwitchLeftRight = Input.GetAxisRaw("SwitchLeftRight");
		        inputAimBlock = Input.GetAxisRaw("Aim");
		        inputAiming = Input.GetButton("Aiming");
		        inputHorizontal = Input.GetAxisRaw("Horizontal");
		        inputVertical = Input.GetAxisRaw("Vertical");
		        inputFace = Input.GetMouseButton(1);
		        inputFacingHorizontal = Input.GetAxisRaw("FacingHorizontal");
		        inputFacingVertical = Input.GetAxisRaw("FacingVertical");
		        inputRoll = Input.GetButtonDown("L3");
		        inputShield = Input.GetButtonDown("Shield");
		        inputRelax = Input.GetButtonDown("Relax");

		        // Injury toggle.
		        if (rpgCharacterController.HandlerExists(HandlerTypes.Injure)) {
			        if (Input.GetKeyDown(KeyCode.I)) {
				        if (!rpgCharacterController.TryStartAction(HandlerTypes.Injure))
						{ rpgCharacterController.TryEndAction(HandlerTypes.Injure); }
			        }
		        }
		        // Headlook toggle.
		        if (Input.GetKeyDown(KeyCode.L)) { rpgCharacterController.ToggleHeadlook(); }

		        // Slow time toggle.
		        if (rpgCharacterController.HandlerExists(HandlerTypes.SlowTime)) {
			        if (Input.GetKeyDown(KeyCode.T)) {
				        if (!rpgCharacterController.TryStartAction(HandlerTypes.SlowTime, 0.125f))
						{ rpgCharacterController.TryEndAction(HandlerTypes.SlowTime); }
			        }
			        // Pause toggle.
			        if (Input.GetKeyDown(KeyCode.P)) {
				        if (!rpgCharacterController.TryStartAction(HandlerTypes.SlowTime, 0f))
						{ rpgCharacterController.TryEndAction(HandlerTypes.SlowTime); }
			        }
		        }
	        }
	        catch (Exception)
			{ Debug.LogError("Inputs not found! Please read Readme, or watch https://www.youtube.com/watch?v=ruufqlXrCzU"); }
        }

		public bool HasMoveInput() => moveInput.magnitude > 0.1f;

		public bool HasAimInput() => inputAiming || inputAimBlock < -0.1f;

		public bool HasBlockInput() => inputAimBlock > 0.1;

		public bool HasFacingInput() => (inputFacingHorizontal < -0.05 || inputFacingHorizontal > 0.05) ||
				   (inputFacingVertical < -0.05 || inputFacingVertical > 0.05) ||
				   inputFace;

        public void Blocking()
        {
			// Check to make sure Block Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Block)) { return; }

			var blocking = HasBlockInput();
			if (blocking && rpgCharacterController.TryStartAction(HandlerTypes.Block)) { blockToggle = true; }
			else if (!blocking && blockToggle && rpgCharacterController.TryEndAction(HandlerTypes.Block)) { blockToggle = false; }
        }

        public void Moving()
		{
			moveInput = new Vector3(inputHorizontal, inputVertical, 0f);

			// Filter the 0.1 threshold of HasMoveInput.
			if (HasMoveInput()) { rpgCharacterController.SetMoveInput(moveInput); }
			else { rpgCharacterController.SetMoveInput(Vector3.zero); }
		}

		private void Jumping()
		{
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Jump)) { return; }

			// Set the input on the jump axis every frame.
			var jumpInput = isJumpHeld ? Vector3.up : Vector3.zero;
			rpgCharacterController.SetJumpInput(jumpInput);

			// If we pressed jump button this frame, jump.
			if (inputJump && rpgCharacterController.CanStartAction(HandlerTypes.Jump))
			{ rpgCharacterController.StartAction(HandlerTypes.Jump); }

			// Or doublejump if already jumped.
			else if (inputJump && rpgCharacterController.CanStartAction(HandlerTypes.DoubleJump))
			{ rpgCharacterController.StartAction(HandlerTypes.DoubleJump); }
		}

		public void Rolling()
        {
            if (!inputRoll ||
                !rpgCharacterController.HandlerExists(HandlerTypes.DiveRoll) ||
                !rpgCharacterController.CanStartAction(HandlerTypes.DiveRoll)) { return; }

			rpgCharacterController.StartAction(HandlerTypes.DiveRoll, 1);
        }

        private void Aiming()
        {
            if (rpgCharacterController.hasAimedWeapon) {
				if (rpgCharacterController.HandlerExists(HandlerTypes.Aim)) {
					if (HasAimInput()) { rpgCharacterController.TryStartAction(HandlerTypes.Aim); }
					else { rpgCharacterController.TryEndAction(HandlerTypes.Aim); }
				}
                if (rpgCharacterController.rightWeapon == Weapon.TwoHandBow) {

                    // If using the bow, we want to pull back slowly on the bow string while the
                    // Left Mouse button is down, and shoot when it is released.
                    if (Input.GetMouseButton(0)) {  bowPull += 0.05f; }
					else if (Input.GetMouseButtonUp(0)) {
						if (rpgCharacterController.HandlerExists(HandlerTypes.Shoot))
						{ rpgCharacterController.TryStartAction(HandlerTypes.Shoot); }
                    }
					else { bowPull = 0f; }
                    bowPull = Mathf.Clamp(bowPull, 0f, 1f);
                }
				else {
					// If using a gun or a crossbow, we want to fire when the left mouse button is pressed.
					if (rpgCharacterController.HandlerExists(HandlerTypes.Shoot)) {
						if (Input.GetMouseButtonDown(0)) { rpgCharacterController.TryStartAction(HandlerTypes.Shoot); }
					}
                }
				// Reload.
				if (rpgCharacterController.HandlerExists(HandlerTypes.Reload)) {
					if (Input.GetMouseButtonDown(2)) { rpgCharacterController.TryStartAction(HandlerTypes.Reload); }
				}
				// Finally, set aim location and bow pull.
				if (rpgCharacterController.target != null) {
					rpgCharacterController.SetAimInput(rpgCharacterController.target.position);
					rpgCharacterController.SetBowPull(bowPull);
				}
            }
			else { Strafing(); }
        }

        private void Strafing()
        {
			// Check to make sure Strafe Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Strafe) || !rpgCharacterController.canStrafe) { return; }

			if (inputAimBlock < -0.1f || inputAiming) { rpgCharacterController.TryStartAction(HandlerTypes.Strafe); }
			else { rpgCharacterController.TryEndAction(HandlerTypes.Strafe); }
        }

        private void Facing()
        {
			// Check to make sure Face Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Face)) { return; }
			if (!rpgCharacterController.canFace) { return; }

			if (HasFacingInput()) {
				if (inputFace) {

					// Get world position from mouse position on screen and convert to direction from character.
					var playerPlane = new Plane(Vector3.up, transform.position);
					var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					var hitdist = 0.0f;
					if (playerPlane.Raycast(ray, out hitdist)) {
						var targetPoint = ray.GetPoint(hitdist);
						var lookTarget = new Vector3(targetPoint.x - transform.position.x, transform.position.z - targetPoint.z, 0);
						rpgCharacterController.SetFaceInput(lookTarget);
					}
				}
				else { rpgCharacterController.SetFaceInput(new Vector3(inputFacingHorizontal, inputFacingVertical, 0)); }

				rpgCharacterController.TryStartAction(HandlerTypes.Face);
			}
			else { rpgCharacterController.TryEndAction(HandlerTypes.Face); }
        }

        private void Attacking()
        {
			// Check to make sure Attack and Cast Actions exist.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.Attack)
				&& rpgCharacterController.HandlerExists(HandlerTypes.AttackCast)) { return; }

			// If already casting, stop casting.
            if ((inputCastL || inputCastR) && rpgCharacterController.IsActive(HandlerTypes.AttackCast)) {
				rpgCharacterController.EndAction(HandlerTypes.AttackCast);
				return;
			}

			// Check to make character can Attack.
			if (!rpgCharacterController.CanStartAction(HandlerTypes.Attack)) { return; }

            if (inputAttackL)
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, Side.Left)); }
			else if (inputAttackR)
			{ rpgCharacterController.StartAction(HandlerTypes.Attack, new AttackContext(HandlerTypes.Attack, Side.Right)); }
			else if (inputCastL)
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Left)); }
			else if (inputCastR)
			{ rpgCharacterController.StartAction(HandlerTypes.AttackCast, new AttackCastContext(AnimationVariations.AttackCast.TakeRandom(), Side.Right)); }
        }

        private void Damage()
        {
			// Check to make sure GetHit Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.GetHit)) {
				if (inputLightHit) { rpgCharacterController.StartAction(HandlerTypes.GetHit, new HitContext()); }
			}

			// Check to make sure Death Action exists.
			if (rpgCharacterController.HandlerExists(HandlerTypes.Death)) {
				if (inputDeath && rpgCharacterController.CanStartAction(HandlerTypes.Death))
				{ rpgCharacterController.StartAction(HandlerTypes.Death); }
				else if (inputDeath && !rpgCharacterController.TryStartAction(HandlerTypes.Death))
				{ rpgCharacterController.TryEndAction(HandlerTypes.Death); }
			}
        }

		/// <summary>
		/// Cycle weapons using directional pad input. Up and Down cycle forward and backward through
		/// the list of two handed weapons. Left cycles through the left hand weapons. Right cycles through
		/// the right hand weapons.
		/// </summary>
		private void SwitchWeapons()
		{
			// Check to make sure SwitchWeapon Action exists.
			if (!rpgCharacterController.HandlerExists(HandlerTypes.SwitchWeapon)) { return; }

			// Bail out if we can't switch weapons.
			if (!rpgCharacterController.CanStartAction(HandlerTypes.SwitchWeapon)) { return; }

			// Switch to Relaxed.
			if (inputRelax) {
				rpgCharacterController.StartAction(HandlerTypes.Relax);
				return;
			}

			var doSwitch = false;
			var context = new SwitchWeaponContext();
			var weaponNumber = Weapon.Unarmed;

			// Switch to Shield.
			if (inputShield) {
				doSwitch = true;
				context.side = "Left";
				context.type = "Switch";
				context.leftWeapon = Weapon.Shield;
				context.rightWeapon = Weapon.Relax;
				rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context);
				return;
			}

			// Cycle through 2Handed weapons if any input happens on the up-down axis.
			if (Mathf.Abs(inputSwitchUpDown) > 0.1f) {
				var twoHandedWeapons = new Weapon[] {
					Weapon.TwoHandSword,
					 Weapon.TwoHandSpear,
					 Weapon.TwoHandAxe,
					 Weapon.TwoHandBow,
					 Weapon.TwoHandCrossbow,
					 Weapon.TwoHandStaff,
					 Weapon.Rifle,
				};
				// If we're not wielding 2Handed weapon already, just switch to the first one in the list.
				if (System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon) == -1)
				{ weaponNumber = twoHandedWeapons[0]; }

				// Otherwise, loop through them.
				else {
					var index = System.Array.IndexOf(twoHandedWeapons, rpgCharacterController.rightWeapon);
					if (inputSwitchUpDown < -0.1f) { index = (index - 1 + twoHandedWeapons.Length) % twoHandedWeapons.Length; }
					else if (inputSwitchUpDown > 0.1f) { index = (index + 1) % twoHandedWeapons.Length; }
					weaponNumber = twoHandedWeapons[index];
				}
				// Set up the context and flag that we actually want to perform the switch.
				doSwitch = true;
				context.type = HandlerTypes.Switch;
				context.side = "None";
				context.leftWeapon = Weapon.Relax;
				context.rightWeapon = weaponNumber;
			}

			// Cycle through 1Handed weapons if any input happens on the left-right axis.
			if (inputSwitchLeftRight > 0.1f || inputSwitchLeftRight < -0.1f) {
				doSwitch = true;
				context.type = HandlerTypes.Switch;

				// Left-handed weapons.
				if (inputSwitchLeftRight < -0.1f) {
					var leftWeaponType = rpgCharacterController.leftWeapon;

					// If we are not wielding a left-handed weapon, switch to Left Sword.
					if (Array.IndexOf(WeaponGroupings.LeftHandedWeapons, leftWeaponType) == -1)
					{ weaponNumber = Weapon.LeftSword; }

					// Otherwise, cycle through the list.
					else {
						var currentIndex = Array.IndexOf(WeaponGroupings.LeftHandedWeapons, leftWeaponType);
						weaponNumber = WeaponGroupings.LeftHandedWeapons[(currentIndex + 1) % WeaponGroupings.LeftHandedWeapons.Length];
					}

					context.side = "Left";
					context.leftWeapon = weaponNumber;
					context.rightWeapon = Weapon.Relax;
				}
				// Right-handed weapons.
				else if (inputSwitchLeftRight > 0.1f) {
					var rightWeaponType = rpgCharacterController.rightWeapon;

					// If we are not wielding a right-handed weapon, switch to Unarmed.
					if (Array.IndexOf(WeaponGroupings.RightHandedWeapons, rightWeaponType) == -1)
					{ weaponNumber = Weapon.Unarmed; }

					// Otherwise, cycle through the list.
					else {
						var currentIndex = Array.IndexOf(WeaponGroupings.RightHandedWeapons, rightWeaponType);
						weaponNumber = WeaponGroupings.RightHandedWeapons[(currentIndex + 1) % WeaponGroupings.RightHandedWeapons.Length];
					}
					context.side = "Right";
					context.leftWeapon = Weapon.Relax;
					context.rightWeapon = weaponNumber;
				}
			}
			// If we've received input, then "doSwitch" is true, and the context is filled out,
			// so start the SwitchWeapon action.
			if (doSwitch) { rpgCharacterController.StartAction(HandlerTypes.SwitchWeapon, context); }
		}
	}
}