using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FishAppearance {
	public GameObject prefab;
	public int patternIdx;
	public Color color;

	public override string ToString() {
		return "appearance: prefab " + prefab.ToString () + " color " + color.ToString () + " patternIdx " + patternIdx;
	}

	static public bool AreDistinct(FishAppearance a, FishAppearance b) {
		return a.prefab != b.prefab || a.patternIdx != b.patternIdx || ColorSelectorHsv.AreDistinct (a.color, b.color);
	}
}

public class FishAppearanceBehaviour : MonoBehaviour {
	public FishAppearance appearance;
}
