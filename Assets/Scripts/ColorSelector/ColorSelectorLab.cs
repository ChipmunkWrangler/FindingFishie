using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// TODO Colorblind version that varies only luminance and fixes hue
public static class ColorSelectorLab {
	// We operate in Luv color space because then Euclidean distance corresponds to perceptual difference
	public const float MAX_SIMILAR_COLOR_DISTANCE = 20f;
	public const float MIN_DISTINCT_COLOR_DISTANCE = 35f;
	public const float MAX_COLOR_DISTANCE = 150f;

	public static Color GetRandomColor() {
		return Random.ColorHSV (0f, 1f, 1f, 1f, 0.8f, 1f);
	}

	public static Color GetColorSimilarTo(Color color) { 
		return GetColorWithinRangeOf(color, 0, MAX_SIMILAR_COLOR_DISTANCE);
	}
		
	public static Color GetColorDistinctFrom(Color color) { // easily distinguishable from reference color, otherwise random
		return GetColorWithinRangeOf(color, MIN_DISTINCT_COLOR_DISTANCE, MAX_COLOR_DISTANCE);
	}

	public static bool AreSimilar(Color a, Color b) {
		LuvColor luvA = ColorSpaces.RGBToLuv (a);
		LuvColor luvB = ColorSpaces.RGBToLuv (b);
		Vector3 difference = new Vector3(luvA.L - luvB.L, luvA.u - luvB.u, luvA.v - luvB.v);
		return difference.sqrMagnitude <= MAX_SIMILAR_COLOR_DISTANCE * MAX_SIMILAR_COLOR_DISTANCE;
	}

	static public bool AreDistinct(Color a, Color b) {
		LuvColor luvA = ColorSpaces.RGBToLuv (a);
		LuvColor luvB = ColorSpaces.RGBToLuv (b);
		Vector3 difference = new Vector3(luvA.L - luvB.L, luvA.u - luvB.u, luvA.v - luvB.v);
		return difference.sqrMagnitude >= MIN_DISTINCT_COLOR_DISTANCE * MIN_DISTINCT_COLOR_DISTANCE;		
	}


	static Color GetColorWithinRangeOf (Color color, float MIN_DISTANCE, float MAX_DISTANCE ) {
		Color newRgbColor;
		do {
			newRgbColor = ColorSpaces.LuvToRGB (GetColorWithinRangeOf (ColorSpaces.RGBToLuv (color), MIN_DISTANCE, MAX_DISTANCE));
		} while (newRgbColor == Color.black); // black means "doesn't exist in RGB space"
		return newRgbColor;
	}

	static LuvColor GetColorWithinRangeOf(LuvColor luv, float MIN_DISTANCE, float MAX_DISTANCE ) {
		Vector3 vec = GetRandomPointInHollowSphere(MIN_DISTANCE, MAX_DISTANCE);
		LuvColor offset = new LuvColor{ L = vec.x, u = vec.y, v = vec.z };
		return luv + offset;
	}

	static Vector3 GetRandomPointInHollowSphere(float innerRadius, float outerRadius) {
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		float r = Mathf.Pow (Random.Range (Mathf.Pow(innerRadius, 3f), Mathf.Pow(outerRadius, 3f)), 1f/3f);
		return onUnitSphere * r;
	}
}
	