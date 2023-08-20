using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;


    public int maxPlayers = 16;


    private void Awake()
    {

        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();

        foreach(PhotonView view in photonViews)
        {
            if(view.ViewID == 999)
            {
                if(view.gameObject != this.gameObject)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        
        instance = this;
       
       

        DontDestroyOnLoad(instance);
        

    }



    // Start is called before the first frame update
    void Start()
    {
        // connect to master server
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        print("We have connected to master sever");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedRoom()
    {
        print("Joined a room "+PhotonNetwork.CurrentRoom.Name);
      
    }


    //called when we create a new room
    public void createRoom(string roomName)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;

        PhotonNetwork.CreateRoom(roomName, options);

    }
    //called when we join a room
    public void joinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameManager.instance.playersAlive--;

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.checkWinCondition();
        }
    }



    [PunRPC]
    public void changeScenes(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);


    }

}
