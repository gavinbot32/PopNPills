using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{

    public float postGameTime;



    [Header("Player Vars")]
    public string playerPrefabLocation;
    public PlayerController[] players;
    public Transform[] spawnPoints;
    public int playersAlive;
    public List<Transform> tempSpawns;
    private int playersInGame;
    public Color[] playerColors;


    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        playersAlive = players.Length;
        foreach(Transform t in spawnPoints)
        {
            tempSpawns.Add(t);
        }
        photonView.RPC("imInGame", RpcTarget.AllBuffered);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void imInGame()
    {
        playersInGame++;
        if(PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("spawnPlayer", RpcTarget.All);
        }
    }

    public Transform randomSpawnPos()
    {
        Transform ret;
        int number = Random.Range(0, tempSpawns.Count);
        ret = tempSpawns[number];
        tempSpawns.Remove(ret);
        return ret;
    }

    [PunRPC]
    void spawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation,randomSpawnPos().position, Quaternion.identity);
        playerObj.GetComponent<PlayerController>().photonView.RPC("Initialized", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
    
    public PlayerController getPlayer(int playerID)
    {
        return players.First(x => x.punId == playerID);
    }
    public PlayerController getPlayer(GameObject playerobj)
    {
        return players.First(x => x.gameObject == playerobj);
    }


    public void checkWinCondition()
    {
        if(playersAlive <= 1)
        {
            photonView.RPC("winGame", RpcTarget.All, players.First(x => x.isDead).punId);  
        }
    }

    [PunRPC]
    private void winGame(int winID)
    {
        // Set the UI Win Text
        Invoke("backToMenu", postGameTime); 


    }


    void backToMenu()
    {
        Destroy(NetworkManager.instance);
        NetworkManager.instance.changeScenes("Menu");
    }

}
