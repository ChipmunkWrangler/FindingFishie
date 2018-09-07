using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class LoopMovementMode : MovementMode {
	[SerializeField] float MIN_RADIUS;
	[SerializeField] float MAX_RADIUS;
	[SerializeField] float TIMES_TO_LOOP = 1.0f;


	Vector3 center;
	float radius;
	float degreesPerSec;
	float speed;
	float degreesLooped;
	//--------------------------------------------------------------------
	public override void ResetMovement() {
		center = cameraUtils.GetOnCameraPos (mover.transform.position.z);
		radius = Random.Range (MIN_RADIUS, MAX_RADIUS);
		speed = GetRandomSpeed ();
		degreesPerSec = speed * 360.0f / (Mathf.PI * 2.0f * radius);
		degreesLooped = -1;
		LookAt (center);
	}

	protected override bool OverrideMove() {
		if (ReachedLoop()) {
			float degreesThisFrame = degreesPerSec * Time.deltaTime;
			mover.transform.Rotate (0, 0, degreesThisFrame);
			mover.transform.position += mover.transform.right * speed * Time.deltaTime;
			degreesLooped += degreesThisFrame;
			if (degreesLooped > GetDegreesToLoop ()) {
				ResetMovement ();
			}
		} else if (Vector3.Distance (center, mover.transform.position) > radius) {
			mover.transform.position = Vector3.MoveTowards (mover.transform.position, center, speed * Time.deltaTime);
		} else {
			degreesLooped = 0;
		}
		return true;
	}

	float GetDegreesToLoop() {
		return 360.0f * TIMES_TO_LOOP;
	}

	bool ReachedLoop() { 
		return degreesLooped >= 0;
	}
}
