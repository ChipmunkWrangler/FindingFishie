using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
	public int numTestObjectsWide;
	public GameObject testPrefab;
	public float spacing;

	// Use this for initialization
	void Start () {
		ColorSpaces.Test ();
//		ShowAllColors ();
		ShowColorsSurrounding(Random.ColorHSV (0f, 1f, 1f, 1f, 0.8f, 1f));

	}

	void ShowColorsSurrounding(Color color) {
		LuvColor luvCenter = ColorSpaces.RGBToLuv (color);
		GameObject centerSphere = Instantiate<GameObject> (testPrefab, Vector3.zero, Quaternion.identity);
		centerSphere.GetComponent<Renderer> ().material.color = color;

		float MAX_INDEX = numTestObjectsWide;
		float INNER_SHELL_FRACTION = ColorSelectorLab.MAX_SIMILAR_COLOR_DISTANCE / ColorSelectorLab.MAX_COLOR_DISTANCE;
		float INNER_SHELL_FRACTION_SQUARED = INNER_SHELL_FRACTION * INNER_SHELL_FRACTION;
		Debug.Log (INNER_SHELL_FRACTION_SQUARED); 
		for (int i = -numTestObjectsWide; i <= numTestObjectsWide; ++i) {
			float iFraction = (float)i / MAX_INDEX;
			float x = i * spacing;
			for (int j = -numTestObjectsWide; j <= numTestObjectsWide; ++j) {
				float jFraction = (float)j / MAX_INDEX;
				float y = j * spacing;
				for (int k = -numTestObjectsWide; k <= numTestObjectsWide; ++k) {
					float kFraction = (float)k / MAX_INDEX;
					float squaredDistanceToCenter = iFraction * iFraction + jFraction * jFraction + kFraction * kFraction;
					if (squaredDistanceToCenter >= INNER_SHELL_FRACTION_SQUARED && squaredDistanceToCenter <= 1f) {
						float z = k * spacing;
						Debug.Log (x + ", " + y + ", " + z + ". Dist " + squaredDistanceToCenter);
						LuvColor luv = new LuvColor { 
							L = luvCenter.L + ColorSelectorLab.MAX_COLOR_DISTANCE * iFraction,
							u = luvCenter.u + ColorSelectorLab.MAX_COLOR_DISTANCE * jFraction,
							v = luvCenter.v + ColorSelectorLab.MAX_COLOR_DISTANCE * kFraction
						};
						Color sphereColor = ColorSpaces.LuvToRGB (luv);
						if (sphereColor != Color.black) {
							GameObject o = Instantiate<GameObject> (testPrefab, new Vector3 (x, y, z), Quaternion.identity);					
							o.GetComponent<Renderer> ().material.color = sphereColor;
						}
					}
				}
			}
		}
	}

	void ShowAllColors() {
		float MAX_INDEX = numTestObjectsWide - 1;
		for (int i = 0; i < numTestObjectsWide; ++i) {
			float iFraction = (float)i / MAX_INDEX;
			for (int j = 0; j < numTestObjectsWide; ++j) {
				float jFraction = (float)j / MAX_INDEX;
				for (int k = 0; k < numTestObjectsWide; ++k) {
					float kFraction = (float)k / MAX_INDEX;
					//					Color rgb = new Color((float)i / (float)(numTestObjectsWide-1), (float)j / (float)(numTestObjectsWide-1), (float)k / (float)(numTestObjectsWide-1));
					LuvColor luv = new LuvColor{ 
						L = LuvColor.MinL * (1f - iFraction) + LuvColor.MaxL * iFraction,
						u = LuvColor.MinU * (1f - jFraction) + LuvColor.MaxU * jFraction, 
						v = LuvColor.MinV * (1f - kFraction) + LuvColor.MaxV * kFraction
					};
					//					LuvColor luv = ColorSpaces.RGBToLuv(rgb);
					Color color = ColorSpaces.LuvToRGB(luv);
					if (color != Color.black) {
						GameObject o = Instantiate<GameObject> (testPrefab, new Vector3 (i * spacing, j * spacing, k * spacing), Quaternion.identity);					
						o.GetComponent<Renderer> ().material.color = color;
					}
				}
			}
		}
	}

	Vector3  GetRandomPointInHollowSphere(float innerRadius, float outerRadius) {
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		float r = Mathf.Pow (Random.Range (Mathf.Pow(innerRadius, 3f), Mathf.Pow(outerRadius, 3f)), 1f/3f);
		return onUnitSphere * r;
	}
}
