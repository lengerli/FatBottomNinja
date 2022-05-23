using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;


/// <summary>
/// THIS VERSION OF RopeBendController.cs IS DEBUGGED AND REFACTORED FOR BETTER PERFORMANCE ON 25.12.2019, 04.02.2020 and finally 02.05.2022 by Mert
/// DEBUG NOTES:
/// DATE: 25.12.2019
/// 1-) Most important bug was checking swing direction while adding new pivot, which in turn caused unwrapping of rope problems.
/// Checking swing direction while adding a new pivot was made by rigidbody velocity, which changed when the ninja bounced from an obstacle.
/// So when the bounce and pivot addition coincided on the same frame, the swing direction was calculated wrong.
/// *To solve this issue, swing calculation is now made by position of ninja instead of rigidbody.velocity.
/// **But using position did not solve the problem since bounce also changed position momentrarily at that specific frame too.
/// ***In order to better calculate the swing, pivot position was augmented by making the pivot on the  "convex" side obstacle by using
/// the position of center of mass of the obstacle and adding the distance between center of mass to the pivot point. This way, even if the 
/// position of ninja moves a little at the bounce, it cannot move enough to jeopardize swing calculation. The calculations can be found at
/// CheckSwingDirection....() method
/// 
/// 2-) Bir objeye anchor etmişken, rope başka (küçük) bir objeye dolanıyorsa, ve o objecinin bir tarafından diğer tarafına geçiyorsa, diğer taraftan geri dönerken
/// bir önceki pivot ile ( anchor ettiği objede bulunan eski pivot) line of sight oluşabiliyor. Bu durum özellikle ikinci obje çok küçükse oluyor.
/// Eğer swing de doğru ise, line of sight olduğu için unwrap etmemesi gerekirken unwrap ediyordu. Bu BUG'ı engellemek için, "sondan ikinci pivot" (eski-anchor edilen
/// objedeki pivot)  "yeni pivot" (küçük objedeki pivot)  ve ninja pozisyonu vectorleri alınır, bu vektörler arasındaki açı hesaplanır (0-180 derece arası ve her
/// zaman pozitif derece verecek şekilde).   Eğer bu derece 2 derece gibi küçük bir derecenin üstündeyse unwrap başlanır. Maksat ninja, pivot-son, pivot-sondan iki
/// noktalarının align edilip edilmediğini, line of sight'tan sonra tekrar kontrol etmektir. Eski usulde derece hesabı çok saçmaydı. Bu usul hem daha mantıklı
/// hem de daha  bugsız..
/// 
/// 3-)After changing the anchoring of rope method from raycasthit and delegating it to AnchorRope objects collider, there has been raised an issue which in turn
/// brought out the main reason of the bug. When the rope is unwrapping, it should checks line of sight with second oldest pivot. But it is found out by chance
/// that it checks the line of sight with the oldest pivot. The stem cause of the problem is the first in last out ordering of pivotList array unlike the 
/// swingdirection list and linerenderer position list which are stack lists. Thus, the line of sight check method checks the very first pivot, which is the anchor
/// pivot, instead of checking the last pivot. And since the anchor pivot has ropeAnchors Collider, it never has ling of sight in some cases. 
/// In order to debug, the pivot index is changed from "pivotList.count -1"  to simply "1" (not "0" since its ninjas position) which is the  last pivot which solved the problem.
/// 
/// DATE: 04.02.2020
/// 1-) Rope Elasticity is now determined by add force rather then spring joint for better game feel.
/// 
/// DATE: 02.05.2022
/// 1-) After noticing that we are overchecking the swing direction for unwrapping the rope, the excess CheckSwingDirection... methods are removed. Leaving only the necassary method.
/// 2-) The code could further be improved by removing the need for recording, and hence checking, of swing direction alltogether. (NOTE: that this is not implemented since there is no more performance upgrades necessary)
///     2.1-) Consider there are two consecutive pivots. If the Ninja has clear sight of the former pivot and last pivot at the same time, we can clear the last pivot. Unless there is an object between them.
///     2.2-) To ensure that there are no objects inside the "Ninja-Pivot1-Pivot2" triangle, we need only to Raycast at a point, that is close to Pivot1, but infinismally moved towards Pivot2.
///     2.3-) If that Raycast returns null, than there are no objects inside that trianle and we can clear the Pivot1. No need for checking or recording swings.
/// 3-) Pivot list ve linerenderer her frame'de refresh ediliyordu. Count'lar değişmedikçe refresh/resynch etme diyerek düzeltildi.
/// </summary>

