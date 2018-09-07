using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerTwoAltTarget : PlayerTwo {
	public TargetUI ALT_TARGET_UI;
	public int FOREGROUND_FISH_TO_ELIMINATE;
	public int BACKGROUND_FISH_TO_ELIMINATE;
	public int SCENERY_TO_ELIMINATE;
////////////////////////////////////////////////////////////////////////
	GameObject altTargetFish;
	HashSet<GameObject> altTargets;
////////////////////////////////////////////////////////////////////////
	public void OnTargetClicked() {
		ClearAltTargetFishList ();
		DisableAltTargetFishUI ();
	}

	public void OnFishClicked(TouchParam touchParam) {
		if (IsAltTargetFish (touchParam.touchedObj)) {
			AltTargetClicked (touchParam.touchedObj);
		} else {
			ShowWrongSelection (touchParam.touchedObj);
		}
	}

	override public bool ShouldReturnToScreen(GameObject fish) { 
		return IsLastInstanceOfAltTarget (fish);
	}

	override public void OnDestroyFish(GameObject fish) {
		altTargets.Remove(fish);
	}

	override protected void DoStartRound() {
		InitAltTargetFish ();
	}
		
	override public void SetLeft(bool b) {
		ALT_TARGET_UI.SetLeft (b);
	}

	override protected void DoAddFish(GameObject fish) {
		SetAsAltTargetIfIndistinguishable (fish);
	}
		
////////////////////////////////////////////////////////////////////////
	void Start() {
 		altTargets = new HashSet<GameObject> ();
	}

	void DisableAltTargetFishUI() {
		ALT_TARGET_UI.SetEnabled (false);
	}

	void ClearAltTargetFishList() {
		altTargetFish = null;
		altTargets.Clear ();
	}

	int RemoveAltTargetFish(GameObject tappedFish) {
//		int numRemoved = altTargets.Count;
//		foreach (GameObject fish in altTargets) {
//			numFish.ReduceDesiredNumFish (fishContainers.GetLayer(fish), 1);
//			callbacks.DestroyFish (fish, true);
//		}
//		ClearAltTargetFishList ();
//		return numRemoved;
		numFish.ReduceDesiredNumFish (fishContainers.GetLayer(tappedFish), 1);
		callbacks.DestroyFish (tappedFish, true);
		ClearAltTargetFishList ();
		return 1;
	}

	bool IsAltTargetFish(GameObject fish) {
		return altTargets.Contains (fish);
	}

	bool IsLastInstanceOfAltTarget(GameObject fish) {
		return (GetNumAltTargetFish() == 1 && IsAltTargetFish(fish));
	}

	int GetNumAltTargetFish() {
		return altTargets.Count;
	}

	GameObject ChooseAltTargetFish() {
		for (int i = 0; i < fishContainers.foreground.childCount; ++i) {
			GameObject fish = fishContainers.foreground.GetChild (i).gameObject;
			if (fish.activeSelf) {
				return fish;
			}
		}
		for (int i = 0; i < fishContainers.background.childCount; ++i) {
			GameObject fish = fishContainers.background.GetChild (i).gameObject;
			if (fish.activeSelf && fish != targetFish) { // TODO compare appearence, not identity, so we can have nonunique tgt fish
				return fish;
			}
		}
		return callbacks.AddAFish (FishSpawnPos.LAYER.FOREGROUND);
	}

	void InitAltTargetFish() {
		Assert.AreEqual (altTargets.Count, 0);
		Assert.IsNull (altTargetFish);
		altTargetFish = ChooseAltTargetFish ();
		altTargets.Add (altTargetFish);
		ALT_TARGET_UI.Init(altTargetFish);
		ALT_TARGET_UI.SetEnabled (true);
		SetAltFishInLayer (fishContainers.foreground);
		SetAltFishInLayer (fishContainers.background);
	}

	void SetAltFishInLayer(Transform fishContainer) {
		for (int i = 0; i < fishContainer.childCount; ++i) {
			GameObject fish = fishContainer.GetChild (i).gameObject;
			if (fish.activeSelf && AreIndistinguishable (altTargetFish, fish)) {
				MakeAltTarget (fish);
			}
		}
	}

	void MakeAltTarget(GameObject fish) {
		altTargets.Add (fish);
	}

	void SetAsAltTargetIfIndistinguishable(GameObject fish ) {
		if (altTargetFish) {
			if (AreIndistinguishable (altTargetFish, fish)) {
				MakeAltTarget (fish);
			}
		}
	}

	bool AreIndistinguishable(GameObject fishA, GameObject fishB) {
		SpriteRenderer rendererA = fishA.GetComponent<SpriteRenderer> ();
		SpriteRenderer rendererB = fishB.GetComponent<SpriteRenderer> ();
		return (rendererA.sprite == rendererB.sprite && ColorSelectorHsv.AreSimilar (rendererA.color, rendererB.color));
	}

	string GetFeedbackText(int numFishRemoved) {
		return (numFishRemoved > 1) ? numFishRemoved + " Fish Caught!" : "Fish Caught";
	}		

	void AltTargetClicked(GameObject fish) {
		++numAssists;
//		int numTargetFishRemoved = 
		RemoveAltTargetFish (fish);
		DisableAltTargetFishUI ();
		InitAltTargetFish ();
//		EliminateRandomScenery (SCENERY_TO_ELIMINATE);
//		ShowFeedbackText (GetFeedbackText (numTargetFishRemoved));
	}

}
