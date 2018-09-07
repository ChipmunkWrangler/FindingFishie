using UnityEngine;
using UnityEngine.UI;

public class ShowNumFish : MonoBehaviour {
	public Transform[] fishContainers;

	private Text text;

	void Start () {
		text = GetComponent<Text> ();
	}

	void Update () {
		int childCount = 0;
		foreach (var fc in fishContainers) {
			childCount += fc.childCount;
		}
		text.text = System.String.Format ("Num Fish: {0}", childCount);

	}
}
