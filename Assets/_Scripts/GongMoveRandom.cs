using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GongMoveRandom : MonoBehaviour
{
    Rigidbody2D gongRgbd;
    // Start is called before the first frame update
    public float gongForceMin;
    public float gongForceMax;
    void Start()
    {
        gongRgbd = GetComponent<Rigidbody2D>();
        StartCoroutine(ApplyForceToGong());
    }

    IEnumerator ApplyForceToGong()
    {
        float nextForceWaitTime = Random.Range(0.5f, 1f);
        yield return new WaitForSeconds(nextForceWaitTime);
        gongRgbd.AddForce(new Vector2(Random.Range(gongForceMin,gongForceMax), 0f) );
        StartCoroutine(ApplyForceToGong());
    }
}
