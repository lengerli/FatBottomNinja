using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameWonActions : MonoBehaviour
{
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void GameWonNetworkSync()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("GameWonCallNetwork", RpcTarget.All);
        }
    }

    [PunRPC]
    public void GameWonCallNetwork()
    {
        /*
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        */
        gameObject.GetComponent<RopeShootControls>().ReleaseRopeNetwork();
        gameObject.GetComponent<RopeShootControls>().enabled = false;
        gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
        gameObject.SetActive(false);
    }



}
