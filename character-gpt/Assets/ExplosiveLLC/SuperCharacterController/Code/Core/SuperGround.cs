using UnityEngine;

public partial class SuperCharacterController
{
	public class SuperGround
	{
		public SuperGround(LayerMask walkable, SuperCharacterController controller, QueryTriggerInteraction triggerInteraction)
		{
			this.walkable = walkable;
			this.controller = controller;
			this.triggerInteraction = triggerInteraction;
		}

		private class GroundHit
		{
			public Vector3 point { get; private set; }
			public Vector3 normal { get; private set; }
			public float distance { get; private set; }

			public GroundHit(Vector3 point, Vector3 normal, float distance)
			{
				this.point = point;
				this.normal = normal;
				this.distance = distance;
			}
		}

		private LayerMask walkable;
		private SuperCharacterController controller;
		private readonly QueryTriggerInteraction triggerInteraction;

		private GroundHit primaryGround;
		private GroundHit nearGround;
		private GroundHit farGround;
		private GroundHit stepGround;
		private GroundHit flushGround;

		public SuperCollisionType superCollisionType { get; private set; }
		public Transform transform { get; private set; }

		private const float groundingUpperBoundAngle = 60.0f;
		private const float groundingMaxPercentFromCenter = 0.85f;
		private const float groundingMinPercentFromcenter = 0.50f;

		/// <summary>
		/// Scan the surface below us for ground. Follow up the initial scan with subsequent scans
		/// designed to test what kind of surface we are standing above and handle different edge cases.
		/// </summary>
		/// <param name="origin">Center of the sphere for the initial SphereCast.</param>
		/// <param name="iter">Debug tool to print out which ProbeGround iteration is being run.
		/// (3 are run each frame for the controller)</param>
		public void ProbeGround(Vector3 origin, int iter)
		{
			ResetGrounds();

			Vector3 up = controller.up;
			Vector3 down = -up;

			Vector3 o = origin + (up * Tolerance);

			// Reduce our radius by Tolerance squared to avoid failing the SphereCast due to clipping with walls.
			float smallerRadius = controller.radius - (Tolerance * Tolerance);

			if (Physics.SphereCast(o, smallerRadius, down, out RaycastHit hit, Mathf.Infinity, walkable, triggerInteraction)) {
				SuperCollisionType superColType = hit.collider.gameObject.GetComponent<SuperCollisionType>();

				if (superColType == null) { superColType = defaultCollisionType; }

				superCollisionType = superColType;
				transform = hit.transform;

				// By reducing the initial SphereCast's radius by Tolerance, our casted sphere no longer fits with
				// our controller's shape. Reconstruct the sphere cast with the proper radius.
				SimulateSphereCast(hit.normal, out hit);

				primaryGround = new GroundHit(hit.point, hit.normal, hit.distance);

				// If we are standing on a perfectly flat surface, we cannot be either on an edge,
				// On a slope or stepping off a ledge.
				if (Vector3.Distance(Math3d.ProjectPointOnPlane(controller.up, controller.transform.position, hit.point),
					controller.transform.position) < TinyTolerance) {
					return;
				}

				// As we are standing on an edge, we need to retrieve the normals of the two
				// faces on either side of the edge and store them in nearHit and farHit.
				Vector3 toCenter = Math3d.ProjectVectorOnPlane(up, (controller.transform.position - hit.point).normalized * TinyTolerance);

				Vector3 awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, up)) * -toCenter;

				Vector3 nearPoint = hit.point + toCenter + (up * TinyTolerance);
				Vector3 farPoint = hit.point + (awayFromCenter * 3);

				Physics.Raycast(nearPoint, down, out RaycastHit nearHit, Mathf.Infinity, walkable, triggerInteraction);
				Physics.Raycast(farPoint, down, out RaycastHit farHit, Mathf.Infinity, walkable, triggerInteraction);

				nearGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance);
				farGround = new GroundHit(farHit.point, farHit.normal, farHit.distance);

