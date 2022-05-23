using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPostureController : MonoBehaviour {

	public RopeShootControls ropeTouchController;
	private bool isRopePostureSet;

	void FixedUpdate () {

		if(ropeTouchController.isRopeShot && !isRopePostureSet)
			SetPosture();
		else if ( !ropeTouchController.isRopeShot)
			isRopePostureSet = false;
	}


	void SetPosture(){
		isRopePostureSet = true;

		if( transform.position.x > ropeTouchController.ropeEndPoint.x)
			transform.localScale = new Vector2 ( Mathf.Abs(transform.localScale.x) * -1f, transform.localScale.y);
		else
			transform.localScale = new Vector2 ( Mathf.Abs(transform.localScale.x) , transform.localScale.y);

	}
}
