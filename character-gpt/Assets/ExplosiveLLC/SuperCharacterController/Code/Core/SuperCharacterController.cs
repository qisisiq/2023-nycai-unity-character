using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom character controller, to be used by attaching the component to an object
/// and writing scripts attached to the same object that recieve the "SuperUpdate" message.
/// </summary>
public partial class SuperCharacterController:MonoBehaviour
{
	[SerializeField]
	private Vector3 debugMove = Vector3.zero;

	[SerializeField]
	private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal;

	[SerializeField]
	private bool fixedTimeStep = false;

	[SerializeField]
	private int fixedUpdatesPerSecond = 0;

	[SerializeField]
	private bool clampToMovingGround = true;

	[SerializeField]
	private bool debugSpheres = true;

	[SerializeField]
	private bool debugGrounding = true;

	[SerializeField]
	private bool debugPushbackMesssages = true;

	[SerializeField]
	private CollisionSphere[] spheres =
		new CollisionSphere[3] {
			new CollisionSphere(0.5f, true, false),
			new CollisionSphere(1.0f, false, false),
			new CollisionSphere(1.5f, false, true),
		};

	public LayerMask Walkable;

	[SerializeField]
	private Collider ownCollider;

	[SerializeField]
	public float radius = 0.5f;

	public float deltaTime { get; private set; }
	public SuperGround currentGround { get; private set; }
	public CollisionSphere feet { get; private set; }
	public CollisionSphere head { get; private set; }

	/// <summary>
	/// Total height of the controller from the bottom of the feet to the top of the head.
	/// </summary>
	public float height => Vector3.Distance(SpherePosition(head), SpherePosition(feet)) + radius * 2;

	public Vector3 up => transform.up;
	public Vector3 down => -transform.up;
	public List<SuperCollision> collisionData { get; private set; }
	public Transform currentlyClampedTo { get; set; }
	public float heightScale { get; set; }
	public float radiusScale { get; set; }
	public bool manualUpdateOnly { get; set; }

	public delegate void UpdateDelegate();
	public event UpdateDelegate AfterSingleUpdate;

	private Vector3 initialPosition;
	private Vector3 groundOffset;
	private Vector3 lastGroundPosition;
	private bool clamping = true;
	private bool slopeLimiting = true;

	private List<Collider> ignoredColliders;
	private List<IgnoredCollider> ignoredColliderStack;

	private const float Tolerance = 0.05f;
	private const float TinyTolerance = 0.01f;
	private const string TemporaryLayer = "TempCast";
	private const int MaxPushbackIterations = 2;
	private int TemporaryLayerIndex;
	private float fixedDeltaTime;

	private static SuperCollisionType defaultCollisionType;

	private void Awake()
	{
		collisionData = new List<SuperCollision>();

		TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

		ignoredColliders = new List<Collider>();
		ignoredColliderStack = new List<IgnoredCollider>();

		currentlyClampedTo = null;

		fixedDeltaTime = 1.0f / fixedUpdatesPerSecond;

		heightScale = 1.0f;

		if (ownCollider) { IgnoreCollider(ownCollider); }
		else { ownCollider = GetComponent<CapsuleCollider>(); }

		foreach (CollisionSphere sphere in spheres) {
			if (sphere.isFeet) { feet = sphere; }
			if (sphere.isHead) { head = sphere; }
		}

		if (feet == null) { Debug.LogError("[SuperCharacterController] Feet not found on controller"); }
		if (head == null) { Debug.LogError("[SuperCharacterController] Head not found on controller"); }

		if (defaultCollisionType == null)
		{ defaultCollisionType = new GameObject("DefaultSuperCollisionType", typeof(SuperCollisionType)).GetComponent<SuperCollisionType>(); }

		currentGround = new SuperGround(Walkable, this, triggerInteraction);

		manualUpdateOnly = false;

		gameObject.SendMessage("SuperStart", SendMessageOptions.DontRequireReceiver);
	}

