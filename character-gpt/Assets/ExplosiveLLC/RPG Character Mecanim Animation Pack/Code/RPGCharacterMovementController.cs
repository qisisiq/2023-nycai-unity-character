using System.Collections;
using RPGCharacterAnims.Actions;
using RPGCharacterAnims.Extensions;
using RPGCharacterAnims.Lookups;
using UnityEngine;

namespace RPGCharacterAnims
{
	public class RPGCharacterMovementController : SuperStateMachine
    {
        // Components.
        private SuperCharacterController superCharacterController;
        private RPGCharacterController rpgCharacterController;
        private Rigidbody rb;
        private Animator animator;
        private CapsuleCollider capCollider;

		/// <summary>
		/// Returns whether the character is within specified distance above a Walkable layer.
		/// </summary>
		public bool acquiringGround => superCharacterController.currentGround.IsGrounded(false, 0.01f);

		/// <summary>
		/// Returns whether the character is within specified distance above a Walkable layer.
		/// </summary>
		public bool maintainingGround => superCharacterController.currentGround.IsGrounded(true, 1f);

		[HideInInspector] public Vector3 lookDirection { get; private set; }

		[Header("Knockback")]
        /// <summary>
        /// Multiplies the amount of knockback force a character recieves when they get hit.
        /// </summary>
        public float knockbackMultiplier = 1f;

		[Header("Movement Multiplier")]
        /// <summary>
        /// Multiplies the speed of animation velocity.
        /// </summary>
        public float movementAnimationMultiplier = 1f;

        /// <summary>
        /// If the character entered the ladder on the bottom.
        /// </summary>
        private bool ladderStartBottom;

        /// <summary>
        /// Vector3 movement velocity.
        /// </summary>
        [HideInInspector] public Vector3 currentVelocity;

        [Header("Movement")]
        /// <summary>
        /// Movement speed while walking and strafing.
        /// </summary>
        public float walkSpeed = .5f;

        /// <summary>
        /// Walking acceleration.
        /// </summary>
        public float walkAccel = 15f;

        /// <summary>
        /// Movement speed while running. (the default movement)
        /// </summary>
        public float runSpeed = 1f;

        /// <summary>
        /// Running acceleration.
        /// </summary>
        public float runAccel = 30f;

        /// <summary>
        /// Movement speed while sprinting.
        /// </summary>
        public float sprintSpeed = 2f;

        /// <summary>
        /// Sprinting acceleration.
        /// </summary>
        public float sprintAccel = 15;

		/// <summary>
		/// Movement speed while crouched.
		/// </summary>
		public float crouchSpeed = 0.25f;

		/// <summary>
		/// Crouched acceleration.
		/// </summary>
		public float crouchAccel = 15;

		/// <summary>
		/// Movement speed while crawling.
		/// </summary>
		public float crawlSpeed = 0.15f;

		/// <summary>
		/// Crawling acceleration.
		/// </summary>
		public float crawlAccel = 15;

		/// <summary>
		/// Movement speed while injured.
		/// </summary>
		public float injuredSpeed = .675f;

        /// <summary>
        /// Acceleration while injured.
        /// </summary>
        public float injuredAccel = 20f;

        /// <summary>
        /// Ground friction, slows the character to a stop.
        /// </summary>
        public float groundFriction = 120f;

        /// <summary>
        /// Speed of rotation when turning the character to face movement direction or target.
        /// </summary>
        public float rotationSpeed = 100f;

        /// <summary>
        /// Determine is the character is sprinting or not.
        /// </summary>
        private bool isSprinting;

        /// <summary>
        /// Internal flag for when the character can jump.
        /// </summary>
        [HideInInspector] public bool canJump;

        /// <summary>
        /// Internal flag for if the player is holding the jump input. If this is released while
        /// the character is still ascending, the vertical speed is damped.
        /// </summary>
        [HideInInspector] public bool holdingJump;

