using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class ScaredMovementMode : IdleMovementMode {
	[SerializeField] float THREAT_RANGE = 10f;
	[SerializeField] float MIN_FLEE_SPEED = 5f;
	[SerializeField] float MAX_FLEE_SPEED = 10f;
	[SerializeField] Vector3 SIGHT_HALF_EXTENTS;

	float THREAT_RANGE_SQUARED;
	bool isTargetValid;
	GameObject debugBox;
	//--------------------------------------------------------------------
	protected override bool OverrideMove() {
		if (isTargetValid) {
			MoveToTarget ();
		} else 
		if (SeesThreat ()) {
			SetTargetAwayFromThreat ();
//			MoveAwayFromThreat();
		} else {
			base.OverrideMove ();
		}
		return true;
	}
	override public void ResetMovement() {
		if (isTargetValid) {
			ReachedTarget ();
		}
		base.ResetMovement ();
	}
	//--------------------------------------------------------------------
	protected override void Awake() {
		THREAT_RANGE_SQUARED = THREAT_RANGE * THREAT_RANGE;
		base.Awake();
	}	

	bool SeesThreat() {
		Vector3 origin = mover.transform.position + mover.transform.right;
		origin.z = partnerFish.transform.position.z;
		Vector3 direction = new Vector3 (mover.transform.right.x, mover.transform.right.y, 0);
		RaycastHit hit;
		ExtDebug.DrawBoxCastBox (origin, SIGHT_HALF_EXTENTS, Quaternion.identity, direction, THREAT_RANGE, Color.red);
//		Vector3 lowerLeft = new Vector3 (origin.x - SIGHT_HALF_EXTENTS.x, origin.y - SIGHT_HALF_EXTENTS.y, origin.z);
//		Vector3 upperRight = new Vector3 (origin.x + SIGHT_HALF_EXTENTS.x, origin.y + SIGHT_HALF_EXTENTS.y, origin.z);
//		Debug.DrawRay(lowerLeft,  direction * THREAT_RANGE);
//		Debug.DrawRay(upperRight, direction * THREAT_RANGE);
//		Debug.DrawRay(origin, direction * THREAT_RANGE);
//		if (debugBox == null ) {
//			debugBox = GameObject.CreatePrimitive (PrimitiveType.Cube);
//			debugBox.transform.localScale = new Vector3(SIGHT_HALF_EXTENTS.x * 2f, SIGHT_HALF_EXTENTS.y * 2f, THREAT_RANGE);
//		}
//		debugBox.transform.position = new Vector3( origin.x, origin.y, origin.z + THREAT_RANGE * 0.5f);
//		debugBox.transform.rotation = gameObject.transform.rotation;
//		Debug.DrawRay (origin, direction);
			
		return (Physics.BoxCast(origin, SIGHT_HALF_EXTENTS, direction, out hit, Quaternion.identity, THREAT_RANGE) && hit.collider == partnerFish.GetComponent<Collider> ());


		//		Ray ray = new Ray(origin, direction);
//		return (Physics.Raycast (ray, out hit) && hit.collider == partnerFish.GetComponent<Collider> ());// && hit.distance <= THREAT_RANGE);
//		return GetSquaredDistanceToThreat() <= THREAT_RANGE_SQUARED && CameraUtils.IsFirmlyOnScreen(mover.transform.position); // TODO Check facing			
	}


	Vector2 GetFlightVector() {
		return mover.transform.position - partnerFish.transform.position; 
	}

	float GetSquaredDistanceToThreat() {
		return GetFlightVector().sqrMagnitude;
	}

	void SetTargetAwayFromThreat() {
		partnerFish.GetComponent<SpriteRenderer> ().color = Color.red;
		mover.GetComponent<SpriteRenderer> ().color = Color.white;
		target = GetTargetAwayFrom (partnerFish.transform.position, MIN_DISTANCE_TO_TARGET, MAX_DISTANCE_TO_TARGET, true);
		LookAt (target);
		isTargetValid = true;
	}

	float GetSpeed() {
		float fractionOfMaxSpeed = Mathf.Max(0, 1 - GetSquaredDistanceToThreat () / THREAT_RANGE_SQUARED);
		UnityEngine.Assertions.Assert.IsTrue (fractionOfMaxSpeed < 1f);
		return MIN_FLEE_SPEED + fractionOfMaxSpeed * (MAX_FLEE_SPEED - MIN_FLEE_SPEED);
	}
		
//	void MoveAwayFromThreat() {
//		Vector3 flightDir = GetFlightVector ().normalized;
//		mover.transform.position += flightDir * GetSpeed() * Time.deltaTime;
//	LookAt (mover.transform.position + flightDir);
//	}

	void MoveToTarget() {
		mover.transform.position = Vector3.MoveTowards (mover.transform.position, target, GetSpeed () * Time.deltaTime);
		if (Vector3.Distance (mover.transform.position, target) <= 1.0f) {
			ReachedTarget ();
		}
	}

	void ReachedTarget() {
		isTargetValid = false;
		partnerFish.GetComponent<SpriteRenderer> ().color = Color.black;
		mover.GetComponent<SpriteRenderer> ().color = Color.grey;
	}

}


			public static class ExtDebug
			{
				//Draws just the box at where it is currently hitting.
				public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
				{
					origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
					DrawBox(origin, halfExtents, orientation, color);
				}

				//Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
				public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
				{
					direction.Normalize();
					Box bottomBox = new Box(origin, halfExtents, orientation);
					Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);

					Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft,    color);
					Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
					Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
					Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight,    color);
					Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft,    color);
					Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
					Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
					Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight,    color);

					DrawBox(bottomBox, color);
					DrawBox(topBox, color);
				}

				public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
				{
					DrawBox(new Box(origin, halfExtents, orientation), color);
				}
				public static void DrawBox(Box box, Color color)
				{
					Debug.DrawLine(box.frontTopLeft,     box.frontTopRight,    color);
					Debug.DrawLine(box.frontTopRight,     box.frontBottomRight, color);
					Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
					Debug.DrawLine(box.frontBottomLeft,     box.frontTopLeft, color);

					Debug.DrawLine(box.backTopLeft,         box.backTopRight, color);
					Debug.DrawLine(box.backTopRight,     box.backBottomRight, color);
					Debug.DrawLine(box.backBottomRight,     box.backBottomLeft, color);
					Debug.DrawLine(box.backBottomLeft,     box.backTopLeft, color);

					Debug.DrawLine(box.frontTopLeft,     box.backTopLeft, color);
					Debug.DrawLine(box.frontTopRight,     box.backTopRight, color);
					Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
					Debug.DrawLine(box.frontBottomLeft,     box.backBottomLeft, color);
				}

				public struct Box
				{
					public Vector3 localFrontTopLeft     {get; private set;}
					public Vector3 localFrontTopRight    {get; private set;}
					public Vector3 localFrontBottomLeft  {get; private set;}
					public Vector3 localFrontBottomRight {get; private set;}
					public Vector3 localBackTopLeft      {get {return -localFrontBottomRight;}}
					public Vector3 localBackTopRight     {get {return -localFrontBottomLeft;}}
					public Vector3 localBackBottomLeft   {get {return -localFrontTopRight;}}
					public Vector3 localBackBottomRight  {get {return -localFrontTopLeft;}}

					public Vector3 frontTopLeft     {get {return localFrontTopLeft + origin;}}
					public Vector3 frontTopRight    {get {return localFrontTopRight + origin;}}
					public Vector3 frontBottomLeft  {get {return localFrontBottomLeft + origin;}}
					public Vector3 frontBottomRight {get {return localFrontBottomRight + origin;}}
					public Vector3 backTopLeft      {get {return localBackTopLeft + origin;}}
					public Vector3 backTopRight     {get {return localBackTopRight + origin;}}
					public Vector3 backBottomLeft   {get {return localBackBottomLeft + origin;}}
					public Vector3 backBottomRight  {get {return localBackBottomRight + origin;}}

					public Vector3 origin {get; private set;}

					public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
					{
						Rotate(orientation);
					}
					public Box(Vector3 origin, Vector3 halfExtents)
					{
						this.localFrontTopLeft     = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
						this.localFrontTopRight    = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
						this.localFrontBottomLeft  = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
						this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

						this.origin = origin;
					}


					public void Rotate(Quaternion orientation)
					{
						localFrontTopLeft     = RotatePointAroundPivot(localFrontTopLeft    , Vector3.zero, orientation);
						localFrontTopRight    = RotatePointAroundPivot(localFrontTopRight   , Vector3.zero, orientation);
						localFrontBottomLeft  = RotatePointAroundPivot(localFrontBottomLeft , Vector3.zero, orientation);
						localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
					}
				}

				//This should work for all cast types
				static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
				{
					return origin + (direction.normalized * hitInfoDistance);
				}

				static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
				{
					Vector3 direction = point - pivot;
					return pivot + rotation * direction;
				}
			}