				// If we are currently standing on ground that should be counted as a wall,
				// we are likely flush against it on the ground. Retrieve what we are standing on.
				if (Vector3.Angle(hit.normal, up) > superColType.StandAngle) {

					// Retrieve a vector pointing down the slope.
					Vector3 r = Vector3.Cross(hit.normal, down);
					Vector3 v = Vector3.Cross(r, hit.normal);

					Vector3 flushOrigin = hit.point + hit.normal * TinyTolerance;

					if (Physics.Raycast(flushOrigin, v, out RaycastHit flushHit, Mathf.Infinity, walkable, triggerInteraction)) {

						if (SimulateSphereCast(flushHit.normal, out RaycastHit sphereCastHit))
						{ flushGround = new GroundHit(sphereCastHit.point, sphereCastHit.normal, sphereCastHit.distance); }

						// Uh oh.
						else { }
					}
				}

				// If we are currently standing on a ledge then the face nearest the center of the
				// controller should be steep enough to be counted as a wall. Retrieve the ground
				// it is connected to at it's base, if there exists any.
				if (Vector3.Angle(nearHit.normal, up) > superColType.StandAngle || nearHit.distance > Tolerance) {
					SuperCollisionType col = null;

					if (nearHit.collider != null) { col = nearHit.collider.gameObject.GetComponent<SuperCollisionType>(); }

					if (col == null) { col = defaultCollisionType; }

					// We contacted the wall of the ledge, rather than the landing. Raycast down
					// the wall to retrieve the proper landing.
					if (Vector3.Angle(nearHit.normal, up) > col.StandAngle) {

						// Retrieve a vector pointing down the slope.
						Vector3 r = Vector3.Cross(nearHit.normal, down);
						Vector3 v = Vector3.Cross(r, nearHit.normal);

						if (Physics.Raycast(nearPoint, v, out RaycastHit stepHit, Mathf.Infinity, walkable, triggerInteraction))
						{ stepGround = new GroundHit(stepHit.point, stepHit.normal, stepHit.distance); }
					}
					else { stepGround = new GroundHit(nearHit.point, nearHit.normal, nearHit.distance); }
				}
			}
			// If the initial SphereCast fails, likely due to the controller clipping a wall,
			// fallback to a raycast simulated to SphereCast data.
			else if (Physics.Raycast(o, down, out hit, Mathf.Infinity, walkable, triggerInteraction)) {
				SuperCollisionType superColType = hit.collider.gameObject.GetComponent<SuperCollisionType>();

				if (superColType == null) { superColType = defaultCollisionType; }

				superCollisionType = superColType;
				transform = hit.transform;

				if (SimulateSphereCast(hit.normal, out RaycastHit sphereCastHit))
				{ primaryGround = new GroundHit(sphereCastHit.point, sphereCastHit.normal, sphereCastHit.distance); }
				else { primaryGround = new GroundHit(hit.point, hit.normal, hit.distance); }
			}
			else { Debug.LogWarning("[SuperCharacterController.cs]: WALKABLE LAYER NOT SET OR MISSING 'TempCast' LAYER.  SEE README FILE."); }
		}

		private void ResetGrounds()
		{
			primaryGround = null;
			nearGround = null;
			farGround = null;
			flushGround = null;
			stepGround = null;
		}

		public bool IsGrounded(bool currentlyGrounded, float distance)
		{ return IsGrounded(currentlyGrounded, distance, out Vector3 n); }

		public bool IsGrounded(bool currentlyGrounded, float distance, out Vector3 groundNormal)
		{
			groundNormal = Vector3.zero;

			if (primaryGround == null || primaryGround.distance > distance) { return false; }

			// Check if we are flush against a wall.
			if (farGround != null && Vector3.Angle(farGround.normal, controller.up)
				> superCollisionType.StandAngle) {
				if (flushGround != null && Vector3.Angle(flushGround.normal, controller.up)
					< superCollisionType.StandAngle && flushGround.distance < distance) {
					groundNormal = flushGround.normal;
					return true;
				}
				return false;
			}

			// Check if we are at the edge of a ledge, or on a high angle slope.
			if (farGround != null && !OnSteadyGround(farGround.normal, primaryGround.point)) {

				// Check if we are walking onto steadier ground.
				if (nearGround != null && nearGround.distance
					< distance && Vector3.Angle(nearGround.normal, controller.up)
					< superCollisionType.StandAngle && !OnSteadyGround(nearGround.normal, nearGround.point)) {
					groundNormal = nearGround.normal;
					return true;
				}
				// Check if we are on a step or stair.
				if (stepGround != null && stepGround.distance
					< distance && Vector3.Angle(stepGround.normal, controller.up)
					< superCollisionType.StandAngle) {
					groundNormal = stepGround.normal;
					return true;
				}
				return false;
			}

			if (farGround != null) { groundNormal = farGround.normal; }
			else { groundNormal = primaryGround.normal; }

			return true;
		}

		/// <summary>
		/// To help the controller smoothly "fall" off surfaces and not hang on the edge of ledges,
		/// check that the ground below us is "steady", or that the controller is not standing
		/// on too extreme of a ledge.
		/// </summary>
		/// <param name="normal">Normal of the surface to test against.</param>
		/// <param name="point">Point of contact with the surface.</param>
		/// <returns>True if the ground is steady.</returns>
		private bool OnSteadyGround(Vector3 normal, Vector3 point)
		{
			float angle = Vector3.Angle(normal, controller.up);
			float angleRatio = angle / groundingUpperBoundAngle;
			float distanceRatio = Mathf.Lerp(groundingMinPercentFromcenter, groundingMaxPercentFromCenter, angleRatio);
			Vector3 p = Math3d.ProjectPointOnPlane(controller.up, controller.transform.position, point);
			float distanceFromCenter = Vector3.Distance(p, controller.transform.position);

			return distanceFromCenter <= distanceRatio * controller.radius;
		}

		public Vector3 PrimaryNormal()
		{ return primaryGround.normal; }

		public float Distance()
		{ return primaryGround.distance; }

		public void DebugGround(bool primary, bool near, bool far, bool flush, bool step)
		{
			if (primary && primaryGround != null)
			{ DebugDraw.DrawVector(primaryGround.point, primaryGround.normal, 2.0f, 1.0f, Color.yellow, 0, false); }

			if (near && nearGround != null)
			{ DebugDraw.DrawVector(nearGround.point, nearGround.normal, 2.0f, 1.0f, Color.blue, 0, false); }

			if (far && farGround != null)
			{ DebugDraw.DrawVector(farGround.point, farGround.normal, 2.0f, 1.0f, Color.red, 0, false); }

			if (flush && flushGround != null)
			{ DebugDraw.DrawVector(flushGround.point, flushGround.normal, 2.0f, 1.0f, Color.cyan, 0, false); }

			if (step && stepGround != null)
			{ DebugDraw.DrawVector(stepGround.point, stepGround.normal, 2.0f, 1.0f, Color.green, 0, false); }
		}

		/// <summary>
		/// Provides raycast data based on where a SphereCast would contact the specified normal
		/// Raycasting downwards from a point along the controller's bottom sphere, based on the provided
		/// normal.
		/// </summary>
		/// <param name="groundNormal">Normal of a triangle assumed to be directly below the controller.</param>
		/// <param name="hit">Simulated SphereCast data.</param>
		/// <returns>True if the raycast is successful.</returns>
		private bool SimulateSphereCast(Vector3 groundNormal, out RaycastHit hit)
		{
			float groundAngle = Vector3.Angle(groundNormal, controller.up) * Mathf.Deg2Rad;

			Vector3 secondaryOrigin = controller.transform.position + controller.up * Tolerance;

			if (!Mathf.Approximately(groundAngle, 0)) {
				float horizontal = Mathf.Sin(groundAngle) * controller.radius;
				float vertical = (1.0f - Mathf.Cos(groundAngle)) * controller.radius;

				// Retrieve a vector pointing up the slope.
				Vector3 r2 = Vector3.Cross(groundNormal, controller.down);
				Vector3 v2 = -Vector3.Cross(r2, groundNormal);

				secondaryOrigin += Math3d.ProjectVectorOnPlane(controller.up, v2).normalized * horizontal + controller.up * vertical;
			}

			if (Physics.Raycast(secondaryOrigin, controller.down, out hit, Mathf.Infinity, walkable, triggerInteraction)) {

				// Remove the tolerance from the distance travelled.
				hit.distance -= Tolerance + TinyTolerance;

				return true;
			}
			else { return false; }
		}
	}
}