        /// <summary>
        /// Internal flag for if the character can perform a double jump.
        /// </summary>
        [HideInInspector] public bool canDoubleJump = false;
        private bool doublejumped = false;

        [Header("Jumping")]
        /// <summary>
        /// Jumping speed while ascending.
        /// </summary>
        public float jumpSpeed = 12f;

        /// <summary>
        /// Gravity while ascending.
        /// </summary>
        public float jumpGravity = 24f;

		/// <summary>
		/// Allow doubleJumping.
		/// </summary>
		public bool allowDoubleJump = true;

		/// <summary>
		/// Double jump speed.
		/// </summary>
		public float doubleJumpSpeed = 8f;

        /// <summary>
        /// Horizontal speed while in the air.
        /// </summary>
        public float inAirSpeed = 8f;

        /// <summary>
        /// Horizontal acceleration while in the air.
        /// </summary>
        public float inAirAccel = 16f;

		/// <summary>
		/// Gravity while descending. Default is higher than ascending gravity (like a Mario jump).
		/// </summary>
		public float fallGravity = 32f;

		/// <summary>
		/// Allows control while character is falling.
		/// </summary>
		public bool fallingControl = false;

        [Header("Swimming")]
        /// <summary>
        /// Horizontal swim speed.
        /// </summary>
        public float swimSpeed = 4f;

        /// <summary>
        /// Swimming acceleration.
        /// </summary>
        public float swimAccel = 4f;

        /// <summary>
        /// Vertical swim speed.
        /// </summary>
        public float strokeSpeed = 6f;

        /// <summary>
        /// Friction in water which slows the character to a stop.
        /// </summary>
        public float waterFriction = 5f;

		[Header("Debug Options")]
		public bool debugMessages;

		#region Initiation

		private void Awake()
        {
            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterController.SetHandler(HandlerTypes.AcquiringGround, new SimpleActionHandler(() => { }, () => { }));
            rpgCharacterController.SetHandler(HandlerTypes.MaintainingGround, new SimpleActionHandler(() => { }, () => { }));
            rpgCharacterController.SetHandler(HandlerTypes.ClimbLadder, new ClimbLadder(this));
            rpgCharacterController.SetHandler(HandlerTypes.DiveRoll, new DiveRoll(this));
            rpgCharacterController.SetHandler(HandlerTypes.DoubleJump, new DoubleJump(this));
            rpgCharacterController.SetHandler(HandlerTypes.Fall, new Fall(this));
            rpgCharacterController.SetHandler(HandlerTypes.GetHit, new GetHit(this));
            rpgCharacterController.SetHandler(HandlerTypes.Idle, new Idle(this));
            rpgCharacterController.SetHandler(HandlerTypes.Jump, new Jump(this));
            rpgCharacterController.SetHandler(HandlerTypes.Knockback, new Knockback(this));
            rpgCharacterController.SetHandler(HandlerTypes.Knockdown, new Knockdown(this));
            rpgCharacterController.SetHandler(HandlerTypes.Move, new Move(this));
			rpgCharacterController.SetHandler(HandlerTypes.Roll, new Roll(this));
			rpgCharacterController.SetHandler(HandlerTypes.Swim, new Swim(this));
			rpgCharacterController.SetHandler(HandlerTypes.Crawl, new Crawl(this));
		}

        private void Start()
        {
            // Get other RPG Character components.
            superCharacterController = GetComponent<SuperCharacterController>();

            // Check if Animator exists, otherwise pause script.
            animator = GetComponentInChildren<Animator>();
			if (!animator) {
				Debug.LogError("ERROR: THERE IS NO ANIMATOR COMPONENT ON CHILD OF CHARACTER.");
				Debug.Break();
			}

			// Setup Collider and Rigidbody for collisions.
			capCollider = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();

            // Set restraints on startup if using Rigidbody.
            if (rb != null) { rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; }

            rpgCharacterController.OnLockMovement += LockMovement;
            rpgCharacterController.OnUnlockMovement += UnlockMovement;

            var animatorEvents = rpgCharacterController.GetAnimatorTarget().GetComponent<RPGCharacterAnimatorEvents>();
            animatorEvents.OnMove.AddListener(AnimatorMove);
        }

