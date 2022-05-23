using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyCenterAttachToNinja: MonoBehaviour
{
    Transform ninjaTrn;


    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("Transform position assignment at update will cause jitters. You can see the jitters in only the scene mode though.");
        ninjaTrn = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = ninjaTrn.position;
    }
}
