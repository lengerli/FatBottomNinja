using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon;
using UnityEngine;

public class WeaponDamage : MonoBehaviour {
    [SerializeField]
    float damageAmount;

    [SerializeField]
    GameObject bloodPrefab;

    [SerializeField]
    GameObject ownerNinja;

    
    public GameObject OwnerNinja    
    {
       get { return ownerNinja; }
       set { ownerNinja = value; }
    }
	
    IEnumerator DelayedDestroyThisNinjaStar()
    {
        yield return new WaitForSeconds(5f);
        DestroyThisNinjaStar(false);
    }


	void OnTriggerEnter2D (Collider2D other) 
    {
        if (OwnerNinja.GetComponent<PhotonView>().IsMine)            /* A weapon can deal damage, if only it is fired by a local player*/
            DealDamageInTheNameOfLocalNinjaShooter(other);
        else
            PretendDamageForCopycatNinjaShooter(other);
    }

    void DealDamageInTheNameOfLocalNinjaShooter(Collider2D vurulanNinjaVeyaObje)
    {
        if (gameObject.tag == "Katana" && IsThisA_NinjaHit(vurulanNinjaVeyaObje.gameObject) && vurulanNinjaVeyaObje.gameObject != gameObject.transform.parent.gameObject)
        {
            vurulanNinjaVeyaObje.gameObject.GetComponent<HealthControl>().DecreaseHealth(damageAmount, ownerNinja.GetComponent<PhotonView>().ViewID);
            ShedBlood(vurulanNinjaVeyaObje.gameObject.transform.position);
        }
        else if (gameObject.tag == "NinjaStar")
        {
            if (IsThisA_NinjaHit(vurulanNinjaVeyaObje.gameObject))
            {
                vurulanNinjaVeyaObje.gameObject.GetComponent<HealthControl>().DecreaseHealth(damageAmount, ownerNinja.GetComponent<PhotonView>().ViewID);
                DestroyThisNinjaStar(true);
            }
            else
            {
                //Stop the star movement and rotation
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                //Stop the star movement and rotation by disabling NinjaStarController
                GetComponent<WeaponSelfController>().enabled = false;
                StartCoroutine(DelayedDestroyThisNinjaStar());
            }

        }
    }

    void PretendDamageForCopycatNinjaShooter (Collider2D vurulanNinjaVeyaObje)
    {
        if (gameObject.name.Contains("Katana") == false)
        {
            if (IsThisA_NinjaHit(vurulanNinjaVeyaObje.gameObject))
                DestroyThisNinjaStar(vurulanNinjaVeyaObje.tag.Contains("Player"));
            else
            {                 //Stop the star movement and rotation
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                //Stop the star movement and rotation by disabling NinjaStarController
                GetComponent<WeaponSelfController>().enabled = false;
                StartCoroutine(DelayedDestroyThisNinjaStar());
            }
        }
        else if(vurulanNinjaVeyaObje.gameObject.tag.Contains("Player") && vurulanNinjaVeyaObje.gameObject.name != transform.parent.gameObject.name)
            ShedBlood(vurulanNinjaVeyaObje.ClosestPoint(transform.position));

    }

    bool IsThisA_NinjaHit(GameObject hitObject)
    {
        if (hitObject.tag == "Player")
        {
            Debug.Log("Ninja Hit: " + hitObject.name +" Parent is: " + gameObject.transform.gameObject.name);
            return true;
        }
        else
        {
            return false;
        }
    }

    void DestroyThisNinjaStar(bool isNinjaHit)
    {

        if (isNinjaHit && bloodPrefab != null)
            ShedBlood(transform.position);

            Destroy(gameObject);
    }

    void ShedBlood(Vector3 bloodDrawPosition)
    {
        GameObject blooddd = Instantiate(bloodPrefab, bloodDrawPosition, transform.rotation);
        blooddd.transform.localScale *= 0.3f;

    }

}
