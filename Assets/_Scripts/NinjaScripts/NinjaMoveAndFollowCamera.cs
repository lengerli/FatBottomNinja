using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NinjaMoveAndFollowCamera : MonoBehaviour {


    /// IMPORTANT NOTE: JELLY DUMMY IS CREATED AND CONTROLLED BY HEALTCHCONTROL SCRIPT

	[SerializeField]
	float moveForceConstant;
	[SerializeField]
	float maxVelocityNinja;
	[SerializeField]
	float jumpForceConstant;

    [SerializeField]
    Vector2 horizontalJumpDirectionVector;

    [SerializeField]
    float horizontalJumpInterval;

    [SerializeField]
    float horizontalForceApplyingInterval;

    [SerializeField]
    float assDistance;

    public bool canMoveRight;
    public bool canMoveLeft;
    public bool canMoveUp;
    public bool canMoveDown;

    private bool isNinjaOnVerticalJump;

    public bool assIsTouchingGround;

    Vector2 priorPosition;
    PhotonView photonView;

    public PhysicsMaterial2D localCopyMaterialNoBounce;

    public Vector2 calculatedVelocity = Vector2.zero;
    public float calculatedVelocityDividerCoefficient;
    public bool useCalculatedVelocityForOffline;

    public float noInputTimeLimit;
    public float noInputTimeCounter = 0;

    private Rigidbody2D ninjaRigidBody;
    private RopeShootControls ropeShootControls;


    public AudioSource hopAudio;
    public AudioSource jumpAudio;

    private INinjaInputInterface inputComponent;
    void Start()    
    {
        inputComponent = GetComponent<INinjaInputInterface>();

        ninjaRigidBody = GetComponent<Rigidbody2D>();

        ropeShootControls = GetComponent<RopeShootControls>();

        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine == false)
        {
            priorPosition = transform.position;
            ninjaRigidBody.sharedMaterial = localCopyMaterialNoBounce;
        }
        else
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>().TheNinja = gameObject;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            CheckPlayerInputForMove();
            ApplyForceWithInput();
            ninjaRigidBody.velocity = Vector2.ClampMagnitude(ninjaRigidBody.velocity, maxVelocityNinja);
        }
    }

    void CheckPlayerInputForMove()
    {
        if (inputComponent.GetKeyUp(KeyCode.Tab) && assIsTouchingGround && !isNinjaOnVerticalJump)
            JumpTheNinja();
        else 
        {
            bool isThereInput = false;

            #region *** KeyBoard Movement Inputs ***

            if (inputComponent.GetKeyDown(KeyCode.D) || inputComponent.GetKey(KeyCode.D))
            {
                canMoveRight = true;
                noInputTimeCounter = 0;
                isThereInput = true;
            }

            else 
                canMoveRight = false;
            if (inputComponent.GetKeyDown(KeyCode.A) || inputComponent.GetKey(KeyCode.A))
            {
                canMoveLeft = true;
                noInputTimeCounter = 0;
                isThereInput = true;
            }
            else 
                canMoveLeft = false;
            if (inputComponent.GetKeyDown(KeyCode.S) || inputComponent.GetKey(KeyCode.S))
            {
                canMoveDown = true;
                noInputTimeCounter = 0;
                isThereInput = true;
            }
            else 
                canMoveDown = false;
            if ((inputComponent.GetKeyDown(KeyCode.W) || inputComponent.GetKey(KeyCode.W)) && !assIsTouchingGround && ropeShootControls.isRopeAnchored)
            {
                canMoveUp = true;
                noInputTimeCounter = 0;
                isThereInput = true;
            }
            else 
                canMoveUp = false;

            #endregion *** KeyBoard Movement Inputs ***

            if (!isThereInput && noInputTimeCounter < noInputTimeLimit)
                noInputTimeCounter += Time.fixedDeltaTime;
            else if (noInputTimeCounter > noInputTimeLimit)
            {
                canMoveRight = false;
                canMoveLeft = false;
                canMoveUp = false;
                canMoveDown = false;

                noInputTimeCounter = 0;
            }
        }
    }

    public float timeSinceLastJump = 0f;

    void ApplyForceWithInput()
    {
        if (canMoveRight)
            ApplyHorizontalForce(true);
        if (canMoveLeft)
            ApplyHorizontalForce(false);
        if (canMoveDown)
            ninjaRigidBody.AddForce(moveForceConstant * Vector3.down, ForceMode2D.Force);
        if (canMoveUp)
            ninjaRigidBody.AddForce(moveForceConstant/2f * Vector3.up, ForceMode2D.Force);

        if (timeSinceLastJump < horizontalForceApplyingInterval || timeSinceLastJump < horizontalJumpInterval)
            timeSinceLastJump += Time.smoothDeltaTime;
    }

    void ApplyHorizontalForce(bool isRightDirection)
    {
        //Set direction vector for right jump
        Vector2 moveDirectionVector = Vector2.right;
        Vector2 jumpDirectionVector = horizontalJumpDirectionVector;

        //Set direction vector for left jump
        if (!isRightDirection)
        {
            moveDirectionVector *= -1f;
            jumpDirectionVector = new Vector2(-1f*horizontalJumpDirectionVector.x, horizontalJumpDirectionVector.y);
        }

        //Apply horizontal jump force in the direction
        if (assIsTouchingGround && timeSinceLastJump > horizontalJumpInterval)
        {
            timeSinceLastJump = 0f;
            ninjaRigidBody.AddForce(jumpForceConstant / 4f * jumpDirectionVector, ForceMode2D.Impulse);
            hopAudio.Play();
        }
        //Apply force for movement in air (not jump)
        else if (!assIsTouchingGround && timeSinceLastJump > horizontalForceApplyingInterval)
        {
            if( ropeShootControls.isRopeAnchored )
                ninjaRigidBody.AddForce(moveForceConstant * moveDirectionVector, ForceMode2D.Force);
            else
                ninjaRigidBody.AddForce(moveForceConstant * moveDirectionVector/2f, ForceMode2D.Force);
        }
    }

    void JumpTheNinja()
    {
        isNinjaOnVerticalJump = true;
        ninjaRigidBody.AddForce(jumpForceConstant * Vector2.up, ForceMode2D.Impulse);
        timeSinceLastJump = 0f;
        jumpAudio.Play();
        StartCoroutine( NinjaIsOnJump() );
    }

    IEnumerator NinjaIsOnJump()
    {
        yield return new WaitForSeconds(1.2f);
        isNinjaOnVerticalJump = false;
    }

    public void WeaponBlastDelayed(Vector2 blastForce)
    {
        StartCoroutine ( WeaponBlast (blastForce) );
    }

    IEnumerator WeaponBlast(Vector2 blastforce)
    {
        yield return new WaitForSeconds(0.01f);
        ninjaRigidBody.AddForce(blastforce, ForceMode2D.Impulse);
    }

    private void OnEnable()
    {
        isNinjaOnVerticalJump = false;
    }
}
