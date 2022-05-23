using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    #region Singleton Decleration
    private static GameManager instance;
    internal static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "GameManager";
                    instance = obj.AddComponent<GameManager>();
                }
            }

            return instance;
        }
    }
    #endregion

    public float ninjaGameWonScale;
    public GameObject ninjaPrefab;

    public List<GameObject> ninjasOnScene;
    public Vector3 defaulNinjaLocalScale;
    public float defaultNinjaHealthPoints;
    public Vector3 defaulNinjaHealthbarLocalScale;
    public int defaultNinjaScore;
    
    public int scoreToWin;

    public float maxSpawnPosX, maxSpawnPosY;

    public bool isGameWaitingToRestart;

    public Canvas restartButtonCanvas;

    public GameObject localPlayer;
    bool isLocalPlayerFound;

    public Text[] scorePanelTexts;
    public List<Text> scoreTextsAssigned;

    public GameObject optionsMenuCanvas;
    public Button[] ButtonsNotCrosshairClickable;

    public GameObject restartCountdownAnimation;

    public Text pingDisplayText;
    float pingResetTime = 0;
    int pingAverage = 0;

    public GameObject restartAvatarAndObjects;
    public SpriteRenderer restartAvatarHeadband;

    void Start()
    {
        GetScoreTexts();
        StoreDefaultNinjaVariablesToUseAtRestart(ninjaPrefab);

        ButtonsNotCrosshairClickable = optionsMenuCanvas.GetComponentsInChildren<Button>();
    }

    public void AddNewNinja(GameObject ninjaNew)
    {
        ninjasOnScene.Add(ninjaNew);
    }

    void StoreDefaultNinjaVariablesToUseAtRestart(GameObject ninjaGeneric)
    {
        defaulNinjaLocalScale = ninjaGeneric.transform.localScale;

        defaultNinjaHealthPoints = ninjaGeneric.GetComponent<HealthControl>().healthPoints;

        defaulNinjaHealthbarLocalScale = ninjaGeneric.GetComponent<ScoreController>().healthBarSelf.transform.localScale;

        defaultNinjaScore = ninjaGeneric.GetComponent<ScoreController>().ScoreCurrent;
    }

    public void RemoveDeadNinja(GameObject ninjaDead)
    {
        ninjasOnScene.Remove(ninjaDead);
    }



    // Update is called once per frame
    void Update()
    {
        pingResetTime += Time.deltaTime;

        if (pingResetTime > 0.75f)
        {
            pingResetTime = 0f;
            DisplayAndResetPing();
        }
        else
            CalculatePing();

        if (ninjasOnScene != null && isGameWaitingToRestart == false)
        {
            CheckGameOver();
            CheckDeadNinjaToRespawn();

            if (!isLocalPlayerFound)
                GetLocalPlayer();
        }

        if (Input.GetKey(KeyCode.Escape))
            SceneManager.LoadScene(0);

        CheckCursorAndButtonVisibilityByInput();
    }

    void CalculatePing()
    {
        if (pingAverage > 0.001f)
            pingAverage = (PhotonNetwork.GetPing() + pingAverage) / 2;
        else
            pingAverage = PhotonNetwork.GetPing();
    }

    void DisplayAndResetPing()
    {
        pingDisplayText.text = "average ping: " + pingAverage;
        pingAverage = 0;
    }


    void CheckCursorAndButtonVisibilityByInput()
    {
        //Press the "M" key to apply no locking to the Cursor
        if ( Input.GetKey(KeyCode.M) && (Cursor.lockState == CursorLockMode.Locked || Cursor.visible == false))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            MakeMenuButtonsEnabled();
        }
        else if (Cursor.lockState == CursorLockMode.Locked || Cursor.visible == false)
            MakeMenuButtonsDisable();
    }

    void MakeMenuButtonsEnabled()
    {
        for (int i = 0; i < ButtonsNotCrosshairClickable.Length; i++)
        {
            ButtonsNotCrosshairClickable[i].interactable = true;
        }
    }

    void MakeMenuButtonsDisable()
    {
        for (int i = 0; i < ButtonsNotCrosshairClickable.Length; i++)
        {
            ButtonsNotCrosshairClickable[i].interactable = false;
        }
    }

    void GetScoreTexts()
    {
        Object[] canvases = Object.FindObjectsOfType<Canvas>();

        Canvas scoreCanvas = new Canvas();

        foreach (Canvas xvas in canvases)
        {
            if (xvas.name.Contains("ScoreBoard"))
                scoreCanvas = xvas;
        }

        GameObject scorePanel = scoreCanvas.transform.GetChild(0).gameObject;
        scorePanelTexts = scorePanel.transform.GetComponentsInChildren<Text>();

        foreach (Text x in scorePanelTexts)
            scoreTextsAssigned.Add(x);
    }

    void CheckGameOver()
    {
            foreach (GameObject ninja in ninjasOnScene)
            {
                if (ninja != null && ninja.GetComponent<ScoreController>().ScoreCurrent == scoreToWin)
                {
                    isGameWaitingToRestart = true;
                    GameWonBy(ninja);
                    break;
                }
            }
    }

    void CheckDeadNinjaToRespawn()
    {
        foreach (GameObject ninja in ninjasOnScene)
        {
            if (ninja != null && !isGameWaitingToRestart && ninja.GetComponent<HealthControl>().isNinjaDead)
                StartCoroutine(RespawnThis(ninja,2.5f));            
        }
    }

    void GameWonBy(GameObject characterToWin)
    {
        isGameWaitingToRestart = true;
        characterToWin.GetComponent<GameWonActions>().GameWonNetworkSync();
        restartButtonCanvas.gameObject.SetActive(true);
        restartAvatarHeadband.color = characterToWin.GetComponent<NinjaColorControler>().headBandSprite.color;
        restartAvatarAndObjects.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, restartAvatarAndObjects.transform.position.z);
        restartAvatarAndObjects.SetActive(true);
    }

    public void RestartGame()
    {
        localPlayer.SetActive(true);
        localPlayer.GetComponent<ConnectToGameManager>().CallRestartOnGameManagerNW();
    }

    public void RestartGameNW()
    {
        Debug.Log("restart is called by winner");
        restartButtonCanvas.gameObject.SetActive(false);
        StartCoroutine(RestartCountdown());
    }

    IEnumerator RestartCountdown()
    {
        restartCountdownAnimation.SetActive(true);
        yield return new WaitForSeconds(4.2f);
        restartCountdownAnimation.SetActive(false);
        StartTheRestart();
    }

    void StartTheRestart()
    {
        //TODO Add audio clip here (the gong)

        restartAvatarAndObjects.SetActive(false);

        foreach (GameObject ninja in ninjasOnScene)
        {
            if (ninja != null)
            {
                ninja.transform.localScale = defaulNinjaLocalScale;
                ninja.GetComponent<RopeShootControls>().enabled = true;
                ninja.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
                ninja.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

                ninja.GetComponent<HealthControl>().healthPoints = defaultNinjaHealthPoints;
   
                ninja.GetComponent<ScoreController>().healthBarSelf.transform.localScale = defaulNinjaHealthbarLocalScale;

                ninja.GetComponent<ScoreController>().ScoreCurrent = defaultNinjaScore;
                ninja.GetComponent<ScoreController>().UpdateScoreDisplay();
                ninja.GetComponent<HealthControl>().isNinjaDead = false;

                ninja.GetComponent<RopeShootControls>().ReleaseRopeNetwork();
                ninja.GetComponent<RopeShootControls>().ResetRope();

                Vector2 spawnPosition = GetSafeSpawningPosition();
                ninja.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, ninja.transform.position.z);

                ninja.SetActive(true);
            }
        }
        isGameWaitingToRestart = false;
    }

    IEnumerator RespawnThis(GameObject ninja, float waitToRespawnSeconds)
    {
        yield return new WaitForSeconds(waitToRespawnSeconds);

        if (ninja.activeSelf == false)
        {
            Vector2 spawnPosition = GetSafeSpawningPosition();
            ninja.transform.position = new Vector3( spawnPosition.x, spawnPosition.y, ninja.transform.position.z);

            ninja.GetComponent<HealthControl>().isNinjaDead = false;

            ninja.GetComponent<HealthControl>().healthPoints = 100f;

            ninja.SetActive(true);

            ninja.GetComponent<ScoreController>().UpdateHealthBar(ninja.GetComponent<HealthControl>().healthPoints);
        }
    }

    int spawnPosTrialNum = 0;

    public Vector2 GetSafeSpawningPosition()
    {
        spawnPosTrialNum++;
        Vector2 spawnPos;

        float posX = Random.Range(-maxSpawnPosX, maxSpawnPosX);
        float posY = Random.Range(-maxSpawnPosY, maxSpawnPosY);
        spawnPos = new Vector2(posX, posY);

        
        if (spawnPosTrialNum < 10 && Physics2D.OverlapPoint(spawnPos) != null && Physics2D.OverlapPoint(spawnPos).gameObject.tag != "Player")
            spawnPos = GetSafeSpawningPosition();

        spawnPosTrialNum = 0;

        return spawnPos;
    }


    public void RelocateThisNinja (GameObject ninja)
    {
        if (ninja.activeSelf == true)
        {
            float posX = Random.Range(-maxSpawnPosX, maxSpawnPosX);
            float posY = Random.Range(-maxSpawnPosY, maxSpawnPosY);
            ninja.transform.position = new Vector3(posX, posY, ninja.transform.position.z);
        }
    }

    void GetLocalPlayer()
    {        
            foreach (GameObject ninja in ninjasOnScene)
            {
                if (ninja.GetComponent<PhotonView>().IsMine)
                {
                     isLocalPlayerFound = true;
                    localPlayer = ninja;
                    break;
                }
            }        
    }

    public Text AssignScoreBoardText(GameObject demandingNinja)
    {
        Text returnText = scoreTextsAssigned[0];
        scoreTextsAssigned.RemoveAt(0);
        return returnText;
    }


}
