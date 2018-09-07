using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

abstract public class MovementMode : MonoBehaviour {
	[SerializeField] protected float MIN_SPEED = 1f;
	[SerializeField] protected float MAX_SPEED = 5f;
	[SerializeField] protected GameObject mover;
	SpriteRenderer spriteRenderer;
	public GameObject partnerFish { protected get; set; }

	protected CameraUtils cameraUtils;

	bool canLeaveScreen;

	public bool Move () {
		spriteRenderer.flipY = IsFacingLeft();
		return OverrideMove();
	} 

	public void Enter(bool _canLeaveScreen, CameraUtils cu) {
		Init ();
		canLeaveScreen = _canLeaveScreen;
		cameraUtils = cu;
		UnityEngine.Assertions.Assert.IsNotNull (cameraUtils);
		OverrideOnEntry ();
	}

	public void Exit() {
		OverrideOnExit ();
	}

	public virtual void ResetMovement() {}

	void Init() {
		if(!spriteRenderer) {
			if (!mover) {
				mover = gameObject;
			}
			spriteRenderer = mover.GetComponent<SpriteRenderer> ();
			Assert.IsTrue (0 <= MIN_SPEED);
			Assert.IsTrue (MIN_SPEED <= MAX_SPEED);
			Assert.IsNotNull (mover);
		}
	}
	protected virtual void Awake() {
		Init ();
	}

	protected bool CanLeaveScreen() {
		return canLeaveScreen;
	}

	virtual protected void OverrideOnEntry() {}

	virtual protected void OverrideOnExit() {}

	virtual protected bool OverrideMove () { return true; } // return false when the mode has completed, if applicable 

	protected float GetRandomSpeed() { // [Pure]
		return Random.Range (MIN_SPEED, MAX_SPEED);
	}

	protected void LookAt(Vector3 p) {
		mover.transform.right = p - mover.transform.position;
	}

	protected bool IsFacingLeft() {
		return mover.transform.up.y < 0;
	}

	protected Vector3 PickFleeDirection(Vector3 fleePoint) {
		Vector3 dir = (mover.transform.position - fleePoint);
		dir.z = 0;
		return dir;
	}

	protected Vector3 GetTargetAwayFrom(Vector3 fleePoint, float minDistToTarget, float maxDistToTarget, bool allowOffscreen = false) { // [pure]
		Ray ray = new Ray (mover.transform.position, PickFleeDirection(fleePoint));
		UnityEngine.Assertions.Assert.AreApproximatelyEqual (ray.direction.z, 0);
		float distanceToScreenEdge = allowOffscreen ? maxDistToTarget : cameraUtils.GetDistanceToFrustumEdge (ray);
		float maxDist = Mathf.Min (maxDistToTarget, distanceToScreenEdge);
		float minDist = Mathf.Min (minDistToTarget, maxDist);
		return ray.GetPoint( Random.Range( minDist, maxDist) );
	}
}
