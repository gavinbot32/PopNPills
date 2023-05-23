using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Net.Http.Headers;
using UnityEngine.Device;
using Unity.VisualScripting;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine.Rendering;

public class MenuController : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    #region Variables

    [Header("Menu Screens")]
    public GameObject mainScreen;
    public GameObject createRoom;
    public GameObject roomLobbyScreen;
    public GameObject roomSelectLobbyScreen;

    [Header("Main Screen Components")]
    public Button createRoomBttn_ms;
    public Button findRoomBttn_ms;
    public TMP_InputField name_input;

    [Header("Room Screen Components")]
    public Button createRoomBttn_cr;
    public Button backBttn_Cr;
    public TMP_InputField name_input_Cr;

    [Header("Room Lobby Components")]
    public TextMeshProUGUI playerList;
    public TextMeshProUGUI roomName;
    public Button startGame_rls;
    public Button leaveLobby;
    public TMP_Dropdown level_select;

    [Header("Room Selection Lobby Screen ")]
    public RectTransform roomListContainer;
    public GameObject roomBttnPrefab;


    [Header("Universal Components")]
    public TextMeshProUGUI toast_message;
    public GameObject hourGlass;

    [Header("Universal Variables")]
    public string name;
    public string room_name;
    public string selected_level;

    [SerializeField]
    private List<GameObject> roomBttnList = new List<GameObject>();
    [SerializeField]
    private List<RoomInfo> roomInfoList = new List<RoomInfo>();
    #endregion




    // Start is called before the first frame update
    void Start()
    {
        createRoomBttn_ms.interactable = false;
        findRoomBttn_ms.interactable = false;
        name_input.interactable = false;
        Cursor.lockState = CursorLockMode.None;

       
        hourGlass.SetActive(!PhotonNetwork.IsConnected);
        toast_message.gameObject.SetActive(!PhotonNetwork.IsConnected);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;

        }

    }

    //all screen


    public void setScreen(GameObject screen)
    {
        mainScreen.SetActive(false);
        createRoom.SetActive(false);
        roomLobbyScreen.SetActive(false);
        roomSelectLobbyScreen.SetActive(false);

        screen.SetActive(true);
        if(screen == roomSelectLobbyScreen)
        {
            updateRoomSelectionLobby();
        }
    }


    public override void OnConnectedToMaster()
    {
        name_input.interactable = true;
        hourGlass.SetActive(false);
        showMessage("Connected to server", 2f);
    }
    public void showMessage(string message)
    {
        toast_message.text = message;
        toast_message.gameObject.SetActive(true);
    }

    public void showMessage(string message, float timer)
    {
        toast_message.text = message;
        StartCoroutine(toastAMessage(timer));
    }
    public void hideMessage()
    {
        toast_message.gameObject.SetActive(false);

    }
    // specific


    public void OnCreateRoomBttn_ms()
    {
        PhotonNetwork.NickName = name;
        name_input_Cr.text = null;
        createRoomBttn_cr.interactable = false;
        setScreen(createRoom);
    }


    public void OnRoomNameChange()
    {
        room_name = name_input_Cr.text;
        if (room_name.Length > 2 && room_name.Length < 13)
        {
            if (!createRoomBttn_cr.interactable)
            {
                createRoomBttn_cr.interactable = true;
                hideMessage();
            }
        }
        else
        {
            showMessage("Room Name must be between 3 and 13 characters");
            createRoomBttn_cr.interactable = false;
        }
    }

    public void OnRoomNameSelect()
    {
        name_input_Cr.text = null;

    }

    public void OnFindRoomBttn_ms()
    {
        PhotonNetwork.NickName = name;
        setScreen(roomSelectLobbyScreen);
    }

    public void onBackBttn()
    {
        setScreen(mainScreen);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

    }

    public void PlayerNameValueChanged()
    {

        name = name_input.text;
        if (name.Length > 2 && name.Length < 13)
        {
            if (!createRoomBttn_ms.interactable)
            {
                createRoomBttn_ms.interactable = true;
                findRoomBttn_ms.interactable = true;
                hideMessage();
            }
        }
        else
        {
            showMessage("Name must be between 3 and 13 characters");
            createRoomBttn_ms.interactable = false;
            findRoomBttn_ms.interactable = false;
        }



    }

    public void PlayerNameOnSelect()
    {
        name_input.text = "";

    }

    public void OnCreateRoom_crs()
    {
        NetworkManager.instance.createRoom(room_name);

    }
    public void onCreateRoomBttn_rsl()
    {
        name_input_Cr.text = null;
        createRoomBttn_cr.interactable = false;
        setScreen(createRoom);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        updateUI();
    }

    public void onLevelSelectChange()
    {
        selected_level = level_select.options[level_select.value].text;

    }

    public override void OnJoinedRoom()
    {
        setScreen(roomLobbyScreen);
        photonView.RPC("updateUI", RpcTarget.All);

    }

    [PunRPC]
    void updateUI() {
        //update lobby name, player list
        roomName.text = "<b>" + PhotonNetwork.CurrentRoom.Name + "</b>";


        playerList.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerList.text += (player.NickName + "\n");
        }
        //disable/enable buttons
        startGame_rls.interactable = PhotonNetwork.IsMasterClient;
        level_select.interactable = PhotonNetwork.IsMasterClient;

        selected_level = "Arena";

        selected_level = level_select.options[0].text;


    }

    public void onStartGame_rl()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;

        NetworkManager.instance.photonView.RPC("changeScenes", RpcTarget.All, selected_level);
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomInfoList = roomList;
    }

    public GameObject createNewRoomBttn()
    {
        GameObject bttn = Instantiate(roomBttnPrefab, roomListContainer.transform);
        roomBttnList.Add(bttn);
        return bttn;
    }

    public void onJoinRoomBttn_rls(string roomName)
    {
        foreach (RoomInfo roomInfo in roomInfoList)
        {
            if (roomInfo.Name == roomName)
            {
                NetworkManager.instance.joinRoom(roomName);
            }
        }
    }

    public void onRefreshBttn()
    {
        updateRoomSelectionLobby();
    }

    public void updateRoomSelectionLobby()
    {
        foreach(GameObject bttn in roomBttnList)
        {
            bttn.SetActive(false);
        }
        foreach(RoomInfo room in roomInfoList)
        {
            if(room.PlayerCount <= 0)
            {
                roomInfoList.Remove(room);
            }
        }
        for(int i = 0; i < roomInfoList.Count; i++)
        {
            GameObject bttn = i >= roomBttnList.Count ? createNewRoomBttn() : roomBttnList[i];
            bttn.SetActive(true);
            bttn.transform.Find("roomName Text").GetComponent<TextMeshProUGUI>().text = roomInfoList[i].Name;
            bttn.transform.Find("playerCountText").GetComponent<TextMeshProUGUI>().text = roomInfoList[i].PlayerCount.ToString()+" / "+ roomInfoList[i].MaxPlayers.ToString();
            string rn = roomInfoList[i].Name;
            Button bttncomp = bttn.GetComponent<Button>();
            bttncomp.onClick.RemoveAllListeners();
            bttncomp.onClick.AddListener(() => { onJoinRoomBttn_rls(rn); });
        }
    }
    IEnumerator toastAMessage(float time)
    {
        toast_message.gameObject.SetActive(true);

        yield return new WaitForSeconds(time);
        toast_message.gameObject.SetActive(false);

    }



}
