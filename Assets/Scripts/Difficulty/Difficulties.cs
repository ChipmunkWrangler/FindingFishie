using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Difficulties {
	public DifficultyAdjustment[] p1Difficulties;
//	public Difficulty[] p2Difficulties { get; private set; }
	string playerKey;
//	minMaxPair<int> difficultyRange;
	const string p1Key = "p1";
//	const string p2Key = "p2";

	public Difficulties(string _playerKey, minMaxPair<int> diffRange, int numP1Modes) {
		playerKey = _playerKey;
//		difficultyRange = diffRange;
		p1Difficulties = new DifficultyAdjustment[numP1Modes];
//		p2Difficulties = new Difficulty[numP2Modes];
//		var numForegroundFish = PlayerPrefsX.GetIntArray (GetKey (p1Key, "foreground"), 10, numP1Modes);
//		var numBackgroundFish = PlayerPrefsX.GetIntArray (GetKey (p1Key, "background"), 10, numP1Modes);
//		p2Difficulties = PlayerPrefsX.GetVector2Array (GetKey (p2Key), Difficulty.defaultLevel, numP2Modes);
		for (int i = 0; i < p1Difficulties.Length; ++i) {
			p1Difficulties [i] = new DA_Time ();
			p1Difficulties [i].SetKey (GetKey (p1Key, i));
		}
//
//			Diff = new DifficultyAdjustment_LinearWeightedMovingAverage {
//				numVisibleForegroundFishDesired = numForegroundFish [i],
//				numVisibleBackgroundFishDesired = numBackgroundFish [i]
//			};
//		}
	}

	public void Reset() {
		foreach (var p1Difficulty in p1Difficulties) {
			p1Difficulty.Reset ();
		}
	}

	string GetKey(string roleKey, int idx) {
		return playerKey + "-" + roleKey + "-" + idx.ToString();
	}
}
