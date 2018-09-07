using UnityEngine;

[System.Serializable]
public class Difficulty {
	[HideInInspector] public int fishPerLayer = 5;
	[SerializeField] minMaxIntPair fishPerLayerRange = new minMaxIntPair{ min = 5, max = 15 };

	string prefsKey;
	minMaxPair<int> levelRange = new minMaxPair<int> { min = 1, max = 5 };
	int level = 1;

	public void SetKey(string key) { 		
		prefsKey = key;
		Load ();
	}

	public bool IsLegal() { return true; }

	public void Increase() {
		if (++level > levelRange.max) {
			level = levelRange.max;// + 1;
		} else {
			UpdateData ();
		}
	}

	public void Decrease() {
		if (--level < levelRange.min) {
			level = levelRange.min;// - 1;
		} else {
			UpdateData ();
		}
	}

	public void Reset() {
//		UnityEngine.PlayerPrefs.DeleteAll ();
		UnityEngine.PlayerPrefs.DeleteKey (prefsKey);
	}

	public int CalcValFromRangeAndCurLevel(minMaxIntPair range) {
		float extraPerLevel = (range.max - range.min) / (float)(levelRange.max - levelRange.min);
		return Mathf.RoundToInt(range.min + extraPerLevel * (level - levelRange.min));
	}

	void Load() {
		level = UnityEngine.PlayerPrefs.GetInt (prefsKey, level);
		UpdateData ();
	}

	void Save() {
		UnityEngine.PlayerPrefs.SetInt (prefsKey, level);
	}

	void UpdateData() {
		UnityEngine.Assertions.Assert.IsTrue (levelRange.min <= level && level <= levelRange.max);
		fishPerLayer = CalcValFromRangeAndCurLevel (fishPerLayerRange);
		Save ();
	}
}
