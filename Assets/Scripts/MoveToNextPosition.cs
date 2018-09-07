using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToNextPosition : MonoBehaviour {
	public Vector3 POSITION_SHIFT;
	public float SPEED;

	private Vector3 desiredPosition;
	private float CLOSE_ENOUGH = 0.01f;

	public void OnTargetClicked() {
		desiredPosition = transform.position + POSITION_SHIFT;
	}

	public bool IsMoving() {
		return Vector3.Distance(transform.position, desiredPosition) > CLOSE_ENOUGH;
	}

	void Start() {
		desiredPosition = transform.position;
	}

	void Update() {
		transform.position = Vector3.MoveTowards(transform.position, desiredPosition, SPEED * Time.deltaTime);
	}
}
