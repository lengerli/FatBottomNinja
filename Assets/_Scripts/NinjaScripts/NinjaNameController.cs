using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class NinjaNameController : MonoBehaviour
{
    Camera mainCam;

    public Vector3 nameHoverDistanceY;

    [SerializeField]
    public string displayName;

    //this is the ui element
    RectTransform playerNameRect;
    //first you need the RectTransform component of your canvas
    RectTransform CanvasRect;

    float canvasRectSizeDeltaX;
    float canvasRectSizeDeltaY;

    public Canvas canvasForNamePrefab;
    public Canvas theCanvasForName;
    
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        mainCam = Camera.main;
        theCanvasForName = Instantiate(canvasForNamePrefab);
        CanvasRect = theCanvasForName.GetComponent<RectTransform>();
        playerNameRect = theCanvasForName.transform.GetChild(0).gameObject.GetComponent<RectTransform>();

        canvasRectSizeDeltaX = CanvasRect.sizeDelta.x;
        canvasRectSizeDeltaY = CanvasRect.sizeDelta.y;

        //Get the playername from player prefs. 
        if (photonView.IsMine)
            SetNameForNinja(PlayerPrefs.GetString("NickName"));
        //Get playername from original ninja
        else
            photonView.RPC("RequestOriginalName", RpcTarget.Others);
    }


    // Update is called once per frame
    void Update()
    {
        HoverTheName();
    }

    [PunRPC]
    public void RequestOriginalName()
    {
        photonView.RPC("SyncMyNameNetwork", RpcTarget.Others, displayName);
    }

    [PunRPC]
    public void SyncMyNameNetwork(string theName)
    {
        SetNameForNinja(theName);
    }

    private void SetNameForNinja(string theName)
    {
        displayName = theName;
        gameObject.name = displayName + " - NinjaCharacter(Clone)";
        theCanvasForName.transform.GetChild(0).gameObject.GetComponent<Text>().text = displayName;
        GetComponent<ScoreController>().UpdateScoreDisplay();
    }

    void HoverTheName()
    {
        //Calculate the position of the UI element
        //"(0,0)" origin of the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0.
        //This is why you need to subtract the "height/width" ratio of the canvas multiplied by "0.5f" to get the correct position.


        Vector2 ViewportPosition = mainCam.WorldToViewportPoint(gameObject.transform.position + nameHoverDistanceY );
        Vector2 WorldObject_ScreenPosition = new Vector2(
        (canvasRectSizeDeltaX * (ViewportPosition.x - 0.5f)),
        (canvasRectSizeDeltaY * (ViewportPosition.y - 0.5f)));

        //now you can set the position of the ui element
        playerNameRect.anchoredPosition = WorldObject_ScreenPosition;
    }


    private void OnDestroy()
    {
        GameObject.Destroy(theCanvasForName.gameObject);
    }

    private void OnDisable()
    {
        theCanvasForName.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if(theCanvasForName != null)
           theCanvasForName.gameObject.SetActive(true);
    }

}
