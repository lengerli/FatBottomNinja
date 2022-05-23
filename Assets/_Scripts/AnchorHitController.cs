using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnchorHitController : MonoBehaviour
{
    public Collider2D anchorHitCol;
    public float anchorShootVel;

    public GameObject crossHairArm;

    public float anchorGravityScale;

    // Start is called before the first frame update
    void Start()
    {
    //    GetComponent<Rigidbody2D>().velocity = Vector2.right * anchorShootVel;   
    }


    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D other)
    {
        anchorHitCol = other;

        if ( 
            !(other.gameObject.name.Contains("Camera")
            || other.gameObject.name.Contains("inja")
            || other.gameObject.name.Contains("ullet")
            || other.gameObject.name.Contains("enade")) )
        {
            transform.parent.GetComponent<RopeShootControls>().SetAnchorHit(transform.position); //SetAnchorHit only works for local player

            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().gravityScale = 0;
        }

    }

    void OnTriggerExit2D()
    {
        anchorHitCol = null;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if ( anchorHitCol == null)
        {
            anchorHitCol = other;
            if (!(other.gameObject.name.Contains("Camera")
                || other.gameObject.name.Contains("inja")
                || other.gameObject.name.Contains("ullet")
                || other.gameObject.name.Contains("enade")))
            {
                transform.parent.GetComponent<RopeShootControls>().SetAnchorHit(transform.position);//SetAnchorHit only works for local player

                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                GetComponent<Rigidbody2D>().gravityScale = 0;
            }
        }
    }

    public void DisableAnchor()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    public void EnableAnchor()
    {
        GetComponent<Rigidbody2D>().gravityScale = anchorGravityScale;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}
