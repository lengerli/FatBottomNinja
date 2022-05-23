using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public float upperBoundForCameraPos;
	public float lowerBoundForCameraPos;
	public float leftBoundForCameraPos;
	public float rightBoundfForCameraPos;
	public float cameraStartY;
	public float cameraNinjaDistance;
	public float letRightBoundaryGoFreeAfterThisYPosition;

    public float cameraDistanceForLerpLimit;

	public GameObject TheNinja;

    public bool isNinjaActive = false;

	void FixedUpdate () {

		if ( transform.position.y > cameraStartY && cameraNinjaDistance <2.5f)
			cameraNinjaDistance = (transform.position.y - cameraStartY) /5f;
		else if ( transform.position.y > cameraStartY && cameraNinjaDistance >2.5f)
			cameraNinjaDistance = 2.5f;


        if (TheNinja != null && TheNinjaJustGotActive())
            LerpTowardsNinja(new Vector3(TheNinja.transform.position.x, TheNinja.transform.position.y + cameraNinjaDistance, transform.position.z));
        else if (TheNinja != null && TheNinja.activeSelf )
			transform.position = new Vector3(  TheNinja.transform.position.x, TheNinja.transform.position.y + cameraNinjaDistance, transform.position.z);


        //SET BOUNDARIES FOR CAMERA POSITION TO GIVE THE SENSE OF MAP BOUNDARIES

        //For Horizontal Boundaries
        if (transform.position.x > rightBoundfForCameraPos )
			transform.position = new Vector3 ( rightBoundfForCameraPos, transform.position.y, transform.position.z);
		else if (transform.position.x < leftBoundForCameraPos )
			transform.position = new Vector3 ( leftBoundForCameraPos, transform.position.y, transform.position.z);

		//For Vertical Boundaries
		if (transform.position.y > upperBoundForCameraPos )
			transform.position = new Vector3 ( transform.position.x, upperBoundForCameraPos, transform.position.z);
		else if (transform.position.y < lowerBoundForCameraPos )
			transform.position = new Vector3 ( transform.position.x, lowerBoundForCameraPos, transform.position.z);

	}

    float CameraNinjaDistance()
    {
        float theDistance = 0;

        Vector3 distanceVector = transform.position - new Vector3(TheNinja.transform.position.x, TheNinja.transform.position.y + cameraNinjaDistance, transform.position.z);

        theDistance = distanceVector.magnitude;

        return Mathf.Abs(theDistance);
    }

    void LerpTowardsNinja(Vector3 positionToLerpTo)
    {
        transform.position = Vector3.Lerp(transform.position, positionToLerpTo, 0.04f);
    }

    bool TheNinjaJustGotActive()
    {
        if (!isNinjaActive && TheNinja.activeSelf)
        {
            isNinjaActive = true;
            return true;// true since ninja just got active or we just learned about it
        }
        else if (isNinjaActive && !TheNinja.activeSelf)
        {
            isNinjaActive = false;
            return false;
        }
        else
            return false;
    }
}
