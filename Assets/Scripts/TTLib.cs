using UnityEngine.Assertions;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class TTLib { 
	public static int GetRandomIdxWhere<T> (T[] originalList, System.Func<T, bool> predicate) {
		var idxs = originalList.Where (predicate);
		if (idxs.Count () == 0) {
			return -1;
		}
		int appearanceIdx = Random.Range (0, idxs.Count());
		while (!predicate(originalList [appearanceIdx])) {
			++appearanceIdx;
		}
		return appearanceIdx;
	}

	public static void Populate<T>(this T[] arr, T value ) {
		for ( int i = 0; i < arr.Length;i++ ) {
			arr[i] = value;
		}
	}
}

 public class minMaxPair<T>  {
	public T min;
	public T max;
	override public string ToString() {
		return "(" + min + ", " + max + ")";
	}

};

[System.Serializable] public class minMaxVector2Pair : minMaxPair<Vector2> {};
[System.Serializable] public class minMaxIntPair : minMaxPair<int> {};


