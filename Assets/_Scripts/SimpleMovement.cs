using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


public class SimpleMovement : MonoBehaviour
{
    public float moveScale;

    PhotonView photonView;

    Rigidbody2D rigidBody;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.UpArrow))
                rigidBody.velocity = Vector2.up * moveScale ;
            if (Input.GetKey(KeyCode.DownArrow))
                rigidBody.velocity = Vector2.down * moveScale ;
            if (Input.GetKey(KeyCode.RightArrow))
                rigidBody.velocity = Vector2.right * moveScale ;
            if (Input.GetKey(KeyCode.LeftArrow))
                rigidBody.velocity = Vector2.left * moveScale ;
        }
    }
}