		#endregion

		#region Updates

		/*
		Update is normally run once on every frame update. We won't be using it in this case, since the SuperCharacterController
        component sends a callback Update called SuperUpdate. SuperUpdate is recieved by the SuperStateMachine, and then fires
        further callbacks depending on the state.

        If SuperCharacterController is disabled then we still want the SuperStateMachine to run, so we call SuperUpdate manually.
        */

		void Update()
        {
            if (!superCharacterController.enabled)
			{ gameObject.SendMessage("SuperUpdate", SendMessageOptions.DontRequireReceiver); }
        }

        protected override void EarlyGlobalSuperUpdate()
        {
	        if (acquiringGround) { rpgCharacterController.StartAction(HandlerTypes.AcquiringGround); }
			else { rpgCharacterController.EndAction(HandlerTypes.AcquiringGround); }

            if (maintainingGround) { rpgCharacterController.StartAction(HandlerTypes.MaintainingGround); }
			else { rpgCharacterController.EndAction(HandlerTypes.MaintainingGround); }
        }

		// Put any code in here you want to run AFTER the state's update function.
		// This is run regardless of what state you're in.
        protected override void LateGlobalSuperUpdate()
        {
            // If the movement controller itself is disabled, this shouldn't run.
            if (!enabled) { return; }

            // Move the player by our velocity every frame.
            transform.position += currentVelocity * superCharacterController.deltaTime;

            // If alive and is moving, set animator.
            if (!rpgCharacterController.isDead && rpgCharacterController.canMove) {
                if (currentVelocity.magnitude > 0f) {
                    animator.SetFloat(AnimationParameters.VelocityX, 0);
                    animator.SetFloat(AnimationParameters.VelocityZ, transform.InverseTransformDirection(currentVelocity).z * movementAnimationMultiplier);
                    animator.SetBool(AnimationParameters.Moving, true);
                }
				else {
                    animator.SetFloat(AnimationParameters.VelocityX, 0f);
                    animator.SetFloat(AnimationParameters.VelocityZ, 0f);
                    animator.SetBool(AnimationParameters.Moving, false);
                }
            }
			// Aiming.
			if (rpgCharacterController.isAiming || rpgCharacterController.isStrafing)
			{ RotateTowardsTarget(rpgCharacterController.aimInput); }

			// Facing.
			else if (rpgCharacterController.isFacing) { RotateTowardsDirection(rpgCharacterController.faceInput); }
			else if (rpgCharacterController.canMove) { RotateTowardsMovementDir(); }

            if (currentState == null && rpgCharacterController.CanStartAction(HandlerTypes.Idle))
			{ rpgCharacterController.StartAction(HandlerTypes.Idle); }

			// Update animator with local movement values.
			animator.SetFloat(AnimationParameters.VelocityX, transform.InverseTransformDirection(currentVelocity).x * movementAnimationMultiplier);
			animator.SetFloat(AnimationParameters.VelocityZ, transform.InverseTransformDirection(currentVelocity).z * movementAnimationMultiplier);
		}

        #endregion

        #region States
        // Below are the state functions. Each one is called based on the name of the state, so when currentState = Idle,
        // we call Idle_EnterState. If currentState = Jump, we call Jump_SuperUpdate().

        private void Idle_EnterState()
        {
			if (debugMessages) { Debug.Log("Idle_EnterState"); }
			superCharacterController.EnableSlopeLimit();
            superCharacterController.EnableClamping();
            canJump = true;
            doublejumped = false;
            canDoubleJump = false;
        }

        // Run every frame character is in the idle state.
        private void Idle_SuperUpdate()
        {
			// Check if the character starts falling.
			if (rpgCharacterController.TryStartAction(HandlerTypes.Fall)) { return; }

			// Apply friction to slow to a halt.
			currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, groundFriction * superCharacterController.deltaTime);
			rpgCharacterController.TryStartAction(HandlerTypes.Move);
        }

