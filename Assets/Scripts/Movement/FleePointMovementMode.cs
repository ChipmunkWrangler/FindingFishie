using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleePointMovementMode : MovementMode {
	public float MIN_DISTANCE_TO_TARGET = 20.0f;
	public float MAX_DISTANCE_TO_TARGET = 20.0f;

	Vector3 target;
	float speed;

	override protected bool OverrideMove () {
		UnityEngine.Assertions.Assert.AreNotEqual (target, Vector3.zero, "Call SetFleePoint before activating FleePointMovementMode");	
		mover.transform.position = Vector3.MoveTowards (mover.transform.position, target, speed * Time.deltaTime);
		return (Vector3.Distance (mover.transform.position, target) >= 1.0f);
	}

	public void SetFleePoint(Vector3 fleePoint) {
		target = mover.transform.position;
		if (CameraUtils.IsFirmlyOnScreen(mover.transform.position)) {
			target = GetTargetAwayFrom (fleePoint, MIN_DISTANCE_TO_TARGET, MAX_DISTANCE_TO_TARGET);
			LookAt (target);
		}
		speed = GetRandomSpeed ();
	}

	override protected void OverrideOnEntry() {
		target = mover.transform.position;
	}



}
