using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelfController : MonoBehaviour {
    public GameObject spriteForRotationOnly;

    public bool isNinjaStar;

	public Vector3 velocityWhenShot;

    public float shootInterval;

    public float reloadInterval;

    public int shotsInOneClip;

    public bool isBacklashEnabled;

    public float backlashCoeff;

    // Use this for initialization
    void Start () {
		ShootTheBullet();
	}
	
    public Vector2 BulletVelocity
    {
        set { velocityWhenShot = value; }
        get { return velocityWhenShot; }
    }

	public virtual void ShootTheBullet()	{
		GetComponent<Rigidbody2D>().velocity = (Vector2) transform.TransformDirection( velocityWhenShot);
	}


    public float starSpinSpeed;



     void FixedUpdate()
    {
        //STOP the spin in OnTriggerEnter, when it hits ninja or obstacle
        if(isNinjaStar)
            spriteForRotationOnly.transform.Rotate(Vector3.forward, starSpinSpeed * Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

    }

}
