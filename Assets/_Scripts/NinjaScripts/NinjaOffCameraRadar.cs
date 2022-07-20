using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaOffCameraRadar : MonoBehaviour
{
    public List<GameObject> ninjaRadarBlipList;
    public GameObject radarBlipPrefab;

    public GameObject localNinja;
    public Transform cameraTransform;

    public float allowableDistanceX;
    public float allowableDistanceY;

    public float radarRadius;

    void Awake()
    {
        localNinja = Camera.main.gameObject;
        //This awake method sets localNinja as camera to avoid null ref exception until the player choses the ninja color and spawns the actual local ninja
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (ninjaRadarBlipList != null && ninjaRadarBlipList.Count > 0)
        {
            PositionAndRotateRadarBlipsForAllNinjas();
        }
    }

    public GameObject AddBlipForThisNinja(Color ninjaCol, GameObject thisNinja)//Called by non-local ninjas
    {
        GameObject newBlip = Instantiate(radarBlipPrefab, Vector3.zero, Quaternion.identity);
        newBlip.transform.parent = thisNinja.transform;
        newBlip.GetComponent<SpriteRenderer>().color = ninjaCol;
        ninjaRadarBlipList.Add(newBlip);
        return newBlip;
    }

    public void RemoveMyBlip(GameObject blipToRemove)//Called by non-local ninjas's OnDestroy() method
    {
        foreach(GameObject blip in ninjaRadarBlipList)
        {
            if (blip.Equals(blipToRemove))
            {
                int blipIndex = ninjaRadarBlipList.IndexOf(blip);
                ninjaRadarBlipList.Remove(blip);
            }
        }
    }

    void PositionAndRotateRadarBlipsForAllNinjas()
    {
        
        foreach (GameObject blip in ninjaRadarBlipList)
        {
            if (blip != null)
            {

                Vector3 distanceToLocal = blip.transform.parent.position - localNinja.transform.position;
                Vector3 distanceToCamera = blip.transform.parent.position - cameraTransform.position;

                Vector2 distanceAbs = new Vector2 ( Mathf.Abs(distanceToCamera.x), Mathf.Abs(distanceToCamera.y));

                if (distanceAbs.x > allowableDistanceX || distanceAbs.y > allowableDistanceY ) //than the related ninja is out of camera view
                { 
                    //Set blip position
                    blip.GetComponent<SpriteRenderer>().enabled = true;
                    distanceToLocal = Vector3.ClampMagnitude(distanceToLocal, radarRadius);
                    blip.transform.position = localNinja.transform.position + distanceToLocal;


                    //Set blip rotation
                    blip.transform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, (Vector2)distanceToLocal));
                }
                else
                    blip.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
                ninjaRadarBlipList.Remove(blip);
        }
    }



}
