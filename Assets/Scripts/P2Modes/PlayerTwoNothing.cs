using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerTwoNothing : PlayerTwo {
	public void OnFishClicked(TouchParam touchParam) {
		ShowWrongSelection (touchParam.touchedObj);
	}
}
