using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryManager : MonoBehaviour {
/// ///////////////////////////////////////////
	public Transform sceneryParent;
/// ///////////////////////////////////////////
	List<Transform> scenery;
/// ///////////////////////////////////////////
	public int RemoveScenery(int numToRemove) {
		int numRemoved = 0;
		for (int i = 0; i < numToRemove; ++i) {
			numRemoved += RemoveSingleScenery ();
		}
		return numRemoved;
	}

	public void OnTargetClicked() {
		Reset ();
	}
/// ///////////////////////////////////////////
	void Awake() {
		scenery = new List<Transform> ();
	}

	void Start() {
		Reset ();
	}
		
	int RemoveSingleScenery() {
		if (scenery.Count == 0) {
			return 0;
		}

		int i = Random.Range (0, scenery.Count);
		scenery [i].gameObject.SetActive (false);
		scenery.RemoveAt (i);
		return 1;
	}

	void Reset() {
		scenery.Clear ();
		for(int i = 0; i < sceneryParent.childCount; ++i) {
			Transform child = sceneryParent.GetChild (i);
			scenery.Add (child);
			child.gameObject.SetActive (true);
		}
	}

}
