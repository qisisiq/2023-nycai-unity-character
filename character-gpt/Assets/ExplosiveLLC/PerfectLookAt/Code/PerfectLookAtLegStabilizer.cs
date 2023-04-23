using UnityEngine;

[System.Serializable]
public class PerfectLookAtLegStabilizer
{
	public Transform m_FootBone;
	private Quaternion m_footRotBeforeLookAt;
	private Vector3 m_footPosBeforeLookAt;
	private Vector3 m_middleBonePosBeforeLookAt;
	private Vector3 m_firstBonePosBeforeLookAt;
	private Vector3 m_originalSwivelDir = Vector3.zero;
	private Vector3 m_afterSolvingSwivelDir = Vector3.zero;
	public float m_iKWeight = 1.0f;
	private float[] m_BoneLengths;
	private float m_IKChainLength = 0.0f;
	public uint m_BonesCount = 3;

	private Vector3 FindProjectionVector(Vector3 startingPoint, Vector3 middlePoint, Vector3 endingPoint)
	{
		Vector3 startToMiddle = middlePoint - startingPoint;
		Vector3 startToEnd = (endingPoint - startingPoint).normalized;
		float cosAngle = Vector3.Dot(startToMiddle.normalized, startToEnd);

		Vector3 Ret = startToMiddle - startToEnd * startToMiddle.magnitude * cosAngle;
		return Ret;
	}

	private void FixSwivelAngle(ref Vector3[] positions)
	{
		if (m_afterSolvingSwivelDir == Vector3.zero) { return; }

		float angle = Vector3.Angle(m_afterSolvingSwivelDir, m_originalSwivelDir);

		if (angle > 0.0f) {
			Vector3 swivelAngleAxis = positions[positions.Length - 1] - positions[0];

			m_originalSwivelDir.Normalize();
			m_afterSolvingSwivelDir.Normalize();

			Vector3 CrossVec = Vector3.Cross(m_afterSolvingSwivelDir, m_originalSwivelDir);

			if (Mathf.Abs(CrossVec.z) > Mathf.Epsilon) {
				if (CrossVec.y < 0.0f) { swivelAngleAxis = -swivelAngleAxis; }
			}
			else if (Mathf.Abs(CrossVec.x) > Mathf.Epsilon) {
				if (CrossVec.y < 0.0f) { swivelAngleAxis = -swivelAngleAxis; }
			}
			else if (Mathf.Abs(CrossVec.z) > Mathf.Epsilon) {
				if (CrossVec.x < 0.0f) { swivelAngleAxis = -swivelAngleAxis; }
			}

			Quaternion swivelRot = PerfectLookAt.QuaternionFromAngleAxis(ref swivelAngleAxis, Mathf.Deg2Rad * angle);

			Vector3 currentPos;

			for (int i = positions.Length - 1; i > 0; i--) {
				currentPos = positions[i] - positions[0];
				currentPos = swivelRot * currentPos;

				positions[i] = positions[0] + currentPos;
			}
		}
	}

	public Vector3 GetMiddleBonePos()
	{
		Vector3 ret = Vector3.zero;
		Transform buffTr = m_FootBone;

		for (int i = 0; i < (m_BonesCount / 2); i++) { buffTr = buffTr.parent; }

		ret = buffTr.transform.position;

		return ret;
	}

	public Vector3 GetFirstBonePos()
	{
		Vector3 ret = Vector3.zero;
		Transform buffTr = m_FootBone;

		for (int i = 0; i < m_BonesCount - 1; i++) { buffTr = buffTr.parent; }

		ret = buffTr.transform.position;

		return ret;
	}

	public void Initialize()
	{
		if (m_BonesCount <= 1) {
			Debug.LogWarning("PerfectLookAtLegStabilizer bone count is set to 0 or 1. It will be ignored.");
			return;
		}

		m_IKChainLength = 0.0f;
		m_BoneLengths = new float[m_BonesCount - 1];

		Transform buffTr = m_FootBone;

		for (int i = 0; i < m_BonesCount - 1; i++) {
			m_BoneLengths[i] = (buffTr.transform.position - buffTr.parent.transform.position).magnitude;
			buffTr = buffTr.parent;
			m_IKChainLength += m_BoneLengths[i];
		}
	}

