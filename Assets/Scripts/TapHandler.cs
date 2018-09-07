using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TouchParam {
	public int touchId;
	public GameObject touchedObj;
	public Vector2 touchedPosition;
}

public class TapHandler : MonoBehaviour {
	Dictionary<int, GameObject> touchedObjects;

	void Awake() {
		touchedObjects = new Dictionary<int, GameObject>();
	}

	void Update () {
		if (Input.touchSupported) {
			foreach (Touch touch in Input.touches) {
				switch (touch.phase) {
				case TouchPhase.Began:
					BeginTouch (touch);
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					ContinueTouch (touch);
					break;
				case TouchPhase.Canceled:
				case TouchPhase.Ended:
					EndTouch (touch);
					break;
				}
			}
		} else {
			if (Input.GetButtonDown("Fire1"))
			{
				HandleButtonEvent ("OnTTBeginTouch", Input.mousePosition, 0);
			} else if (Input.GetButton("Fire1"))  {
				HandleButtonEvent("OnTTTouchEnter", Input.mousePosition, 0);
			}
			if (Input.GetButtonUp("Fire1"))
			{
				HandleButtonEvent ("OnTTEndTouch", Input.mousePosition, 0);
			}
		}
	}

	void BeginTouch(Touch touch) {
		Assert.IsNull (touchedObjects [touch.fingerId]);
		HandleButtonEvent ("OnTTBeginTouch",  touch.position, touch.fingerId);

	}

	void ContinueTouch(Touch touch) {
		GameObject touchedObj = CameraUtils.GetTouchedObject (touch.position);
		if (touchedObj != touchedObjects [touch.fingerId]) {
			HandleButtonEvent ("OnTTTouchEnter", touch.position, touch.fingerId);
		}
	}

	void EndTouch(Touch touch) {
		HandleButtonEvent ("OnTTEndTouch", touch.position, touch.fingerId);
	}

	GameObject HandleButtonEvent(string msg, Vector2 touchedPosition, int touchId) {
//		print ("Handle " + msg + " " + touchedObj.GetInstanceID () + " touch#" + touchId + " time " + Time.time);
		GameObject touchedObj = CameraUtils.GetTouchedObject (touchedPosition);
		touchedObjects [touchId] = touchedObj;
		if (touchedObj) {
			touchedObj.SendMessage (msg, new TouchParam{ touchId = touchId, touchedObj = touchedObj, touchedPosition = touchedPosition }, SendMessageOptions.DontRequireReceiver); 
		}
		return touchedObj;
	}

}
