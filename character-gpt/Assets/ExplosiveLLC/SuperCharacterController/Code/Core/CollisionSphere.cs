using System;

[Serializable]
public class CollisionSphere
{
	public float offset;
	public bool isFeet;
	public bool isHead;

	public CollisionSphere(float offset, bool isFeet, bool isHead)
	{
		this.offset = offset;
		this.isFeet = isFeet;
		this.isHead = isHead;
	}
}