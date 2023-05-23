using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("bullet stats")]
    public int dmg;
    public float bulletSpeed;
    public float range;
    public int attackerID;
    public bool isMine;

    private Rigidbody rig;

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }


    public void initialized(Vector3 dir, int id, bool ismine)
    {
        rig.velocity = dir * bulletSpeed;
        isMine = ismine;
        attackerID = id;
        Destroy(gameObject, range);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.getPlayer(other.gameObject);
            if (player.punId != attackerID)
            {
                player.photonView.RPC("takeDamage", player.photonPlayer, attackerID, this.dmg);
            }
        }
        Destroy(gameObject);
    }

}
