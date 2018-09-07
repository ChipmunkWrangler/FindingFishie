using UnityEngine;
using UnityEngine.Assertions;

struct LuvColor {
	public float L;
	public float u;
	public float v;

	// Note: not all valid Luv values convert to valid RGB values!
	public const float MaxL = 100f;
	public const float MinL = 0;
	public const float MinU = -83.08054f;
	public const float MaxU = 175.0526f;
	public const float MinV = -134.1082f;
	public const float MaxV = 107.4164f;
	public static readonly LuvColor White = new LuvColor{ L = MaxL, u = 0, v = 0 };

	public static LuvColor operator +(LuvColor a, LuvColor b) 
	{
		return new LuvColor {
			L = a.L + b.L, 
			u = a.u + b.u,
			v = a.v + b.v
		};
	}

	public override string ToString()
	{
		return System.String.Format("({0:F8}, {1:F8}, {2:F8})", L, u, v);
	}
}

// formulae from easyrgb.com
static class ColorSpaces {
	static public LuvColor RGBToLuv(Color rgb) {
		return UVYToLuv( XYZToUVY( RGBToXYZ (rgb) ) );
	}

	static public Color LuvToRGB(LuvColor luv) {
		return XYZToRGB( UVYToXYZ( LuvToUVY( luv ) ));
	}

	static public void Test() {
		TestRGBToLuvAndBack ();
		TestRGBToXYZ ();
		TestXYZToRGB ();
		TestXYZToUVY ();
		TestUVYToXYZ ();
		TestUVYToLuv ();
		TestLuvToUVY ();
	}

	////////////////////////////////////////////////////////////////////
	static readonly Vector3 xyzReferenceWhite = new Vector3(95.05f, 100.000f, 108.9f);
	static readonly float refK = getKFromXYZ (xyzReferenceWhite);
	static readonly UVY uvyReferenceWhite = new UVY { 
		u = (4f * xyzReferenceWhite.x) / refK,
		v = (9f * xyzReferenceWhite.y) / refK,
		y = 1f
	};
	static readonly Vector3 xyzReferenceBlue = new Vector3(18.05f, 7.22f , 95.05f);

	const float yShift = 16f / 116f;

	struct UVY {
		public float u;
		public float v;
		public float y;

		public override string ToString()
		{
			return System.String.Format("({0:F8}, {1:F8}, {2:F8})", u, v, y);
		}
	}				

	static LuvColor UVYToLuv(UVY uvy) {
		float L = (116f * uvy.y) - 16f;
		return new LuvColor { 
			L = L, 
			u = 13f * L * (uvy.u - uvyReferenceWhite.u), 
			v = 13f * L * (uvy.v - uvyReferenceWhite.v) 
		};
	}

	static UVY LuvToUVY(LuvColor luv) {
		return new UVY {
			u = (luv.L == 0) ? 0 : uvyReferenceWhite.u + luv.u / (13f * luv.L),
			v = (luv.L == 0) ? 0 : uvyReferenceWhite.v + luv.v / (13f * luv.L),
			y = (luv.L + 16f) / 116f
		};
	}


	static UVY XYZToUVY(Vector3 xyz) { // u in [0, uvyReferenceWhite.u], v in [0, uvyReferenceWhite.v], y in [yShift, uvyReferenceWhite.y]
		float k = getKFromXYZ (xyz);
		float u = (k == 0) ? 0 : 4f * xyz.x / k;
		float v = (k == 0) ? 0 : 9f * xyz.y / k;
		float y = xyz.y * 0.01f;
		if (y > 0.008856f) {
			y = Mathf.Pow (y, 1f / 3f);
		} else {
			y = (7.787f * y) + yShift;
		}
		return new UVY { u = u, v = v, y = y};
	}

	static Vector3 UVYToXYZ(UVY uvy) {
		Vector3 vec = new Vector3 ();
		float yCubed = Mathf.Pow (uvy.y, 3f);
		if (yCubed > 0.008856f) {
			vec.y = yCubed;
		} else {
			vec.y = (uvy.y - yShift) / 7.787f;
		}
//		Assert.AreEqual (vec.y != 0, Differs(uvy.u, 0), System.String.Format("uvy {0} vec.y {1:F4}", uvy, vec.y));
//		Assert.AreEqual (vec.y != 0, Differs(uvy.v, 0), System.String.Format("uvy {0} vec.y {1:F4}", uvy, vec.y));
		if (uvy.v == 0) {
			vec.y = 0;
		} else {
			vec.y = vec.y * 100f;
			vec.x = (vec.y == 0) ? 0 : -(9f * vec.y * uvy.u) / ((uvy.u - 4f) * uvy.v - uvy.u * uvy.v);
			vec.z = (vec.y == 0) ? 0 : (9f * vec.y - (15f * uvy.v * vec.y) - (uvy.v * vec.x)) / (3f * uvy.v);
		}
		return vec;
	}

	static float RGBTermToXYZTerm(float term) {
		return 
			(term > 0.04045f) 
			? Mathf.Pow ((term + 0.055f) / 1.055f, 2.4f)
			: term / 12.92f;
	}

