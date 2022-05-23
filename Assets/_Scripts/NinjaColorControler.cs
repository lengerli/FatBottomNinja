using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

/*Summary: All the color related issues are controlled by this component.
 Not only the color of the local player, but also avatar on other players' computers are controlled here.
 Also keep in mind, ninjaRadar or Score related colors are referenced here.

 How it works: When NinjaSpawnerForPUN instantiates a ninja, it sets the color according to color chosing button.
 This color setting action is done right after the instantiation. 
 When a ninja is instantiated on another player's computer (when ninja is not local) it should seek its color from the original ninja player's computer.*/


public class NinjaColorControler : MonoBehaviour
{
    public PhotonView photonView;

    public SpriteRenderer headBandSprite;

    [SerializeField]
    private Color ninjaColor;

    public GameObject ninjaRadar;
    private GameObject myRadarBlip;

    public ScoreController scoreContr;

    private bool isNinjaColorSet = false;

    public void SetNinjaColor(Color chosenNinjaColor)
    {
        /* Ninjanın kendi color variable'ı dışında, bu variable'ın sahneye uygulanması da
        * bu metodla kontrol ediliyor ( radar, healthbar, score vs gibi componentlerdeki renkleri)*/

        if(isNinjaColorSet == false)
        {
            isNinjaColorSet = true;
            ninjaColor = chosenNinjaColor;
            headBandSprite.color = ninjaColor;
            ninjaRadar = GameObject.FindWithTag("NinjaRadar");
            StartCoroutine(DelayedSetScoreDisplayColor());

            if (photonView.IsMine == false)
                AddRadarBlipWithMyColor(ninjaColor);
            //Now that the original ninja is instantiated, it should set the color for its copies on other computers
            else if (photonView.IsMine == true)
            {
                float[] ninjaRGB = new float[] {ninjaColor.r,ninjaColor.g,ninjaColor.b };
                photonView.RPC("SynchColorsForNinjaCopies", RpcTarget.Others, ninjaRGB);
                ninjaRadar.GetComponent<NinjaOffCameraRadar>().localNinja = gameObject;
            }
        }
    }

    //Wait one frame so that ninja is instantiated properly before setting display color variable. So that we can avoid NullReference errors
    IEnumerator DelayedSetScoreDisplayColor()
    {
        yield return new WaitForEndOfFrame();
        scoreContr.scoreDisplayText.color = ninjaColor;
    }

    [PunRPC]
    public void SynchColorsForNinjaCopies(float[] ninjaRGB)
    {
        Color chosenColorFromOriginalNinja = new Color (ninjaRGB[0], ninjaRGB[1], ninjaRGB[2]);
        SetNinjaColor(chosenColorFromOriginalNinja);
    }

    void AddRadarBlipWithMyColor(Color myColorrr)
    {
        if (photonView.IsMine == false)
            myRadarBlip = ninjaRadar.GetComponent<NinjaOffCameraRadar>().AddBlipForThisNinja(myColorrr, gameObject);
    }

    private void OnDestroy()
    {
        ninjaRadar.GetComponent<NinjaOffCameraRadar>().RemoveMyBlip(myRadarBlip);
    }
}
