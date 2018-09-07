using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class DifficultyAdjustment {
	public Difficulty difficulty = new Difficulty();

	protected minMaxPair<int> optimalRange;
	bool increaseOnExceedMax;

	public void StartRound (minMaxPair<int> optimalAssists, bool _increaseOnExceedMax) {
		increaseOnExceedMax = _increaseOnExceedMax;
		DoStartRound (optimalAssists);
	}

	public void UpdateDifficulty(int numAssists) {
		int measuredVar = CalcMeasuredVar (numAssists);
		if (measuredVar < optimalRange.min) {
			if (increaseOnExceedMax) {
				difficulty.Decrease ();
			} else {
				difficulty.Increase ();
			}
		} else if (measuredVar > optimalRange.max) {
			if (increaseOnExceedMax) {
				difficulty.Increase ();
			} else {
				difficulty.Decrease ();
			}
		}
	}

	public Difficulty GetDifficulty() { // should be const
		return difficulty;
	}

	public void SetKey(string key) {
		difficulty.SetKey(key);
	}

	public void Reset() {
		difficulty.Reset ();
	}

	protected abstract int CalcMeasuredVar (int numAssists);
	protected abstract void DoStartRound(minMaxPair<int> optimalAssist);
}

