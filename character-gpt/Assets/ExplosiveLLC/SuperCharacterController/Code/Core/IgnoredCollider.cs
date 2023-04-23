using UnityEngine;

public partial class SuperCharacterController
{
	protected struct IgnoredCollider
	{
		public Collider collider;
		public int layer;

		public IgnoredCollider(Collider collider, int layer)
		{
			this.collider = collider;
			this.layer = layer;
		}
	}
}