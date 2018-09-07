using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTwo : MonoBehaviour {
	public NumFish numFish;
	public FishContainers fishContainers;
	public SceneryManager scenery;
	public FeedbackText feedbackText;
	public int TIMES_TO_BLINK;
	public float SECS_PER_BLINK;
	[SerializeField] string modeName;
	[SerializeField] minMaxIntPair optimalAssists;
	public bool allowSwitch;
	public bool highAssistsIncreaseDifficulty;
	public int numAssists { get; protected set; }

	protected PlayerTwoCallbacks callbacks;
	protected GameObject targetFish { get; private set; }

	HashSet<GameObject> blinkingFish;

	virtual public string GetModeName (Difficulty difficulty) { return modeName; }
	virtual public bool ShouldReturnToScreen (GameObject fish) { return false; }
	virtual public void OnDestroyFish (GameObject fish) {}
	public void StartRound(GameObject tgtFish) { 
		numAssists = 0;
		targetFish = tgtFish;
		DoStartRound ();
	}
	virtual public void SetLeft (bool b) {}
	virtual public int GetMinFishInCollection(Difficulty difficulty) { return 1; }
	public minMaxPair<int> GetOptimalAssists () { return optimalAssists; }
	virtual protected void DoAddFish(GameObject fish) {}
	virtual protected void DoAwake() {}
	virtual protected void DoStartRound() {}

	public void OnAddFish (GameObject fish) {
		SubscribeTo (fish);
		DoAddFish (fish);
	}

	public void SetCallbacks (PlayerTwoCallbacks _callbacks) {
		callbacks = _callbacks;
	}


	protected void ShowFeedbackText(string msg) {
		feedbackText.ShowMessage (msg);
	}

	protected int EliminateRandomScenery(int numToRemove) { 
		return scenery.RemoveScenery (numToRemove);
	}

	protected void ShowWrongSelection (GameObject fish) {
		BlinkFish (fish, Color.red);
	}

	protected void BlinkFish(GameObject fish, Color c) {
		StartCoroutine (BlinkSprite(fish, c));
	}

	void Awake() {
		blinkingFish = new HashSet<GameObject>();
		DoAwake ();
	}

	void SubscribeTo(GameObject o) {
		Publisher pub = o.GetComponent<Publisher> ();
		pub.Subscribe (gameObject);
	}

	IEnumerator BlinkSprite(GameObject o, Color c) {
		if (blinkingFish.Add (o)) {
			SpriteRenderer r = o.GetComponent<SpriteRenderer> ();
			Color oldColor = r.color;
			for (int i = 0; i < TIMES_TO_BLINK; ++i) {
				if (o) {
					r.color = c;
					yield return new WaitForSeconds (SECS_PER_BLINK * 0.5f);
				}
				if (o) {
					r.color = oldColor;
					yield return new WaitForSeconds (SECS_PER_BLINK * 0.5f);
				}
			}
			blinkingFish.Remove (o);
		}
	}

}

public interface PlayerTwoCallbacks { 
	bool DestroyFish(GameObject fish, bool isPrivate = false);
	GameObject AddAFish (FishSpawnPos.LAYER layer);
	bool IsTarget(GameObject fish);
}