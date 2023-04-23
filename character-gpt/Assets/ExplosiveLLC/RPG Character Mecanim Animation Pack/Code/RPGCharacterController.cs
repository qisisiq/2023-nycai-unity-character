using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
	/// <summary>
    /// RPGCharacterController is the main entry point for triggering animations and holds all the
    /// state related to a character. It is the core component of this package–no other controller
    /// will run without it.
    /// </summary>
    public class RPGCharacterController : MonoBehaviour
    {
		#region Events

		/// <summary>
		/// Event called when actions are locked by an animation.
		/// </summary>
		public event System.Action OnLockActions = delegate { };

        /// <summary>
        /// Event called when actions are unlocked at the end of an animation.
        /// </summary>
        public event System.Action OnUnlockActions = delegate { };

        /// <summary>
        /// Event called when movement is locked by an animation.
        /// </summary>
        public event System.Action OnLockMovement = delegate { };

        /// <summary>
        /// Event called when movement is unlocked at the end of an animation.
        /// </summary>
        public event System.Action OnUnlockMovement = delegate { };

		#endregion

		#region Components

		/// <summary>
		/// Unity Animator component.
		/// </summary>
		[HideInInspector] public Animator animator;

		/// <summary>
		/// Unity Camera component.
		/// </summary>
		[HideInInspector] public Camera mainCamera;

		/// <summary>
		/// IKHands component.
		/// </summary>
		[HideInInspector] public IKHands ikHands;

        /// <summary>
        /// Nearby ladder collider.
        /// </summary>
        [HideInInspector] public Collider ladder;

        /// <summary>
        /// Nearby cliff collider.
        /// </summary>
        [HideInInspector] public Collider cliff;

		/// <summary>
		/// Target for Aiming/Strafing.
		/// </summary>
		public Transform target;

        private PerfectLookAt headLookController;

		#endregion

		#region Variables

		/// <summary>
		/// Animation speed control. Doesn't affect lock timing.
		/// </summary>
		public float animationSpeed = 1;

        /// <summary>
        /// Whether to use PerfectLookAt headlook.
        /// </summary>
        public bool headLook = false;

		/// <summary>
		/// Returns whether the character is using headlook.
		/// </summary>
		public bool isHeadlook => _isHeadlook;
        private bool _isHeadlook = false;

		/// <summary>
		/// Whether to play idle alert animations.
		/// </summary>
		public bool idleAlert = true;

		/// <summary>
		/// Returns whether the character can take actions.
		/// </summary>
		public bool canAction => _canAction && !isDead && !isSpecial && !isNavigating && !isCrawling;
		private bool _canAction;

        /// <summary>
        /// Returns whether the character can face.
        /// </summary>
        public bool canFace => _canFace && !isDead && !isSwimming && !isCrawling;
        private bool _canFace = true;

        /// <summary>
        /// Returns whether the character can move.
        /// </summary>
        public bool canMove => _canMove && !isDead;
        private bool _canMove;

        /// <summary>
        /// Returns whether the character can strafe.
        /// </summary>
        public bool canStrafe => _canStrafe && !isDead && !isSwimming && !isCrawling;
        private bool _canStrafe = true;

        /// <summary>
        /// Returns whether the AcquiringGround action is active, signifying that the character is
        /// landing on the ground. AcquiringGround is added by RPGCharacterMovementController.
        /// </summary>
		public bool acquiringGround => TryGetHandlerActive(HandlerTypes.AcquiringGround);

        /// <summary>
		/// Returns whether the Aim action is active.
		/// </summary>
		public bool isAiming => TryGetHandlerActive(HandlerTypes.Aim);

        /// <summary>
		/// Returns whether the Attack action is active.
		/// </summary>
		public bool isAttacking => _isAttacking;
		private bool _isAttacking;

        /// <summary>
		/// Returns whether the Block action is active.
		/// </summary>
		public bool isBlocking => TryGetHandlerActive(HandlerTypes.Block);

        /// <summary>
		/// Returns whether the Cast action is active.
		/// </summary>
		public bool isCasting
		{
			get
			{
				if (TryGetHandlerActive(HandlerTypes.Cast) || TryGetHandlerActive(HandlerTypes.AttackCast)) { return true; }
				else { return false; }
			}
		}

		/// <summary>
		/// Returns whether the ClimbLadder action is active. ClimbLadder is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isClimbing => TryGetHandlerActive(HandlerTypes.ClimbLadder);

        /// <summary>
		/// Returns whether the Crouch action is active.
		/// </summary>
		public bool isCrouching => TryGetHandlerActive(HandlerTypes.Crouch);

        /// <summary>
		/// Returns whether the Crouch action is active.
		/// </summary>
		public bool isCrawling => TryGetHandlerActive(HandlerTypes.Crawl);

        /// <summary>
		/// Returns whether the Death action is active.
		/// </summary>
		public bool isDead => TryGetHandlerActive(HandlerTypes.Death);

        /// <summary>
		/// Returns whether the Facing action is active.
		/// </summary>
		public bool isFacing => TryGetHandlerActive(HandlerTypes.Face);

        /// <summary>
		/// Returns whether the Fall action is active. Fall is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isFalling => TryGetHandlerActive(HandlerTypes.Fall);

        /// <summary>
		/// Returns whether the HipShoot action is active.
		/// </summary>
		public bool isHipShooting => TryGetHandlerActive(HandlerTypes.HipShoot);

        /// <summary>
		/// Returns whether the Idle action is active. Idle is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isIdle => TryGetHandlerActive(HandlerTypes.Idle);

        /// <summary>
		/// Returns whether the Injure action is active.
		/// </summary>
		public bool isInjured => TryGetHandlerActive(HandlerTypes.Injure);

        /// <summary>
		/// Returns whether the Move action is active. Idle is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isMoving => TryGetHandlerActive(HandlerTypes.Move);

        /// <summary>
		/// Returns whether the Navigation action is active. Navigation is added by
		/// RPGCharacterNavigationController.
		/// </summary>
		public bool isNavigating => TryGetHandlerActive(HandlerTypes.Navigation);

        /// <summary>
		/// Returns whether the character is near a cliff. Set by RPGCharacterMovementController.
		/// </summary>
		[HideInInspector] public bool isNearCliff = false;

        /// <summary>
        /// Returns whether the character is within the collision trigger. Set by RPGCharacterMovementController.
        /// </summary>
        [HideInInspector] public bool isNearLadder = false;

		/// <summary>
		/// Returns whether the Relax action is active. Relax is added by RPGCharacterWeapon
		/// controller. If this action does not exist, returns whether rightWeapon and leftWeapon
		/// are -1.
		/// </summary>
		public bool isRelaxed
		{
			get {
				if (HandlerExists(HandlerTypes.Relax)) { return IsActive(HandlerTypes.Relax); }
				return rightWeapon == Weapon.Relax && leftWeapon == Weapon.Relax;
			}
		}

		/// <summary>
		/// Returns whether the Roll action is active. Roll is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isRolling => TryGetHandlerActive(HandlerTypes.Roll);

        /// <summary>
		/// Returns whether the Roll action is active. Roll is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isKnockback => TryGetHandlerActive(HandlerTypes.Knockback);

        /// <summary>
		/// Returns whether the Roll action is active. Roll is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isKnockdown => TryGetHandlerActive(HandlerTypes.Knockdown);

        /// <summary>
		/// Returns whether the character is sitting or sleeping. This flag is set by the Emote
		/// action.
		/// </summary>
		[HideInInspector] public bool isSitting = false;

        /// <summary>
        /// Always returns true because all characters are special. Just kidding, this returns
        /// whether the character is performing a special attack. This flag is set by the Attack
        /// action.
        /// </summary>
        [HideInInspector] public bool isSpecial = false;

        /// <summary>
        /// Returns whether the Sprint action is active.
        /// </summary>
		public bool isSprinting => TryGetHandlerActive(HandlerTypes.Sprint);

        /// <summary>
		/// Returns whether the Strafe action is active.
		/// </summary>
		public bool isStrafing => TryGetHandlerActive(HandlerTypes.Strafe);

        /// <summary>
		/// Returns whether the Swim action is active. Swim is added by
		/// RPGCharacterMovementController.
		/// </summary>
		public bool isSwimming => TryGetHandlerActive(HandlerTypes.Swim);

        /// <summary>
		/// Returns whether the character is talking. This flag is set by the Emote action.
		/// </summary>
		[HideInInspector] public bool isTalking = false;

        /// <summary>
        /// Returns whether the MaintainingGround action is active, signifying that the character
        /// is on the ground. MaintainingGround is added by RPGCharacterMovementController. If the
        /// action does not exist, this defaults to true.
        /// </summary>
        public bool maintainingGround => TryGetHandlerActive(HandlerTypes.MaintainingGround);

        /// <summary>
        /// Vector3 for move input. Use SetMoveInput to change this.
        /// </summary>
        public Vector3 moveInput => _moveInput;
        private Vector3 _moveInput;

        /// <summary>
        /// Vector3 for aim input. Use SetAimInput to change this.
        /// </summary>
        public Vector3 aimInput => _aimInput;
        private Vector3 _aimInput;

        /// <summary>
        /// Vector3 for facing. Use SetFaceInput to change this.
        /// </summary>
        public Vector3 faceInput => _faceInput;
        private Vector3 _faceInput;

        /// <summary>
        /// Vector3 for jump input. Use SetJumpInput to change this.
        /// </summary>
        public Vector3 jumpInput => _jumpInput;
        private Vector3 _jumpInput;

        /// <summary>
        /// Camera relative input in the XZ plane. This is calculated when SetMoveInput is called.
        /// </summary>
        public Vector3 cameraRelativeInput => _cameraRelativeInput;
        private Vector3 _cameraRelativeInput;

        public float bowPull => _bowPull;
        private float _bowPull;

		/// <summary>
		/// Integer weapon number for the right hand. See the Weapon enum in AnimationData.cs for a
		/// full list.
		/// </summary>
		[HideInInspector] public Weapon rightWeapon = Weapon.Unarmed;

		/// <summary>
		/// Integer weapon number for the left hand. See the Weapon enum in AnimationData.cs for a
		/// full list.
		/// </summary>
		[HideInInspector] public Weapon leftWeapon = Weapon.Unarmed;

		/// <summary>
		/// Returns whether a weapon is held in the right hand. This is false if the character is
		/// unarmed or relaxed.
		public bool hasRightWeapon => rightWeapon.IsRightHandedWeapon();

		/// <summary>
		/// Returns whether a weapon is held in the left hand. This is false if the character is
		/// unarmed or relaxed.
		/// </summary>
		public bool hasLeftWeapon => leftWeapon.IsLeftHandedWeapon();

		/// <summary>
		/// Returns whether a weapon is held in both hands (hasRightWeapon && hasLeftWeapon).
		/// </summary>
		public bool hasDualWeapons => hasLeftWeapon && hasRightWeapon;

		/// <summary>
		/// Returns whether the character is holding a two-handed weapon. Two-handed weapons are
		/// "held" in the right hand.
		/// </summary>
		public bool hasTwoHandedWeapon => rightWeapon.Is2HandedWeapon();

		/// <summary>
		/// Returns whether the character is holding a shield. Shields are held in the left hand.
		/// </summary>
		public bool hasShield => leftWeapon == Weapon.Shield;

		/// <summary>
		/// Returns whether the character is holding a weapon that can be aimed.
		/// </summary>
		public bool hasAimedWeapon => rightWeapon.IsAimedWeapon();

		/// <summary>
		/// Returns whether the character is in Unarmed or Relax state.
		/// </summary>
		public bool hasNoWeapon => rightWeapon.HasNoWeapon() && leftWeapon.HasNoWeapon();

		/// <summary>
		/// Returns whether the character is holding a weapon that can be cast.
		/// </summary>
		public bool hasCastableWeapon => rightWeapon.IsCastableWeapon() && leftWeapon.IsCastableWeapon();

		#endregion

		private Dictionary<string, IActionHandler> actionHandlers = new Dictionary<string, IActionHandler>();

		#region Initialization

        private void Awake()
        {
            // Setup Animator, add AnimationEvents script.
            animator = GetComponentInChildren<Animator>();

            if (!animator) {
                Debug.LogError("ERROR: There is no Animator Component on child of character.");
                Debug.Break();
            }

            animator.gameObject.AddComponent<RPGCharacterAnimatorEvents>();
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            animator.SetInteger(AnimationParameters.Weapon, 0);
            animator.SetInteger(AnimationParameters.WeaponSwitch, 0);

			// Cache Main camera for cameraRelativeInput.
			mainCamera = Camera.main;
			if (!mainCamera) { Debug.LogError("ERROR: No Main Camera found."); }

			// Find HeadLookController if applied.
			headLookController = GetComponent<PerfectLookAt>();

			// Setup IKhands if used.
            ikHands = GetComponentInChildren<IKHands>();

            SetHandler(HandlerTypes.Aim, new SimpleActionHandler(() => { }, StopAim));
            SetHandler(HandlerTypes.Attack, new Attack());
            SetHandler(HandlerTypes.Block, new SimpleActionHandler(StartBlock, EndBlock));
            SetHandler(HandlerTypes.Cast, new Cast());
            SetHandler(HandlerTypes.AttackCast, new AttackCast());
            SetHandler(HandlerTypes.Crouch, new SimpleActionHandler(StartCrouch, EndCrouch));
            SetHandler(HandlerTypes.Death, new SimpleActionHandler(Death, Revive));
            SetHandler(HandlerTypes.Dodge, new Dodge());
            SetHandler(HandlerTypes.Talk, new Talk());
            SetHandler(HandlerTypes.Emote, new Emote());
            SetHandler(HandlerTypes.EmoteCombat, new EmoteCombat());
            SetHandler(HandlerTypes.Face, new SimpleActionHandler(StartFace, EndFace));
            SetHandler(HandlerTypes.HipShoot, new SimpleActionHandler(() => { }, () => { }));
            SetHandler(HandlerTypes.Injure, new SimpleActionHandler(StartInjured, EndInjured));
            SetHandler(HandlerTypes.Null, new Null());
            SetHandler(HandlerTypes.Reload, new Reload());
            SetHandler(HandlerTypes.Shoot, new Shoot());
            SetHandler(HandlerTypes.SlowTime, new SlowTime());
            SetHandler(HandlerTypes.Sprint, new SimpleActionHandler(StartSprint, EndSprint));
            SetHandler(HandlerTypes.Strafe, new SimpleActionHandler(StartStrafe, EndStrafe));
            SetHandler(HandlerTypes.Turn, new Turn());

            OnLockActions += LockHeadlook;
            OnUnlockActions += UnlockHeadlook;

            // Unlock actions and movement.
            Unlock(true, true);

			// Set Aim Input.
			if (target != null) { SetAimInput(target.transform.position); }
			else { Debug.LogError("ERROR: No Target set for RPGCharacter."); }
		}

		#endregion

		#region ActionHandlers

		/// <summary>
		/// Set an action handler.
		/// </summary>
		/// <param name="action">Name of the action.</param>
		/// <param name="handler">The handler associated with this action.</param>
		public void SetHandler(string action, IActionHandler handler)
        { actionHandlers[action] = handler; }

        /// <summary>
        /// Get an action handler by name. If it doesn't exist, return the Null handler.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        public IActionHandler GetHandler(string action)
        {
            if (HandlerExists(action)) { return actionHandlers[action]; }
            Debug.LogError("RPGCharacterController: No handler for action \"" + action + "\"");
            return actionHandlers[HandlerTypes.Null];
        }

        /// <summary>
        /// Check if a handler exists.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether or not that action exists on this controller.</returns>
        public bool HandlerExists(string action)
        { return actionHandlers.ContainsKey(action); }

        public bool TryGetHandlerActive(string action)
		{ return HandlerExists(action) && IsActive(action); }

        /// <summary>
        /// Check if an action is active.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether the action is active. If the action does not exist, returns false.</returns>
        public bool IsActive(string action)
        { return GetHandler(action).IsActive(); }

        /// <summary>
        /// Check if an action can be started.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether the action can be started. If the action does not exist, returns false.</returns>
        public bool CanStartAction(string action)
        { return GetHandler(action).CanStartAction(this); }

        public bool TryStartAction(string action, object context = null)
        {
	        if (!CanStartAction(action)) { return false; }

	        if (context == null) { StartAction(action); }
	        else { StartAction(action, context);}

	        return true;
        }

        public bool TryEndAction(string action)
        {
	        if (!CanEndAction(action)) { return false; }
	        EndAction(action);
	        return true;
        }

        /// <summary>
        /// Check if an action can be ended.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <returns>Whether the action can be ended. If the action does not exist, returns false.</returns>
        public bool CanEndAction(string action)
        { return GetHandler(action).CanEndAction(this); }

        /// <summary>
        /// Start the action with the specified context. If the action does not exist, there is no effect.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        /// <param name="context">Contextual object used by this action. Leave blank if none is required.</param>
        public void StartAction(string action, object context = null)
        { GetHandler(action).StartAction(this, context); }

        /// <summary>
        /// End the action. If the action does not exist, there is no effect.
        /// </summary>
        /// <param name="action">Name of the action.</param>
        public void EndAction(string action)
        { GetHandler(action).EndAction(this); }

        #endregion

        #region Updates

        private void LateUpdate()
        {
            // Update Animator animation speed.
            animator.SetFloat(AnimationParameters.AnimationSpeed, animationSpeed);

			// Aiming.
            if (isAiming) { Aim(isAiming, aimInput, _bowPull); }
        }

        #endregion

        #region Input

        /// <summary>
        /// Set move input. This method expects the x-axis to be left-right input and the
        /// y-axis to be up-down input.
        ///
        /// The z-axis is ignored, but the type is a Vector3 in case you wish to use the z-axis.
        ///
        /// This method computes CameraRelativeInput using the x and y axis of the move input
        /// and the main camera, producing a normalized Vector3 in the XZ plane.
        /// </summary>
        /// <param name="_moveInput">Vector3 move input.</param>
        public void SetMoveInput(Vector3 _moveInput)
        {
            this._moveInput = _moveInput;

            // Forward vector relative to the camera along the x-z plane.
            var forward = mainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            forward = forward.normalized;

            // Right vector relative to the camera always orthogonal to the forward vector.
            var right = new Vector3(forward.z, 0, -forward.x);
            var relativeVelocity = _moveInput.x * right + _moveInput.y * forward;

            // Reduce input for diagonal movement.
            if (relativeVelocity.magnitude > 1) { relativeVelocity.Normalize(); }

            _cameraRelativeInput = relativeVelocity;
        }

        /// <summary>
        /// Set facing input. This is a position in world space of the object that the character
        /// is facing towards.
        /// </summary>
        /// <param name="_faceInput">Vector3 face input.</param>
        public void SetFaceInput(Vector3 _faceInput)
        { this._faceInput = _faceInput; }

        /// <summary>
        /// Set aim input. This is a position in world space of the object that the character
        /// is aiming at, so that you can easily lock on to a moving target.
        /// </summary>
        /// <param name="_aimInput">Vector3 aim input.</param>
        public void SetAimInput(Vector3 _aimInput)
        { this._aimInput = _aimInput; }

        /// <summary>
        /// Set jump input. Use this with Vector3.up and Vector3.down (y-axis).
        ///
        /// The X and Z axes are  ignored, but the type is a Vector3 in case you wish to
        /// use the X and Z axes for other actions.
        /// </summary>
        /// <param name="_jumpInput">Vector3 jump input.</param>
        public void SetJumpInput(Vector3 _jumpInput)
        { this._jumpInput = _jumpInput; }

        /// <summary>
        /// Set bow pull. This is the amount between 0 and 1 that the character
        /// is drawing back a bowstring. This is only used when the character is wielding a
        /// 2-handed bow.
        /// </summary>
        /// <param name="_bowPull">Float between 0-1.</param>
        public void SetBowPull(float _bowPull)
        { this._bowPull = _bowPull; }

        #endregion

        #region Toggles

        /// <summary>
        /// Toggles headlook on and off.
        /// </summary>
        public void ToggleHeadlook()
        {
			if (headLookController) {
				if (!headLook) { headLookController.EnablePerfectLookat(0.1f); }
				else { headLookController.DisablePerfectLookat(0.1f); }
				headLook = !headLook;
				_isHeadlook = headLook;
			}
		}

        #endregion

        #region Aiming / Shooting

        /// <summary>
        /// Triggers the shoot animation. Use the "Shoot" action for a friendly interface.
        /// </summary>
        /// <param name="action">Which animation to play: 1- normal shoot, 2- hip shooting with rifle.</param>
        public void Shoot(int action)
        { animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, action); }

        /// <summary>
        /// Triggers the reload animation.
        ///
        /// Use the "Reload" action for a friendly interface.
        /// </summary>
        public void Reload()
        {
            animator.SetAnimatorTrigger(AnimatorTrigger.ReloadTrigger);
			SetIKPause(2f);
		}

        /// <summary>
        /// Updates the animator for directional aiming used by 2-Handed Bow and Rifle.
        ///
        /// Use the "Aim" action for a friendly interface.
        /// </summary>
        /// <param name="aiming">Whether or not aiming is enabled.</param>
        /// <param name="target">The position in world space of the target.</param>
        public void Aim(bool aiming, Vector3 target, float bowPull)
        {
            animator.SetBool(AnimationParameters.Aiming, aiming);
            if (!aiming) return;

            var aimTarget = target - transform.position;
            var horizontalTarget = Vector3.ProjectOnPlane(aimTarget, Vector3.up);
            var aimRotation = Quaternion.LookRotation(horizontalTarget, Vector3.up);

            var verticalAngle = Vector3.Angle(horizontalTarget, aimTarget);
            if (aimTarget.y - horizontalTarget.y < 0) { verticalAngle *= -1f; }
            verticalAngle /= 90f;

            var horizontalAngle = Vector3.Angle(transform.forward, horizontalTarget);
            var angleDirection = (((aimRotation.eulerAngles.y - transform.rotation.eulerAngles.y) + 360f) % 360f) > 180f ? -1 : 1;
            horizontalAngle = (horizontalAngle / 180f) * angleDirection;

            animator.SetFloat(AnimationParameters.AimHorizontal, horizontalAngle);
            animator.SetFloat(AnimationParameters.AimVertical, verticalAngle);
            animator.SetFloat(AnimationParameters.BowPull, bowPull);
        }

        /// <summary>
        /// Resets aiming values.
        ///
        /// Use the "Aim" action for a friendly interface.
        /// </summary>
        public void StopAim()
        { Aim(false, Vector3.zero, 0f); }

        #endregion

        #region Movement

        /// <summary>
        /// Sets "Sprinting" in Animator.
        /// </summary>
        public void StartSprint()
        {
            animator.SetBool(AnimationParameters.Sprint, true);
            _canStrafe = false;
        }

        /// <summary>
        /// Unset "Sprinting" in Animator.
        /// </summary>
        public void EndSprint()
        {
            animator.SetBool(AnimationParameters.Sprint, false);
            _canStrafe = true;
        }

        /// <summary>
        /// Turn the character 90/180 degrees.
        ///
        /// Use the "Turn" action for a friendly interface.
        /// </summary>
        /// <param name="direction">1 = Left90, 2 = Right90, 3 = Left180, 4 = Right180.</param>
        public void Turn(TurnType turnType)
        {
	        animator.TriggerTurn(turnType);
			float locktime = 0f;
			if (turnType == TurnType.Left || turnType == TurnType.Right) { locktime = 0.55f; }
			else { locktime = 1.45f; }
            Lock(true, true, true, 0, locktime);
        }

        /// <summary>
        /// Dive Roll.
        ///
        /// Use the "DiveRoll" action for a friendly interface.
        /// </summary>
        /// <param name="rollType">1- Forward.</param>
        public void DiveRoll(DiveRollType rollType)
        {
	        animator.TriggerDiveRoll(rollType);
            Lock(true, true, true, 0, 1f);
			SetIKPause(1.05f);
        }

        /// <summary>
        /// Roll in the specified direction.
        ///
        /// Use the "Roll" action for a friendly interface.
        /// </summary>
        /// <param name="rollNumber">1- Forward, 2- Right, 3- Backward, 4- Left.</param>
        public void Roll(RollType rollNumber)
        {
	        animator.TriggerRoll(rollNumber);
            Lock(true, true, true, 0, 0.5f);
			SetIKPause(0.75f);
		}

        /// <summary>
        /// Knockback in the specified direction.
        ///
        /// Use the "Knockback" action for a friendly interface. Forwards only for Unarmed state.
        /// </summary>
        /// <param name="direction">1- Backwards, 2- Backward version2.</param>
        public void Knockback(KnockbackType direction)
        {
	        animator.TriggerKnockback(direction);
			switch (direction) {
				case KnockbackType.Knockback1: SetIKPause(1.125f);
					Lock(true, true, true, 0, 1f);
					break;
				case KnockbackType.Knockback2: SetIKPause(1f);
					Lock(true, true, true, 0, 0.8f);
					break;
			}
        }

        /// <summary>
        /// Knockdown in the specified direction. Currently only backwards.
        ///
        /// Use the "Knockdown" action for a friendly interface.
        /// </summary>
        /// <param name="direction">1- Backwards.</param>
        public void Knockdown(KnockdownType direction)
        {
	        animator.TriggerKnockdown(direction);
            Lock(true, true, true, 0, 5.25f);
			SetIKPause(5.25f);
		}

        /// <summary>
        /// Dodge the specified direction.
        ///
        /// Use the "Dodge" action for a friendly interface.
        /// </summary>
        /// <param name="direction">1- Left, 2- Right, 3-Backward.</param>
        public void Dodge(DodgeType dodgeType)
        {
            animator.TriggerDodge(dodgeType);
            Lock(true, true, true, 0, 0.55f);
        }

        /// <summary>
        /// Triggers ladder climbing animations.
        ///
        /// Use the "ClimbLadder" action for a friendly interface.
        /// </summary>
        /// <param name="action">1- Climb Up, 2- Climb Down, 3- Dismount Top, 4- Dismount Bottom, 5- Mount Top, 6- Mount Bottom.</param>
        public void ClimbLadder(ClimbType climbType)
        {
            var duration = 0f;

            switch (climbType) {
                case ClimbType.ClimbUp:
                case ClimbType.ClimbDown:
                case ClimbType.MountBottom:
                    duration = 1.167f;
                    break;
                case ClimbType.DismountTop:
                case ClimbType.MountTop:
                    duration = 2.667f;
                    break;
                case ClimbType.DismountBottom:
                    duration = 1.0f;
                    break;
                default:
                    return;
            }

            // Lock actions and set IK off when getting on the ladder.
            if (climbType == ClimbType.MountTop || climbType == ClimbType.MountBottom) {
				SetIKOff();
				Lock(false, true, false, 0f, 0f);
			}

			// Trigger animation.
            animator.TriggerClimb(climbType);

			// If we are getting off the ladder, we should unlock actions too.
			if (climbType == ClimbType.DismountTop || climbType == ClimbType.DismountBottom)
			{ StartCoroutine(_Lock(true, true, true, 0f, duration)); }

			// Manually start the coroutine to lock movement here so that it doesn't clobber
			// the one we started above to lock actions.
			else { StartCoroutine(_Lock(true, false, true, 0f, duration)); }
        }

		/// <summary>
		/// Dodge the specified direction.
		///
		/// Use the "Crawl" action for a friendly interface.
		/// </summary>
		public void Crawl()
		{
			EndAction(HandlerTypes.Strafe);
			EndAction(HandlerTypes.Aim);
			Lock(false, true, false, 0f, 1f);
			SetIKOff();
			animator.TriggerCrawl(CrawlType.Crawl);
		}

		/// <summary>
		/// End Crawling.
		/// </summary>
		public void EndCrawl()
		{ animator.TriggerCrawl(CrawlType.StopCrawl); }

		#endregion

		#region Combat

		/// <summary>
		/// Ends the relaxed state. This is useful for actions which put the character in combat.
		/// </summary>
		public void GetAngry()
        {
            if (isRelaxed) { EndAction(HandlerTypes.Relax); }
        }

        /// <summary>
        /// Trigger an attack animation.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        /// <param name="attackNumber">Animation number to play. See AnimationData.RandomAttackNumber for details.</param>
        /// <param name="attackSide">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
        /// <param name="leftWeapon">Left side weapon. See Weapon enum in AnimationData.cs.</param>
        /// <param name="rightWeapon">Right-hand weapon. See Weapon enum in AnimationData.cs.</param>
        /// <param name="duration">Duration in seconds that animation is locked.</param>
        public void Attack(int attackNumber, Side attackSide, Weapon leftWeapon, Weapon rightWeapon, float duration)
        {
	        animator.SetSide(attackSide);
			_isAttacking = true;
            Lock(true, true, true, 0, duration);

			// If shooting, use regular or hipshooting attack.
			if (rightWeapon == Weapon.Rifle) {
				if (attackSide == Side.None) {
					if (isHipShooting) { attackNumber = 2; }
					else { attackNumber = 1; }
				}
			}

			// Trigger the animation.
			var attackTriggerType = attackSide == Side.Dual ? AnimatorTrigger.AttackDualTrigger : AnimatorTrigger.AttackTrigger;
			animator.SetActionTrigger(attackTriggerType, attackNumber);
		}

        /// <summary>
        /// Trigger the running attack animation.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        /// <param name="side">Side of the attack: 0- None, 1- Left, 2- Right, 3- Dual.</param>
        /// <param name="leftWeapon">Whether to attack on the left side.</param>
        /// <param name="rightWeapon">Whether to attack on the right side.</param>
        /// <param name="dualWeapon">Whether to attack on both sides.</param>
        /// <param name="twoHandedWeapon">If wielding a two-handed weapon.</param>
        public void RunningAttack(Side side, bool leftWeapon, bool rightWeapon, bool dualWeapon, bool twoHandedWeapon)
        {
			if (side == Side.Left && leftWeapon || twoHandedWeapon)
			{ animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, 1); }
			else if (side == Side.Right && rightWeapon)
			{ animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, 4); }
			else if (side == Side.Dual && dualWeapon)
			{ animator.SetActionTrigger(AnimatorTrigger.AttackDualTrigger, 1); }
			else if (hasNoWeapon) {
				animator.SetSide(side);
				animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, 1);
			}
        }

        /// <summary>
        /// Trigger the air attack animation.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        public void AirAttack()
        {
			animator.SetActionTrigger(AnimatorTrigger.AttackTrigger, 1);
			Lock(false, true, true, 0, 0.5f);
			SetIKPause(0.75f);
		}

        /// <summary>
        /// Trigger a kick animation.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        /// <param name="kickSide">1- Left, 2- Right.</param>
        public void AttackKick(int kickSide)
        {
            animator.SetActionTrigger(AnimatorTrigger.AttackKickTrigger, kickSide);
			_isAttacking = true;
            Lock(true, true, true, 0, 0.9f);
        }

        /// <summary>
        /// Start a special attack.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        /// <param name="special">Number of the attack.</param>
        public void StartSpecial(int special)
        {
            animator.SetActionTrigger(AnimatorTrigger.SpecialAttackTrigger, special);
			_isAttacking = true;
            Lock(true, true, true, 0, 0.5f);
        }

        /// <summary>
        /// End a special attack.
        ///
        /// Use the "Attack" action for a friendly interface.
        /// </summary>
        public void EndSpecial()
        {
            animator.SetAnimatorTrigger(AnimatorTrigger.SpecialEndTrigger);
			_isAttacking = false;
            Lock(true, true, true, 0, 0.6f);
            Unlock(true, true);
			SetIKPause(0.6f);
        }

        /// <summary>
        /// Cast a spell.
        ///
        /// Use the "Cast" action for a friendly interface.
        /// </summary>
        /// <param name="attackSide">0- None, 1- Left, 2- Right, 3- Dual.</param>
        /// <param name="castType">Type of spell to cast: Cast | AOE | Summon | Buff.</param>
        public void StartCast(CastType castType, Side attackSide)
        {
	        animator.SetSide(attackSide);
	        animator.TriggerCast(castType);
	        Lock(true, true, false, 0, 0.8f);
        }

        public void StartCast(AttackCastType attackCastType, Side attackSide)
        {
	        animator.SetSide(attackSide);
	        animator.TriggerAttackCast(attackCastType);
			_isAttacking = true;
	        Lock(true, true, false, 0, 0.8f);
        }

        /// <summary>
        /// End spellcasting.
        ///
        /// Use the "Cast" action for a friendly interface.
        /// </summary>
        public void EndCast()
        {
            animator.SetAnimatorTrigger(AnimatorTrigger.CastEndTrigger);
            Lock(true, true, true, 0, 0.1f);
        }

        /// <summary>
        /// Block attacks.
        ///
        /// Use the "Block" action for a friendly interface.
        /// </summary>
        public void StartBlock()
        {
            animator.SetBool(AnimationParameters.Blocking, true);
            animator.SetAnimatorTrigger(AnimatorTrigger.BlockTrigger);
            Lock(true, true, false, 0f, 0f);
			if (hasAimedWeapon) { SetIKOff(); }
        }

        /// <summary>
        /// Stop blocking attacks.
        ///
        /// Use the "Block" action for a friendly interface.
        /// </summary>
        public void EndBlock()
        {
            animator.SetBool(AnimationParameters.Blocking, false);
            Unlock(true, true);
			if (hasAimedWeapon) { SetIKOn(( Weapon )animator.GetInteger(AnimationParameters.Weapon)); }
		}

        /// <summary>
        /// Run left and right while still facing a target.
        ///
        /// Use the "Face" action for a friendly interface.
        /// </summary>
        public void StartFace()
        {
        }

        /// <summary>
        /// Stop facing.
        ///
        /// Use the "Face" action for a friendly interface.
        /// </summary>
        public void EndFace()
        {
        }

        /// <summary>
        /// Strafe left and right while still facing a target.
        ///
        /// Use the "Strafe" action for a friendly interface.
        /// </summary>
        public void StartStrafe()
        {
        }

        /// <summary>
        /// Stop strafing.
        ///
        /// Use the "Strafe" action for a friendly interface.
        /// </summary>
        public void EndStrafe()
        {
        }

        /// <summary>
        /// Get hit with an attack.
        ///
        /// Use the "GetHit" action for a friendly interface.
        /// </summary>
        public void GetHit(int hitNumber)
        {
			GetAngry();
            animator.TriggerGettingHit(hitNumber);
			if (isBlocking) {
				Lock(true, true, false, 0f, 0f);
				SetIKOff();
			}
			else {
				Lock(true, true, true, 0.1f, 0.4f);
				SetIKPause(0.6f);
			}
		}

        /// <summary>
        /// Fall over unconscious.
        ///
        /// Use the "Death" action for a friendly interface.
        /// </summary>
        public void Death()
        {
            EndAction(HandlerTypes.Block);
            animator.SetAnimatorTrigger(AnimatorTrigger.DeathTrigger);
            Lock(true, true, false, 0.1f, 0f);
			SetIKOff();
        }

        /// <summary>
        /// Regain consciousness.
        ///
        /// Use the "Death" action for a friendly interface.
        /// </summary>
        public void Revive()
        {
            animator.SetAnimatorTrigger(AnimatorTrigger.ReviveTrigger);
            GetAngry();
            Lock(true, true, true, 0f, 1f);
			SetIKPause(1f);
        }

        #endregion

        #region Emotes

        /// <summary>
        /// Sit down.
        ///
        /// Use the "Emote" action for a friendly interface.
        /// </summary>
        public void Sit()
        {
	        animator.TriggerEmote(EmoteType.Sit);
            Lock(true, true, false, 0f, 0f);
        }

        /// <summary>
        /// Lay down and sleep.
        ///
        /// Use the "Emote" action for a friendly interface.
        /// </summary>
        public void Laydown()
        {
			animator.TriggerEmote(EmoteType.Laydown);
			Lock(true, true, false, 0f, 0f);
		}

        /// <summary>
        /// Stand when sitting or sleeping.
        ///
        /// Use the "Emote" action for a friendly interface.
        /// </summary>
        public void Stand()
        {
	        var currentActionType = animator.GetInteger(AnimationParameters.Action);

			// Sitting.
			if (currentActionType == 0)	{
		        animator.TriggerEmote(EmoteType.StandFromSitting);
		        Lock(true, true, true, 0f, 1f);
	        }
			// Lying Down.
			else if (currentActionType == 1) {
		        animator.TriggerEmote(EmoteType.StandFromLaying);
		        Lock(true, true, true, 0f, 2f);
	        }
        }

        /// <summary>
        /// Pickup an item.
        ///
        /// Use the "EmoteCombat" action for a friendly interface.
        /// </summary>
        public void Pickup()
        {
			if (hasLeftWeapon) { animator.SetInteger("Side", 2); }
			else { animator.SetInteger("Side", 1); }
	        animator.TriggerEmote(EmoteType.Pickup);
            Lock(true, true, true, 0, 1.4f);
			SetIKPause(1.2f);
        }

		/// <summary>
		/// Activate a button or switch.
		///
		/// Use the "EmoteCombat" action for a friendly interface.
		/// </summary>
		public void Activate()
		{
			if (hasLeftWeapon) { animator.SetInteger("Side", 2); }
			else { animator.SetInteger("Side", 1); }
			animator.TriggerEmote(EmoteType.Activate);
			Lock(true, true, true, 0, 1.2f);
			SetIKPause(rightWeapon == Weapon.TwoHandAxe ? 1.4f : 1f);
		}

		/// <summary>
		/// Take a swig.
		///
		/// Use the "Emote" action for a friendly interface.
		/// </summary>
		public void Drink()
        {
	        animator.TriggerEmote(EmoteType.Drink);
            Lock(true, true, true, 0, 1f);
        }

		/// <summary>
		/// Take a bow.
		///
		/// Use the "Emote" action for a friendly interface.
		/// </summary>
		public void Bow()
		{
			var bowType = AnimationData.RandomBow();
			animator.TriggerEmote(bowType);
			Lock(true, true, true, 0, 3f);
		}

		/// <summary>
		/// Shake head no.
		///
		/// Use the "Emote" action for a friendly interface.
		/// </summary>
		public void No()
		{
			animator.TriggerEmote(EmoteType.No);
			Lock(true, true, true, 0, 1f);
		}

		/// <summary>
		/// Nod head yes.
		///
		/// Use the "Emote" action for a friendly interface.
		/// </summary>
		public void Yes()
		{
			animator.TriggerEmote(EmoteType.Yes);
			Lock(true, true, true, 0, 1f);
		}

		/// <summary>
		/// Do a victorious leap.
		///
		/// Use the "EmoteCombat" action for a friendly interface.
		/// </summary>
		public void Boost()
		{
			SetIKPause(1f);
			animator.TriggerEmote(EmoteType.Boost);
			Lock(true, true, true, 0, 1f);
		}

		/// <summary>
		/// Switch to the injured state.
		///
		/// Use the "Injure" action for a friendly interface.
		/// </summary>
		public void StartInjured()
		{ animator.SetBool(AnimationParameters.Injured, true); }

		/// <summary>
		/// Recover from the injured state.
		///
		/// Use the "Injure" action for a friendly interface.
		/// </summary>
		public void EndInjured()
        { animator.SetBool(AnimationParameters.Injured, false); }

        /// <summary>
        /// Crouch to move stealthily.
        ///
        /// Use the "Crouch" action for a friendly interface.
        /// </summary>
        public void StartCrouch()
        { animator.SetBool(AnimationParameters.Crouch, true); }

        /// <summary>
        /// Stand from a crouching position
        ///
        /// Use the "Crouch" action for a friendly interface.
        /// </summary>
        public void EndCrouch()
        { animator.SetBool(AnimationParameters.Crouch, false); }

        /// <summary>
        /// Start a conversation.
        ///
        /// Use the "Emote" action for a friendly interface.
        /// </summary>
        public void StartConversation()
        {
			Debug.Log("StartConversation.");
            StartCoroutine(_PlayConversationClip());
            Lock(true, true, false, 0f, 0f);
        }

        /// <summary>
        /// Stop a conversation.
        ///
        /// Use the "Emote" action for a friendly interface.
        /// </summary>
        public void EndConversation()
        {
	        animator.TriggerTalking(TalkType.None);
            StopCoroutine(nameof(_PlayConversationClip));
            Unlock(true, true);
        }

        /// <summary>
        /// Plays a random conversation animation.
        /// </summary>
        /// <returns>IEnumerator for use with StartCoroutine.</returns>
        private IEnumerator _PlayConversationClip()
        {
            if (!isTalking) { yield break; }
            var talkingType = AnimationVariations.Conversations.TakeRandom();
            animator.TriggerTalking(talkingType);
            yield return new WaitForSeconds(2f);
            StartCoroutine(_PlayConversationClip());
        }

        /// <summary>
        /// Plays random idle animation. Currently only Alert1 animation.
        /// </summary>
        public void RandomIdle()
        {
	        if (!idleAlert || !isIdle || isRelaxed || isAiming) return;
	        animator.SetActionTrigger(AnimatorTrigger.IdleTrigger, 1);
	        Lock(true, true, true, 0, 1.25f);
	        SetIKPause(2.125f);
        }

        #endregion

        #region Misc

        /// <summary>
        /// Gets the object with the animator on it. Useful if that object is a child of this one.
        /// </summary>
        /// <returns>GameObject to which the animator is attached.</returns>
        public GameObject GetAnimatorTarget()
        { return animator.gameObject; }

		/// <summary>
		/// Returns the current animation length of the given animation layer.
		/// </summary>
		/// <param name="animationlayer">The animation layer being checked.</param>
		/// <returns>Float time of the currently played animation on animationlayer.</returns>
		private float CurrentAnimationLength(int animationlayer)
		{ return animator.GetCurrentAnimatorClipInfo(animationlayer).Length; }

        /// <summary>
        /// Stop character from looking at target.
        /// </summary>
        private void LockHeadlook()
        {
			if (headLook) {
				_isHeadlook = false;
				if (headLookController && headLook) { headLookController.DisablePerfectLookat(0.1f); }
			}
		}

        /// <summary>
        /// Make character look at target.
        /// </summary>
        private void UnlockHeadlook()
        {
            if (headLook) {
				_isHeadlook = true;
				if (headLookController) { headLookController.EnablePerfectLookat(0.1f); }
			}
        }

        /// <summary>
        /// Lock character movement and/or action, on a delay for a set time.
        /// </summary>
        /// <param name="lockMovement">If set to <c>true</c> lock movement.</param>
        /// <param name="lockAction">If set to <c>true</c> lock action.</param>
        /// <param name="timed">If set to <c>true</c> timed.</param>
        /// <param name="delayTime">Delay time.</param>
        /// <param name="lockTime">Lock time. Set to -1 for infinite.</param>
        public void Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime)
        {
            StopCoroutine("_Lock");
            StartCoroutine(_Lock(lockMovement, lockAction, timed, delayTime, lockTime));
        }

        private IEnumerator _Lock(bool lockMovement, bool lockAction, bool timed, float delayTime, float lockTime)
        {
            if (delayTime > 0) { yield return new WaitForSeconds(delayTime); }

            if (lockMovement) {
                _canMove = false;
                OnLockMovement();
            }
            if (lockAction) {
                _canAction = false;
                OnLockActions();
            }
            if (timed) {
                if (lockTime > 0) { yield return new WaitForSeconds(lockTime); }
                Unlock(lockMovement, lockAction);
            }
        }

        /// <summary>
        /// Let character move and act again.
        /// </summary>
        /// <param name="movement">Unlock movement if true.</param>
        /// <param name="actions">Unlock actions if true.</param>
        public void Unlock(bool movement, bool actions)
        {
            if (movement) {
                _canMove = true;
                OnUnlockMovement();
            }
			if (actions) {
				_canAction = true;
				if (_isAttacking) { _isAttacking = false; }
				OnUnlockActions();
			}
        }

		/// <summary>
		/// Turns IK to 0 instantly.
		/// </summary>
		public void SetIKOff()
		{
			if (!ikHands) return;
			ikHands.leftHandPositionWeight = 0;
			ikHands.leftHandRotationWeight = 0;
			ikHands.canBeUsed = false;
		}

		/// <summary>
		/// Turns IK to 1 instantly.
		/// </summary>
		public void SetIKOn(Weapon weapon)
		{
			if (ikHands) {
				ikHands.canBeUsed = true;
				ikHands.BlendIK(true, 0, 0, weapon);
			}
		}

		/// <summary>
		/// Pauses IK while character uses Left Hand during an animation.
		/// </summary>
		public void SetIKPause(float pauseTime)
		{
			if (ikHands && ikHands.isUsed) { ikHands.SetIKPause(pauseTime); }
		}

		#endregion
	}
}