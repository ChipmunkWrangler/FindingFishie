using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RoleManager : MonoBehaviour {
	[SerializeField] PlayerOne[] playerOnes;
	[SerializeField] PlayerTwo[] playerTwos;
	[SerializeField] minMaxPair<int> difficultyRange;
	[SerializeField] Text leftPlayerModeText;
	[SerializeField] Text rightPlayerModeText;
	[SerializeField] int switchRolesAfterNumFish; // if switchRolesAfterTime is not set, switch roles every time one player gets this many fish. 
	[SerializeField] int switchRolesAfterSecs; // if switchRolesAfterNumFish is not set, switch roles after this much time
	[SerializeField] FeedbackText feedbackText;
	[SerializeField] AudioSource switchSound;
	// if neither switchRolesAfterTime nor switchRolesAfterTime is set, switch at the beginning of a round
	// if both are set, switch when a fish is tapped if both criteria are fulfilled
	Difficulties leftPlayerDifficulties;
	Difficulties rightPlayerDifficulties;

	const string leftPlayerKey = "leftPlayer";
	const string rightPlayerKey = "rightPlayer";

	bool isLeftPlayerP1;

	public PlayerOne activePlayerOne { get; private set; }
	public PlayerTwo activePlayerTwo { get; private set; }
	DifficultyAdjustment p1DifficultyAdjustment;
	PlayerOne forceNextPlayerOne;
	PlayerTwo forceNextPlayerTwo;

	int numFishSinceRoleSwitch;
	float roleSwitchTime;

	public void OnAddFish(GameObject fish) {
		fish.GetComponent<Publisher> ().Subscribe (gameObject);
		activePlayerTwo.OnAddFish (fish);
	}

	public int GetFishPerLayer() {
		return (activePlayerOne.fishPerLayer > 0) ? activePlayerOne.fishPerLayer : p1DifficultyAdjustment.difficulty.fishPerLayer;
	}

	public Difficulty GetDifficulty() {
		return p1DifficultyAdjustment.difficulty;
	}

	public void StartRound() {
		SetNewPlayerOne (forceNextPlayerOne);
		SetNewPlayerTwo (activePlayerOne.forcePlayerTwo == null ? forceNextPlayerTwo : activePlayerOne.forcePlayerTwo);
		SwapPlayers();
		p1DifficultyAdjustment.StartRound (activePlayerTwo.GetOptimalAssists(), activePlayerTwo.highAssistsIncreaseDifficulty);
		forceNextPlayerOne = null;
		forceNextPlayerTwo = null;
	}

	void SwapPlayers() {
		
		isLeftPlayerP1 = !isLeftPlayerP1;
		activePlayerOne.SetLeft (isLeftPlayerP1);
		activePlayerTwo.SetLeft (!isLeftPlayerP1);
		leftPlayerModeText.text = isLeftPlayerP1 ? activePlayerOne.GetModeName() : activePlayerTwo.GetModeName(GetDifficulty());
		rightPlayerModeText.text = isLeftPlayerP1 ? activePlayerTwo.GetModeName(GetDifficulty()) : activePlayerOne.GetModeName();
		numFishSinceRoleSwitch = 0;
		roleSwitchTime = Time.time + switchRolesAfterSecs;
	}

	public void UpdateDifficulty () {
		p1DifficultyAdjustment.UpdateDifficulty (activePlayerTwo.numAssists);
	}

	void Awake() {
		leftPlayerDifficulties = new Difficulties(leftPlayerKey, difficultyRange, playerOnes.Length);
		rightPlayerDifficulties = new Difficulties(rightPlayerKey, difficultyRange, playerOnes.Length);
	}

	void SetNewPlayerOne(PlayerOne force) {
		DifficultyAdjustment[] difficultyArray = (isLeftPlayerP1 ? leftPlayerDifficulties : rightPlayerDifficulties).p1Difficulties;
		UnityEngine.Assertions.Assert.AreEqual (difficultyArray.Length, playerOnes.Length);
		int i;
		if (force) {
			activePlayerOne = force;
			i = GetIndexForPlayerOne (force);
		} else {
			i = TTLib.GetRandomIdxWhere (difficultyArray, difficultyAdjustment => difficultyAdjustment.difficulty.IsLegal ());
			activePlayerOne = playerOnes [i];
		}
		foreach (var p1 in playerOnes) {
			p1.gameObject.SetActive(  p1 == activePlayerOne );
		}
		p1DifficultyAdjustment = difficultyArray [i];
	}

	void SetNewPlayerTwo(PlayerTwo force) {
//		Difficulty[] difficultyArray = (isLeftPlayerP1 ? rightPlayerDifficulties : leftPlayerDifficulties).p1Difficulties;
//		UnityEngine.Assertions.Assert (difficultyArray.Length == playerTwos.Length);
//		int i = TTLib.GetRandomIdxWhere (difficultyArray, difficulty => difficulty.IsLegal ());
		if (force) {
			activePlayerTwo = force;
		} else {
			int i = TTLib.GetRandomIdxWhere (playerTwos, playerTwo => true);
			activePlayerTwo = playerTwos [i];
		}
		foreach (var p2 in playerTwos) {
			p2.gameObject.SetActive( p2 == activePlayerTwo );
		}
	}

	void OnUseNextPlayerTwo() {
		forceNextPlayerTwo = playerTwos [GetIndexForPlayerTwo (activePlayerTwo) + 1];
		gameObject.SendMessage ("OnForceLevelEnd"); 
	}

	void OnUseNextPlayerOne() {
		int i = (GetIndexForPlayerOne (activePlayerOne) + 1) % playerOnes.Length;
		forceNextPlayerOne = playerOnes [i];
		gameObject.SendMessage ("OnForceLevelEnd"); 
	}

	void OnResetDifficulty() {
		leftPlayerDifficulties.Reset ();
		rightPlayerDifficulties.Reset ();
	}

	int GetIndexForPlayerOne(PlayerOne p1) {
		return Array.IndexOf(playerOnes, p1);
	}
		
	int GetIndexForPlayerTwo(PlayerTwo p2) {
		return Array.IndexOf(playerTwos, p2);
	}

	void OnFishClicked(TouchParam touchParam) {
		if (switchRolesAfterNumFish == 0 && switchRolesAfterSecs == 0) {
			return;
		}		
		if (!activePlayerTwo.allowSwitch) {
			return;
		}
		++numFishSinceRoleSwitch;
		if ((switchRolesAfterNumFish == 0 || numFishSinceRoleSwitch >= switchRolesAfterNumFish)
		   && (switchRolesAfterSecs == 0 || Time.time >= roleSwitchTime)) {
			SwapPlayers ();
			feedbackText.ShowMessage ("Switch!");
			switchSound.Play ();
		}
	}
}
