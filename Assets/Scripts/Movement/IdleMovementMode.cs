using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IdleMovementMode : MovementMode {
	public float MAX_DIR_CHANGE = 20.0f; // degrees

	public float MIN_DISTANCE_TO_TARGET = 5.0f;
	public float MAX_DISTANCE_TO_TARGET = 20.0f;

	public float CHANCE_TO_FLIP = 0.25f;
	public float MAX_ASCENT_ANGLE = 30.0f; 

	//---------------------------------------------------------------------
	protected float speed;
	protected Vector3 target;
//	GameObject sphere;
	float AVOID_ROUNDING_ERRORS = 0.95f;
	//---------------------------------------------------------------------

	override protected bool OverrideMove () {
		mover.transform.position = Vector3.MoveTowards (mover.transform.position, target, speed * Time.deltaTime);
		if (Vector3.Distance (mover.transform.position, target) < 1.0f) {
			SetTarget(PickTarget());
		}
		return true;
	}
		
	override public void ResetMovement() {
		SetTarget(PickTarget());
	}

	override protected void OverrideOnEntry() {
		target = mover.transform.position;
	}

	//---------------------------------------------------------------------
	void Start () {
		Assert.IsTrue (0 <= MAX_ASCENT_ANGLE && MAX_ASCENT_ANGLE <= 90.0f);
		Assert.IsTrue (0 <= MAX_DIR_CHANGE && MAX_DIR_CHANGE <= 90.0f, "Flipping left/right direction is handled separately.");
		Assert.IsTrue (MIN_DISTANCE_TO_TARGET <= MAX_DISTANCE_TO_TARGET);
		Assert.IsTrue (0 <= CHANCE_TO_FLIP && CHANCE_TO_FLIP <= 1.0f);
		Assert.IsTrue (0 <= MIN_DISTANCE_TO_TARGET);
	}

	Vector3 PickTarget() {
		Assert.IsNotNull (mover);
		if (!CameraUtils.IsFirmlyOnScreen (mover.transform.position)) {
			return cameraUtils.GetOnCameraPos (mover.transform.position.z);
		}
		var dir = PickDirection ();
		Ray ray = new Ray (mover.transform.position, dir);
		if (Random.value < CHANCE_TO_FLIP) {
			ray.direction = -ray.direction;
		}
		UnityEngine.Assertions.Assert.AreApproximatelyEqual (ray.direction.z, 0);
		float maxDist = MAX_DISTANCE_TO_TARGET;
		if (!CanLeaveScreen ()) {
			float distanceToScreenEdge = cameraUtils.GetDistanceToFrustumEdge (ray);
			if (distanceToScreenEdge < MIN_DISTANCE_TO_TARGET) {
				ray.direction = -ray.direction;
				distanceToScreenEdge = cameraUtils.GetDistanceToFrustumEdge (ray);
			}
			maxDist = Mathf.Min (maxDist, distanceToScreenEdge * AVOID_ROUNDING_ERRORS);
		}
		float minDist = Mathf.Min (MIN_DISTANCE_TO_TARGET, maxDist);
		return ray.GetPoint( Random.Range( minDist, maxDist) );
	}

	void SetTarget(Vector3 p) {
		target = p;
		Assert.IsTrue (CanLeaveScreen () || CameraUtils.IsOnScreen (p), p.ToString() + " curPos = " + mover.transform.position + " isonscreen: " + CameraUtils.IsFirmlyOnScreen (mover.transform.position));
//		if (sphere == null ) {
//			sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
//			sphere.mover.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
//		}
//		sphere.mover.transform.position = target;
		LookAt (target);
		speed = GetRandomSpeed ();
	}

	Vector2 PickDirection () { // [Pure]
		float curAngle = mover.transform.localEulerAngles.z;
		float startAngle = RestrictAscentAngle(curAngle - MAX_DIR_CHANGE);
		float endAngle = RestrictAscentAngle(curAngle + MAX_DIR_CHANGE);
		float newAngle = TTMath.GetRandomAngleIn (startAngle, endAngle);
		return Quaternion.Euler (new Vector3 (0, 0, newAngle)) * Vector3.right;
	}


	float RestrictAscentAngle(float angle) {
		angle = TTMath.GetNormalizedAngle (angle);
		float rangeCenter = (90.0f <= angle && angle <= 270.0f) ? 180.0f : 0;
		return TTMath.ClampAngle (angle, TTMath.GetNormalizedAngle(rangeCenter - MAX_ASCENT_ANGLE), TTMath.GetNormalizedAngle(rangeCenter + MAX_ASCENT_ANGLE));
	}

}
