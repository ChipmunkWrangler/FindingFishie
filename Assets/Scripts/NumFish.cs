using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NumFish : MonoBehaviour {
	public int originalFishPerLayer { private get; set; }
	Dictionary<FishSpawnPos.LAYER, int> numFishEliminated;

	public int GetNumDesired(FishSpawnPos.LAYER layer) {
		return Mathf.Max(0, originalFishPerLayer - numFishEliminated[layer]);
	}

	public void OnNewTarget() {
		ClearNumFishEliminated ();
	}

	public void ReduceDesiredNumFish(FishSpawnPos.LAYER layer, int num) {
		if (GetNumDesired (layer) > 0) {
			numFishEliminated[layer] += num;
		}
	}
		
	void Start () {
		numFishEliminated = new Dictionary<FishSpawnPos.LAYER, int>();
		ClearNumFishEliminated ();
	}

	void ClearNumFishEliminated() {
		foreach (var layer in FishSpawnPos.GetLayers()) {
			numFishEliminated [layer] = 0;
		}
	}

}
