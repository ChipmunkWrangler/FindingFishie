using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetUI : MonoBehaviour {
	[SerializeField] float delayShow = 3f;
	[SerializeField] Vector2 left;
	[SerializeField] Vector2 right;
	[SerializeField] Vector3 leftMargin;
	[SerializeField] Vector3 rightMargin;
	[SerializeField] bool isLeft;
	public GameObject parent;

	private Vector2 originalSize;

	public void Init(GameObject fish) {
		SpriteRenderer renderer = fish.GetComponent<SpriteRenderer> ();
		var rectTransform = GetComponent<RectTransform> ();
		var imageComponent = GetComponent<Image> ();
		imageComponent.sprite = renderer.sprite;
		imageComponent.color = renderer.color;

		var newSize = renderer.sprite.bounds.size;
		var originalSize = GetOriginalSize ();
		float scale = Mathf.Min (originalSize.x / newSize.x, originalSize.y / newSize.y);
		rectTransform.sizeDelta = new Vector2 (scale * newSize.x, scale * newSize.y);
	}

	public void SetEnabled(bool b) {
		SetVisibleImmediate (false);
		parent.SetActive (b);
		if (b) {
			StartCoroutine (SetVisible (true, delayShow));
		}
	}
		
	public bool IsEnabled() {
		return parent.activeInHierarchy;
	}

	public void SetLeft(bool b) {
		isLeft = b;
		if (isLeft) {
			MoveTo (left, leftMargin);
		} else {
			MoveTo (right, rightMargin);
		}
	}

	IEnumerator SetVisible(bool b, float delay) {
		yield return new WaitForSeconds (delay);
		SetVisibleImmediate (b);
	}

	void SetVisibleImmediate(bool b) {
		parent.GetComponent<Image> ().enabled = b;
		GetComponent<Image> ().enabled = b;
	}

	Vector2 GetOriginalSize() {
		if (originalSize == Vector2.zero) {
			originalSize = GetComponent<RectTransform> ().sizeDelta;
		}
		return originalSize;
	}

	void MoveTo(Vector2 pivot, Vector2 margin) {
		var shiftingTransform = parent.GetComponent<RectTransform> ();
		shiftingTransform.pivot = pivot;
		shiftingTransform.anchorMin = pivot;
		shiftingTransform.anchorMax = pivot;
		shiftingTransform.anchoredPosition = margin;
	}
}
