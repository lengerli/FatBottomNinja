using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public class NinjaSpawnerForPUN : MonoBehaviour
{

    [SerializeField]
    private GameObject ninjaToSpawn;

    [SerializeField]
    float maxSpawnPosX; 
    [SerializeField]
    float maxSpawnPosY;

    Vector3 spawnPosition;

    GameObject spawnedNinja;
    //!!! Reference: Spawning method is called by the buttons in Ninja Choose Menu
    public void SpawnTheNinja(Button callingNinjaColorButton )
    {
        Vector2 safeSpawnPos = GameManager.Instance.GetSafeSpawningPosition();

        spawnPosition = new Vector3(safeSpawnPos.x, safeSpawnPos.y, ninjaToSpawn.transform.position.z);

        spawnedNinja = PhotonNetwork.Instantiate (ninjaToSpawn.name, spawnPosition, ninjaToSpawn.transform.rotation);

        spawnedNinja.GetComponent<NinjaColorControler>().SetNinjaColor(callingNinjaColorButton.colors.normalColor); 
    }

}
