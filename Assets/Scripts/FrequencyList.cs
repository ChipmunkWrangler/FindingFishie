using System.Linq;

public class FrequencyPair<T> {
	public int frequency;
	public T item;
}

public class FrequencyList<T, U> where T : FrequencyPair<U> {
	// a collection of Ts, each associated with a relative frequency.
	// e.g. (Red, 10), (Blue, 2) means that red should be chosen 10/(10+2) of the time
	public T[] frequencyPairs;

	private T[] filteredFrequencyPairs;
	private int totalFrequencies = 0;
	private bool isInitialized = false;

	public void AddFilter(System.Predicate<U> predicate) {
		filteredFrequencyPairs = filteredFrequencyPairs.Where(pair => predicate(pair.item)).ToArray();
	}

	public void ClearFilters() {
		filteredFrequencyPairs = frequencyPairs;
		isInitialized = false;
	}

	public FrequencyList () {
		ClearFilters();
	}

	public U ChooseItem() { 
		if (!isInitialized) {
			Init ();
		}

		int frequency = UnityEngine.Random.Range (0, totalFrequencies);
		foreach (var pair in filteredFrequencyPairs) {
			frequency -= pair.frequency;
			if (frequency < 0) {
				return pair.item;
			}
		}
		throw(new System.ApplicationException("FrequencyList.ChooseItem failed: " + frequency.ToString()));
	}

	private void Init() {
		totalFrequencies = 0;
		foreach (T pair in filteredFrequencyPairs) {
			totalFrequencies += pair.frequency;
		}
		isInitialized = true;
	}
}

// Clients use by implementing subclasses. 
// That is dumb, espectially the redundant type, but it's the best I can do. See below for alternative implementations.

//[System.Serializable]
//public class FishShapeFrequencyPair : FrequencyPair<GameObject> {
//}
//
//[System.Serializable]
//public class FishShapeFrequencyList : FrequencyList<FishShapeFrequencyPair, GameObject> {
//}




///////////////////
// This is what you want, but it doesn't work because Unity can't display it in the inspector
//[System.Serializable]
//public class FrequencyPair<T> {
//	public int frequency;
//	public T o;
//}
//
//public class FrequencyList<T> {
//	// a collection of Ts, each associated with a relative frequency.
//	// e.g. (Red, 10), (Blue, 2) means that red should be chosen 10/(10+2) of the time
//	public FrequencyPair<T>[] frequencies;
//
//	private int total = 0;
//
//	public FrequencyList() {
//		foreach (FrequencyPair<T> frequency in frequencies) {
//			total += frequency.frequency;
//		}
//	}
//}
//
//[System.Serializable]
//public class FishShapeFrequencyList : FrequencyList<GameObject> {
//}




///////////////////
// Works. Problems: Can't access GameObject from FrequencyList. Need do define two silly classes to use.
//public class FrequencyPair {
//	public int frequency;
//}
//
//public class FrequencyList<T> where T : FrequencyPair {
//	// a collection of Ts, each associated with a relative frequency.
//	// e.g. (Red, 10), (Blue, 2) means that red should be chosen 10/(10+2) of the time
//	public T[] frequencies;
//
//	private int total = 0;
//
//	public FrequencyList() {
//		foreach (T frequency in frequencies) {
//			total += frequency.frequency;
//		}
//	}
//}
//
//[System.Serializable]
//public class FishShapeFrequencyPair : FrequencyPair {
//	public GameObject shape;
//}
//
//[System.Serializable]
//public class FishShapeFrequencyList : FrequencyList<FishShapeFrequencyPair> {
//}