using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RopeShootControls : MonoBehaviour {


	public GameObject theNinja;
    private INinjaInputInterface inputInterface;
	public bool isRopeShot;
	public Vector2 ropeEndPoint;
	private bool isRopeRayHitAnything;
	public float ropeRange;
	public int ropeStepsForCompletion;
	private int totalRopeStepped = 0;
	public LineRenderer lineRenderer;
	public bool disableShooting;
	public bool isRopeAnchored;

    public GameObject ropeAnchor;

    PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        inputInterface = GetComponent<INinjaInputInterface>();
        ReleaseRopeNetwork();//Release the rope at the start to remove anchor from scene properly
    }

    void Update()
    {
        /** MOUSE CONTROLS FOR ROPE SHOOT AND RELEASE **/
        if (photonView.IsMine && !disableShooting && inputInterface.GetMouseButtonUp(1)) 
        {
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(inputInterface.MousePosition);
            RopeShootOrRelease(targetPos);
            StartCoroutine(DisableTouchFor5Frames());
        }
        else if (photonView.IsMine && !disableShooting && !isRopeShot && !isRopeAnchored)
            ResetRope();
    }

    void FixedUpdate () {
        if (isRopeShot)
        {
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, ropeAnchor.transform.position);
            lineRenderer.SetPosition(0, transform.position);
            SetLineRendererColorVisible(true);
        }
        else
        {
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, theNinja.transform.position);
            lineRenderer.SetPosition(0, theNinja.transform.position);
            SetLineRendererColorVisible(false);
        }
    }

	IEnumerator DisableTouchFor5Frames(){
		disableShooting = true;
		for( int i =0; i<5; i++)
			yield return new WaitForFixedUpdate();
		disableShooting = false;
	}

    void RopeShootOrRelease(Vector3 mouseTargetPos)
    {
        if (!isRopeShot && !isRopeAnchored)
            ShootRope(mouseTargetPos);
        else if (isRopeShot && isRopeAnchored)
        {
            ReleaseRopeNetwork();
            photonView.RPC("ReleaseRopeNetwork", RpcTarget.Others);
        }
    }

	void ShootRope (Vector3 mouseTargetPos)
    {
        ropeAnchor.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

        Vector2 touchPosition = (Vector2) mouseTargetPos;
		isRopeShot = true;
        Vector2 anchorVel = Vector2.right * ropeAnchor.GetComponent<AnchorHitController>().anchorShootVel;
        ropeAnchor.transform.position = transform.position;
        float rotAngle = ropeAnchor.GetComponent<AnchorHitController>().crossHairArm.transform.rotation.eulerAngles.z;
        ropeAnchor.transform.rotation = Quaternion.Euler(0f, 0f,rotAngle);
        ropeAnchor.GetComponent<Rigidbody2D>().velocity = (Vector2)ropeAnchor.transform.TransformDirection(anchorVel);
        ropeAnchor.GetComponent<AnchorHitController>().EnableAnchor();

        photonView.RPC("ShootRopeNetwork", RpcTarget.Others, (Vector2)transform.position, ropeAnchor.transform.rotation.eulerAngles.z);
	}

    [PunRPC]
    public void ShootRopeNetwork( Vector2 anchorStartPos, float rotationAng )//ONLY FOR NON-LOCAL PLAYER
    {
        ropeAnchor.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

        isRopeShot = true;
        Vector2 anchorVel = Vector2.right * ropeAnchor.GetComponent<AnchorHitController>().anchorShootVel;

        ropeAnchor.transform.rotation = Quaternion.Euler(0f, 0f, rotationAng);
        ropeAnchor.transform.position = anchorStartPos;
        ropeAnchor.GetComponent<Rigidbody2D>().velocity = (Vector2)ropeAnchor.transform.TransformDirection(anchorVel);
        ropeAnchor.GetComponent<AnchorHitController>().EnableAnchor();
    }


    public void SetAnchorHit(Vector2 anchorPos){
        //Call this method from AnchorHitController class when following appropriate collision occurs
        //Also isRopeRayHitAnything bool is no longer necessary..
        //Call the AnchorTheRope by network event call

        if (photonView.IsMine)
        {
            ropeEndPoint = anchorPos;
            AnchorTheRopeNetwork(ropeEndPoint);
            photonView.RPC("AnchorTheRopeNetwork", RpcTarget.Others, (Vector2)ropeEndPoint);
        }
	}

    [PunRPC]
    public void AnchorTheRopeNetwork( Vector2 ropeEndPointFinal)
    {
        if (isRopeShot)
        {
            ropeEndPoint = ropeEndPointFinal;
            ropeAnchor.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            ropeAnchor.transform.position = ropeEndPointFinal;
            ropeAnchor.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, ropeEndPointFinal);
            lineRenderer.SetPosition(0, transform.position);
            SetLineRendererColorVisible(true);
            isRopeAnchored = true;
        }
    }


    [PunRPC]
	public void ReleaseRopeNetwork()
    {
        ropeAnchor.GetComponent<AnchorHitController>().DisableAnchor();
		isRopeShot = false;
		isRopeAnchored = false;
		ResetRope();
//		lineRenderer.enabled =false;
		SetLineRendererColorVisible(false);
	}
	public void ResetRope()
    {
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, theNinja.transform.position);
		lineRenderer.SetPosition(1, theNinja.transform.position);
	}

	void SetLineRendererColorVisible(bool isVisible)
    {
		float alphaValue = 0;
		if (isVisible)
			alphaValue = 1;
		
		Color lineStartColorInvisible = lineRenderer.startColor;
		lineStartColorInvisible.a = alphaValue;
		Color lineEndColorInvisible = lineRenderer.startColor;
		lineEndColorInvisible.a = alphaValue;

		lineRenderer.startColor = lineStartColorInvisible;
		lineRenderer.endColor = lineEndColorInvisible;
	}

    private void OnEnable() //In case during a death the rope was just shot, disable shooting might not be able to finish DisableShootFor5Frames coroutine.
    {
        ropeAnchor.GetComponent<AnchorHitController>().DisableAnchor();
        isRopeShot = false;
        isRopeAnchored = false;
        ResetRope();
        disableShooting = false;

    }
}
