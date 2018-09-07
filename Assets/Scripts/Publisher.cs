using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Publisher : MonoBehaviour {
	public List<GameObject> subscribers = new List<GameObject>();
	public string buttonDownMsg;
	public string buttonUpMsg;
	public string mouseEnterMessage;

	public void Subscribe(GameObject o) {
		subscribers.Add (o);
	}

	void OnTTBeginTouch (object param) {
		InformSubscribers (buttonDownMsg, param);
	}

	void OnTTEndTouch(object param) {
		InformSubscribers (buttonUpMsg, param);
	}

	void OnTTTouchEnter(object param) {
		InformSubscribers(mouseEnterMessage, param);
	}

	void InformSubscribers(string msgName, object param) {
		if (msgName != "") {
			for (int i = subscribers.Count - 1; i >= 0; --i) {
				var o = subscribers[i];
				if (o) {
//					print ("Send msg " + msgName);
					o.SendMessage (msgName, param, SendMessageOptions.DontRequireReceiver);
				} else {
					subscribers.RemoveAt (i);
				}
			}
		}
	}
}
