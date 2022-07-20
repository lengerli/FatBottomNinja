using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This component controls the movement of mid bottom ref points of jelly ninja.
/// When this suspender moves one unit, the suspender shall move 2 units;
/// Thus moving suspender much more higher than it should be when the ninja sits on flat floor.
/// So that the ninja bottom can look flat.
/// </summary>

public class NinjaSuspenderControls : MonoBehaviour
{
    public Transform connectedRefPointTransform;
    public float startingAnchorRelativeLocalPosition_Y; //This should be set by JellySprite.cs when this prefab is instantiated.
    public Transform suspenderTransform;

    [SerializeField]
    private float currentDistanceSuspenderToStartPos_Y;
    [SerializeField]
    private float refPointYAugmentationCoeff;
    [SerializeField]
    private float yForRefPoint;
    [SerializeField]
    private float maxYforRefPoint;

    private void Start()
    {
        if (refPointYAugmentationCoeff == 0)
            Debug.LogError("ref point augmetnttion coeff cannot be 0, otherwise the ref points will always be at 0 y point");
    }

    private void FixedUpdate()
    {
        //TODO calculate the y position of ref point, according to suspender distance from start.
        currentDistanceSuspenderToStartPos_Y = suspenderTransform.localPosition.y - startingAnchorRelativeLocalPosition_Y;

        yForRefPoint = (currentDistanceSuspenderToStartPos_Y * refPointYAugmentationCoeff) + startingAnchorRelativeLocalPosition_Y;

        connectedRefPointTransform.localPosition = new Vector2(suspenderTransform.localPosition.x, yForRefPoint);
    }
}
