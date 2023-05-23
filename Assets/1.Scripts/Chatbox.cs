using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;


public class Chatbox : MonoBehaviourPun
{
    public TextMeshProUGUI chatLog;
    public TMP_InputField chatInput;
    public TMP_ScrollbarEventHandler scrollHandler;
    public static Chatbox instance;

   
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)) {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (EventSystem.current.currentSelectedGameObject == chatInput.gameObject)
            {
                onChatInputSend();
               

            }
            else
            {
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
            }
             
        }
    }

    

    public void onChatInputSend()
    {
        if (chatInput.text.Length > 0)
        {
            Color myColor = GameManager.instance.playerColors[PhotonNetwork.LocalPlayer.ActorNumber - 1];
            string hexCC = toRGBHex(myColor);
            photonView.RPC("log",RpcTarget.All,PhotonNetwork.LocalPlayer.NickName,chatInput.text,hexCC);
            chatInput.text = "";
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    [PunRPC]
    void log(string playerName, string message, string color)
    {
        chatLog.text += string.Format("<color={2}><b>{0}:</b></color> {1}\n",playerName,message,color);
    }

    public static string toRGBHex(Color c)
    {
        return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte) (f * 255);
    }

}
