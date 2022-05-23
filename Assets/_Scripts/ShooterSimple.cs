using Photon.Pun;
using UnityEngine;

public class ShooterSimple : MonoBehaviour {


    [SerializeField]
	private GameObject[] weaponList;//0 katana, 1 ninjaStar (since there are not many weapons, we dont need struct

    private int currentWeapon = 0;

    [SerializeField]
    GameObject crossHair;

    [SerializeField]
    GameObject localThreePointCrosshair_MidPointObject;

    [SerializeField]
    GameObject weaponTitles;

    [SerializeField]
    GameObject weaponStatusBars;

    GameObject weaponTitlesDisplayed;
    GameObject weaponStatusBarsDisplayed;

    public float currentCrosshairAngle = 0;
    
    public float[] shootTimeIntervals;
    public float[] reloadTimeIntervals;
    public int[] shotsLeft;


    PhotonView photonView;

    public CrossHairMovement crosshairMover;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        weaponTitlesDisplayed = Instantiate(weaponTitles);
        weaponStatusBarsDisplayed = Instantiate(weaponStatusBars);
        weaponTitlesDisplayed.transform.GetChild(0).gameObject.SetActive(true);
        weaponTitlesDisplayed.transform.GetChild(1).gameObject.SetActive(true);
        weaponTitlesDisplayed.transform.GetChild(2).gameObject.SetActive(true);
        weaponTitlesDisplayed.transform.GetChild(3).gameObject.SetActive(true);

        if (photonView.IsMine)
        {
            weaponTitlesDisplayed.transform.parent = Camera.main.transform;
            weaponStatusBarsDisplayed.transform.parent = Camera.main.transform;
        }
        else
        {
            weaponTitlesDisplayed.layer = LayerMask.NameToLayer("InvisibleToAllLayer") ;
            foreach (Transform theTransform in weaponTitlesDisplayed.transform)          
                theTransform.gameObject.layer = LayerMask.NameToLayer("InvisibleToAllLayer");
            
            weaponStatusBarsDisplayed.layer = LayerMask.NameToLayer("InvisibleToAllLayer");
            foreach (Transform theTransform in weaponStatusBarsDisplayed.transform)
                theTransform.gameObject.layer = LayerMask.NameToLayer("InvisibleToAllLayer");
        }

        shootTimeIntervals = new float[weaponList.Length];
        reloadTimeIntervals = new float[weaponList.Length];
        shotsLeft = new int [weaponList.Length];

        for (int i =0; i< weaponList.Length -1; i++) //For Katana -1 is added , Debug
        {
            shootTimeIntervals[i] = weaponList[i].GetComponent<WeaponSelfController>().shootInterval;
            reloadTimeIntervals[i] = weaponList[i].GetComponent<WeaponSelfController>().reloadInterval;
            shotsLeft[i] = weaponList[i].GetComponent<WeaponSelfController>().shotsInOneClip;
        }

