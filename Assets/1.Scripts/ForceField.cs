using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class ForceField : MonoBehaviour
{
    public float shrinkWaitTime, shrinkAmount, shrinkDuration, minShrinkAmount, lastShrinkTime, targetDiameter, lastDmgTime, dmgCooldown;
    public int playerDamage;
    public bool shrinking;
    //public PostProcessVolume postProcess;


    // Start is called before the first frame update
    void Start()
    {
        //postProcess.enabled = false;
        lastShrinkTime = Time.time;
        targetDiameter = transform.localScale.x;

    }

    // Update is called once per frame
    void Update()
    {
        checkPlayers();
        if(shrinking)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, (shrinkAmount/shrinkDuration)*Time.deltaTime);
            if(transform.localScale.x == targetDiameter)
            {
                shrinking = false;
            }
        }
        else
        {
            if(Time.time-lastShrinkTime >= shrinkWaitTime && transform.localScale.x > minShrinkAmount)
            {
                shrink();
            }
        }
    }

    public void shrink()
    {

        shrinking = true;

        if(transform.localScale.x - shrinkAmount > minShrinkAmount)
        {
            targetDiameter -= shrinkAmount;
        }
        else
        {
            targetDiameter = minShrinkAmount;
        }
        lastShrinkTime = Time.time + shrinkDuration;
    }

    void checkPlayers()
    {
        if(Time.time - lastDmgTime > dmgCooldown)
        {
            lastDmgTime = Time.time;

            foreach(PlayerController player in GameManager.instance.players)
            {
                //postProcess.enabled = false;

                if (player.isDead || !player) {
                    continue;
                }
                if (Vector3.Distance(transform.position,player.transform.position) >= transform.localScale.x)
                {
                    player.photonView.RPC("takeDamage", player.photonPlayer, 0, playerDamage);
                    //postProcess.enabled = true;
                }
            }
        }
    }

}
