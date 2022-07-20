using Photon.Pun;
using System.Collections;
using UnityEngine;

public class KatanaSlicer : MonoBehaviour
{
    /// <summary>
    /// 
    /// This is the script component attached to Katana GameObject
    /// The Katana object should be attached to NinjaCharacter Prefab and it should never be destroyed, unlike bullet prefabs. 
    /// This script draws the katana by increasing localscale, than swipes the katana over the ninja character
    /// Katana does not stop until it reaches final destination, so it might cause damage to multiple players.
    /// 
    /// </summary>
    public GameObject headBand;

    [SerializeField]
    Vector3 katanaDimensions;

    [SerializeField]
    float katanaAngularSwipeSpeed;

    [SerializeField]
    float katanaAngleForStart;

    public Vector3 katanaRotationAtStart;

    bool isNinjaLookingRight;

    [SerializeField]
    float katanaAngleToTravel;


    [SerializeField]
    bool isKatanaSheathed;
     
    [SerializeField]
    bool canKatanaSwipe;

    [SerializeField]
    float katanaCanCutAngle;

    public AudioSource katanaWindShoutAudio;

    private INinjaInputInterface input;

    public bool IsKatanaSheathed
    {
        get{ return isKatanaSheathed; }
    }

    private void Awake()
    {
        ResetKatana();
    }

    private void Start()
    {
        ResetKatana();
        input = transform.GetComponent<INinjaInputInterface>();
    }

    //When the katana object is activated, start drawing the sword
    public void DoKatanaCut()
    {
        if (isKatanaSheathed)
        {
            MakeKatanaOpaque();
            isKatanaSheathed = false;
            canKatanaSwipe = false;
            CheckNinjaDirection();
            StartCoroutine(DrawTheKatana());
        }
    }
    
    //When the katana object is deactivated, reset the sword dimension
    void OnDisable()
    {
        ResetKatana();
    }

    IEnumerator DrawTheKatana()
    {
        yield return new WaitForFixedUpdate();

         //Lerp the localscale towards katanaDimensions
        transform.localScale = Vector3.Lerp( transform.localScale, katanaDimensions, Time.deltaTime*10f);


        if ( Mathf.Abs (transform.localScale.y - katanaDimensions.y) < 0.15f * katanaDimensions.y)
            transform.localScale = katanaDimensions ;

        transform.localScale = new Vector3(katanaDimensions.x, transform.localScale.y, transform.localScale.z);

        if (transform.localScale.y > 0.99f * katanaDimensions.y)
        {
            canKatanaSwipe = true;
        }
        else
            StartCoroutine(DrawTheKatana());
    }

    [PunRPC]
    void SwipeTheKatanaNetwork()//Is called in FixedUpdate if the katana can swipe (DrawTheKatana coroutine is complete) and if the mouse button is not held down 
    {
        katanaWindShoutAudio.Play();
        StartCoroutine(WindKatanaBeforeSwipe());
    }

    [SerializeField]
    private int windedKatanaAngleCurrent = 0;
    public float windedKatanaAngleMax;
    [SerializeField]
    private bool katanaIsWinding;
    public float waitAfterWindingBeforeSwiping;
    IEnumerator WindKatanaBeforeSwipe()
    {
        if (isNinjaLookingRight)
        {
            if (canKatanaSwipe && katanaIsWinding == false && windedKatanaAngleCurrent == 0)
            {
                canKatanaSwipe = false;
                katanaIsWinding = true;
                MakeKatanaOpaque();

                //Enable PARTICLES
                foreach (ParticleSystem trails in GetComponentsInChildren<ParticleSystem>())
                    trails.Play(true);

                transform.Rotate(new Vector3(0, 0, 3f));
                windedKatanaAngleCurrent += 3;
                yield return new WaitForFixedUpdate();
                StartCoroutine(WindKatanaBeforeSwipe());
            }
            else if (katanaIsWinding && windedKatanaAngleCurrent > 2 && windedKatanaAngleCurrent < windedKatanaAngleMax)
            {
                transform.Rotate(new Vector3(0, 0, 3f));
                windedKatanaAngleCurrent += 3;
                yield return new WaitForFixedUpdate();
                StartCoroutine(WindKatanaBeforeSwipe());
            }
            else if (katanaIsWinding && windedKatanaAngleCurrent > (windedKatanaAngleMax-1))
            {
                transform.Rotate(new Vector3(0, 0, 3f));
                windedKatanaAngleCurrent = 0;
                katanaIsWinding = false;
                //Wait at wind ending position for natural look.
                yield return new WaitForSeconds(waitAfterWindingBeforeSwiping);
                SwipeTheKatanaWhenWindingIsCompleteOnLocal();
            }
            else
            {
                yield return new WaitForFixedUpdate();
                StartCoroutine(WindKatanaBeforeSwipe());
            }
        }
        else
        {
            if (canKatanaSwipe && katanaIsWinding == false && windedKatanaAngleCurrent == 0)
            {
                canKatanaSwipe = false;
                katanaIsWinding = true;
                MakeKatanaOpaque();

                //Enable PARTICLES
                foreach (ParticleSystem trails in GetComponentsInChildren<ParticleSystem>())
                    trails.Play(true);

                transform.Rotate(new Vector3(0, 0, -3f));
                windedKatanaAngleCurrent -= 3;
                yield return null;
                StartCoroutine(WindKatanaBeforeSwipe());
            }
            else if (katanaIsWinding && windedKatanaAngleCurrent < -2 && windedKatanaAngleCurrent > -windedKatanaAngleMax)
            {
                transform.Rotate(new Vector3(0, 0, -3f));
                windedKatanaAngleCurrent -= 3;
                yield return null;
                StartCoroutine(WindKatanaBeforeSwipe());
            }
            else if (katanaIsWinding && windedKatanaAngleCurrent < -(windedKatanaAngleMax-1))
            {
                transform.Rotate(new Vector3(0, 0, -3f));
                windedKatanaAngleCurrent = 0;
                katanaIsWinding = false;
                //Wait at wind ending position for natural look.
                yield return new WaitForSeconds(waitAfterWindingBeforeSwiping);
                SwipeTheKatanaWhenWindingIsCompleteOnLocal();
            }
            else
            {
                yield return null;
                StartCoroutine(WindKatanaBeforeSwipe());
            }
        }
    }