	private void Update()
	{
		// If we are using a fixed timestep, ensure we run the main update loop
		// a sufficient number of times based on the Time.deltaTime.
		if (manualUpdateOnly) { return; }

		if (!fixedTimeStep) {
			deltaTime = Time.deltaTime;
			SingleUpdate();
			return;
		}
		else {
			float delta = Time.deltaTime;
			while (delta > fixedDeltaTime) {
				deltaTime = fixedDeltaTime;
				SingleUpdate();
				delta -= fixedDeltaTime;
			}
			if (delta > 0f) {
				deltaTime = delta;
				SingleUpdate();
			}
		}
	}

    private void LateUpdate()
    {
        bool isClamping = clamping || currentlyClampedTo != null;
        Transform clampedTo = currentlyClampedTo != null ? currentlyClampedTo : currentGround.transform;

        if (clampToMovingGround && isClamping && clampedTo != null && clampedTo.position - lastGroundPosition != Vector3.zero)
        { transform.position += clampedTo.position - lastGroundPosition; }
    }

	public void ManualUpdate(float deltaTime)
	{
		this.deltaTime = deltaTime;
		SingleUpdate();
	}

	private void SingleUpdate()
	{
		initialPosition = transform.position;

		ProbeGround(1);

		transform.position += debugMove * deltaTime;

		gameObject.SendMessage("SuperUpdate", SendMessageOptions.DontRequireReceiver);

		collisionData.Clear();

		RecursivePushback(0, MaxPushbackIterations);

		ProbeGround(2);

		if (slopeLimiting) { SlopeLimit(); }

		ProbeGround(3);

		if (clamping) { ClampToGround(); }

		bool isClamping = clamping || currentlyClampedTo != null;
		Transform clampedTo = currentlyClampedTo != null ? currentlyClampedTo : currentGround.transform;

		if (isClamping) { lastGroundPosition = clampedTo.position; }

		if (debugGrounding) { currentGround.DebugGround(true, true, true, true, true); }

		if (AfterSingleUpdate != null) { AfterSingleUpdate(); }
	}

	private void ProbeGround(int iter)
	{
		PushIgnoredColliders();
		currentGround.ProbeGround(SpherePosition(feet), iter);
		PopIgnoredColliders();
	}

	/// <summary>
	/// Prevents the player from walking up slopes of a larger angle than the object's SlopeLimit.
	/// </summary>
	/// <returns>True if the controller attemped to ascend a too steep slope and had their movement limited.</returns>
	private bool SlopeLimit()
	{
		Vector3 n = currentGround.PrimaryNormal();
		float a = Vector3.Angle(n, up);

		if (a > currentGround.superCollisionType.SlopeLimit) {
			Vector3 absoluteMoveDirection = Math3d.ProjectVectorOnPlane(n, transform.position - initialPosition);

			// Retrieve a vector pointing down the slope.
			Vector3 r = Vector3.Cross(n, down);
			Vector3 v = Vector3.Cross(r, n);

			float angle = Vector3.Angle(absoluteMoveDirection, v);

			if (angle <= 90.0f) { return false; }

			// Calculate where to place the controller on the slope, or at the bottom, based on the desired movement distance.
			Vector3 resolvedPosition = Math3d.ProjectPointOnLine(initialPosition, r, transform.position);
			Vector3 direction = Math3d.ProjectVectorOnPlane(n, resolvedPosition - transform.position);

			// Check if our path to our resolved position is blocked by any colliders.
			if (Physics.CapsuleCast(SpherePosition(feet),
				SpherePosition(head), radius, direction.normalized, out RaycastHit hit,
				direction.magnitude, Walkable, triggerInteraction)) {
				transform.position += v.normalized * hit.distance;
			}
			else { transform.position += direction; }

			return true;
		}

		return false;
	}

	private void ClampToGround()
	{
		float d = currentGround.Distance();
		transform.position -= up * d;
	}

	public void EnableClamping()
	{ clamping = true; }

	public void DisableClamping()
	{ clamping = false; }

	public void EnableSlopeLimit()
	{ slopeLimiting = true; }

	public void DisableSlopeLimit()
	{ slopeLimiting = false; }

	public bool IsClamping()
	{ return clamping; }

