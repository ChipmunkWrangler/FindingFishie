using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using System;


public class FishSpawnPos : MonoBehaviour {
	public minMaxVector2Pair TGT_FISH_PHYS_SIZE; // normalized to standardFish
	public Transform scenery;
	public enum LAYER { 
		FOREGROUND,
		BACKGROUND
	};

	CameraUtils cameraUtils;
	minMaxPair<float> distToCameraForeground;
	minMaxPair<float> distToCameraBackground;
	const float HALF_DIST_BETWEEN_FORE_AND_BACKGROUND = 0.05f;

	public static System.Collections.Generic.IEnumerable<LAYER> GetLayers() {
		System.Array values = System.Enum.GetValues(typeof(LAYER));
		return values.Cast<LAYER>();
	}

	public Vector3 GetSpawnPos(bool swimRightToLeft, GameObject fish, LAYER layer) { // [Pure]
		Vector2 halfFishSize = fish.GetComponent<SpriteRenderer> ().bounds.size * 0.5f;
		halfFishSize.Scale (fish.transform.lossyScale); // lossyScale is for rotated fish
		minMaxPair<float> distPair = (layer == LAYER.BACKGROUND) ? distToCameraBackground : distToCameraForeground;
		float distanceToCamera = GetDesiredDistanceToCamera(distPair);
		return cameraUtils.GetOffCameraPos (swimRightToLeft, halfFishSize, distanceToCamera);
	}

	public void SetStandardFish(GameObject stdFish) {
		InitCameraDistBounds (stdFish);
		InitScenery ();
	}
		
	void Awake () {
		Assert.IsTrue (TGT_FISH_PHYS_SIZE.min.x <= TGT_FISH_PHYS_SIZE.max.x);
		Assert.IsTrue (TGT_FISH_PHYS_SIZE.min.y <= TGT_FISH_PHYS_SIZE.max.y);
		cameraUtils = new CameraUtils ();
		distToCameraForeground = new minMaxPair<float> ();
		distToCameraBackground = new minMaxPair<float> ();			
	}

	void InitCameraDistBounds(GameObject standardFish) {
		Assert.AreApproximatelyEqual (standardFish.transform.lossyScale.x, 1.0f);
		float stdFishZ = standardFish.transform.position.z;
		Vector2 actualScreenDimensions = CameraUtils.GetScreenDimensions (standardFish.GetComponent<SpriteRenderer> ().bounds.size, stdFishZ);
		float curCamDist = CameraUtils.GetDistToCamera (stdFishZ);
		distToCameraForeground.min = GetCameraDistanceForPhysicalDimensions (TGT_FISH_PHYS_SIZE.max, actualScreenDimensions, curCamDist, Mathf.Max);
		distToCameraBackground.max = GetCameraDistanceForPhysicalDimensions (TGT_FISH_PHYS_SIZE.min, actualScreenDimensions, curCamDist, Mathf.Min);
		float midPoint = (distToCameraForeground.min + distToCameraBackground.max) * 0.5f;
		distToCameraForeground.max = midPoint - HALF_DIST_BETWEEN_FORE_AND_BACKGROUND;
		distToCameraBackground.min = midPoint + HALF_DIST_BETWEEN_FORE_AND_BACKGROUND;
//		minMaxPair<float> foregroundZ = new minMaxPair<float> { 
//			min = DIST_TO_CAMERA_FOREGROUND.min + Camera.main.transform.position.z,
//			max = DIST_TO_CAMERA_FOREGROUND.max + Camera.main.transform.position.z
//		};
//		minMaxPair<float> backgroundZ = new minMaxPair<float> { 
//			min = DIST_TO_CAMERA_BACKGROUND.min + Camera.main.transform.position.z,
//			max = DIST_TO_CAMERA_BACKGROUND.max + Camera.main.transform.position.z
//		};
//		minMaxVector2Pair foregroundPhysDimensions = new minMaxVector2Pair {
//			min = CameraUtils.GetScreenDimensions (standardFish.GetComponent<SpriteRenderer> ().bounds.size, foregroundZ.max) *2.54f / Screen.dpi,
//			max = CameraUtils.GetScreenDimensions (standardFish.GetComponent<SpriteRenderer> ().bounds.size, foregroundZ.min) *2.54f/ Screen.dpi
//		};
//		minMaxVector2Pair backgroundPhysDimensions = new minMaxVector2Pair {
//			min = CameraUtils.GetScreenDimensions (standardFish.GetComponent<SpriteRenderer> ().bounds.size, backgroundZ.max) *2.54f / Screen.dpi,
//			max = CameraUtils.GetScreenDimensions (standardFish.GetComponent<SpriteRenderer> ().bounds.size, backgroundZ.min)  *2.54f/ Screen.dpi
//		};
		Assert.IsTrue (distToCameraForeground.min <= distToCameraForeground.max, distToCameraForeground.ToString());
		Assert.IsTrue (distToCameraBackground.min <= distToCameraBackground.max, distToCameraBackground.ToString());
		Assert.IsTrue (distToCameraForeground.max <= distToCameraBackground.min, distToCameraForeground.max + " > " + distToCameraBackground.min);
	}