    public AudioSource katanaSwingMetalAudio;

    void SwipeTheKatanaWhenWindingIsCompleteOnLocal()//Call WindKatanaBeforeSwipe() coroutine before this one. 
    {
        katanaSwingMetalAudio.Play();
            
            //Enable PARTICLES
            foreach (ParticleSystem trails in GetComponentsInChildren<ParticleSystem>())
                trails.Play(true);

            GetComponent<Rigidbody2D>().angularVelocity = katanaAngularSwipeSpeed;
    }

    IEnumerator SheatheTheKatana()//Reset the katana and deactivate it for sheathing
    {
        canKatanaSwipe = false;

        //STOP PARTICLES
        foreach (ParticleSystem trails in GetComponentsInChildren<ParticleSystem>())
            trails.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield return new WaitForFixedUpdate();

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.15f);

        if (transform.localScale.y < 0.25f )
            transform.localScale = Vector3.zero;

        if (transform.localScale.y < 0.1f)
            ResetKatana();
        else
            StartCoroutine(SheatheTheKatana());
    }

    void ResetKatana()
    {
        GetComponent<Collider2D>().enabled = false;
        transform.localScale = new Vector3(0, 0, 0);

        //STOP PARTICLES
        foreach (ParticleSystem trails in GetComponentsInChildren<ParticleSystem>() )
            trails.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        isKatanaSheathed = true;
        canKatanaSwipe = false;
    }

    private void FixedUpdate()
    {
        if(canKatanaSwipe == true)
          CheckNinjaDirection();

        if (input.GetMouseButton(0) == false && canKatanaSwipe && GetComponent<PhotonView>().IsMine)
            GetComponent<PhotonView>().RPC("SwipeTheKatanaNetwork", RpcTarget.All);

        transform.position = transform.parent.position;

        float currentAngle = AngleTraveled (Mathf.Abs(transform.rotation.eulerAngles.z - katanaRotationAtStart.z) );


        if ( currentAngle > katanaCanCutAngle)
            GetComponent<Collider2D>().enabled = true;


        //TODO Check if the katana reached target rotation
        if ( currentAngle > katanaAngleToTravel && Mathf.Abs( GetComponent<Rigidbody2D>().angularVelocity ) > 1f ) //Katana is close enough to target rotation (we add +90f angle since the default angle difference to aimcrosshair is 90 angles)
        {
            GetComponent<Rigidbody2D>().angularVelocity = 0f;
            StartCoroutine(SheatheTheKatana());
        }
    }

    float AngleTraveled(float currentAngle)
    {
        if (currentAngle > 180f)
            return (360f - currentAngle);
        else
            return currentAngle;
    }

    void CheckNinjaDirection()
    {
        if(headBand.GetComponent<SpriteRenderer>().flipX == false)
        {
            isNinjaLookingRight = true;
            transform.rotation = Quaternion.Euler(0, 0, katanaAngleForStart);
            transform.localScale = new Vector3(katanaDimensions.x, transform.localScale.y, transform.localScale.z);
            katanaAngularSwipeSpeed = -1f * Mathf.Abs(katanaAngularSwipeSpeed);
        }
        else
        { 
            isNinjaLookingRight = false;
            transform.rotation = Quaternion.Euler(0, 0, -1f * katanaAngleForStart);
            transform.localScale = new Vector3(-1f * katanaDimensions.x, transform.localScale.y, transform.localScale.z);
            katanaAngularSwipeSpeed = Mathf.Abs(katanaAngularSwipeSpeed);
        }
        //Than ninja is looking right, set katana angle and scale x to positive, otherwise set to negative
    }

    bool increaseKatanaAlpha;

    private void Update()
    {
        if (canKatanaSwipe == true)
        {
            Color tempCol = GetComponent<SpriteRenderer>().color;
            if (tempCol.a < 0.45f)
                increaseKatanaAlpha = true;
            else if (tempCol.a > 0.96f)
                increaseKatanaAlpha = false;

            if (increaseKatanaAlpha)
                tempCol.a += 0.02f;
            else
                tempCol.a -= 0.02f;

            GetComponent<SpriteRenderer>().color = tempCol;
        }
    }

    void MakeKatanaOpaque()
    {
        Color tempCol = GetComponent<SpriteRenderer>().color;
        tempCol.a = 1f;
        GetComponent<SpriteRenderer>().color = tempCol;
    }

    private void OnEnable()
    {
        //Reset katana initial variables in case it was about to be drawn at moment of death
        ResetKatana();
    }
}