	public void CacheBones()
	{
		if (m_BonesCount <= 1) {
			Debug.LogWarning("PerfectLookAtLegStabilizer bone count is set to 0 or 1. It will be ignored.");
			return;
		}

		m_footRotBeforeLookAt = m_FootBone.rotation;
		m_footPosBeforeLookAt = m_FootBone.position;
		m_middleBonePosBeforeLookAt = GetMiddleBonePos();
		m_firstBonePosBeforeLookAt = GetFirstBonePos();

		// If knee angle is completely extended and angle between thigh and calf is small we skip updating swivel dir and use the last frame swivel dir.
		// This helps to have the right knee hinge axis every frame.
		float kneeAngle = Vector3.Angle((m_middleBonePosBeforeLookAt - m_firstBonePosBeforeLookAt), (m_footPosBeforeLookAt - m_firstBonePosBeforeLookAt));
		if (kneeAngle > Mathf.Epsilon) {
			m_originalSwivelDir = FindProjectionVector(m_firstBonePosBeforeLookAt, m_middleBonePosBeforeLookAt, m_footPosBeforeLookAt);
		}
	}

	public void FixLeg(byte iterations, float minErrorToStartSolving)
	{
		if (m_iKWeight <= Mathf.Epsilon || m_BonesCount <= 1) { return; }

		if ((m_footPosBeforeLookAt - m_FootBone.transform.position).magnitude < minErrorToStartSolving) { return; }

		// Check if the bone numbers are changed at run-time or not to reinitialize and avoid getting null reference exceptions.
		if (m_BoneLengths.Length != m_BonesCount - 1) { Initialize(); }

		Transform buffTr = m_FootBone;
		Quaternion originalFootRotMS = m_FootBone.transform.rotation;
		Transform[] transforms = new Transform[m_BonesCount];
		Vector3[] positions = new Vector3[m_BonesCount];
		Vector3[] fwdVecs = new Vector3[m_BonesCount - 1];
		Vector3 firstBonePos = Vector3.zero;

		for (int i = 0; i < m_BonesCount; i++) {
			positions[i] = buffTr.transform.position;
			transforms[i] = buffTr.transform;

			if (i < m_BonesCount - 1) {
				fwdVecs[i] = buffTr.transform.position - buffTr.parent.transform.position;
				buffTr = buffTr.parent;
			}
			else { firstBonePos = buffTr.transform.position; }
		}

		m_originalSwivelDir = FindProjectionVector(m_firstBonePosBeforeLookAt, m_middleBonePosBeforeLookAt, m_footPosBeforeLookAt);
		FABRIKSolver.SolveFabrik(ref positions, ref m_BoneLengths, m_footPosBeforeLookAt, m_IKChainLength, iterations, minErrorToStartSolving);

		// If knee angle is completely extended and angle between thigh and calf is small we skip updating swivel dir and use the last frame swivel dir.
		// This helps to have the right knee hinge axis every frame.
		float solvedKneeAngle = Vector3.Angle(m_FootBone.transform.position - firstBonePos, positions[m_BonesCount / 2] - firstBonePos);
		if (solvedKneeAngle > Mathf.Epsilon) {
			m_afterSolvingSwivelDir = FindProjectionVector(firstBonePos, positions[m_BonesCount / 2], m_footPosBeforeLookAt);
		}

		FixSwivelAngle(ref positions);

		Quaternion RotBuff = Quaternion.identity;
		Vector3 currentFwdVec;
		float iKBlendWeight = Mathf.Clamp01(m_iKWeight);

		for (uint i = m_BonesCount - 1; i > 0; i--) {
			currentFwdVec = positions[i - 1] - positions[i];
			if (i == 1) { fwdVecs[0] = m_FootBone.transform.position - m_FootBone.parent.transform.position; }

			RotBuff = Quaternion.Slerp(Quaternion.identity, PerfectLookAt.GetWorldLookAtRotation(currentFwdVec, fwdVecs[i - 1]), iKBlendWeight);
			transforms[i].rotation = RotBuff * transforms[i].rotation;
		}

		m_FootBone.transform.rotation = Quaternion.Slerp(m_FootBone.transform.rotation, m_footRotBeforeLookAt, iKBlendWeight);
	}
}