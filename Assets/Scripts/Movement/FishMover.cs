using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishMover : MonoBehaviour {
	public bool canLeaveScreen;
	//---------------------------------------------------------------------
	public MovementMode special { private get; set; }
	[SerializeField] MovementMode idle;
	[SerializeField] FleePointMovementMode reactToScreenTouch;
	GameObject fishTracker;
	bool paused = false;
	MovementMode activeMovementMode;
	CameraUtils cameraUtils;
//	bool tempAllowLeaveScreen;
	//---------------------------------------------------------------------
	public void OnAddedToSet() {
		SetPaused(true);
	}

	public void OnRemovedFromSet() {
		SetPaused(false);
	}

	public void SetPaused(bool b) {
		paused = b;
	}

	public void OnBackgroundTouched(TouchParam touchParam) {
		if (!paused) {
			FleeFromTouch ();
		}
	}

	//---------------------------------------------------------------------
	public void OnBecameInvisible() {
		if (fishTracker != null && gameObject.activeInHierarchy) 
			fishTracker.SendMessage ("OnFishLeftScreen", gameObject);
	}

	public void SetFishTracker(GameObject _fishTracker) {
		fishTracker = _fishTracker;
	}

	public void ResetMovement() {
		TransitionTo( GetStandardMovement() );
		activeMovementMode.ResetMovement ();
	}

	public void SetCameraUtils(CameraUtils cu) {
		cameraUtils = cu;
	}


	//---------------------------------------------------------------------
	void Start() {
		UnityEngine.Assertions.Assert.IsNotNull (activeMovementMode); // ResetMovement has already been called
	}

	void Update () {
		if (!paused) {
			if (!activeMovementMode.Move ()) {
				TransitionTo (GetStandardMovement());
			}
		}
	}

	void FleeFromTouch() {
		if (reactToScreenTouch != null) {
			TransitionTo (reactToScreenTouch);
			reactToScreenTouch.SetFleePoint (GetFleePoint ());				
		}
	}

	Vector3 GetFleePoint() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
		float distToCamera = transform.position.z  - Camera.main.transform.position.z;
		UnityEngine.Assertions.Assert.IsTrue (distToCamera > 0);
		return ray.GetPoint (distToCamera);
	}

	void TransitionTo(MovementMode newMode) {
		UnityEngine.Assertions.Assert.IsFalse (paused);
		if (activeMovementMode) {
			activeMovementMode.Exit ();
		}
		newMode.Enter (canLeaveScreen, cameraUtils);
		activeMovementMode = newMode;
	}

	MovementMode GetStandardMovement() {
		return special ? special : idle;
	}

}