		private void Idle_ExitState()
		{
			if (debugMessages) { Debug.Log("Idle_ExitState"); }
		}

		private void Move_EnterState()
		{
			if (debugMessages) { Debug.Log("Move_EnterState"); }
		}

		// Run every frame character is moving.
		private void Move_SuperUpdate()
        {
			// Check if the character starts falling.
			if (rpgCharacterController.TryStartAction(HandlerTypes.Fall)) { return; }

            // Set speed determined by movement type.
            if (rpgCharacterController.canMove) {
                var moveSpeed = runSpeed;
                var moveAccel = runAccel;

				if (rpgCharacterController.isInjured) {
                    moveSpeed = injuredSpeed;
                    moveAccel = injuredAccel;
                }
				else if (rpgCharacterController.isStrafing) {
                    moveSpeed = walkSpeed;
                    moveAccel = walkAccel;
                }
				else if (rpgCharacterController.isSprinting) {
                    moveSpeed = sprintSpeed;
                    moveAccel = sprintAccel;
                }
				else if (rpgCharacterController.isCrouching) {
					moveSpeed = crouchSpeed;
					moveAccel = crouchAccel;
				}

				currentVelocity = Vector3.MoveTowards(currentVelocity,
					rpgCharacterController.cameraRelativeInput * moveSpeed,
					moveAccel * superCharacterController.deltaTime);
			}
			// If there is no movement detected, go into Idle.
            rpgCharacterController.TryStartAction(HandlerTypes.Idle);
        }

		private void Move_ExitState()
		{
			if (debugMessages) { Debug.Log("Move_ExitState"); }
		}

		private void Jump_EnterState()
        {
			if (debugMessages) { Debug.Log("Jump_EnterState"); }
			superCharacterController.DisableClamping();
            superCharacterController.DisableSlopeLimit();

            var ySpeed = (CharacterState)lastState == CharacterState.Swim ? strokeSpeed : jumpSpeed;
            currentVelocity = new Vector3(currentVelocity.x, ySpeed, currentVelocity.z);
            animator.SetInteger(AnimationParameters.Jumping, 1);
            animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
            canJump = false;
        }

        private void Jump_SuperUpdate()
        {
            holdingJump = rpgCharacterController.jumpInput.y != 0f;

            // Cap jump speed if we stop holding the jump button.
            if (!holdingJump && currentVelocity.y > (jumpSpeed / 4f)) {
	            var destination = new Vector3(currentVelocity.x, (jumpSpeed / 4f), currentVelocity.z);
                currentVelocity = Vector3.MoveTowards(currentVelocity, destination, fallGravity * superCharacterController.deltaTime);
            }

            var planarMoveDirection = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
            var verticalMoveDirection = currentVelocity - planarMoveDirection;

            // Falling.
            if (currentVelocity.y < 0) {
                currentVelocity = planarMoveDirection;
                currentState = CharacterState.Fall;
                return;
            }

			planarMoveDirection = Vector3.MoveTowards(planarMoveDirection,
				rpgCharacterController.cameraRelativeInput * inAirSpeed,
				inAirAccel * superCharacterController.deltaTime);

            verticalMoveDirection -= superCharacterController.up * jumpGravity * superCharacterController.deltaTime;
            currentVelocity = planarMoveDirection + verticalMoveDirection;
        }

        private void DoubleJump_EnterState()
        {
			if (debugMessages) { Debug.Log("DoubleJump_EnterState"); }
			currentVelocity = new Vector3(currentVelocity.x, doubleJumpSpeed, currentVelocity.z);
            canDoubleJump = false;
            doublejumped = true;
            animator.SetInteger(AnimationParameters.Jumping, 3);
            animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
        }

		/// <summary>
		/// DoubleJump uses the same SuperUpdate as Jump.
		/// </summary>
        private void DoubleJump_SuperUpdate()
        { Jump_SuperUpdate(); }

