using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaJellySync : MonoBehaviour
{
    public float rigidSyncThreshold;

    [SerializeField]
    Rigidbody2D jellyRigidBodyCentral;

    public GameObject jellyObjectOriginal;

    private void Start()
    {
        //TODO Instantiate a jellySprite object with this Ninjas photon id 
        //TODO assign the instantiated object to jellyObjectOriginal

        jellyObjectOriginal = GameObject.FindWithTag("JellyDummy");
        StartCoroutine(GetJellyCentral());
    }

    public Rigidbody2D Rigidbody2DJelly 
    {
        get { return jellyRigidBodyCentral; }
        set { jellyRigidBodyCentral = value; }
    }


    IEnumerator GetJellyCentral()
    {
        yield return null;
        if (GameObject.Find(jellyObjectOriginal.name + " Reference Points") != null)
            ExtractJellyObject();
        else
            StartCoroutine(GetJellyCentral());
    }

    void ExtractJellyObject()
    {
        GameObject jellyRefParentObj = GameObject.Find(jellyObjectOriginal.name + " Reference Points");
        GameObject jellyCentral = jellyRefParentObj.transform.GetChild(0).gameObject;
        jellyRigidBodyCentral = jellyCentral.GetComponent<Rigidbody2D>();
        GetComponent<Rigidbody2D>().position = jellyRigidBodyCentral.position;
    }

    private void FixedUpdate()
    {
        jellyRigidBodyCentral.position = GetComponent<Rigidbody2D>().position;
        //jellyRigidBodyCentral.velocity = GetComponent<Rigidbody2D>().velocity;

        //if(Vector2.Distance(GetComponent<Rigidbody2D>().position, jellyRigidBodyCentral.position) > rigidSyncThreshold)
        //GetComponent<Rigidbody2D>().position = jellyRigidBodyCentral.position;
    }

}
