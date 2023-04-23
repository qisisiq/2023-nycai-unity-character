using RPGCharacterAnims.Lookups;
using UnityEngine;
using UnityEngine.AI;

namespace RPGCharacterAnims
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(RPGCharacterController))]
    public class RPGCharacterNavigationController : MonoBehaviour
    {
        // Components.
        [HideInInspector] public NavMeshAgent navMeshAgent;
        private RPGCharacterController rpgCharacterController;
        private RPGCharacterMovementController rpgCharacterMovementController;
        private Animator animator;

		// Variables.
		public bool debugNavigation;
		[HideInInspector] public bool isNavigating;
        [SerializeField] float moveSpeed = 1f;
        [SerializeField] float rotationSpeed = 1f;
        [SerializeField] float maxNavPathLength = 40f;

        void Awake()
        {
            // In order for the navMeshAgent not to interfere with other movement, we want it to be
            // enabled ONLY when we are actually using it.
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = false;

            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgCharacterMovementController = GetComponent<RPGCharacterMovementController>();
            rpgCharacterController.SetHandler(HandlerTypes.Navigation, new Actions.Navigation(this));
		}

        void Start()
        {
            // Check if Animator exists, otherwise pause script.
            animator = rpgCharacterController.animator;
            if (animator != null) { return; }
            Debug.LogError("No Animator component found!");
            Debug.Break();
        }

		void Update()
		{
			if (isNavigating) {

				RotateTowardsMovementDir();

				// Nav mesh speed compared to RPGCharacterMovementController speed is 7:1.
				navMeshAgent.speed = moveSpeed * 7;

				// Set the Animation Controller if the character is moving.
				if (navMeshAgent.velocity.sqrMagnitude > 0) {
					animator.SetBool(AnimationParameters.Moving, true);
					animator.SetFloat(AnimationParameters.VelocityZ, moveSpeed);

					// If sprinting, multiply by the sprint speed.
					if (rpgCharacterController.isSprinting) {
						animator.SetFloat(AnimationParameters.VelocityZ, moveSpeed
						* rpgCharacterMovementController.sprintSpeed);

						navMeshAgent.speed = rpgCharacterMovementController.sprintSpeed * 7;
					}

					// If crawling, multiply by the crawl speed.
					else if (rpgCharacterController.isCrawling) {
						animator.SetFloat(AnimationParameters.VelocityZ, moveSpeed
						* rpgCharacterMovementController.crawlSpeed);

						navMeshAgent.speed = rpgCharacterMovementController.crawlSpeed * 7;
					}

					// If crouching, multiply by the crouch speed.
					else if (rpgCharacterController.isCrouching) {
						animator.SetFloat(AnimationParameters.VelocityZ, moveSpeed
						* rpgCharacterMovementController.crouchSpeed);

						navMeshAgent.speed = rpgCharacterMovementController.crouchSpeed * 7;
					}
				}
				// Stop animation.
				else { StopAnimation(); }
			}

			// Disable the navMeshAgent once the character has reached its destination and set the animation speed to 0.
			if (isNavigating && navMeshAgent.destination == transform.position) {
				StopNavigating();
				StopAnimation();
			}
		}

		/// <summary>
		/// Get length of path for NavMesh.
		/// </summary>
		/// <param name="path">NavMeshPath to get length of.</param>
		private float GetPathLength(NavMeshPath path)
        {
            float total = 0;
			if (path.corners.Length < 2) { return total; }
            for (int i = 0; i < path.corners.Length - 1; i++)
			{ total += Vector3.Distance(path.corners[i], path.corners[i + 1]); }

            return total;
        }

		/// <summary>
		/// Check if a destination can be reached.
		/// </summary>
		/// <param name="destination">Point in world space to check if can be navigated to.</param>
		public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
			if (!hasPath) { return false; }
			if (path.status != NavMeshPathStatus.PathComplete) { return false; }
			if (GetPathLength(path) > maxNavPathLength) { return false; }

            return true;
        }

		/// <summary>
		/// Navigate to the destination using Unity's NavMeshAgent.
		/// </summary>
		/// <param name="destination">Point in world space to navigate to.</param>
		public void MeshNavToPoint(Vector3 destination)
        {
			if (!CanMoveTo(destination)) return;
			if (debugNavigation) { Debug.Log("MeshNavToPoint: " + destination); }

			navMeshAgent.enabled = true;
			navMeshAgent.SetDestination(destination);

			if (navMeshAgent.speed > 0.1f) { isNavigating = true; }
			if (rpgCharacterMovementController != null) { rpgCharacterMovementController.enabled = false; }
		}

        /// <summary>
        /// Stop navigating to the current destination.
        /// </summary>
        public void StopNavigating()
        {
			if (debugNavigation) { Debug.Log("StopNavigating"); }
			isNavigating = false;
			navMeshAgent.enabled = false;
			if (rpgCharacterMovementController != null) { rpgCharacterMovementController.enabled = true; }
		}

		/// <summary>
		/// Stop animation by setting VelocityZ to 0 and Moving to false.
		/// </summary>
		public void StopAnimation()
		{
			animator.SetFloat(AnimationParameters.VelocityZ, 0f);
			animator.SetBool(AnimationParameters.Moving, false);
		}

		/// <summary>
		/// Keeps the character pointing towards the movement direction.
		/// </summary>
		private void RotateTowardsMovementDir()
        {
			if (navMeshAgent.velocity.sqrMagnitude > 0.01f) {
				transform.rotation = Quaternion.Slerp(transform.rotation,
					Quaternion.LookRotation(navMeshAgent.velocity),
					Time.deltaTime * navMeshAgent.angularSpeed * rotationSpeed);

				// Keep X and Z rotation at 0.
				Quaternion q = transform.rotation;
				q.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
				transform.rotation = q;
			}
		}
    }
}