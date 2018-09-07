using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DA_Time : DifficultyAdjustment {
	float lastTimeTargetWasFound = 0;
	////////////////////////////////////////////////////////////////////////
	public DA_Time() {
		optimalRange = new minMaxPair<int>{ min = 30, max = 60 };
	}

	override protected void DoStartRound(minMaxPair<int> optimalAssists) {
		lastTimeTargetWasFound = Time.time;
	}

	override protected int CalcMeasuredVar(int numAssists) {
		return Mathf.RoundToInt(Time.time - lastTimeTargetWasFound);
	}
}
