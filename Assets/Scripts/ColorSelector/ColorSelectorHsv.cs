using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// TODO Colorblind version that varies only luminance and fixes hue
public static class ColorSelectorHsv {
	static Color[] colors = {
		new Color (1.0f, 0.25f, 0),
		new Color (1.0f, 0.5f, 0.5f),
		new Color (1.0f, 0.5f, 0),
		new Color (1.0f, 0.75f, 0),
		new Color (1.0f, 1.0f, 0),
		new Color (0.75f, 1.0f, 0),
		new Color (0.5f, 1.0f, 0),
		new Color (0.5f, 0, 0),
		new Color (0, 1.0f, 0.5f),
		new Color (0, 1.0f, 0.75f),
		new Color (0, 1.0f, 1.0f),
		new Color (0, 0.75f, 1.0f),
		new Color (0, 0.5f, 1.0f),
		new Color (0, 0.25f, 1.0f),
		new Color (0.5f, 0, 1.0f),
		new Color (0.75f, 0, 1.0f),
		new Color (1.0f, 0, 1.0f),
		new Color (1.0f, 0, 0.75f),
		new Color (1.0f, 0, 0.5f),
		new Color(1.0f, 0.62f, 0.5f),
		new Color(1.0f, 0.75f, 0.5f),
		new Color(1.0f, 0.87f, 0.5f),
		new Color(0.5f, 0.5f, 0),
		new Color(0.35f, 0.7f, 0),
		new Color(0.5f, 1.0f, 0.75f),
		new Color(0, 0.5f, 0.375f),
		new Color(0.25f, 0.75f, 0.75f),
		new Color(0.5f, 0.87f, 1.0f),
		new Color(0, 0.25f, 0.5f),
		new Color(0.375f, 0.4375f, 0.62f),
		new Color(0.5f, 0.5f, 1.0f),
		new Color(0.125f, 0, 0.5f),
		new Color(0.5f, 0.25f, 0.75f),
		new Color(0.87f, 0.5f, 1.0f),
		new Color(0.5f, 0, 0.5f),
		new Color(0.62f, 0.375f, 143),
		new Color(1.0f, 0.5f, 0.75f),
		new Color(0.5f, 0, 0.125f)
	};


	public static Color GetRandomColor() {
		return colors[Random.Range(0, colors.Length)];
	}

	public static Color GetColorDistinctFrom(Color color) { // easily distinguishable from reference color, otherwise random
		int i = System.Array.FindIndex(colors, otherColor => color == otherColor );
		Assert.AreNotEqual (i, -1);
		int rnd = (i + Random.Range (1, colors.Length)) % colors.Length;
		return colors[rnd];
	}

	public static bool AreSimilar(Color a, Color b) {
		return a == b;
	}

	static public bool AreDistinct(Color a, Color b) {
		return a != b;
	}
}
	