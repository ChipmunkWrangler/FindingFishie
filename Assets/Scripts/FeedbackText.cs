using UnityEngine;
using UnityEngine.UI;

public class FeedbackText : MonoBehaviour {
	public Color finalColor;
	public Vector3 finalScale;
	public float transitionTime;

	Text text;
	Color initialColor;
	Vector3 initialScale;

	public void ShowMessage (string msg) {
		AbortTransitions ();
		text.text = msg;
		iTween.ValueTo(gameObject, iTween.Hash(
			"name", "showmessagecolor",
			"from", initialColor,
			"to", finalColor,
			"time", transitionTime,
			"onupdate", "UpdateColor",
			"easeType", iTween.EaseType.easeOutBounce,
			"oncomplete", "HideMessage"));
		iTween.ScaleTo (gameObject, iTween.Hash(
			"name", "showmessagescale",
			"scale", finalScale, 
			"time", transitionTime));
	}

	void Start () {
		text = GetComponent<Text> ();
		iTween.Init (gameObject);
		initialColor = text.color;
		initialScale = gameObject.transform.localScale;
	}

	void AbortTransitions() {
		iTween.StopByName ("showmessagecolor");
		iTween.StopByName ("showmessagescale");
		iTween.StopByName ("hidemessage");
		gameObject.transform.localScale = initialScale;
	}

	void UpdateColor(Color newColor) {
		text.color = newColor;
	}

	void HideMessage() {
		iTween.ValueTo(gameObject, iTween.Hash(
			"name", "hidemessage",
			"from", text.color,
			"to", initialColor,
			"time", transitionTime,
			"onupdate", "UpdateColor"));
	}
}
