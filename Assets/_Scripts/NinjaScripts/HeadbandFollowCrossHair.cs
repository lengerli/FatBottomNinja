using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadbandFollowCrossHair : MonoBehaviour
{
    [SerializeField]
    GameObject crossHair;
    [SerializeField]
    float maxRotation;
    [SerializeField]
    GameObject localConfinedCrosshair;

    // Update is called once per frame
    void Update()
    {
        RotateHeadBandWithCrosshair();
        CheckHeadFlipToCrossHairSide();
    }

    void RotateHeadBandWithCrosshair()
    {
        if (transform.position.x > localConfinedCrosshair.transform.position.x)
            RotateHeadLeftDirection();
        else if (transform.position.x < localConfinedCrosshair.transform.position.x)
            RotateHeadRightDirection();
    }

    void RotateHeadRightDirection()
    {
        if (crossHair.transform.rotation.eulerAngles.z < maxRotation + 0.1f)
        {
            transform.rotation = crossHair.transform.rotation;
            transform.localPosition = new Vector3((crossHair.transform.rotation.eulerAngles.z / -300f), transform.localPosition.y, transform.localPosition.z);


        }
        else if (crossHair.transform.rotation.eulerAngles.z > (360f - maxRotation))
        {
            transform.rotation = crossHair.transform.rotation;
            transform.localPosition = new Vector3(((crossHair.transform.rotation.eulerAngles.z - 360f) / -350f), transform.localPosition.y, transform.localPosition.z);
        }
        else if (crossHair.transform.rotation.eulerAngles.z > maxRotation && crossHair.transform.rotation.eulerAngles.z < 91f)
            transform.rotation = Quaternion.Euler(0, 0, maxRotation);
        else if (crossHair.transform.rotation.eulerAngles.z < (360f - maxRotation))
            transform.rotation = Quaternion.Euler(0, 0, (360f - maxRotation));
    }

    void RotateHeadLeftDirection()
    {
        if (crossHair.transform.rotation.eulerAngles.z > 180f - maxRotation && crossHair.transform.rotation.eulerAngles.z < (180f + maxRotation))
        {
            transform.rotation = Quaternion.Euler(crossHair.transform.rotation.eulerAngles - new Vector3(0, 0, 180f));
            transform.localPosition = new Vector3(((crossHair.transform.rotation.eulerAngles.z - 180f) / 300f), transform.localPosition.y, transform.localPosition.z);

            if (transform.rotation.eulerAngles.z < 23f)
                transform.localPosition -= new Vector3(transform.rotation.eulerAngles.z/200f,0,0);
        }
        else if (crossHair.transform.rotation.eulerAngles.z < 180f - maxRotation)
            transform.rotation = Quaternion.Euler(0, 0, 360f - maxRotation);
        else if (crossHair.transform.rotation.eulerAngles.z > (180f + maxRotation))
            transform.rotation = Quaternion.Euler(0, 0, maxRotation);
    }

    void CheckHeadFlipToCrossHairSide()
    {
        if (transform.position.x < localConfinedCrosshair.transform.position.x)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            transform.GetChild(0).gameObject.GetComponentInChildren<SpriteRenderer>().flipX = false;
        }
        else if (transform.position.x > localConfinedCrosshair.transform.position.x)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            transform.GetChild(0).gameObject.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
    }
}
