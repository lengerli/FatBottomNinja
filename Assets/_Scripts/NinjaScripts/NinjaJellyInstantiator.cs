using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaJellyInstantiator : MonoBehaviour
{
    public GameObject jellyDummyPrefab;
    GameObject currentJellyObject;
    public Vector3 jellyScale; // Vector3(5.555555f, 5.555555f, 1f) for regular sized ninja

    //Add jellyprefab when activate (first time spawning or respawning
    private void OnEnable()
    {
        currentJellyObject = Instantiate(jellyDummyPrefab);
        StartCoroutine(InitiateJelly());
    }

    IEnumerator InitiateJelly()
    {
        currentJellyObject.transform.parent = gameObject.transform;
        currentJellyObject.transform.localScale = jellyScale; 
        currentJellyObject.transform.localPosition = Vector3.zero;
        yield return new WaitForFixedUpdate();

        currentJellyObject.SetActive(true);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        currentJellyObject.GetComponent<JellySprite>().Scale(10);
    }

    //When dead, ninja is deactivated. Remove all jelly related components. Because it causes bugs on web browser after respawn. 
    private void OnDisable()
    {
        if (currentJellyObject != null)
        {
            Destroy(GetComponent<JellySpriteReferencePoint>());

            for (int i = 1; i < transform.childCount; i++) //Destroy the jelly main object and the jelly reference points
                if (transform.GetChild(i).gameObject.name.Contains("JellyDummy"))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

            foreach (SliderJoint2D sliderForJelly in GetComponents<SliderJoint2D>())//remove the sliders added to ninja
                Destroy(sliderForJelly);

        }

    }
}
