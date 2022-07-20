using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField]
    NinjaColorControler colorController;

    [SerializeField]
    int scoreCurrent = 0;
    int scorePrior = 0;

    public GameObject healthBar;
    public GameObject healthBarSelf;

    public GameObject scoreImages;

    public GameManager gameManager;

    public Text scoreDisplayText;

    PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        gameManager = GameManager.Instance;

        GetScoreDisplayText();
    }

    void GetScoreDisplayText()
    {
        scoreDisplayText = gameManager.AssignScoreBoardText(gameObject).GetComponent<Text>();
        scoreDisplayText.text = GetComponent<NinjaNameController>().displayName + ": 0";
    }

    public int ScoreCurrent
    {
        get { return scoreCurrent; }
        set { scoreCurrent = value; }
    }

    public void UpdateHealthBar(float currentHealth) 
    {
            healthBarSelf.transform.localScale = new Vector3(currentHealth / 10f, healthBarSelf.transform.localScale.y, 0);
    }
    public void UpdateScoreDisplay()
    {
            UpdateScoreDisplayDeathMatch();
    }

    void UpdateScoreDisplayDeathMatch()
    {
        scoreDisplayText.text = GetComponent<NinjaNameController>().displayName + ": " + scoreCurrent;
    }

    private void OnDestroy()
    {
       
    }

}
