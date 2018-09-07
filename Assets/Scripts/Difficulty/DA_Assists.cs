using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DA_Assists : DifficultyAdjustment {
	override protected void DoStartRound(minMaxPair<int> optimalAssists) {
		optimalRange = optimalAssists;
	}

	override protected int CalcMeasuredVar(int numAssists) {
		return numAssists;
	}
}