	static float XYZTermToRGBTerm(float term) {
		float rgbTerm =  
			(term > 0.0031308f)
			? 1.055f * Mathf.Pow (term, 1f / 2.4f) - 0.055f
			: term * 12.92f;
		if (rgbTerm < 0 && !Differs (rgbTerm, 0)) {
			rgbTerm = 0;
		} else if (rgbTerm > 1f && !Differs (rgbTerm, 1f)) {
			rgbTerm = 1f;
		}
		return rgbTerm;
	}

	static Vector3 RGBToXYZ(Color color) { // RGB in range 0 to 1, X in [0, xyzReferenceWhite.x], Y (luminance) in [0, 100], Z in [0, xyzReferenceWhite.z]
		float _x = RGBTermToXYZTerm(color.r);
		float _y = RGBTermToXYZTerm(color.g);
		float _z = RGBTermToXYZTerm(color.b);

		return new Vector3 (
			_x * 41.24f + _y * 35.76f + _z * xyzReferenceBlue.x, 
			_x * 21.26f + _y * 71.52f + _z * xyzReferenceBlue.y,
			_x * 1.93f + _y * 11.92f + _z * xyzReferenceBlue.z
		);
	}

	static Color XYZToRGB(Vector3 xyz) {
		float _x = xyz.x * 0.032406f + xyz.y * -0.015372f + xyz.z * -0.004986f; // OK
		float _y = xyz.x * -0.009689f + xyz.y * 0.018758f + xyz.z *  0.000415f; // OK
		float _z = xyz.x * 0.000557f + xyz.y * -0.002040f + xyz.z *  0.010570f; // OK

		float r = XYZTermToRGBTerm (_x);
		float g = XYZTermToRGBTerm (_y);
		float b = XYZTermToRGBTerm (_z);

		if (r >= 0 && r <= 1f && g >= 0 && g <= 1f && b >= 0 && b <= 1f) {
			return new Color (r, g, b);
		} else {
//			Debug.Log ("Colour out of range " + r + ", " + g + ", " + b);
			return new Color (0, 0, 0);
		}
	}

	static float getKFromXYZ(Vector3 xyz) {
		return xyz.x + (15f * xyz.y) + (3f * xyz.z);
	}
	////////////////////////////////////////////////////////////////////
	static readonly Vector3 xyzReferenceBlack = new Vector3(0,0,0);
	static readonly UVY uvyReferenceBlack = new UVY{ u = 0, v = 0, y = yShift };
	static readonly LuvColor luvReferenceBlack = new LuvColor{ L = 0, u = 0, v = 0 };

//	static readonly UVY uvyReferenceBlue = new UVY{ u = 0, v = 0, y = yShift };
//	static readonly LuvColor luvReferenceBlue = new LuvColor{ L = 0, u = 0, v = 0 };
//
	static bool Differs(float a, float b) {
		return Mathf.Abs (a - b) > 0.001f;
	}

	static void TestRGBToLuvAndBack() {
//		float minU, maxU, minV, maxV;
//		minU = minV = float.MaxValue;
//		maxU = maxV = float.MinValue;
		const int samplesPerDimension = 11;
		for (int i = 0; i < samplesPerDimension; ++i) {
			for (int j = 0; j < samplesPerDimension; ++j) {
				for (int k = 0; k < samplesPerDimension; ++k) {
					Color rgb = new Color ((float)i / (float)(samplesPerDimension-1), (float)j / (float)(samplesPerDimension-1), (float)k / (float)(samplesPerDimension-1));
					LuvColor luv = ColorSpaces.RGBToLuv (rgb);

//					if (luv.u < minU) {
//						minU = luv.u;
//					} else if (luv.u > maxU) {
//						maxU = luv.u;
//					}
//					if (luv.v < minV) {
//						minV = luv.v;
//					} else if (luv.v > maxV) {
//						maxV = luv.v;
//					}

					Color newColor = ColorSpaces.LuvToRGB (luv);
					if (Differs (newColor.r, rgb.r) || Differs (newColor.g, rgb.g) || Differs (newColor.b, rgb.b)) {
						Debug.Log (System.String.Format ("rgb: {0:F8}, {1:F8}, {2:F8}. new: {3:F8}, {4:F8}, {5:F8}, Diff: {6:F8}, {7:F8}, {8:F8}", rgb.r, rgb.g, rgb.b, newColor.r, newColor.g, newColor.b, newColor.r - rgb.r, newColor.g - rgb.g, newColor.b - rgb.b));
					}
				}
			}
		}
//		Debug.Log (System.String.Format ("U between {0:f8} and {1:f8}, V between {2:f8} and {3:f8}", minU, maxU, minV, maxV));
	}

