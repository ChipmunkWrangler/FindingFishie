using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class PlayerTwoCollection : PlayerTwo {
	//-------------------------------------------------------------------------
	class CollectedFish {
		public GameObject o;

		HashSet<int> touchIds;
		float deleteAtTime;

		public CollectedFish(GameObject newO, int touchId) {
			o = newO;
			touchIds = new HashSet<int> ();
			touchIds.Add (touchId);
		}

		public void Touch(int touchId) {
			touchIds.Add (touchId);
		}

		public void Release(int touchId) {
			if (touchIds.Remove (touchId)) {
				if (!IsStillTouched ()) {
					deleteAtTime = Time.time + RELEASE_DELAY;
				}
			}
		}

		public bool IsTouchedBy(int touchId) {
			return touchIds.Contains (touchId);
		}

		public bool IsStillTouched() {
			return touchIds.Count > 0;
		}
		public bool ShouldRemove() {
			return !IsStillTouched () && deleteAtTime <= Time.time;
		}
	};
	//-------------------------------------------------------------------------
	[SerializeField] FishAppearanceChooser appearanceChooser;
	[SerializeField] minMaxIntPair MIN_FISH_IN_COLLECTION_RANGE; // at lowest difficulty, a collection must contain at least MIN_FISH_IN_COLLECTION_RANGE.min fish. 
	[SerializeField] int MAX_SECS_BETWEEN_FISH;
	[SerializeField] int MAX_WRONG_FISH;
	[SerializeField] int FISH_TO_ELIMINATE_MULTIPLIER;
	[SerializeField] int SCENERY_TO_ELIMINATE_MULTIPLIER = 1;
	[SerializeField] bool allowDrag;
	//-------------------------------------------------------------------------
	bool eliminateFishOnMatch;
	FishAppearance activeAppearance;
	List<CollectedFish> collectedFish;
	float mustAddFishBy;
	int numWrongFish;
	Dictionary<int, GameObject> lastTouchedObjByTouchId;
	int minFishInCollection;
	//-------------------------------------------------------------------------
	const float RELEASE_DELAY = 0.5f;
	//-------------------------------------------------------------------------
	override public string GetModeName (Difficulty difficulty) {
		return base.GetModeName(difficulty) + " " + GetMinFishInCollection(difficulty);
	}

	public void OnTargetClicked() {
		ResetState ();
	}

	public void OnFishClicked(TouchParam touchParam) {
		Assert.IsTrue (touchParam.touchId >= 0);
		FishAppearance appearance = GetAppearance (touchParam.touchedObj);
		if (appearance == null) { // touchedObj is not a fish
			return;
		}
		if (collectedFish.Count == 0) {
			AddFirstFish (touchParam, appearance);
		} else if (allowDrag) {
			ShowWrongSelection (touchParam.touchedObj);
			return;
		} else {
			ConsiderFish (touchParam);
		}
	}
		
	public void OnBackgroundReleased(TouchParam touchParam) {
		OnTouchReleased (touchParam.touchId);
	}

	public void OnFishReleased(TouchParam touchParam) {
		OnTouchReleased (touchParam.touchId);
	}

	public void OnFishEntered(TouchParam touchParam) {
		if (!allowDrag // ignore onFishEntered entirely
			|| !collectedFish.Exists(touchedFish => touchedFish.IsTouchedBy(touchParam.touchId))) // this is a touch that is not part of the current collection drag
		{
			return;
		}
		ConsiderFish (touchParam);
	}
		
	override protected void DoStartRound() {
		ResetState ();
	}

	void RejectFish(TouchParam touchParam) {
		++numWrongFish;
		ShowWrongSelection (touchParam.touchedObj);
		if (numWrongFish > MAX_WRONG_FISH) {
			EndCollection ();
		}
	}

	void AddFirstFish(TouchParam touchParam, FishAppearance appearance) {
		activeAppearance = appearance;
		AddFishToSet (touchParam);
		numWrongFish = 0;
	}

	void ConsiderFish(TouchParam touchParam) {
		Assert.IsTrue (collectedFish.Count > 0 && activeAppearance != null);
		FishAppearance touchedAppearance = GetAppearance (touchParam.touchedObj);
		if (touchedAppearance == null) {
			return; // not a fish
		}
		GameObject lastTouched;
		if (lastTouchedObjByTouchId.TryGetValue(touchParam.touchId, out lastTouched) && lastTouched == touchParam.touchedObj) {
			return;
		}
		lastTouchedObjByTouchId[touchParam.touchId] = touchParam.touchedObj;
		if (touchedAppearance == activeAppearance) {
			AddFishToSet (touchParam);
		} else {
			RejectFish (touchParam);
		}
	}

	override public int GetMinFishInCollection(Difficulty difficulty = null) {
		if (difficulty != null) {
			minFishInCollection = difficulty.CalcValFromRangeAndCurLevel (MIN_FISH_IN_COLLECTION_RANGE);
		}
		return minFishInCollection;
	}

	override protected void DoAwake() {
		collectedFish = new List<CollectedFish>();
		lastTouchedObjByTouchId = new Dictionary<int, GameObject> ();
		eliminateFishOnMatch = allowDrag;
	}

//////////////////////////////////////////////////////////////////////////
	void Update() {
		if (activeAppearance != null && mustAddFishBy <= Time.time) {
			EndCollection ();
		}
		if (collectedFish.Count > 0) {
			if (collectedFish.TrueForAll (collected => !collected.IsStillTouched ())) {
				EndCollection ();
			} else {
				var toUnpause = collectedFish.Where (collected => collected.ShouldRemove ());
				foreach (var u in toUnpause) {
					Unpause (u.o);
				}
				collectedFish = collectedFish.Except (toUnpause).ToList();
			}	
		}
	}
		
	void OnTouchReleased(int touchId) {
		foreach (var collected in collectedFish) {
			collected.Release(touchId);
			lastTouchedObjByTouchId [touchId] = null;
		}
	}

	void EndCollection() {
		if (collectedFish.Count >= minFishInCollection) {
			OnCollectionCompleted ();
		} 
		ResetState ();
	}
		
	void ResetState() {
		UnpauseAll ();
		activeAppearance = null;
		numWrongFish = -1;
		collectedFish.Clear ();
		lastTouchedObjByTouchId.Clear();
	}

	FishAppearance GetAppearance(GameObject fish) {
		return fish.GetComponent<FishAppearanceBehaviour> ().appearance;
	}
		
	int GetNumBackgroundFishToEliminate(int collectionSize) {
		return (1 + collectionSize - minFishInCollection) * FISH_TO_ELIMINATE_MULTIPLIER;
	}

	int GetNumSceneryToEliminate(int collectionSize, int numFishEliminated) {
		int numToEliminate = collectionSize - minFishInCollection;
		if (numFishEliminated == 0) {
			++numToEliminate; // we have run out of background fish, so hit the scenery harder
		}
		return numToEliminate * SCENERY_TO_ELIMINATE_MULTIPLIER;
	}

	void AddFishToSet(TouchParam touchParam) {
		GameObject newO = touchParam.touchedObj;
		CollectedFish alreadyCollected = collectedFish.Find (c => c.o == newO);
		if (alreadyCollected == null) {			
			collectedFish.Add (new CollectedFish (newO, touchParam.touchId));
			newO.SendMessage ("OnAddedToSet");
			mustAddFishBy = Time.time + MAX_SECS_BETWEEN_FISH;
			Color blinkColor = Color.white;
			if (collectedFish.Count == 1) {
				blinkColor = Color.grey;
			} else if (collectedFish.Count >= GetMinFishInCollection()) {
				blinkColor = Color.green;
			}
			BlinkFish (newO, blinkColor);
		} else {
			BlinkFish (newO, Color.clear);
			alreadyCollected.Touch(touchParam.touchId);
		}
	}

	int EliminateRandomFish(Transform fishContainer, FishSpawnPos.LAYER layer, int numToEliminate) {
		int numFishEliminated = 0;
		for (int i = fishContainer.childCount - 1; i >= 0 && numFishEliminated < numToEliminate; --i) {			
			var fish = fishContainer.GetChild (i).gameObject;
			if (!callbacks.IsTarget(fish) && callbacks.DestroyFish (fish)) {
				++numFishEliminated;
			}
		}
		numFish.ReduceDesiredNumFish (layer, numFishEliminated);
		return numFishEliminated;
	}

	void EliminateActiveFish() {
		foreach (var c in collectedFish) {
			callbacks.DestroyFish (c.o);
		}
	}

	void OnCollectionCompleted() {
		++numAssists;
		RewardCollectionCompleted (collectedFish.Count);
		EliminateActiveFish ();
	}

	void RewardCollectionCompleted(int collectionSize) {
		int numFishEliminated = 0;
		int numSceneryEliminated = 0;
		if (eliminateFishOnMatch) {
			numFishEliminated = EliminateRandomFish (fishContainers.background, FishSpawnPos.LAYER.BACKGROUND, GetNumBackgroundFishToEliminate (collectionSize));
			numSceneryEliminated = EliminateRandomScenery (GetNumSceneryToEliminate (collectionSize, numFishEliminated));
			if (numFishEliminated == 0 && numSceneryEliminated == 0) { // we have run out of background fish and scenery, so hit the foreground fish
				numFishEliminated = collectionSize;
				numFish.ReduceDesiredNumFish (FishSpawnPos.LAYER.FOREGROUND, numFishEliminated);
			}
		}
		ShowFeedbackText (GetFeedbackText (collectionSize, numFishEliminated, numSceneryEliminated));
	}

	void UnpauseAll() {
		foreach (var c in collectedFish) {
			Unpause(c.o);
		}
	}

	void Unpause(GameObject o ){
		if (o != null) {
			o.SendMessage ("OnRemovedFromSet");
		}	
	}

	string GetFeedbackText(int collectionSize, int numFishEliminated, int numSceneryEliminated) {
		string msg = "";
		if (collectionSize > minFishInCollection) {
			msg = "COMBO " + collectionSize + new string ('!', collectionSize - minFishInCollection) + "\n";
		} else if (!eliminateFishOnMatch) {
			msg = "MATCH!\n";
		}
		if (eliminateFishOnMatch) { 
			if (numFishEliminated > 0) {			
				msg += "Removing " + numFishEliminated + " fish!\n";
			}
			if (numSceneryEliminated > 0) {
				msg += "Removing " + numSceneryEliminated + " obstacle(s)!";
			}
		}
		return msg;
	}

}
