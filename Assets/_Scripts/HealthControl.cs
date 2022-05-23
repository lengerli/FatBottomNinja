 using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon;
using UnityEngine;

public class HealthControl : MonoBehaviour {

	public float healthPoints;
    [SerializeField]
    GameObject bloodBathPrefab;
    
    public bool isNinjaDead;
    

    PhotonView photonView;

    public bool isThisRoomMastersPC;
    public bool isThisLocalNinja;

    public AudioSource hitAudio;
    public AudioSource deathAudio;
 
    // Use this for initialization
    void Start () {
        photonView = GetComponent<PhotonView>();
        isNinjaDead = false;
        isThisRoomMastersPC = PhotonNetwork.IsMasterClient;
        isThisLocalNinja = photonView.IsMine;
        
    }
	
	public void DecreaseHealth(float damage, int shooterPhotonID)	
    {
        //Damage can register only in room master's computer for networking consistency. So we ask the master to count our damage. NOTE: If the ninja we hit is already dead in masters PC, the hit will not register.
            photonView.RPC("RequestDecreaseHealthNetwork", RpcTarget.MasterClient ,damage, shooterPhotonID);
	}

    [PunRPC]
    public void RequestDecreaseHealthNetwork(float damage, int shooterPhotonID)
    {
        if(gameObject.activeSelf == true) //Although an inactive object should not be able to process a method call, maybe PUN can somehow process it. Let's check, just in case.
        {
            RegisterHealthDecrease(damage, shooterPhotonID);
            photonView.RPC("RegisterHealthDecrease", RpcTarget.Others, damage, shooterPhotonID);
        }

    }

    [PunRPC]
    public void RegisterHealthDecrease (float damage, int shooterPhotonID)
    {
        if (healthPoints > 0)
        {
            healthPoints -= damage;
            hitAudio.Play();
            GetComponent<ScoreController>().UpdateHealthBar(healthPoints);
            CheckDeath(shooterPhotonID);
        }
        else if ((healthPoints < 0.01f) && !isNinjaDead)
        {
            GetComponent<ScoreController>().UpdateHealthBar(0);
            CheckDeath(shooterPhotonID);
        }
    }

    void CheckDeath(int shooterPhotonID)
    {
        if (healthPoints < 0.1f && !isNinjaDead)
        {
            isNinjaDead = true;
            DieAndScoreToMyShooter(shooterPhotonID);
        }
    }

    private void DieAndScoreToMyShooter(int shooterPhotonId)
    {
        if ( photonView.ViewID != shooterPhotonId ) //Someone else shot me.
        {
            List<GameObject> ninjas = GameManager.Instance.ninjasOnScene;

            foreach (GameObject theNinja in ninjas)
            {
                if (theNinja.GetComponent<PhotonView>().ViewID == shooterPhotonId)
                {
                    theNinja.GetComponent<ScoreController>().ScoreCurrent++;
                    theNinja.GetComponent<ScoreController>().UpdateScoreDisplay();
                }
            }
        }
        else //You shot yourself dummy..
        {
            GetComponent<ScoreController>().ScoreCurrent--;
            GetComponent<ScoreController>().UpdateScoreDisplay();
        }


        //Time to die
        Instantiate(deathAudio, transform.position, transform.rotation);
        BloodBath();
        GetComponent<NinjaNameController>().theCanvasForName.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void BloodBath()
    {
        Instantiate(bloodBathPrefab, transform.position, transform.rotation);
    }

}
