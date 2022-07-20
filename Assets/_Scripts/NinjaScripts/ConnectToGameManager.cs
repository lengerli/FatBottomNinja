using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ConnectToGameManager : MonoBehaviour
{
    bool isGameManagerAdded;

    const string ADDNINJATOMANAGER = "AddNinjaToManager";
    GameManager gameManager;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (!isGameManagerAdded)
            AddNinjaToManager();
    }

    // Update is called once per frame
    void AddNinjaToManager()
    {
        if(!isGameManagerAdded)
             gameManager.AddNewNinja(gameObject);
        isGameManagerAdded = true;
    }

    public void CallRestartOnGameManagerNW()
    {
        GetComponent<PhotonView>().RPC("CallRestartInAllGameManagers", RpcTarget.All);
    }

    [PunRPC]
    public void CallRestartInAllGameManagers()
    {
        gameManager.RestartGameNW();
    }

    private void OnDestroy()
    {
        gameManager.RemoveDeadNinja(gameObject);
    }
}
