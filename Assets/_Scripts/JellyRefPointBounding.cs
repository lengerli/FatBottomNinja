using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides movement boundary for reference points of ninja. 
/// This script should be added at creation method in JellySprite.cs script.
/// </summary>

public class JellyRefPointBounding : MonoBehaviour

{
    public float defaultForce;

    public float xLocalPosMax;
    public float xLocalPosMin;
    public float yLocalPosMax;
    public float yLocalPosMin;

    public Vector2 defaultRefLocalPos;
    public float externalBoundDistance = 0.23f;
    public float internalBoundDistance = 0.07f;
    SliderJoint2D sliderJ;
    public Vector2 externalPoint;
    public Vector2 internalPoint;

    private float tempInternalBound;
    private float tempExternalBound;

    public GameObject centralPoint_Ninja;

    public bool isRefPointFixed;

    public bool printFixedPosVariables;


    public List<Vector2> refPointDefPosList = 
        new List<Vector2>(){
            new Vector2(0.9f, 0.0f),
            new Vector2(0.7f, 0.5f),
            new Vector2(0.3f, 0.8f),
            new Vector2(-0.3f, 0.8f),
            new Vector2(-0.7f, 0.5f),
            new Vector2(-0.9f, 0.0f),
            new Vector2(-0.7f, -0.5f),
            new Vector2(-0.3f, -0.73f),//Old value is (-0.3f,-0.8f)
            new Vector2(0.3f, -0.73f),//Old value is (-0.3f,-0.8f)
            new Vector2(0.7f, -0.5f)
        };

    private void Start()
    {
        GetComponent<Rigidbody2D>().mass = 0.0001f;

        centralPoint_Ninja = transform.parent.gameObject;
        StartCoroutine(DefPositionAndBoundStart());
    }

    IEnumerator DefPositionAndBoundStart()
    {
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < 10; i++)
        {
            string refObjName = "JellyDummy(Clone) Ref Point " + (i + 1).ToString();

            if (gameObject.name.Equals(refObjName))
            {
                defaultRefLocalPos = refPointDefPosList[i];
                transform.localPosition = defaultRefLocalPos;
                break;
            }
        }

        if (defaultRefLocalPos.y > -0.6f)
        {
            externalBoundDistance += 0.01f;
            internalBoundDistance += 0.008f;
        }
        else if (defaultRefLocalPos.y < -0.75f)
        {
            externalBoundDistance -= 0.035f;
            internalBoundDistance -= 0.015f;
        }

        SetLocalBounds();
        FixedPositionSetUp();
        MovingPositionSetUp(); //This method can be commented out so that sliders are not added, ref points might (or might not) move horizontally(freely) thus giving a jiggly movement to ninja while stationary