        private void Fall_EnterState()
        {
			if (debugMessages) { Debug.Log("Fall_EnterState"); }
			if (!doublejumped) { canDoubleJump = true; }
            superCharacterController.DisableClamping();
            superCharacterController.DisableSlopeLimit();
            canJump = false;
            animator.SetInteger(AnimationParameters.Jumping, 2);
            animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
        }

        private void Fall_SuperUpdate()
        {
            if (rpgCharacterController.CanStartAction(HandlerTypes.Idle)) {
                currentVelocity = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
                rpgCharacterController.StartAction(HandlerTypes.Idle);
                return;
            }

			// If FallingControl is enabled.
			if (fallingControl) {
				var planarMoveDirection = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
				var verticalMoveDirection = currentVelocity - planarMoveDirection;

				planarMoveDirection = Vector3.MoveTowards(planarMoveDirection,
					rpgCharacterController.cameraRelativeInput * inAirSpeed,
					inAirAccel * superCharacterController.deltaTime);

				verticalMoveDirection -= superCharacterController.up * fallGravity * superCharacterController.deltaTime;
				currentVelocity = planarMoveDirection + verticalMoveDirection;
			}
			else { currentVelocity -= superCharacterController.up * fallGravity * superCharacterController.deltaTime; }
		}

		private void Fall_ExitState()
		{
			if (debugMessages) { Debug.Log("Fall_ExitState"); }
			animator.SetInteger(AnimationParameters.Jumping, 0);
			animator.SetAnimatorTrigger(AnimatorTrigger.JumpTrigger);
		}

		private void Crawl_EnterState()
		{
			if (debugMessages) { Debug.Log("Crawl_EnterState"); }
			superCharacterController.DisableClamping();
			superCharacterController.DisableSlopeLimit();
		}

		private void Crawl_SuperUpdate()
		{
			var cameraRelativeInput = rpgCharacterController.cameraRelativeInput;

			// Set speed.
			if (rpgCharacterController.canMove) {
				var moveSpeed = crawlSpeed;
				var moveAccel = crawlAccel;

				currentVelocity = Vector3.MoveTowards(currentVelocity,
					cameraRelativeInput * moveSpeed, moveAccel
					* superCharacterController.deltaTime);
			}

			RotateTowardsMovementDir();
		}

		private void Crawl_ExitState()
		{
			if (debugMessages) { Debug.Log("Crawl_ExitState"); }
			rpgCharacterController.OnUnlockMovement += InstantSwitchOnceAfterMoveUnlock;
			rpgCharacterController.Lock(true, true, true, 0f, 1f);
			rpgCharacterController.EndCrawl();
		}

		private void Swim_EnterState()
        {
			if (debugMessages) { Debug.Log("Swim_EnterState"); }
			superCharacterController.DisableClamping();
			superCharacterController.DisableSlopeLimit();
			rpgCharacterController.EndAction(HandlerTypes.Strafe);
			rpgCharacterController.EndAction(HandlerTypes.Aim);
			rpgCharacterController.Lock(false, true, false, 0f, 0f);
			animator.SetAnimatorTrigger(AnimatorTrigger.SwimTrigger);
			animator.SetBool(AnimationParameters.Swimming, true);

			// Scale collider to match position of character.
			superCharacterController.radius = 1.5f;
            if (capCollider) { capCollider.radius = 1.5f; }
        }

