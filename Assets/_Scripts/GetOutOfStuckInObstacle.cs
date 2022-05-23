using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GetOutOfStuckInObstacle : MonoBehaviour
{
    /// <summary>
    /// KILLS THE NINJA BY PLAYERS DECISON IN CASE IT IS STUCK, SO THAT IT CAN RESPAWN
    /// </summary>
    /// 

    public int respawnKeyStrokeCounter = 0;
    public bool isNinjaStuck;

    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (respawnKeyStrokeCounter > 2 && photonView.IsMine)
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().RelocateThisNinja(gameObject);
            respawnKeyStrokeCounter = 0;
        }
        else if (Input.GetKeyDown(KeyCode.R) && Physics2D.OverlapCircleAll(transform.position, 0.05f).Length > 2)
            respawnKeyStrokeCounter++;

    }


}
