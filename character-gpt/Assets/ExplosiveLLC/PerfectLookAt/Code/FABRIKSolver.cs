using UnityEngine;

public static class FABRIKSolver
{
	public static void SolveFabrik(ref Vector3[] positions, ref float[] boneLengths, Vector3 ikTarget, float ikChainLength, byte iterationNumbers, float errorDistToStopSolving = 0.01f)
	{
		int highestBoneIndex = positions.Length - 1;
		float distanceToIKTarget = (ikTarget - positions[highestBoneIndex]).magnitude;

		if (distanceToIKTarget > ikChainLength) {
			Vector3 ikTargetToRootVector = (ikTarget - positions[highestBoneIndex]).normalized;
			for (int i = highestBoneIndex - 1; i >= 0; i--) {
				positions[i] = positions[i + 1] + ikTargetToRootVector * boneLengths[i];
			}

			return;
		}

		Vector3 rootPos = positions[highestBoneIndex];
		Vector3 delta = Vector3.zero;
		for (byte i = 0; i < iterationNumbers; i++) {

			// Forward reaching.
			positions[0] = ikTarget;
			for (int j = 1; j <= highestBoneIndex; j++) {
				delta = (positions[j] - positions[j - 1]).normalized * boneLengths[j - 1];
				positions[j] = positions[j - 1] + delta;
			}

			if ((positions[highestBoneIndex] - rootPos).sqrMagnitude < errorDistToStopSolving * errorDistToStopSolving) {
				return;
			}

			// Backward reaching.
			positions[highestBoneIndex] = rootPos;
			for (int j = highestBoneIndex - 1; j >= 0; j--) {
				delta = (positions[j] - positions[j + 1]).normalized * boneLengths[j];
				positions[j] = positions[j + 1] + delta;
			}

			if ((positions[0] - ikTarget).sqrMagnitude < errorDistToStopSolving * errorDistToStopSolving) {
				return;
			}
		}
	}
}