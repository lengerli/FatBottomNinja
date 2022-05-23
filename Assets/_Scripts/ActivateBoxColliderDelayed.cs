using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateBoxColliderDelayed : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine(DelayedActivateBoxCollideR());
	}
	
	IEnumerator DelayedActivateBoxCollideR(){
		yield return new WaitForSecondsRealtime (1f);
	
		GetComponent<BoxCollider2D>().enabled = true;
	
	}
}