public class RopeBendController : MonoBehaviour {
    [SerializeField]
    bool isLineTo2ndClosestPivotClear = false;
    [SerializeField]
    bool isLineTo1stClosestPivotClear = false;
	public List<Vector2> pivotList = new List<Vector2>();
    public List<Vector2> forcePivotList = new List<Vector2>();
    public List<bool> pivotSwingList = new List<bool>();// if bool is true swing is clockwise

    public float pivotForceCoefficient;
    public float minLineForceDistance;
    private float lastAppliedForceCoeff;
    public float appliedForceIncreaseConstant;
    public float maxRopeForce;

    public GameObject theNinja;
    private Rigidbody2D ninjaRigidBody;
	public LineRenderer lineRenderer;
	public RopeShootControls ropeTouchControls;

    private PhotonView photonView;

	public float ropeBendAngleTolerance;
	public int pivotsAdded = 0;

    float oldAngle;
    float currentAngle;

    public float minDistanceAllowedBetweenPivots;

    public bool isNinjaLocal;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        isNinjaLocal = photonView.IsMine;
        ninjaRigidBody = theNinja.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if ( ropeTouchControls.isRopeShot)
        {
            ResynchPivotListWithLineRenderer(); //1st pivot list check to be used in bend check

            RaycastHit2D pivotClosestCheckRay = CheckClosestPivotHit();
            if ( pivotClosestCheckRay.collider != null) 
            {
                Vector2 polygonHitPoint = GetClosestColliderPointFromRaycastHit(pivotClosestCheckRay, pivotClosestCheckRay.collider.gameObject.GetComponent<PolygonCollider2D>());

                if(DistanceToLastPivot(polygonHitPoint) > minDistanceAllowedBetweenPivots)
                {
                    AddSwingDirectionForNewPivot(CenterOfMassConvexAdditionToPivot(pivotClosestCheckRay, polygonHitPoint)); //IT IS CRUCIAL THAT AddSwingDirectionForNewPivot() method starts before AddPivotForLineRen...() method, because the latter has a foreach loop and former uses a variable (pivotList) that is going to be edited by that foreach loop. But before the foreach loop ends, the former might need that variable. To ensure that the variable does not change, the method is called first.
                    AddPivotForLineRendererAndSpringJoint(polygonHitPoint);
                }
            }
            else if (pivotList != null && pivotList.Count >2 && pivotsAdded >0  && CheckSecondClosestPivotHit() 
                        && lineRenderer.positionCount > 2 && CheckAngleTolerance() && IsAngleGettingLarger() && IsPivotAngleOnCounterSwingDirection())
                ClearClosestPivotAndSwingFromList();

            ResynchPivotListWithLineRenderer(); //2nd pivot list check in case it changed

            if (isNinjaLocal && ropeTouchControls.isRopeAnchored) //folowing comment is obsolete ----> do not apply force to non local player, that could potentially mass up rigidbody velocity sync
                ApplyForceTowardsClosestPivot();
            else if (isNinjaLocal)
                lastAppliedForceCoeff = 0;
        }
        else
            lastAppliedForceCoeff = 0;
    }

    float DistanceToLastPivot(Vector2 pivotNew)
    {
        return Vector2.Distance(pivotNew, pivotList[pivotList.Count -1]);
    }

    Vector2 CenterOfMassConvexAdditionToPivot(RaycastHit2D rayPolygonHit, Vector2 pivotHit)
    {
        //At the end of following calculations, we get a position that is a result of pushing the pivotHit point outwards from the polygon that is hit by raycast
        //this helps in calculating the swing direction.
        Vector2 centerOfMassHit = rayPolygonHit.collider.gameObject.GetComponent<Rigidbody2D>().worldCenterOfMass;
        Vector2 additionToPivot = (pivotHit - centerOfMassHit) / 4f;
        Vector2 newPivotForSwingCalculation = pivotHit + additionToPivot;

        return newPivotForSwingCalculation;
    }

    bool CheckAngleTolerance()
    {
        if (GetAngleBetweenPoints(theNinja.transform.position, lineRenderer.GetPosition(1), lineRenderer.GetPosition(2))
                      > ropeBendAngleTolerance)
            return true;
        else
            return false;
    }

    int lastPivotListCount = -777; 
    void ResynchPivotListWithLineRenderer()
    {
        //Do not renew the list if the pivot number is equal to vertice number of the line renderer, saving us from getting in clearing and refilling the list. 
        if (lastPivotListCount != lineRenderer.positionCount)
        {
            pivotList.Clear();
            forcePivotList.Clear();

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                pivotList.Add((Vector2)lineRenderer.GetPosition(i));
                forcePivotList.Add((Vector2)lineRenderer.GetPosition(i));
            }

            lastPivotListCount = pivotList.Count;
        }
        else if (lineRenderer.positionCount > 1) //Only resynch start and end points (when rope is shot anchor is moving, which means end point is moving. When ninja moves, the start position moves. Which is almost always)
        {
            pivotList[0] = lineRenderer.GetPosition(0);
            forcePivotList[0] = lineRenderer.GetPosition(0);

            pivotList[pivotList.Count -1] = lineRenderer.GetPosition(pivotList.Count - 1);
            forcePivotList[pivotList.Count - 1] = lineRenderer.GetPosition(pivotList.Count - 1);
        }
    }

    void ApplyForceTowardsClosestPivot()
    {
        float forceMagnitude = CalculatedForceMagnitude();

        if (lastAppliedForceCoeff < 1.01f)
            lastAppliedForceCoeff += (Time.deltaTime * appliedForceIncreaseConstant * DistanceCoefficient());

        if (lastAppliedForceCoeff > 1f)
            lastAppliedForceCoeff = 1f;

        if (forceMagnitude > maxRopeForce)
            forceMagnitude = maxRopeForce;

        Vector2 forceVector = Vector2.ClampMagnitude(((forcePivotList[1] - (Vector2)theNinja.transform.position) * 1000f), forceMagnitude);

        forceVector *= lastAppliedForceCoeff;

        ninjaRigidBody.AddForce(forceVector, ForceMode2D.Force);
    }


    float CalculatedForceMagnitude()
    {
        float forceMagnitude = 0;

        if ( Vector2.Distance (forcePivotList[1], (Vector2)theNinja.transform.position) > minLineForceDistance)
            for(int i=0; i < forcePivotList.Count-1; i++)
                forceMagnitude += Mathf.Pow( Vector2.Distance(forcePivotList[i], forcePivotList[i+1]), 2f);

        if(forceMagnitude > 0)
          forceMagnitude = Mathf.Sqrt(forceMagnitude) * pivotForceCoefficient ;    

        return forceMagnitude;
    }

    float DistanceCoefficient()
    {
        if (Vector2.Distance(forcePivotList[1], (Vector2)theNinja.transform.position) < minLineForceDistance * 2f)
            return 1000f;
        else if(Vector2.Distance(forcePivotList[1], (Vector2)theNinja.transform.position) < minLineForceDistance * 4f)
            return 10f;
        else if (Vector2.Distance(forcePivotList[1], (Vector2)theNinja.transform.position) < minLineForceDistance * 5f)
            return 1.5f;
        else
            return 0.75f;
    }

    RaycastHit2D CheckClosestPivotHit()
    {
		Vector2 ninjaNextFramePosition =  (Vector2)  theNinja.transform.position;

        float ropeDistance = 0;
        Vector2 rayDirection;
        ropeDistance = Vector2.Distance(pivotList[1], ninjaNextFramePosition);
        rayDirection = pivotList[1] - ninjaNextFramePosition;

        LayerMask layerMask = LayerMask.GetMask("ObstaclesLayer");//We want the rope to wrap only around obstacles

		RaycastHit2D hit = Physics2D.Raycast( ninjaNextFramePosition , rayDirection , ropeDistance -0.1f, layerMask);
        if (hit.collider != null)
            isLineTo1stClosestPivotClear = false;
        else
            isLineTo1stClosestPivotClear = true;

		return hit;
	}

    /// <summary>
    /// This method figures out the closest Polygon collider vertex to a specified Raycast2D hit point in order to assist in 'rope wrapping'
    /// </summary>
    /// <param name="hit">The raycast2d hit</param>
    /// <param name="polyCollider">the reference polygon collider 2D</param>
    /// <returns></returns>
    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, PolygonCollider2D polyCollider)
    {
        // Transform polygoncolliderpoints to world space (default is local)
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)), 
            position => polyCollider.transform.TransformPoint(position));

        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }

	void AddPivotForLineRendererAndSpringJoint (Vector2 polygonHitPoint) 
    {
		pivotsAdded++;
		AddLineRenderBendPoint ( polygonHitPoint );
	}

	void AddLineRenderBendPoint (Vector2 polygonHitPoint)
    {
		Vector2 ninjaNextFramePosition =  (Vector2)  theNinja.transform.position;

		Vector2[] tempPoints = new Vector2 [lineRenderer.positionCount+1];
		tempPoints[0] = lineRenderer.GetPosition(0);//first point is always the same (ninja position)
		tempPoints[1] = polygonHitPoint; //New pivot(from hit) is the 2nd pivot to the origin point of line renderer

		for ( int i = 2; i < lineRenderer.positionCount+1; i++)
				tempPoints[i] = lineRenderer.GetPosition(i-1);
		
		lineRenderer.positionCount++;

		for ( int i = 0; i< tempPoints.Length; i++)
			lineRenderer.SetPosition(i, (Vector3) tempPoints[i]);
	}

    void AddSwingDirectionForNewPivot (Vector2 polygonHitPoint)
    {
		bool isSwingClockWise = CheckSwingDirectionByNinjaPositon(polygonHitPoint);
	
		pivotSwingList.Add(isSwingClockWise);
	}

    bool CheckSwingDirectionByNinjaPositon(Vector2 pivotPosition) 
    {
        bool isSwingClockWise = false;
        float ninjaX = theNinja.transform.position.x;
        float ninjaY = theNinja.transform.position.y;

        Vector3 pivotPoint = (Vector3)pivotPosition;
        Vector3 ninjaOldPoint = pivotList[1];
        Vector3 ninjaNewPoint = new Vector3(ninjaX, ninjaY, 0); ;

        Vector3 firstVector = ninjaOldPoint - pivotPoint;
        Vector3 secondVector = ninjaNewPoint - pivotPoint;

        Vector3 leftHandRuleVector = Vector3.Cross(firstVector, secondVector);

        if (leftHandRuleVector.z > 0)
            isSwingClockWise = true;

        return isSwingClockWise;
    }

    bool IsPivotAngleOnCounterSwingDirection()
    {
        bool isPivotVsNinjaClockWise = false;
        int closestPivotIndex = 1;

        Vector2 pivotPoint = pivotList[closestPivotIndex];
        Vector2 ninjaOldPoint = pivotList[closestPivotIndex+1];
        Vector2 ninjaNewPoint = theNinja.transform.position;

        Vector2 firstVector = ninjaOldPoint - pivotPoint;
        Vector2 secondVector = ninjaNewPoint - pivotPoint;

        Vector3 leftHandRuleVector = Vector3.Cross(firstVector, secondVector);

        if (leftHandRuleVector.z < 0)
            isPivotVsNinjaClockWise = true;

        if (isPivotVsNinjaClockWise == pivotSwingList[pivotSwingList.Count - 1])
            return true;
        else
            return false;
    }

	bool CheckSecondClosestPivotHit() 
    {
		bool isLineToSecondPivotClear = true;

		float ropeDistance =  Vector2.Distance(  pivotList[2], (Vector2)theNinja.transform.position);
		Vector2 rayDirection = pivotList[2] - (Vector2)theNinja.transform.position ;

        LayerMask layerMask = LayerMask.GetMask("ObstaclesLayer");//We want the rope to wrap only around obstacles


        RaycastHit2D hit = Physics2D.Raycast( (Vector2)theNinja.transform.position , rayDirection , ropeDistance -0.1f, layerMask);

        if (hit.collider != null)// && !hit.collider.gameObject.transform.parent.gameObject.name.Contains("bstacle") )
        {
            isLineTo2ndClosestPivotClear = false; //ADDED FOR DEBUG.
            isLineToSecondPivotClear = false;
            Debug.DrawRay((Vector2)theNinja.transform.position, rayDirection, Color.green);
        }
        else
            isLineTo2ndClosestPivotClear = true;

        return isLineToSecondPivotClear;
	}

	void ClearClosestPivotAndSwingFromList ()
    {
        pivotsAdded--;
		DeleteLastLineRenderBendPoint();
	}

	void DeleteLastLineRenderBendPoint ()
    {
        pivotList.RemoveAt(pivotList.Count - 1);
        pivotSwingList.RemoveAt(pivotSwingList.Count - 1);
	
		Vector2[] tempPoints = new Vector2[lineRenderer.positionCount-1];
		tempPoints[0] = lineRenderer.GetPosition(0);//first point is always the same (ninja position)

		for ( int i = 1; i < lineRenderer.positionCount-1; i++)
			tempPoints[i] = lineRenderer.GetPosition(i+1);

		lineRenderer.positionCount--;

		if (lineRenderer.positionCount <2)
			Debug.Log("WTF!!!, lineRenderer position count is smaller than 2!!!");

		for ( int i = 0; i< tempPoints.Length; i++)
			lineRenderer.SetPosition(i, (Vector3) tempPoints[i]);
	}

	float GetAngleBetweenPoints( Vector3 a, Vector3 b, Vector3 c)
    {
        oldAngle = currentAngle;

        float result = 0;

		float ab = Vector2.Distance(a, b);

		float bc = Vector2.Distance(b, c);

		float ac = Vector2.Distance(a, c);

		float cosB = Mathf.Pow(ac, 2) - Mathf.Pow(ab, 2) - Mathf.Pow(bc, 2);

		cosB /= (2 * ab * bc);

		result = Mathf.Acos(cosB) * Mathf.Rad2Deg ;

        currentAngle = result;

		return result;
	}

    bool IsAngleGettingLarger()
    {
        if ( currentAngle > oldAngle)
            return true;
        else
            return false;
    }

	public void ResetBendControls()
    {
		pivotList.Clear();
		pivotsAdded = 0;
		pivotSwingList.Clear();
	}

	public void MovePivotsWithMovingObject(Collider2D movingObject, Vector2 movementVector)
    {
		Vector2[] tempPoints = new Vector2[lineRenderer.positionCount];

		for ( int i = 0; i < lineRenderer.positionCount; i++)
			tempPoints[i] = lineRenderer.GetPosition(i);

		for ( int i = 0; i < lineRenderer.positionCount; i++)
        {
            if ( movingObject.OverlapPoint ( tempPoints[i] ) )
				tempPoints[i] = tempPoints[i] + movementVector;
		}

		for ( int i = 0; i< tempPoints.Length; i++)
			lineRenderer.SetPosition(i, (Vector3) tempPoints[i]);

		ResynchPivotListWithLineRenderer();
	}


}
