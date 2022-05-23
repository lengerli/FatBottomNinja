using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RopeBendControlsClassic : MonoBehaviour
{

    public List<Vector2> pivotList = new List<Vector2>();
    public List<bool> pivotSwingList = new List<bool>();// if bool is true swing is clockwise
    public GameObject theNinja;
    public LineRenderer lineRenderer;
    public SpringJoint2D springJoint;
    public RopeShootControls ropeTouchControls;
    public float ropeBendAngleTolerance;
    public int pivotsAdded = 0;

    void FixedUpdate()
    {
        if (ropeTouchControls.isRopeShot)
        {   // && ropeTouchControls.isRopeAnchored ){

            GetPivotListFromLineRenderer();

            RaycastHit2D pivotClosestCheckRay = CheckClosestPivotHit();
            if (pivotClosestCheckRay.collider != null && !pivotClosestCheckRay.collider.gameObject.name.Contains("nchor") && isNotANinjaLayer(pivotClosestCheckRay))
            {
                Vector2 polygonHitPoint = GetClosestColliderPointFromRaycastHit(pivotClosestCheckRay, (PolygonCollider2D)pivotClosestCheckRay.collider);
                AddPivotForLineRendererAndSpringJoint(polygonHitPoint);
                AddSwingDirectionForNewPivot(polygonHitPoint);
            }
            else if (pivotList != null && pivotsAdded > 0 && CheckSwingForClearingPivot() && CheckSecondClosestPivotHit()
                && lineRenderer.positionCount > 2
                && CheckAngleTolerance())
                ClearClosestPivotAndSwingFromList();

        }
    }

    bool CheckAngleTolerance()
    {
        if (GetAngleBetweenPoints(theNinja.transform.position, lineRenderer.GetPosition(1), lineRenderer.GetPosition(2))
                      < ropeBendAngleTolerance / Vector3.Distance(theNinja.transform.position, lineRenderer.GetPosition(1)))
            return true;
        else
            return false;
    }

    bool isNotANinjaLayer(RaycastHit2D pivotClosestCheckRay)
    {
        int layerIndex = pivotClosestCheckRay.collider.gameObject.layer;
        if (layerIndex == 9 || layerIndex == 8)
            return false;
        else
            return true;
    }

    void GetPivotListFromLineRenderer()
    {
        pivotList.Clear();

        for (int i = 0; i < lineRenderer.positionCount; i++)
            pivotList.Add((Vector2)lineRenderer.GetPosition(i));
    }

    RaycastHit2D CheckClosestPivotHit()
    {
        Vector2 ninjaNextFramePosition = (Vector2.ClampMagnitude(theNinja.GetComponent<Rigidbody2D>().velocity, (lineRenderer.startWidth))) + (Vector2)theNinja.transform.position;

        float ropeDistance = Vector2.Distance(pivotList[pivotList.Count - 1 - pivotsAdded], ninjaNextFramePosition);
        Vector2 rayDirection = pivotList[pivotList.Count - 1 - pivotsAdded] - ninjaNextFramePosition;

        LayerMask layerMask = 1 << theNinja.layer | 1 << (theNinja.layer + 1);
        layerMask = ~layerMask;

        RaycastHit2D hit = Physics2D.Raycast(ninjaNextFramePosition, rayDirection, ropeDistance - 0.01f, layerMask);

        return hit;
    }

    /// <summary>
    /// Figures out the closest Polygon collider vertex to a specified Raycast2D hit point in order to assist in 'rope wrapping'
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

    void AddPivotForLineRendererAndSpringJoint(Vector2 polygonHitPoint)
    {
        pivotsAdded++;

        AddLineRenderBendPoint(polygonHitPoint);

        AddNewAnchorForSpringJoint(polygonHitPoint);
    }

    void AddLineRenderBendPoint(Vector2 polygonHitPoint)
    {
        Vector2 ninjaNextFramePosition = (Vector2)theNinja.transform.position;

        Vector2[] tempPoints = new Vector2[lineRenderer.positionCount + 1];
        tempPoints[0] = lineRenderer.GetPosition(0);//first point is always the same (ninja position)
        tempPoints[1] = polygonHitPoint; //New pivot(from hit) is the 2nd pivot to the origin point of line renderer

        for (int i = 2; i < lineRenderer.positionCount + 1; i++)
            tempPoints[i] = lineRenderer.GetPosition(i - 1);

        lineRenderer.positionCount++;


        for (int i = 0; i < tempPoints.Length; i++)
            lineRenderer.SetPosition(i, (Vector3)tempPoints[i]);
    }

    void SetCircleColliderAsNewPivot()
    {

    }

    void AddNewAnchorForSpringJoint(Vector2 newAnchor)
    {
        springJoint.connectedAnchor = newAnchor;
    }



    void AddSwingDirectionForNewPivot(Vector2 polygonHitPoint)
    {
        bool isSwingClockWise = CheckSwingDirection(polygonHitPoint);

        pivotSwingList.Add(isSwingClockWise);
    }

    bool CheckSwingDirection(Vector2 pivotPosition)
    {
        bool isSwingClockWise = false;
        float ninjaX = theNinja.transform.position.x;
        float ninjaY = theNinja.transform.position.y;

        float ninjaVelX = theNinja.GetComponent<Rigidbody2D>().velocity.x;
        float ninjaVelY = theNinja.GetComponent<Rigidbody2D>().velocity.y;

        Vector3 pivotPoint = (Vector3)pivotPosition;
        Vector3 ninjaOldPoint = new Vector3(ninjaX, ninjaY, 0);
        Vector3 ninjaNewPoint = new Vector3(ninjaX + ninjaVelX, ninjaY + ninjaVelY, 0);

        Vector3 firstVector = ninjaOldPoint - pivotPoint;
        Vector3 secondVector = ninjaNewPoint - pivotPoint;

        Vector3 leftHandRuleVector = Vector3.Cross(firstVector, secondVector);

        if (leftHandRuleVector.z < 0)
            isSwingClockWise = true;

        return isSwingClockWise;
    }

    bool CheckSwingForClearingPivot()
    {
        bool isSwingChanged = false;

        if (pivotSwingList[pivotSwingList.Count - 1] != CheckSwingDirection(pivotList[1]))
            isSwingChanged = true;

        return isSwingChanged;
    }

    bool CheckSecondClosestPivotHit()
    {
        bool isLineToSecondPivotClear = true;

        float ropeDistance = Vector2.Distance(pivotList[pivotList.Count - pivotsAdded], (Vector2)theNinja.transform.position);
        Vector2 rayDirection = pivotList[pivotList.Count - pivotsAdded] - (Vector2)theNinja.transform.position;

        LayerMask layerMask = 1 << theNinja.layer | 1 << (theNinja.layer + 1);
        layerMask = ~layerMask;

        RaycastHit2D hit = Physics2D.Raycast((Vector2)theNinja.transform.position, rayDirection, ropeDistance - 0.01f, layerMask);

        if (hit.collider != null)
            isLineToSecondPivotClear = false;

        return isLineToSecondPivotClear;
    }

    void ClearClosestPivotAndSwingFromList()
    {
        pivotsAdded--;

        DeleteLastLineRenderBendPoint();

        AddNewAnchorForSpringJoint((Vector2)lineRenderer.GetPosition(1));
    }

    void DeleteLastLineRenderBendPoint()
    {
        Vector2[] tempPoints = new Vector2[lineRenderer.positionCount - 1];
        tempPoints[0] = lineRenderer.GetPosition(0);//first point is always the same (ninja position)

        for (int i = 1; i < lineRenderer.positionCount - 1; i++)
            tempPoints[i] = lineRenderer.GetPosition(i + 1);

        lineRenderer.positionCount--;

        if (lineRenderer.positionCount < 2)
        {
            Debug.Break();
            Debug.Log("WTF!!!");
        }

        for (int i = 0; i < tempPoints.Length; i++)
            lineRenderer.SetPosition(i, (Vector3)tempPoints[i]);
    }

    float GetAngleBetweenPoints(Vector3 a, Vector3 b, Vector3 c)
    {
        float result = 0;

        float ab = Vector2.Distance(a, b);

        float bc = Vector2.Distance(b, c);

        float ac = Vector2.Distance(a, c);

        float cosB = Mathf.Pow(ac, 2) - Mathf.Pow(ab, 2) - Mathf.Pow(bc, 2);

        cosB /= (2 * ab * bc);

        result = Mathf.Acos(cosB) * Mathf.Rad2Deg;

        return result;
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

        for (int i = 0; i < lineRenderer.positionCount; i++)
            tempPoints[i] = lineRenderer.GetPosition(i);

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {

            if (movingObject.OverlapPoint(tempPoints[i]))
                tempPoints[i] = tempPoints[i] + movementVector;
        }

        for (int i = 0; i < tempPoints.Length; i++)
            lineRenderer.SetPosition(i, (Vector3)tempPoints[i]);

        GetPivotListFromLineRenderer();

        //Check spring joint too
        if (movingObject.OverlapPoint(springJoint.connectedAnchor))
            springJoint.connectedAnchor = springJoint.connectedAnchor + movementVector;
    }

}
