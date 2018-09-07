using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;

[System.Serializable]
public class FishContainers {
	public Transform background;
	public Transform foreground;

	public Transform GetFishContainerForLayer(FishSpawnPos.LAYER layer ) {
		return (layer == FishSpawnPos.LAYER.FOREGROUND) ? foreground : background;
	}
		
	public FishSpawnPos.LAYER GetLayer(GameObject fish) { 
		return (fish.transform.parent == GetFishContainerForLayer(FishSpawnPos.LAYER.BACKGROUND)) 
			? FishSpawnPos.LAYER.BACKGROUND
			: FishSpawnPos.LAYER.FOREGROUND;			
	}
}

public class FishSpawner : MonoBehaviour, PlayerTwoCallbacks {
	public GameObject targetFish;	
	public Publisher backgroundClickCatcher;
	public FishAppearanceChooser appearanceChooser;
	public NumFish numFish;
	public FishContainers fishContainers;
	[SerializeField] RoleManager roleManager;
////////////////////////////////////////////////////////////////////////

	private enum State {
		PLAY,
		TRANSITION_BETWEEN_LEVELS
	}
////////////////////////////////////////////////////////////////////////
	State curState;
	PlayerOne activePlayerOne;
	PlayerTwo activePlayerTwo;
	CameraUtils cameraUtils;
	const int DEFAULT_NONTARGET_GROUP_SIZE = 2;
	GameObject partnerFish;
////////////////////////////////////////////////////////////////////////

	void OnForceLevelEnd() {
		targetFish.SetActive(true);
		targetFish.SendMessage ("OnTTBeginTouch", new TouchParam()); 
	}

	public void OnFishLeftScreen(GameObject fish) {
		if (curState == State.PLAY && IsTarget(fish) || fish == partnerFish || activePlayerTwo.ShouldReturnToScreen(fish)) {
			SendBackOntoScreen (fish);
		} else {
			DestroyFish (fish);
		}
	}

	public void OnTargetClicked() {
		RemoveTargetFish ();
		AllowFishToLeaveScreen ();
		DestroyFish (partnerFish, true);
		roleManager.UpdateDifficulty ();
		curState = State.TRANSITION_BETWEEN_LEVELS;
	}

	public bool IsTarget(GameObject fish) {
		return fish == targetFish;
	}

	public bool DestroyFish(GameObject fish, bool isPrivate = false ) {
		if (fish == null || !fish.activeSelf) {
			return false;
		}
		if (!isPrivate) {
			activePlayerTwo.OnDestroyFish (fish);
		}
		appearanceChooser.UnregisterNontargetAppearance (fish.GetInstanceID ());
		fish.SetActive (false); // since Destroy is not immediate, we set inactive to be able to identify the destroyed objects in this frame
		Destroy (fish);
		return true;
	}

	public GameObject AddAFish(FishSpawnPos.LAYER layer) {
		GameObject fish = SpawnFishDistinguishableFromTarget (layer);
		InitFish (fish);
		roleManager.OnAddFish (fish);
		return fish;
	}

////////////////////////////////////////////////////////////////////////
	void Start() {		
		cameraUtils = new CameraUtils ();
		StartNextLevel ();
	}
 
	void Update () {
		switch (curState) {
		case State.PLAY:
			UpdatePlay ();
			break;
		case State.TRANSITION_BETWEEN_LEVELS:
			UpdateTransitionBetweenLevels ();
			break;
		default:
			throw(new System.ApplicationException("Unknown state" + curState.ToString()));
		}				
	}
		
	void RemoveTargetFish() {
		targetFish.SetActive (false);
	}

	void AllowFishToLeaveScreen() {
		AllowFishOnLayerToLeaveScreen (fishContainers.background);
		AllowFishOnLayerToLeaveScreen (fishContainers.foreground);
	}

	void AllowFishOnLayerToLeaveScreen(Transform fishContainer) {
		for (int i = 0; i < fishContainer.childCount; ++i) {
			fishContainer.GetChild (i).GetComponent<FishMover> ().SetPaused(true);
		}
	}

	void InitTargetFish() {
		Assert.IsFalse (targetFish.activeSelf);
		SetTargetFishSprite();
		InitFish (targetFish);
		ScaleFishCollider (targetFish);
		targetFish.SetActive (true);
	}

	void SetTargetFishSprite() {
		FishAppearance targetAppearance = appearanceChooser.ChooseTargetFishAppearance ();
		GameObject targetTemplateFish = InstantiateFish (targetAppearance, FishSpawnPos.LAYER.BACKGROUND);
		SpriteRenderer targetRenderer = targetFish.GetComponent<SpriteRenderer> ();
		targetRenderer.sprite = targetTemplateFish.GetComponent<SpriteRenderer> ().sprite;
		targetRenderer.color = targetAppearance.color;
		GetComponent<FishSpawnPos> ().SetStandardFish (targetTemplateFish);
		targetFish.GetComponent<FishAppearanceBehaviour> ().appearance = targetAppearance;
		DestroyFish (targetTemplateFish, true);
	}


