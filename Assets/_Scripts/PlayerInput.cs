using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviour
{
    PhotonView phView;
    bool isLocalPlayer;
    

    void Start()
    {
        phView = GetComponent<PhotonView>();
        isLocalPlayer = phView.IsMine;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            CheckKeyboardInput();
            SendKeyboardInputToMaster();
        }
        
    }
    
    void CheckKeyboardInput()
    {
    }

    void SendKeyboardInputToMaster()
    {

    }
}
