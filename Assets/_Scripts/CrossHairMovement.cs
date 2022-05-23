using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CrossHairMovement : MonoBehaviour
{
    [SerializeField]
    GameObject crossHair;

    [SerializeField]
    GameObject localThreePointCrosshair_MidPointObject;

    public float currentCrosshairAngle = 0;

    PhotonView photonView;

    bool isLocalNinja;

    public float deltaMouseAugmentationCoeff;

    [SerializeField]
    private int lastSynchedCrosshairAngle = 0;

    [SerializeField]
    private float crossHairSynchPeriod; //in seconds

    private float timeSinceLastCrossHairSynch = 0;

    [SerializeField]
    private int currentCrosshairAngleForNetworkUse;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine == false)
            crossHair.GetComponent<SpriteRenderer>().enabled = false;

        isLocalNinja = photonView.IsMine;

    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalNinja)
            FreeMoveCrosshair();
    }

    void FreeMoveCrosshair()
    {
        if (Cursor.visible == false && Cursor.lockState == CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.None;

        crossHair.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int newAngle = (int) CalculateAngleFromMousePosition(Input.mousePosition);


        MoveTheCrosshair(newAngle);
    }




    [PunRPC]
    public void MoveTheCrosshair(int angle)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, (float) angle));
    }

    float CalculateAngleFromNewCrosshair(Vector2 crosshairPos)
    {
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        crosshairPos = Camera.main.WorldToScreenPoint(crosshairPos);
        crosshairPos.x = crosshairPos.x - objectPos.x;
        crosshairPos.y = crosshairPos.y - objectPos.y;

        float angle = Mathf.Atan2(crosshairPos.y, crosshairPos.x) * Mathf.Rad2Deg;
        return angle;
    }

    float CalculateAngleFromMousePosition(Vector3 mousePos)
    {
        mousePos.z = 0;

        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        return angle;
    }

}