	void InitializeTarget() {
		roleManager.StartRound ();
		activePlayerOne = roleManager.activePlayerOne;
		activePlayerTwo = roleManager.activePlayerTwo;
		numFish.originalFishPerLayer = roleManager.GetFishPerLayer();
		appearanceChooser.SetParams (numFish.GetNumDesired (FishSpawnPos.LAYER.FOREGROUND) + numFish.GetNumDesired (FishSpawnPos.LAYER.BACKGROUND), activePlayerTwo.GetMinFishInCollection (roleManager.GetDifficulty()), activePlayerOne.IsOnlyTargetUnique());
		InitTargetFish ();
		partnerFish = AddAFish(FishSpawnPos.LAYER.BACKGROUND);
		partnerFish.transform.position = new Vector3( partnerFish.transform.position.x, partnerFish.transform.position.y, targetFish.transform.position.z );
		activePlayerOne.StartRound (targetFish, partnerFish);
		activePlayerTwo.SetCallbacks (this);
		activePlayerTwo.StartRound (targetFish);
	}

	void AddAFishIfNecessary() {
		bool fishAdded = false;
		foreach (FishSpawnPos.LAYER layer in FishSpawnPos.GetLayers()) {
			if (GetNumFishToAdd (layer) > 0) {
				AddAFish (layer);
				fishAdded = true;
			}
		}
		if (!fishAdded && appearanceChooser.NeedsMoreFish ()) {
			AddAFish (FishSpawnPos.LAYER.FOREGROUND);
		}
	}

	void UpdatePlay() {
		AddAFishIfNecessary ();
	}

	void UpdateTransitionBetweenLevels() {
		if (!Camera.main.GetComponent<MoveToNextPosition> ().IsMoving ()) {
			StartNextLevel ();
		}		
	}

	void StartNextLevel() {
		cameraUtils.UpdateAfterMove ();
		Assert.AreEqual (fishContainers.background.childCount, 1);
		Assert.AreEqual (fishContainers.foreground.childCount, 0);
		numFish.OnNewTarget ();
		InitializeTarget ();
		curState = State.PLAY;
	}

	int GetNumFishToAdd(FishSpawnPos.LAYER layer) { // [Pure]
		return Mathf.Max(0, numFish.GetNumDesired(layer) - fishContainers.GetFishContainerForLayer(layer).childCount); 
	}

	bool IsLastFishToAdd() {
		return FishSpawnPos.GetLayers().Sum (GetNumFishToAdd) == 1;
	}

	void InitFish(GameObject fish) { 
		fish.transform.position = GetComponent<FishSpawnPos>().GetSpawnPos (Random.value <= 0.5f, fish, fishContainers.GetLayer(fish)); 
		FishMover fishMover = fish.GetComponent<FishMover> ();
		fishMover.SetCameraUtils (cameraUtils);
		fishMover.SetFishTracker (gameObject);
		fishMover.SetPaused (false);
		activePlayerOne.AdjustMovement (fishMover, IsTarget (fish));
		fishMover.ResetMovement ();
	}
		
	void ScaleFishCollider(GameObject fish) {
		var collider = fish.GetComponent<BoxCollider> ();
		if (collider) {
			var size = fish.GetComponent<SpriteRenderer> ().bounds.size;
			collider.size = size;
		}
	}

	GameObject SpawnFishDistinguishableFromTarget(FishSpawnPos.LAYER layer) {
		FishAppearance appearance = appearanceChooser.GetNontargetAppearance(IsLastFishToAdd());
		GameObject fish = InstantiateFish (appearance, layer);
		appearanceChooser.RegisterNontargetAppearance (fish.GetInstanceID (), appearance);
		ScaleFishCollider (fish);
		backgroundClickCatcher.Subscribe (fish);
		return fish;
	}
				
	GameObject InstantiateFish(FishAppearance appearance, FishSpawnPos.LAYER layer) {
		GameObject fish = Instantiate<GameObject> (appearance.prefab, fishContainers.GetFishContainerForLayer(layer)) ;
		fish.GetComponent<TextureSelector> ().SetPatternIdx (appearance.patternIdx);
		fish.GetComponent<SpriteRenderer> ().color = appearance.color;
		fish.GetComponent<FishAppearanceBehaviour> ().appearance = appearance;
		return fish;
	}

	void SendBackOntoScreen(GameObject fish) {
		InitFish (fish);
	}
}
