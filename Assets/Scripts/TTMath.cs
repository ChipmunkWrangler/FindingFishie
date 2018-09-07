using UnityEngine.Assertions;
using UnityEngine;

public static class TTMath { 
	// Range is clockwise from start to end, but that means start < end if the range includes the 360/0 point.
	public static float ClampAngle(float angle, float start, float end) {
		Assert.IsTrue (0 <= angle && angle < 360.0f);
		Assert.IsTrue (0 <= start && start < 360.0f);
		Assert.IsTrue (0 <= end && end < 360.0f);
		if (start <= end) { // range doesn't include 0
			return Mathf.Clamp (angle, start, end);
		} 
		// range includes 0
		if (start <= angle || angle <= end) { // inside range
			return angle;
		}
		return (Mathf.Abs(angle - start) < Mathf.Abs(angle - end)) ? start : end; // Outside range so return nearest range limit
	}

	public static float GetRandomAngleIn(float start, float end) {
		Assert.IsTrue (0 <= start && start < 360.0f);
		Assert.IsTrue (0 <= end && end < 360.0f);
		if (start > end) {
			end += 360.0f;
		}
		return GetNormalizedAngle (Random.Range (start, end));
	}

	public static float GetNormalizedAngle(float angle) { 
		angle %= 360.0f;
		while (angle < 0) {
			angle += 360.0f;
		} 
		Assert.IsTrue (0 <= angle && angle < 360.0f, angle.ToString());
		return angle;
	}
}