	float GetCameraDistanceForPhysicalDimensions(Vector2 desiredPhysicalDimensions, Vector2 actualScreenDimensions, float actualDistToCamera, Func<float, float, float> minOrMax) {
		Vector2 desiredScreenDimensions = CameraUtils.GetScreenFromPhysical(desiredPhysicalDimensions);
		Vector2 cameraDistances = CameraUtils.GetCameraDistanceForScreenDimensions (desiredScreenDimensions, actualScreenDimensions, actualDistToCamera);
		return minOrMax (cameraDistances.x, cameraDistances.y);
	}

	void InitScenery() {
		scenery.position = new Vector3( scenery.position.x, scenery.position.y, Camera.main.transform.position.z + (distToCameraBackground.min + distToCameraForeground.max) / 2.0f);
	}

	static float GetDesiredDistanceToCamera(minMaxPair<float> bounds) { // [Pure] (but not idempotent)
		// See comment at end of file for explanation of the formula (d^3 + x (D^3 - d^3))^(1/3)
		const float oneThird = 1f / 3f;
		float dCubed = Mathf.Pow (bounds.min, 3f);
		float DCubed = Mathf.Pow (bounds.max, 3f);
		float x = UnityEngine.Random.Range (0, 1f);
		return Mathf.Pow(dCubed + x * (DCubed - dCubed), oneThird);
	}

	// Let MAX_DIST_TO_CAMERA = D, MIN_DIST_TO_CAMERA = d, FRUSTUM_SLOPE = s and aspectRatio = q
	// Let Frustum(distanceToCamera) be the slice of the frustum from d to distanceToCamera (so Frustum(D) is the whole frustum).
	// Let f:[0,1]->[d, D] s.t. Frustum(f(x))'s fraction of the total frustum volume is x.
	// We want the fish to be, on average, evenly distributed within the frustum.
	//  -> for all x in [0,1], fraction x of the fish should lie in fraction x of the frustum volume.
	//	-> for all x in [0,1], fraction x of the fish should lie in Frustum(f(x)).
	// 	-> for all x in [0,1], fraction x of the fish should have distanceToCamera <= f(x).
	// 	-> for all x in [0,1], we want P(distanceToCamera <= f(x)) = x
	// If, for each fish, we choose k = Random.Range(0,1) and set each fish's distanceToCamera to f(k), then 
	//   For all x in [0,1], P(distanceToCamera <= f(x)) = P(f(k) <= f(x))       (by hypothesis)
	//                                                   = P(k <= x)             (f(k) <= f(x) <-> k <= x because f(x) increases monotonically in x)
	//                                                   = x                     (by definition of Random.Range)
	// So distanceToCamera = f(Random.Range(0,1)) gives us the uniform distribution we seek.
	//
	// But how to find f?
	//
	// Let CDF be the inverse of f. Unlike f, we can calculate CDF directly:
	// Recall that x = Frustum(f(x))'s fraction of the total frustum volume      (by definition of f)
	//   ->   CDF(z) = Frustum(f(CDF(z))'s fraction of the total frustum volume
	//   ->          = Frustum(z)'s fraction of the total frustum volume         (by definition of CDF as inverse)
	// Let V(z) be the volume of Frustum(z). Then
	//   ->   CDF(z) = V(z) / V(D)
	// But we also know (e.g. from geometry) that
	//    V(z) = h / 3 * (A + a + sqrt(Aa)), where 
	//      h = frustum height = z - d
	//      A = area of the frustum at the big end = width * breadth = q * width * width = q(2sz)^2
	//      a = area of the frustm at the little end = q * (2sd)^2
	// -> V(z) = (h / 3) * (q(2sz)^2 + q(2sd)^2 + SQRT((q(2sz)^2) * (q(2sd)^2) ))
	//         = s^2 * 4q / 3 * h * (z^2 + d^2 + zd)
	//         = [s^2 * 4q / 3] * (z - d) * (z^2 + d^2 + zd)
	// -> CDF(z) = [s^2 * 4q / 3] * (z - d) * (z^2 + d^2 + zd)
	//             -------------------------------------------
	//             [s^2 * 4q / 3] * (D - d) * (D^2 + d^2 + Dd)
	//           = (z - d) * (z^2 + d^2 + zd) / ((D - d) * (D^2 + d^2 + Dd))
	//           = (z^3 + zd^2 + z^2d - z^2d - d^3 - zd^2) / (D^3 + Dd^2 + D^2d - dD^2 - d^3 - Dd^2)
	//           = (z^3 - d^3) / (D^3 - d^3)
	// Now we can solve x = (z^3 - d^3) / (D^3 - d^3) for z to get the inverse function
	// f(x) = (d^3 + x (D^3 - d^3))^(1/3)
}
