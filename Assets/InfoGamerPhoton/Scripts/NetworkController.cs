using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkController : MonoBehaviourPunCallbacks
{
    /******************************************************
    * Refer to the Photon documentation and scripting API for official definitions and descriptions
    * 
    * Documentation: https://doc.photonengine.com/en-us/pun/current/getting-started/pun-intro
    * Scripting API: https://doc-api.photonengine.com/en/pun/v2/index.html
    * 
    * If your Unity editor and standalone builds do not connect with each other but the multiple standalones
    * do then try manually setting the FixedRegion in the PhotonServerSettings during the development of your project.
    * https://doc.photonengine.com/en-us/realtime/current/connection-and-authentication/regions
    *
    * ******************************************************/

    [SerializeField]
    private int gameVersion;

    [SerializeField]
    private string serverSelection = "";

    public Dropdown serverDropdown;

    private void Awake()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public void SetServerSelection()
    {
        serverSelection = ConvertDropdownSelectionToPhotonServerCode(serverDropdown.captionText.text);
    }

    public void StartServerConnection()
    {
        PhotonNetwork.GameVersion = gameVersion.ToString();
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = serverSelection;
        PhotonNetwork.ConnectUsingSettings(); //Connects to Photon master servers
        //Other ways to make a connection can be found here: https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_network.html
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server! /n Mert, if you want to connect to a specific region, use ConnectToRegion() method instead..");
    }

    string ConvertDropdownSelectionToPhotonServerCode(string dropdownCaptionText)
    {
        string serverCode = ""; //Default is empty, in this case Photon will connect to closest server

        switch (dropdownCaptionText)
        {
            case "TR Server":
                serverCode = "tr";
                break;
            case "EU Server":
                serverCode = "eu";
                break;
            case "USW Server":
                serverCode = "usw";
                break;
            case "USE Server":
                serverCode = "us";
                break;
            case "CAN Server":
                serverCode = "cae";
                break;
            case "ASIA Server":
                serverCode = "asia";
                break;
            case "BRZ Server":
                serverCode = "sa";
                break;
            case "JP Server":
                serverCode = "jp";
                break;
            case "SAFR Server":
                serverCode = "za";
                break;
        }



        return serverCode;
    }
}
