using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Fish {
	public GameObject prefab;
	public bool legalTarget;
};

[System.Serializable]
public class FishShapeFrequencyPair : FrequencyPair<Fish> {
}

[System.Serializable]
public class FishShapeFrequencyList : FrequencyList<FishShapeFrequencyPair, Fish> {
}

public class FishAppearanceChooser : MonoBehaviour {
	public int NUM_PATTERNS;
	public FishShapeFrequencyList shapeFrequencyList;

	enum DistinguishingFeatures {
		SHAPE,
		PATTERN,
		COLOR,
		COUNT
	}

	[SerializeField] float CHANCE_OF_EXTRA_DISTINGUISHING_FEATURES;
	FishAppearance[] nontargetAppearances;
	FishAppearance targetAppearance;
	int minFishPerCollection;
	bool isOnlyTargetUnique;
	List<int>[] fishIdsByNontargetAppearanceIdx;
		
	public FishAppearance ChooseTargetFishAppearance() {
		ClearNonTargetTypes ();
		targetAppearance = new FishAppearance{ 
			prefab = ChooseTargetPrefab (), 
			color = ColorSelectorHsv.GetRandomColor (), 
			patternIdx = ChoosePatternIdx ()
		};
		return targetAppearance;
	}
		
	public FishAppearance GetNontargetAppearance(bool avoidCreatingNewGroups) {
		return AreNonTargetsGrouped () ? GetNontargetAppearanceGrouped (avoidCreatingNewGroups) : GetAnyAppearanceExceptTarget ();
	}

	public void SetParams(int numFishDesired, int _minFishPerCollection, bool _isOnlyTargetUnique) {
		isOnlyTargetUnique = _isOnlyTargetUnique;
		minFishPerCollection = _minFishPerCollection;
		if (isOnlyTargetUnique && minFishPerCollection <= 1) {
			minFishPerCollection = 2;
		}
		if (AreNonTargetsGrouped()) {
			int numNonTargetAppearances = Mathf.Max (1, Mathf.FloorToInt (numFishDesired / minFishPerCollection));
			nontargetAppearances = new FishAppearance[numNonTargetAppearances];
			fishIdsByNontargetAppearanceIdx = new List<int>[numNonTargetAppearances];
			for (int i = 0; i < fishIdsByNontargetAppearanceIdx.Length; ++i) {
				fishIdsByNontargetAppearanceIdx [i] = new List<int> ();
			}
		} 	
	}

	public void RegisterNontargetAppearance(int instanceId, FishAppearance appearance) {
		if (AreNonTargetsGrouped ()) {
			int i = GetIdxOfNontargetAppearance (appearance, nontargetAppearances);
			if (i != -1) {
				fishIdsByNontargetAppearanceIdx [i].Add (instanceId);
			}
		}
	}
		
	public void UnregisterNontargetAppearance(int instanceId) {
		if (AreNonTargetsGrouped ()) {
			foreach (var idList in fishIdsByNontargetAppearanceIdx) {
				idList.Remove (instanceId);
			}
		}
	}

	public bool NeedsMoreFish() {
		return AreNonTargetsGrouped() 
			&& (fishIdsByNontargetAppearanceIdx.Where (IsUnique).Count () > 0
				|| NoCompleteGroup ());
	}

	bool NoCompleteGroup() {
		return fishIdsByNontargetAppearanceIdx.Where (IsCompleteGroup).Count () == 0;
	}

	bool AreNonTargetsGrouped() {
		return minFishPerCollection > 1;
	}

	bool IsCompleteGroup(IEnumerable<int> idList) {
		return idList.Count() >= minFishPerCollection;
	}	

	bool IsIncompleteGroup(IEnumerable<int> idList) {
		return idList.Count() < minFishPerCollection && idList.Count() > 0;
	}	

	bool IsUnique(IEnumerable<int> idList) {
		return idList.Count() == 1;
	}

	bool IsExistingGroup(IEnumerable<int> idList) {
		return idList.Count() > 0;
	}
		
