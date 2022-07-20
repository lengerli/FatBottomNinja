using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssTouchGroundChecker : MonoBehaviour
{
    [SerializeField]
    NinjaMoveAndFollowCamera ninjaMoveControls;

    void Start()
    {
        ninjaMoveControls = transform.parent.gameObject.GetComponent<NinjaMoveAndFollowCamera>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("ObstaclesLayer"))
            ninjaMoveControls.assIsTouchingGround = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("ObstaclesLayer"))
            ninjaMoveControls.assIsTouchingGround = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("ObstaclesLayer"))
            ninjaMoveControls.assIsTouchingGround = false;
    }

}
