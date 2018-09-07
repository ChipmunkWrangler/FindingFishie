using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOne : MonoBehaviour {
	[SerializeField] bool isTgtAppearanceUnique;
	[SerializeField] int timerSecs; // if this is set, there is no target fish and the round ends only with a timer
	[SerializeField] UnityEngine.UI.Text timerText; // if this is set, there is no target fish and the round ends only with a timer
	public int fishPerLayer; // override the difficulty setting to manager difficulty in another way
	public PlayerTwo forcePlayerTwo;
	public bool allFishCanLeaveScreen; // if true, this overrides the fish-specific setting
	public bool targetFishCannotLeaveScreen; // if true, this overrides the inspector setting of the target fish and allFishCanLeaveScreen
	public TargetUI targetUI;
	[SerializeField] string modeName;
	[SerializeField] MovementMode tgtSpecialMovement;

	float endTime;

	public string GetModeName() {
		return modeName;
	}

	public bool IsOnlyTargetUnique() { 
		return isTgtAppearanceUnique && targetUI == null;
	}

	public void SetLeft(bool b) {
		if (targetUI) {
			targetUI.SetLeft (b);
		}
	}

	public void StartRound(GameObject targetFish, GameObject partnerFish) {
		if (targetUI) {
			targetUI.SetEnabled (true);
			targetUI.Init (targetFish);
		}
		if (IsTimer ()) {
			targetFish.SetActive (false);
			InitTimer (targetFish);
		}
		if (tgtSpecialMovement) {
			tgtSpecialMovement.partnerFish = partnerFish;
		}
	}

	public void AdjustMovement(FishMover fishMover, bool isTarget) {
		if (allFishCanLeaveScreen) {
			fishMover.canLeaveScreen = true;
		}
		if (isTarget) {
			fishMover.special = tgtSpecialMovement;
			if (targetFishCannotLeaveScreen) {
				fishMover.canLeaveScreen = false;
			}
		}	

	}

	void Update() {
		if (timerText) {
			UpdateTimerText();
		}
	}

	void InitTimer(GameObject targetFish) {
		StartCoroutine (EndRoundTimer (targetFish));
		endTime = Time.time + timerSecs;
		if (timerText) {
			timerText.enabled = true;
		}
	}

	void UpdateTimerText() {
		float timeLeft = endTime - Time.time;
		timerText.text = System.String.Format ( "{0:F1}", timeLeft);
	}

	void OnTargetClicked() {
		if (targetUI) {
			targetUI.SetEnabled (false);
		}
	}

	IEnumerator EndRoundTimer(GameObject targetFish) {
		yield return new WaitForSeconds (timerSecs);
		targetFish.SetActive (true);
		if (timerText) {
			timerText.enabled = false;
		}
		targetFish.SendMessage ("OnTTBeginTouch", new TouchParam()); 
	}

	bool IsTimer() { 
		return timerSecs > 0;
	}
}