	/// <summary>
	/// Check if any of the CollisionSpheres are colliding with any walkable objects in the world.
	/// If they are, apply a proper pushback and retrieve the collision data.
	/// </summary>
	private void RecursivePushback(int depth, int maxDepth)
	{
		PushIgnoredColliders();

		bool contact = false;

		foreach (CollisionSphere sphere in spheres) {
			foreach (Collider col in Physics.OverlapSphere((SpherePosition(sphere)), radius, Walkable, triggerInteraction)) {
				Vector3 position = SpherePosition(sphere);
				bool contactPointSuccess = SuperCollider.ClosestPointOnSurface(col, position, radius, out Vector3 contactPoint);

				if (!contactPointSuccess) { return; }

				if (debugPushbackMesssages) { DebugDraw.DrawMarker(contactPoint, 2.0f, Color.cyan, 0.0f, false); }

				Vector3 v = contactPoint - position;
				if (v != Vector3.zero) {

					// Cache the collider's layer so that we can cast against it.
					int layer = col.gameObject.layer;

					col.gameObject.layer = TemporaryLayerIndex;

					// Check which side of the normal we are on.
					bool facingNormal = Physics.SphereCast(new Ray(position, v.normalized),
						TinyTolerance, v.magnitude + TinyTolerance, 1 << TemporaryLayerIndex);

					col.gameObject.layer = layer;

					// Orient and scale our vector based on which side of the normal we are situated.
					if (facingNormal) {
						if (Vector3.Distance(position, contactPoint) < radius) { v = v.normalized * (radius - v.magnitude) * -1; }

						// A previously resolved collision has had a side effect that moved us outside this collider.
						else { continue; }
					}
					else { v = v.normalized * (radius + v.magnitude); }

					contact = true;

					transform.position += v;

					col.gameObject.layer = TemporaryLayerIndex;

					// Retrieve the surface normal of the collided point.
					Physics.SphereCast(new Ray(position + v, contactPoint - (position + v)),
						TinyTolerance, out RaycastHit normalHit, 1 << TemporaryLayerIndex);

					col.gameObject.layer = layer;

					SuperCollisionType superColType = col.gameObject.GetComponent<SuperCollisionType>();

					if (superColType == null) { superColType = defaultCollisionType; }

					// Our collision affected the collider; add it to the collision data.
					SuperCollision collision = new SuperCollision() {
						collisionSphere = sphere,
						superCollisionType = superColType,
						gameObject = col.gameObject,
						point = contactPoint,
						normal = normalHit.normal
					};

					collisionData.Add(collision);
				}
			}
		}

		PopIgnoredColliders();

		if (depth < maxDepth && contact) { RecursivePushback(depth + 1, maxDepth); }
	}

	private void PushIgnoredColliders()
	{
		ignoredColliderStack.Clear();

		for (int i = 0; i < ignoredColliders.Count; i++) {
			Collider col = ignoredColliders[i];
			ignoredColliderStack.Add(new IgnoredCollider(col, col.gameObject.layer));
			col.gameObject.layer = TemporaryLayerIndex;
		}
	}

	private void PopIgnoredColliders()
	{
		for (int i = 0; i < ignoredColliderStack.Count; i++) {
			IgnoredCollider ic = ignoredColliderStack[i];
			ic.collider.gameObject.layer = ic.layer;
		}

		ignoredColliderStack.Clear();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		if (debugSpheres) {
			if (spheres != null) {
				if (heightScale == 0) { heightScale = 1; }

				foreach (CollisionSphere sphere in spheres) {
					Gizmos.color = sphere.isFeet ? Color.green : (sphere.isHead ? Color.yellow : Color.cyan);
					Gizmos.DrawWireSphere(SpherePosition(sphere), radius);
				}
			}
		}
	}

	public Vector3 SpherePosition(CollisionSphere sphere)
	{
		if (sphere.isFeet) { return transform.position + sphere.offset * up; }
		else { return transform.position + sphere.offset * up * heightScale; }
	}

	public bool PointBelowHead(Vector3 point)
	{ return Vector3.Angle(point - SpherePosition(head), up) > 89.0f; }

	public bool PointAboveFeet(Vector3 point)
	{ return Vector3.Angle(point - SpherePosition(feet), down) > 89.0f; }

	public void IgnoreCollider(Collider col)
	{ ignoredColliders.Add(col); }

	public void RemoveIgnoredCollider(Collider col)
	{ ignoredColliders.Remove(col); }

	public void ClearIgnoredColliders()
	{ ignoredColliders.Clear(); }
}