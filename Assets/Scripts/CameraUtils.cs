using UnityEngine;
using System.Linq;

public class CameraUtils {
	float FRUSTUM_SLOPE;
	Plane[] frustumPlanes;
	const float AVOID_ROUNDING_ERRORS = 0.95f;

	public CameraUtils() {
		FRUSTUM_SLOPE = Mathf.Tan (Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
		UpdateAfterMove();
	}

	public Vector3 GetOffCameraPos(bool offRightSide, Vector2 margin, float distanceToCamera ) {
		if (!Camera.main)
			return Vector3.zero;

		Vector2 upperLeftOffscreenPos = GetFrustumUpperLeftCorner (distanceToCamera) - margin;
		return new Vector3 (
			(offRightSide) ? -upperLeftOffscreenPos.x : upperLeftOffscreenPos.x, 
			Random.Range (-upperLeftOffscreenPos.y, upperLeftOffscreenPos.y), 
			distanceToCamera + Camera.main.transform.position.z);
	}

	public Vector3 GetOnCameraPos(float z) {
		float distanceToCamera = z - Camera.main.transform.position.z;
		Vector2 upperLeft = GetFrustumUpperLeftCorner (distanceToCamera) * AVOID_ROUNDING_ERRORS;
		return new Vector3 (
			Random.Range (upperLeft.x, -upperLeft.x),
			Random.Range (-upperLeft.y, upperLeft.y),
			z);			
	}


	public float GetDistanceToFrustumEdge(Ray ray) {
		UnityEngine.Assertions.Assert.IsTrue (IsOnScreen (ray.origin), ray.origin + " viewport: " + Camera.main.WorldToViewportPoint (ray.origin));
		float dist;
		return frustumPlanes.Select(plane => plane.Raycast(ray, out dist) ? dist : float.MaxValue).Min();
	}
		
	public void UpdateAfterMove() {
		frustumPlanes = GeometryUtility.CalculateFrustumPlanes (Camera.main);
	}

	static public bool IsFirmlyOnScreen(Vector3 tgt) {
		Vector3 viewportTarget = Camera.main.WorldToViewportPoint (tgt);
		return (viewportTarget.x >= (1.0f - AVOID_ROUNDING_ERRORS) && viewportTarget.x <= AVOID_ROUNDING_ERRORS && viewportTarget.y >= (1.0f - AVOID_ROUNDING_ERRORS) && viewportTarget.y <= AVOID_ROUNDING_ERRORS);
	}

	static public bool IsOnScreen(Vector3 tgt) {
		Vector3 viewportTarget = Camera.main.WorldToViewportPoint (tgt);
		return (viewportTarget.x >= 0 && viewportTarget.x <= 1.0f && viewportTarget.y >= 0 && viewportTarget.y <= 1.0f);
	}

	static public Vector2 GetScreenFromPhysical(Vector2 dimInInches) {
		return dimInInches * Screen.dpi;
	}

	static public Vector2 GetScreenDimensions(Vector2 spriteDimensionsInPixels, float spriteZ) {
		return Camera.main.WorldToScreenPoint(new Vector3(spriteDimensionsInPixels.x, spriteDimensionsInPixels.y, spriteZ)) - Camera.main.WorldToScreenPoint (new Vector3 (0, 0, spriteZ));
	}

	static public float GetDistToCamera(float z) {
		return z - Camera.main.transform.position.z;
	}

	static public Vector2 GetCameraDistanceForScreenDimensions(Vector2 desiredScreenDimensions, Vector2 currentScreenDimensions, float currentDistToCamera) {
		return currentDistToCamera * new Vector2(currentScreenDimensions.x / desiredScreenDimensions.x, currentScreenDimensions.y / desiredScreenDimensions.y);
	}

	static public GameObject GetTouchedObject (Vector2 touchedPosition) {
		Ray ray = Camera.main.ScreenPointToRay(touchedPosition); 
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit) && hit.collider != null) {
			return hit.collider.gameObject;
		}
		return null;
	}

	//-------------------------------------------------------------------------------------------------------

	Vector2 GetFrustumUpperLeftCorner(float distanceToCamera) { // [Pure]
		if (!Camera.main) 
			return Vector2.zero;
		
		float frustumMaxY = distanceToCamera * FRUSTUM_SLOPE;
		float frustumMinX = -frustumMaxY * Camera.main.aspect;
		return new Vector2 (frustumMinX, frustumMaxY);
	}
}