        private void Swim_SuperUpdate()
        {
            var cameraRelativeInput = rpgCharacterController.cameraRelativeInput;
            var jumpInput = rpgCharacterController.jumpInput;

            // If moving faster than we should be in the water, slow down a lot.
            if (currentVelocity.magnitude > Mathf.Max(swimSpeed, strokeSpeed)) {
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero,
					waterFriction * waterFriction * superCharacterController.deltaTime);
            }
            // Horizontal swim movement.
            if (cameraRelativeInput.magnitude > 0) {
                currentVelocity = Vector3.MoveTowards(currentVelocity, cameraRelativeInput * swimSpeed,
					swimAccel * superCharacterController.deltaTime);
            }
			else {
                // Apply friction to slow character to a halt on horizontal axes.
                currentVelocity = Vector3.MoveTowards(currentVelocity, new Vector3(0, currentVelocity.y, 0),
					waterFriction * superCharacterController.deltaTime);
            }
            // Apply friction to slow character to a halt on vertical axis.
            currentVelocity = Vector3.MoveTowards(currentVelocity, new Vector3(currentVelocity.x, 0, currentVelocity.z),
				waterFriction * superCharacterController.deltaTime);

            if (jumpInput.y == 0f) { holdingJump = false; }

			// Swim up.
            if (!holdingJump && jumpInput.y > 0) {
				currentVelocity += superCharacterController.up * strokeSpeed;
				animator.SetActionTrigger(AnimatorTrigger.JumpTrigger, 1);
				holdingJump = true;
			}
			// Swim down.
			else if (!holdingJump && jumpInput.y < 0) {
				currentVelocity -= superCharacterController.up * strokeSpeed;
				animator.SetActionTrigger(AnimatorTrigger.JumpTrigger, 2);
				holdingJump = true;
			}
        }

        private void Swim_ExitState()
        {
			if (debugMessages) { Debug.Log("Swim_ExitState"); }
            if (capCollider) { capCollider.radius = 0.6f; }
            superCharacterController.radius = 0.6f;
            rpgCharacterController.Unlock(false, true);
        }

        private void ClimbLadder_EnterState()
        {
			if (debugMessages) { Debug.Log("Ladder_EnterState"); }
			var ladder = rpgCharacterController.ladder;
            var ladderTop = new Vector3(ladder.transform.position.x, ladder.bounds.max.y, ladder.transform.position.z);
            var ladderBottom = new Vector3(ladder.transform.position.x, ladder.bounds.min.y, ladder.transform.position.z);
            var startPoint = ladderStartBottom ? ladderBottom : ladderTop;
            var newVector = Vector3.Cross(ladder.transform.forward, ladder.transform.right);
            Vector3 newSpot;

            if (ladderStartBottom) { newSpot = startPoint + (newVector.normalized * 0.71f); }

			else { newSpot = startPoint + (newVector.normalized * -0.87f); }

            superCharacterController.DisableClamping();
            superCharacterController.DisableSlopeLimit();
            superCharacterController.enabled = false;

            LockMovement();

            if (rb != null) { rb.isKinematic = false; }

			transform.position = newSpot;
            transform.rotation = Quaternion.Euler(transform.rotation.x, ladder.transform.rotation.eulerAngles.y, transform.rotation.z);
        }

        private void ClimbLadder_SuperUpdate()
        {
            var moveInput = rpgCharacterController.moveInput;

            // If no input, don't do anything.
            if (moveInput == Vector3.zero) { return; }

            // If we can't move (i.e. because we're animating) ignore input.
            if (!rpgCharacterController.canMove) { return; }

			// Climb Up.
			if (moveInput.y > 0f) {
                var ladder = rpgCharacterController.ladder;

				// Just above the height of the SuperCharacterController isFeet Sphere offset + height of character in animation (0.4871393)
				var ladderTopThreshold = 1.25f;
				var ladderTop = new Vector3(transform.position.x, ladder.bounds.max.y - ladderTopThreshold, transform.position.z);

                // Climb Off Top or Climb Up.
                if (superCharacterController.PointBelowHead(ladderTop)) {
                    rpgCharacterController.ClimbLadder(ClimbType.DismountTop);
                    rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
                }
				else { rpgCharacterController.ClimbLadder(ClimbType.ClimbUp); }
            }
			// Climb Down.
			else if (moveInput.y < 0f) {
                var ladder = rpgCharacterController.ladder;

				// Just above the height of the SuperCharacterController isFeet Sphere offset + height of character in animation (0.4871393)
				var ladderBottomThreshold = 1.1f;
				var ladderBottom = new Vector3(transform.position.x, ladder.bounds.min.y + ladderBottomThreshold, transform.position.z);
				Debug.DrawRay(ladderBottom, Vector3.up, Color.white, 10f);

                // Climb Off Bottom or Climb Down.
                if (superCharacterController.PointAboveFeet(ladderBottom)) {
                    rpgCharacterController.ClimbLadder(ClimbType.DismountBottom);
                    rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
                }
				else { rpgCharacterController.ClimbLadder(ClimbType.ClimbDown); }
            }
        }

        private void ClimbLadder_ExitState()
        {
			if (debugMessages) { Debug.Log("Ladder_ExitState"); }
            if (rb != null) { rb.isKinematic = true; }

            UnlockMovement();

            superCharacterController.enabled = true;
            superCharacterController.EnableClamping();
            superCharacterController.EnableSlopeLimit();
        }

        private void DiveRoll_EnterState()
        {
			if (debugMessages) { Debug.Log("DiveRoll_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

		private void DiveRoll_SuperUpdate()
		{
			if (rpgCharacterController.CanStartAction(HandlerTypes.Idle)) {
				currentVelocity = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentVelocity);
				rpgCharacterController.StartAction(HandlerTypes.Idle);
				return;
			}
			currentVelocity -= superCharacterController.up * (fallGravity / 2) * superCharacterController.deltaTime;
		}

		private void Roll_EnterState()
        {
			if (debugMessages) { Debug.Log("Roll_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

        private void Knockback_EnterState()
        {
			if (debugMessages) { Debug.Log("Knockback_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

        private void Knockdown_EnterState()
        {
			if (debugMessages) { Debug.Log("Knockdown_EnterState"); }
			rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
		}

		#endregion

        /// <summary>
        /// Set the direction that the ladder is being mounted from.
        /// </summary>
        /// <param name="bottom">Where to start climbing the ladder: true- bottom, false- top.</param>
        public void ClimbLadder(bool ladderStartBottom)
        { this.ladderStartBottom = ladderStartBottom; }

        private void RotateTowardsMovementDir()
        {
            var movementVector = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (movementVector.magnitude > 0.01f) {
                transform.rotation = Quaternion.Slerp(transform.rotation,
					Quaternion.LookRotation(movementVector),
					Time.deltaTime * rotationSpeed);
            }
        }

        private void RotateTowardsTarget(Vector3 targetPosition)
        {
			if (debugMessages) { Debug.Log($"RotateTowardsTarget: {targetPosition}"); }
			var lookTarget = new Vector3(targetPosition.x - transform.position.x, 0, targetPosition.z - transform.position.z);
			if (lookTarget != Vector3.zero) {
				var targetRotation = Quaternion.LookRotation(lookTarget);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
			}
        }

		private void RotateTowardsDirection(Vector3 direction)
		{
			if (debugMessages) { Debug.Log($"RotateTowardsDirection: {direction}"); }
			var lookDirection = new Vector3(direction.x, 0, -direction.y);
			var lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
		}

		/// <summary>
		/// Exert a knockback force on the character. Used by the GetHit, Knockdown, and Knockback
		/// actions.
		/// </summary>
		/// <param name="knockDirection">Vector3 direction knock the character.</param>
		/// <param name="knockBackAmount">Amount to knock back.</param>
		/// <param name="variableAmount">Random variance in knockback.</param>
		public void KnockbackForce(Vector3 knockDirection, float knockBackAmount, float variableAmount)
        { StartCoroutine(_KnockbackForce(knockDirection, knockBackAmount, variableAmount)); }

        private IEnumerator _KnockbackForce(Vector3 knockDirection, float knockBackAmount, float variableAmount)
        {
            if (rb == null) { yield break; }

            var startTime = Time.time;
            var elapsed = 0f;

            rb.isKinematic = false;

            while (elapsed < .1f) {
                rb.AddForce(knockDirection
					* ((knockBackAmount + Random.Range(-variableAmount, variableAmount))
					* knockbackMultiplier * 10), ForceMode.Impulse);
                elapsed = Time.time - startTime;
                yield return null;
            }

            rb.isKinematic = true;
        }

        private void OnTriggerEnter(Collider collide)
        {
			Debug.Log($"OnTriggerEnter: {collide}");

			// Entering a water volume.
			if (collide.gameObject.layer == 4) { rpgCharacterController.StartAction(HandlerTypes.Swim); }

            // Near a ladder.
            else if (collide.transform.parent != null) {
                if (collide.transform.parent.name.Contains("Ladder")) {
                    rpgCharacterController.isNearLadder = true;
                    rpgCharacterController.ladder = collide;
                }
            }
            // Near a cliff.
            else if (collide.transform.name.Contains("Cliff")) {
                rpgCharacterController.isNearCliff = true;
                rpgCharacterController.cliff = collide;
            }
        }

        private void OnTriggerExit(Collider collide)
        {
			Debug.Log($"OnTriggerExit: {collide}");

			// Leaving a water volume.
			if (collide.gameObject.layer == 4) {
                animator.SetBool(AnimationParameters.Swimming, false);

                // Normally we don't set the state directly, but here we make an exception.
                // The controller can Jump, though the player cannot.
                currentState = CharacterState.Jump;
            }
            // Leaving a ladder.
            else if (collide.transform.parent != null) {
                if (collide.transform.parent.name.Contains("Ladder")) {
                    rpgCharacterController.isNearLadder = false;
                    rpgCharacterController.ladder = null;
                }
            }
            // Leaving a cliff.
            else if (collide.transform.name.Contains("Cliff")) {
                rpgCharacterController.isNearCliff = false;
                rpgCharacterController.cliff = null;
            }
        }

        /// <summary>
        /// Event listener for when RPGCharacterController.OnLockMovement is called.
        /// </summary>
        public void LockMovement()
        {
            currentVelocity = new Vector3(0, 0, 0);
            animator.SetBool(AnimationParameters.Moving, false);
            animator.applyRootMotion = true;
        }

        /// <summary>
        /// Event listener for when RPGCharacterController.OnUnlockMovement is called.
        /// </summary>
        public void UnlockMovement()
        { animator.applyRootMotion = false; }

        /// <summary>
        /// Event listener for when RPGCharacterAnimatorEvents.OnMove is called.
        /// </summary>
        /// <param name="deltaPosition">Change in position.</param>
        /// <param name="rootRotation">Change in rotation.</param>
        public void AnimatorMove(Vector3 deltaPosition, Quaternion rootRotation)
        {
            transform.position += deltaPosition;
            transform.rotation = rootRotation;
        }

        /// <summary>
        /// Event listener to return to the Idle state once movement is unlocked, which executes
        /// once. Use with the RPGCharacterController.OnUnlockMovement event.
        ///
        /// e.g.: rpgCharacterController.OnUnlockMovement += IdleOnceAfterMoveUnlock;
        /// </summary>
        public void IdleOnceAfterMoveUnlock()
        {
            rpgCharacterController.StartAction(HandlerTypes.Idle);
            rpgCharacterController.OnUnlockMovement -= IdleOnceAfterMoveUnlock;
        }

        /// <summary>
        /// Event listener to instant switch once movement is unlocked, which executes only
        /// once. Use with the RPGCharacterController.OnUnlockMovement event. This is used by
        /// the Crawl->Crouch transition to get back into crouching.
        ///
        /// e.g.: rpgCharacterController.OnUnlockMovement += InstantSwitchOnceAfterMoveUnlock;
        /// </summary>
        public void InstantSwitchOnceAfterMoveUnlock()
        {
            animator.SetAnimatorTrigger(AnimatorTrigger.InstantSwitchTrigger);
            rpgCharacterController.OnUnlockMovement -= InstantSwitchOnceAfterMoveUnlock;
        }
    }
}