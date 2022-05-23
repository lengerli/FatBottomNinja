using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnchorToMidLine : MonoBehaviour {

	public float totalStepsToMidLine;
	private float stepTowardsMidLine;
	private bool isBlockOnMove;
	public GameObject theNinja;
	private RopeBendController ropeBender;

	// Use this for initialization
	void Start () {

		ropeBender = theNinja.GetComponent<RopeBendController>();
		
	}

	void OnTriggerEnter2D(Collider2D other){

		if ( other.gameObject.name.Contains("BlockMoveTrigger") && !isBlockOnMove)
			StartCoroutine(MoveTowardsMidLine());
		
	}

	public IEnumerator MoveTowardsMidLine(){

		isBlockOnMove = true;

		yield return new WaitForSecondsRealtime (0.75f);
		stepTowardsMidLine = -1f * transform.position.x / totalStepsToMidLine;


		int trialIterator = 0;
		do{
			yield return null;

			transform.position = transform.position + ( Vector3.right * stepTowardsMidLine);
			MoveTouchingRopePoints(stepTowardsMidLine);
			trialIterator++;

		}while( Mathf.Abs( transform.position.x ) > 2*stepTowardsMidLine && trialIterator < 3000);

	}


	void MoveTouchingRopePoints(float movementStep){

		ropeBender.MovePivotsWithMovingObject ( GetComponent<Collider2D>(), new Vector2 ( movementStep, 0) );

	
	}

}