        //ForThisWeapon_UpdateShootAndReloadTime(currentWeapon,0);
        ForAllWeapons_UpdateShootAndReloadTime(0.1f);
        UpdateWeaponDataDisplay();

    }

    void Update() 
    {
        if (CanShootWeapon(currentWeapon) && photonView.IsMine && (Input.GetMouseButtonDown(0)))
        {
            currentWeapon = 1;
            DecreaseShootTimesForWeapon(currentWeapon, Time.deltaTime);
            ShootABulletLocal(currentWeapon);
            photonView.RPC("ShootABulletNetwork", RpcTarget.Others, currentWeapon, (int) transform.rotation.eulerAngles.z);//Calls the remote event in all connected clients and self, the event will call the ShootABullet method as per remoteEventManager component attached to NinjaCharacter           

        }
        else if (CanShootWeapon(currentWeapon) && photonView.IsMine && (Input.GetKeyDown(KeyCode.Space)))
        {
            currentWeapon = 0;
            DecreaseShootTimesForWeapon(currentWeapon, Time.deltaTime);
            ShootABulletLocal(currentWeapon);
            photonView.RPC("ShootABulletNetwork", RpcTarget.Others, currentWeapon, (int)transform.rotation.eulerAngles.z);//Calls the remote event in all connected clients and self, the event will call the ShootABullet method as per remoteEventManager component attached to NinjaCharacter           
        }


        ForAllWeapons_UpdateShootAndReloadTime( Time.deltaTime);
        UpdateWeaponDataDisplay();
    }

    public void ShootABulletLocal(int weaponType)
    {
        if (weaponType == 0 && weaponList[weaponType].GetComponent<KatanaSlicer>().IsKatanaSheathed)//Katana must be sheated, check just in case
        {
            weaponList[weaponType].GetComponent<WeaponDamage>().OwnerNinja = transform.parent.gameObject;
            weaponList[weaponType].GetComponent<KatanaSlicer>().DoKatanaCut();
        }
        else if (weaponType == 1)
        {
            GameObject bulletShot = Instantiate(weaponList[weaponType], localThreePointCrosshair_MidPointObject.transform.position, transform.rotation);
            bulletShot.GetComponent<WeaponDamage>().OwnerNinja = transform.parent.gameObject;
            Vector2 bulletDirection = crossHair.transform.position - transform.position;
        }
    }

    [PunRPC]
    public void ShootABulletNetwork(int weaponType, int crossHairAngleAtShooting)
    {
        crosshairMover.MoveTheCrosshair(crossHairAngleAtShooting);         //Shoot düzgün çalışsın istiyorsak, shoot call yaptığımızda ninjanın pozisyonu ve crosshair rotationı da yollamamız lazım. yoksa yamukluk olur. Anchor'da düzgün yapıyoruz galiba ordan bak. Ama bu network sorunu yaratıyor mu hareket senkronunda, uyguladıktan sonra dene bak bir");

        if (weaponType == 0 && weaponList[weaponType].GetComponent<KatanaSlicer>().IsKatanaSheathed)//Katana must be sheated, check just in case
        {
            weaponList[weaponType].GetComponent<WeaponDamage>().OwnerNinja = transform.parent.gameObject;
            weaponList[weaponType].GetComponent<KatanaSlicer>().DoKatanaCut();
        }
        else if (weaponType == 1)
        {
            GameObject bulletShot = Instantiate(weaponList[weaponType], localThreePointCrosshair_MidPointObject.transform.position, transform.rotation);
            bulletShot.GetComponent<WeaponDamage>().OwnerNinja = transform.parent.gameObject;
            Vector2 bulletDirection = crossHair.transform.position - transform.position;
        }
    }

    bool CanShootWeapon(int weaponNum)
    {
        bool canShootThisWeapon = false;

        if (IsWeaponKatana(weaponNum) == false && shotsLeft[weaponNum] > 0 && shootTimeIntervals[weaponNum] > weaponList[weaponNum].GetComponent<WeaponSelfController>().shootInterval)
            canShootThisWeapon = true;
        else if (IsWeaponKatana(weaponNum) && weaponList[weaponNum].GetComponent<KatanaSlicer>().IsKatanaSheathed
                  && shotsLeft[weaponNum] > 0 && shootTimeIntervals[weaponNum] > weaponList[weaponNum].GetComponent<WeaponSelfController>().shootInterval)
        {
            canShootThisWeapon = true;
        }
        
        return canShootThisWeapon;
    }

    bool IsWeaponKatana(int weaponNum)
    {
        if (weaponList[weaponNum].tag == "Katana" ) 
            return true;
        else
            return false;
    }

    void ForThisWeapon_UpdateShootAndReloadTime(int weaponNum, float deltaTimeInput)
    {
        if (shotsLeft[weaponNum] == 0)
        {
            reloadTimeIntervals[weaponNum] += deltaTimeInput;

            if (reloadTimeIntervals[weaponNum] > weaponList[weaponNum].GetComponent<WeaponSelfController>().reloadInterval)
                ReloadWeapon(weaponNum);

        }
        else if (shotsLeft[weaponNum] > 0 && shootTimeIntervals[weaponNum] < weaponList[weaponNum].GetComponent<WeaponSelfController>().shootInterval+1f)
            shootTimeIntervals[weaponNum] += deltaTimeInput;
    }

    void ForAllWeapons_UpdateShootAndReloadTime (float deltaTimeInput) 
    {
        for (int weaponNum = 0; weaponNum < weaponList.Length; weaponNum++)
        {
            if (shotsLeft[weaponNum] == 0)
            {
                if(IsWeaponKatana(weaponNum) == false)
                    reloadTimeIntervals[weaponNum] += deltaTimeInput;
                else if(IsWeaponKatana(weaponNum) && weaponList[weaponNum].GetComponent<KatanaSlicer>().IsKatanaSheathed)
                    reloadTimeIntervals[weaponNum] += deltaTimeInput;

                if (reloadTimeIntervals[weaponNum] > weaponList[weaponNum].GetComponent<WeaponSelfController>().reloadInterval)
                    ReloadWeapon(weaponNum);

            }
            else if (shotsLeft[weaponNum] > 0 && shootTimeIntervals[weaponNum] < weaponList[weaponNum].GetComponent<WeaponSelfController>().shootInterval + 1f)
                shootTimeIntervals[weaponNum] += deltaTimeInput;
        }
    }

    void DecreaseShootTimesForWeapon ( int weaponNum, float deltaTimeInput)
    {
        if(shotsLeft[weaponNum] > 1)
            {
                shotsLeft[weaponNum] -= 1;
                shootTimeIntervals[weaponNum] = 0;
            }
        else if (shotsLeft[weaponNum] == 1)
        {
            shotsLeft[weaponNum] = 0;
            shootTimeIntervals[weaponNum] = 0;
            reloadTimeIntervals[weaponNum] = 0;
        }
    }

    void ReloadWeapon(int weaponNum)
    {
        shotsLeft[weaponNum] = weaponList[weaponNum].GetComponent<WeaponSelfController>().shotsInOneClip;
        reloadTimeIntervals[weaponNum] = weaponList[weaponNum].GetComponent<WeaponSelfController>().reloadInterval;
        shootTimeIntervals[weaponNum] = weaponList[weaponNum].GetComponent<WeaponSelfController>().shootInterval;
    }
    void UpdateWeaponDataDisplay() // (int weaponNum)
    {
        for (int weaponNum = 0; weaponNum <2; weaponNum++)
        {
            float scalePerBullet = 1.5f / weaponList[weaponNum].GetComponent<WeaponSelfController>().shotsInOneClip;
            float scalePerReloadSecond = 1.5f / weaponList[weaponNum].GetComponent<WeaponSelfController>().reloadInterval;

            Vector3 currentScale = weaponStatusBarsDisplayed.transform.GetChild(weaponNum).localScale;

            if (shotsLeft[weaponNum] > 0)
                weaponStatusBarsDisplayed.transform.GetChild(weaponNum).localScale = new Vector3(scalePerBullet * shotsLeft[weaponNum], currentScale.y, currentScale.z);
            else
                weaponStatusBarsDisplayed.transform.GetChild(weaponNum).localScale = new Vector3(scalePerReloadSecond * reloadTimeIntervals[weaponNum], currentScale.y, currentScale.z);
        }
    }

    private void OnDestroy()
    {
        Destroy(weaponTitlesDisplayed);
        Destroy(weaponStatusBarsDisplayed);
    }
}