	static void TestRGBToXYZ() {
		Vector3 xyzBlack = RGBToXYZ (new Color (0, 0, 0));
		if (Differs (xyzBlack.x, xyzReferenceBlack.x) || Differs (xyzBlack.y, xyzReferenceBlack.y) || Differs (xyzBlack.z, xyzReferenceBlack.z)) {
			Debug.Log ("xyzBlackFromRGB" + xyzBlack);
		}

		Vector3 xyzWhite = RGBToXYZ (new Color (1f, 1f, 1f));
		if (Differs (xyzWhite.x, xyzReferenceWhite.x) || Differs (xyzWhite.y, xyzReferenceWhite.y) || Differs (xyzWhite.z, xyzReferenceWhite.z)) {
			Debug.Log ("xyzWhiteFromRBG:" + xyzWhite);
		}

		Vector3 xyzBlue = RGBToXYZ (new Color (0f, 0f, 1f));
		if (Differs (xyzBlue.x, xyzReferenceBlue.x) || Differs (xyzBlue.y, xyzReferenceBlue.y) || Differs (xyzBlue.z, xyzReferenceBlue.z)) {
			Debug.Log ("xyzBlueFromRBG:" + xyzBlue);
		}
	}

	static void TestXYZToRGB() {
		Color black = XYZToRGB (xyzReferenceBlack);
		if (!(black.r == 0 && black.g == 0 && black.b == 0)) {
			Debug.Log (System.String.Format("blackFromXYZ: {0:F8}, {1:F8}, {2:F8}", black.r, black.g, black.b));
		}

		Color white = XYZToRGB (xyzReferenceWhite);
		if (Differs (white.r, 1f) || Differs (white.g, 1f) || Differs (white.b, 1f)) {
			Debug.Log (System.String.Format("whiteFromXYZ: {0:F8}, {1:F8}, {2:F8}", white.r, white.g, white.b));
		}

		Color blue = XYZToRGB (RGBToXYZ (new Color (0f, 0f, 1f)));
		if (Differs (blue.r, 1f) || Differs (blue.g, 1f) || Differs (blue.b, 1f)) {
			Debug.Log (System.String.Format("blueFromXYZ: {0:F8}, {1:F8}, {2:F8}", blue.r, blue.g, blue.b));
		}
	}

	static void TestUVYToXYZ() {
		Vector3 xyzBlack = UVYToXYZ (uvyReferenceBlack);
		if (Differs (xyzBlack.x, xyzReferenceBlack.x) || Differs (xyzBlack.y, xyzReferenceBlack.y) || Differs (xyzBlack.z, xyzReferenceBlack.z)) {
			Debug.Log ("xyzBlackFromUVY: " + xyzBlack);
		}

		Vector3 xyzWhite = UVYToXYZ (uvyReferenceWhite);
		if (Differs (xyzWhite.x, xyzReferenceWhite.x) || Differs (xyzWhite.y, xyzReferenceWhite.y) || Differs (xyzWhite.z, xyzReferenceWhite.z)) {
			Debug.Log ("xyzWhiteFromUVY: " + xyzWhite);
		}
	}

	static void TestXYZToUVY() {
		UVY uvyBlack = XYZToUVY (xyzReferenceBlack);
		if (Differs (uvyBlack.u, uvyReferenceBlack.u) || Differs (uvyBlack.v, uvyReferenceBlack.v) || Differs (uvyBlack.y, uvyReferenceBlack.y)) {
			Debug.Log ("uvyBlackFromXYZ: " + uvyBlack);
		}

		UVY uvyWhite = XYZToUVY (xyzReferenceWhite);
		if (Differs (uvyWhite.u, uvyReferenceWhite.u) || Differs (uvyWhite.v, uvyReferenceWhite.v) || Differs (uvyWhite.y, uvyReferenceWhite.y)) {
			Debug.Log ("uvyWhiteFromXYZ: " + uvyWhite);
		}
	}

	static void TestUVYToLuv() {
		LuvColor luvBlack = UVYToLuv (uvyReferenceBlack);
		if (Differs (luvBlack.L, luvReferenceBlack.L) || Differs (luvBlack.u, luvReferenceBlack.u) || Differs (luvBlack.v, luvReferenceBlack.v)) {
			Debug.Log ("luvBlackFromUVY: " + luvBlack);
		}

		LuvColor luvWhite = UVYToLuv (uvyReferenceWhite);
		if (Differs (luvWhite.L, LuvColor.White.L) || Differs (luvWhite.u, LuvColor.White.u) || Differs (luvWhite.v, LuvColor.White.v)) {
			Debug.Log ("luvWhiteFromUVY: " + luvWhite);
		}
	}

	static void TestLuvToUVY() {
		UVY uvyBlack = LuvToUVY (luvReferenceBlack);
		if (Differs (uvyBlack.u, uvyReferenceBlack.u) || Differs (uvyBlack.v, uvyReferenceBlack.v) || Differs (uvyBlack.y, uvyReferenceBlack.y)) {
			Debug.Log ("uvyBlackFromLuv: " + uvyBlack);
		}

		UVY uvyWhite = LuvToUVY (LuvColor.White);
		if (Differs (uvyWhite.u, uvyReferenceWhite.u) || Differs (uvyWhite.v, uvyReferenceWhite.v) || Differs (uvyWhite.y, uvyReferenceWhite.y)) {
			Debug.Log ("uvyWhiteFromLuv: " + uvyWhite);
		}
	}
}