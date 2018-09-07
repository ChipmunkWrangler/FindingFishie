using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
	public GameObject otherTimer;

	private float lastTimeTargetWasFound;

	private int timesFound = 0;
	private float lastTime = 0;
	private float totalTime = 0;
	private float bestTime = float.MaxValue;

	public void OnTargetClicked() {
		UpdateStoredTimes ();
		UpdateText ();
		Invoke ("Switch", 1f);
	}

	void Switch() {
		otherTimer.SetActive( true );
		gameObject.SetActive (false);
	}

	void UpdateStoredTimes() {
		++timesFound;
		lastTime = Time.time - lastTimeTargetWasFound;
		totalTime += lastTime;
		if (lastTime < bestTime) {
			bestTime = lastTime;
		}
	}

	void UpdateText() {
		float curTime = Time.time - lastTimeTargetWasFound;
		if (timesFound > 0) {
			float avgTime = totalTime / timesFound;
			GetComponent<Text> ().text = System.String.Format ( "Best: {0:F1} Avg: {1:F1} Last: {2:F1} Cur: {3:F1}", bestTime, avgTime, lastTime, curTime);
		} else {
			GetComponent<Text> ().text = System.String.Format ( "Cur: {0:F1}", curTime);
		}
	}

	void Start () {		
		lastTimeTargetWasFound = Time.time;
	}

	void Update() {
		UpdateText ();
	}

	void OnEnable() {
		lastTimeTargetWasFound = Time.time;
	}
}