	FishAppearance GetNontargetAppearanceGrouped(bool avoidCreatingOrphan) {
		int appearanceIdx = TTLib.GetRandomIdxWhere(fishIdsByNontargetAppearanceIdx, IsUnique);
		if (appearanceIdx == -1 && NoCompleteGroup ()) {
			appearanceIdx = TTLib.GetRandomIdxWhere (fishIdsByNontargetAppearanceIdx, IsIncompleteGroup);
		}
		if (appearanceIdx == -1 && avoidCreatingOrphan) {
			appearanceIdx = TTLib.GetRandomIdxWhere (fishIdsByNontargetAppearanceIdx, IsExistingGroup);
		}
		if (appearanceIdx == -1) {
			appearanceIdx = Random.Range (0, nontargetAppearances.Count ()); 
		}
		if (nontargetAppearances[appearanceIdx] == null) {
			nontargetAppearances [appearanceIdx] = CreateUniqueNontargetType ();
		}
		return nontargetAppearances [appearanceIdx];
	}


		
	FishAppearance GetAnyAppearanceExceptTarget() {
		// each fish differs from the target fish in at least one respect
		var chanceOfExtraDistinguishingFeatures = CHANCE_OF_EXTRA_DISTINGUISHING_FEATURES; // TODO by difficulty
		DistinguishingFeatures feature = (DistinguishingFeatures)Random.Range(0, (int)DistinguishingFeatures.COUNT);
		bool hasTargetColor = (feature != DistinguishingFeatures.COLOR) && Random.value >= chanceOfExtraDistinguishingFeatures;
		bool hasTargetShape = feature != DistinguishingFeatures.SHAPE && Random.value >= chanceOfExtraDistinguishingFeatures;
		bool hasTargetPattern = feature != DistinguishingFeatures.PATTERN && Random.value >= chanceOfExtraDistinguishingFeatures;
		GameObject prefab = (hasTargetShape) ? targetAppearance.prefab : ChooseNonTargetPrefab ();
		int patternIdx = (hasTargetPattern) ? targetAppearance.patternIdx : ChooseNonTargetPatternIdx();
		Color color = (hasTargetColor) ? targetAppearance.color : ColorSelectorHsv.GetColorDistinctFrom (targetAppearance.color);
		return new FishAppearance {
			prefab = prefab, 
			patternIdx = patternIdx,
			color = color
		};
	}
		
	void ClearNonTargetTypes() {
		if (nontargetAppearances != null) {
			System.Array.Clear (nontargetAppearances, 0, nontargetAppearances.Length);
		}
	}

	GameObject ChooseNonTargetPrefab() { // [Pure]
		// a filter was added in ChooseTargetPrefab that guarantees that the prefab chosen is nontarget
		return shapeFrequencyList.ChooseItem().prefab ;
	}

	int ChooseNonTargetPatternIdx() { // [Pure]
		int patternIdx = Random.Range (0, NUM_PATTERNS-1);
		if (patternIdx == targetAppearance.patternIdx) {
			++patternIdx;
		}
		return patternIdx;
	}

	FishAppearance CreateUniqueNontargetType() {
		FishAppearance newType;
		do {
			newType = GetAnyAppearanceExceptTarget();
		} while (GetIdxOfNontargetAppearance(newType, nontargetAppearances) != -1);
		return newType;
	}

	int GetIdxOfNontargetAppearance(FishAppearance appearance, FishAppearance[] appearances) {
		for (int i = 0; i < appearances.Length; ++i) {
			FishAppearance otherAppearance = appearances [i];
			if (otherAppearance != null && !FishAppearance.AreDistinct(otherAppearance, appearance)) {
				return i;
			}
		}
		return -1;
	}

	GameObject ChooseTargetPrefab() { // [Pure]
		shapeFrequencyList.ClearFilters ();
		shapeFrequencyList.AddFilter(fish => fish.legalTarget );
		GameObject targetPrefab = shapeFrequencyList.ChooseItem().prefab ;
		shapeFrequencyList.ClearFilters ();
		shapeFrequencyList.AddFilter(fish => fish.prefab != targetPrefab );
		return targetPrefab;
	}
		
	int ChoosePatternIdx() { // [Pure]
		return Random.Range (0, NUM_PATTERNS);
	}
}
