using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSelector : MonoBehaviour {
	public Sprite[] TEXTURES; // WE ASSUME THAT THE PATTERNS ARE ALWAYS IN THE SAME ORDER!!! HACK

	public void SetPatternIdx (int patternIdx) {
		gameObject.GetComponent<SpriteRenderer> ().sprite = TEXTURES [patternIdx];
	}
}
