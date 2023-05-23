using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO.IsolatedStorage;

public class PlayerController : MonoBehaviourPun
{

    [Header("Components")]
    public Rigidbody rig;
    public Camera cam;
    public AudioSource audio;
    public int punId;
    public Player photonPlayer;
    public Color skin;
    public MeshRenderer mr;

    [Header("Movement Stats")]
    public float moveSpeed;
    public float jumpForce;


    [Header("Player Stats")]
    public int curHp;
    public int maxHp;
    public int lives;
    public int maxAmmo;
    public int curAmmo;
    public int score;
    public bool isDead;

    [Header("Endgame Stats")]
    public int dmgTaken;
    public int ammoSpent;
    public int dmgGiven;
    public int elims;
    public int shotsHit;
    public int shotsFired;
    public int items_collected;

    public float accuracy;

    [Header("Weapons")]
    public int gun_index;
    public Transform[] gunList;
    public PlayerWeapon selectedWeapon;
    [Header("Others")]
    private int curAttackerID;
    private bool isFlashing;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        audio = GetComponent<AudioSource>();
        cam = GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gun_index = 0;
        swapGuns(gun_index);

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }


        if (Cursor.lockState == CursorLockMode.Locked)
        {
            move();
            gunSwapKeyPress(gun_index);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                tryJump();
            }
            if(Input.GetKeyDown(KeyCode.R)) {
                tryRelod();
            }

            if (Input.GetMouseButton(0))
            {
                if (selectedWeapon.automatic)
                {
                    pullTrigger();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!selectedWeapon.automatic)
                {
                    pullTrigger();
                }
            }

        }
    } 

    [PunRPC]
    public void Initialized(Player player)
    {
        punId = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[punId - 1] = this;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        skin = GameManager.instance.playerColors[punId - 1];
        mr.material.color = skin;
    }


    public void tryRelod()
    {
        if (curAmmo >= selectedWeapon.clipSize)
        {
            curAmmo -= selectedWeapon.clipSize - selectedWeapon.curClip;
            selectedWeapon.reload();
        }
        else
        {
            if (selectedWeapon.curClip + curAmmo <= selectedWeapon.clipSize)
            {
                selectedWeapon.reload(curAmmo);
                curAttackerID = 0;
            }
            else
            {
                int needbullets = selectedWeapon.clipSize - selectedWeapon.curClip;
                selectedWeapon.reload(needbullets);
                curAmmo -= needbullets;
            }
        }
    }

    private void move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;
        rig.velocity = dir;

         

    }

    private void tryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 1.5f))
        {
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void pullTrigger()
    {
        selectedWeapon.tryShoot();
    }

    public void heal() {

    }

    public void addAmmo()
    {

    }

    [PunRPC]
    public void die()
    {
        skin = Color.black;
        mr.material.color = Color.black;
        curHp = 0;
        isDead = true;
        GameManager.instance.playersAlive--;

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.checkWinCondition();
        }

        if (photonView.IsMine)
        {
            if(curAttackerID != 0)
            {
                PlayerController attacker = GameManager.instance.getPlayer(curAttackerID);
                attacker.photonView.RPC("addKill", RpcTarget.All);
            }
            cam.GetComponent<CameraController>().setSpectator();
            rig.isKinematic = true;
            transform.position = new Vector3(0, -100000, 0);
        }

    }

    [PunRPC]
    public void addKill()
    {
        elims++;
    }



    [PunRPC]
    public void takeDamage(int whoSHOTME, int howMuch)
    {
        if (isDead)
        {
            return;
        }

        curHp -= howMuch;
        curAttackerID = whoSHOTME;

        photonView.RPC("damageFlash", RpcTarget.Others);

        if (curHp <= 0)
        {
            photonView.RPC("die", RpcTarget.All);
        }
    
    
    }

    public void damageFlash()
    {
        if(isFlashing)
        {
            return;
        }
        StartCoroutine(colorFlashCoRoutine(Color.red));
        

    }


    public void gunSwapKeyPress(int index)
    {
        int new_press = index;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            new_press++;
            if (new_press < 0)
            {
                new_press = gunList.Length - 1;
            }
            else if (new_press > gunList.Length - 1)
            {
                new_press = 0;
            }
        }
        else
        {
            return;
        }
        swapGuns(new_press);
        gun_index = new_press;
    }

    public void swapGuns(int index)
    {
        foreach(Transform gun in gunList)
        {
            gun.gameObject.SetActive(false);
        }
        gunList[index].gameObject.SetActive(true);
        selectedWeapon = gunList[index].GetComponent<PlayerWeapon>();

    }

    [PunRPC]
    public void spawnBullet(Vector3 muzzlePos, Vector3 dir, int index)
    {
        gunList[index].GetComponent<PlayerWeapon>().spawnBullet(muzzlePos, dir);
    }

    IEnumerator colorFlashCoRoutine(Color color)
    {
        isFlashing = true;
        for (int i = 0; i < 4; i++)
        {

            isFlashing = true;
            mr.material.color = color;
            yield return new WaitForSeconds(0.05f);
            mr.material.color = skin;
        }
        isFlashing = false;
    }

}