        tempExternalBound = externalBoundDistance;
        tempInternalBound = internalBoundDistance;
        StartCoroutine(KeepBodyInLocalBoundsRoutine());
    }

    private void FixedUpdate()
    {
        #if UNITY_EDITOR
        if ( Mathf.Abs (tempExternalBound - externalBoundDistance) < 0.01f || Mathf.Abs( tempInternalBound - internalBoundDistance) < 0.01f )
            MovingPositionSetUp();
        #endif

        if (isRefPointFixed)
            transform.localPosition = defaultRefLocalPos;
 //       else
 //           KeepBodyInLocalBounds();
    }


    private void FixedPositionSetUp()
    {
        if ( defaultRefLocalPos.y > -0.01f)
        {
            isRefPointFixed = true;

            //Remove all spring joints
            foreach (SpringJoint2D springJ in GetComponents<SpringJoint2D>())
                Destroy(springJ);

            //Remove rigidbodies
            Destroy (GetComponent<Rigidbody2D>());
        }

        StartCoroutine(RemoveIdleSpringJoints());
    }

    IEnumerator RemoveIdleSpringJoints()
    {
        yield return new WaitForFixedUpdate();
        foreach (SpringJoint2D springJ in GetComponents<SpringJoint2D>())
            if (springJ.connectedBody == null)
                springJ.connectedBody = GetComponent<Rigidbody2D>();
    }

    public void KeepBodyInLocalBounds()
    {

        
        if (transform.localPosition.x < xLocalPosMin)
            transform.localPosition = new Vector3(xLocalPosMin + 0.05f, transform.localPosition.y, transform.localPosition.z);
        else if (transform.localPosition.x > xLocalPosMax)
            transform.localPosition = new Vector3(xLocalPosMax - 0.05f, transform.localPosition.y, transform.localPosition.z);

        if (transform.localPosition.y < yLocalPosMin)
            transform.localPosition = new Vector3(transform.localPosition.x, yLocalPosMin + 0.05f, transform.localPosition.z);
        else if (transform.localPosition.y > yLocalPosMax)
            transform.localPosition = new Vector3(transform.localPosition.x, yLocalPosMax - 0.05f, transform.localPosition.z);

        
    }

    IEnumerator KeepBodyInLocalBoundsRoutine()
    {
        yield return new WaitForFixedUpdate();

        KeepBodyInLocalBounds();
        /*         This coroutine has to be called after internal physics of unity is complete
         *         So that all the reference points find their place according to natural collisions first.
         *         After that the boundaries will be checked, if the ref point is beyond a boundary
         *         it will be moved back to appropriate position.
         *          
         *         But this repositioning should be done before the draw method in jellySprite.cs is called;
         *         Which occurs in the Update() method.
         *         
         *         So this boundary check should be after Internal Physics but before Update;
         *         And only "yield return WaitForFixedUpdate() can do that.
         *         Thus, I first thought of recursive calling this coroutine
         *         Which resulted in GC allocation; and it is unnecessary and bad code!!
         *         
         *         Instead of that, we know call the KeepBodyInLocalBounds() method from JellySprite.cs Update() method;
         *         right before the UpdateMesh() is called.
         *         
         *         This way we got rid of GC alloc. It is still not perfect, but still better code.
         */
         
      //StartCoroutine(KeepBodyInLocalBoundsRoutine()); This line is obsolete due to reasons given above in detail
    }


    void SetLocalBounds()
    {
        externalPoint = defaultRefLocalPos.normalized * (defaultRefLocalPos.magnitude + externalBoundDistance);
        internalPoint = defaultRefLocalPos.normalized * (defaultRefLocalPos.magnitude - internalBoundDistance);

        if (externalPoint.x > internalPoint.x)
        {
            xLocalPosMax = externalPoint.x;
            xLocalPosMin = internalPoint.x;
        }
        else
        {
            xLocalPosMin = externalPoint.x;
            xLocalPosMax = internalPoint.x;
        }

        if (externalPoint.y > internalPoint.y)
        {
            yLocalPosMax = externalPoint.y;
            yLocalPosMin = internalPoint.y;
        }
        else
        {
            yLocalPosMin = externalPoint.y;
            yLocalPosMax = internalPoint.y;
        }
    }

    bool isSliderJointAdded;

    //  As extra measures for limited movement of ref points, this method adds slider to
    // central ninja connected to this ref point, if this ref point is not fixed.
    //  Interestingly, without the sliders, the refpoints tend to move horizontally spontaneously while stationary 
    // which gives the ninja jiggly movement while on rest

    void MovingPositionSetUp()
    {
        if ( isRefPointFixed == false && !isSliderJointAdded)
        {
            sliderJ = centralPoint_Ninja.AddComponent<SliderJoint2D>();
            sliderJ.connectedBody = gameObject.GetComponent<Rigidbody2D>();
            JointTranslationLimits2D jointTranslationLimits2D = new JointTranslationLimits2D
            {
                max =  externalBoundDistance,
                min = internalBoundDistance
            };

            sliderJ.limits = jointTranslationLimits2D;

            tempExternalBound = externalBoundDistance;
            tempInternalBound = internalBoundDistance;

            isSliderJointAdded = true;
        }
        else if (isRefPointFixed == false && isSliderJointAdded)//Changes slider joint max & min, if external or internal bounds is changed in Unity Editor Manually during game play
        {
            JointTranslationLimits2D jointTranslationLimits2D = new JointTranslationLimits2D
            {
                max = externalBoundDistance,
                min = internalBoundDistance
            };

            sliderJ.limits = jointTranslationLimits2D;

            tempExternalBound = externalBoundDistance;
            tempInternalBound = internalBoundDistance;
        }
    }

}
