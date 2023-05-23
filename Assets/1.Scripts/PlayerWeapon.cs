using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName;
   
    public int curClip;
    public int clipSize;
    public int index;

    
    [Header("Shooting Stats")]
    public float fireRate;
    public float lastShot;
    public bool automatic;
    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public GameObject flashPrefab;

    [Header("Components")]
    public Animator anim;
    public PlayerController owner;
    public Transform muzzlePos;
    public AudioSource audio;
    public AudioClip[] clips;
    public Camera camera;
   

    // Start is called before the first frame update
    void Start()
    {
     anim = GetComponent<Animator>();
     audio = GetComponent<AudioSource>();
     owner = GetComponentInParent<Transform>().GetComponentInParent<PlayerController>();
     muzzlePos = GameObject.FindWithTag("muzzle").GetComponent<Transform>();
     camera = GetComponentInParent<Camera>();
    }
    public void tryShoot()
    {
        if (owner.isDead)
        {
            return;
        }

        print("You shot the "+ weaponName);
        if(curClip <= 0 || Time.time - lastShot < fireRate)
        {
            return;
        }
        curClip--;
        lastShot = Time.time;
        //update UI
        //Spawn Bullet
        
        owner.photonView.RPC("spawnBullet", RpcTarget.All,muzzlePos.position, camera.transform.forward, index);
        
        //Play Sound
        GameObject flash = Instantiate(flashPrefab,muzzlePos.position,Quaternion.identity);
        flash.transform.forward = camera.transform.forward;
        audio.PlayOneShot(clips[0]);
        Destroy(flash,0.5f);
        //Animation
    }

    public void spawnBullet(Vector3 muzzle, Vector3 dir)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, muzzle, Quaternion.identity);
        bulletObj.transform.forward = dir;
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.initialized(dir, owner.punId, owner.photonView.IsMine);
    }

    public void reload()
    {
        curClip = clipSize;
    }

    public void reload(int bullets)
    {
        curClip += bullets;
    }

}
