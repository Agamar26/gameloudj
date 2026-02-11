using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEditor.XR;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    public RoomOptions roomoptions;
    public bool isConnectedToMasterServer;
    public GameObject roomPref,playMenuUI ,listOfRoomsUI, createRoomUI, currentRoomUI;
    public GameObject contentRooms;
    public TMP_InputField inputFieldRoomName,inputfieldUsername;
    public TextMeshProUGUI textCreateRoomNameUI;
    public List<string> roomNames = new List<string>();
    public List<string> roomPlayerNumbers = new List<string>();
    public List<string> roomMaxPlayerNumbers = new List<string>();
    public List<Player> roomList = new List<Player>();
    
    
    [Header("In room")]
    public bool canLaunchGame;
    public Button buttonPlayGame;
    public TextMeshProUGUI[] playersInRoomsUI;
    public List<string> roomPlayerUsernameList = new List<string>();
    public GameObject messageInRoomUIPref;
    public TMP_InputField inputfieldForMessageInroomUI;
    public GameObject contentMessagesInRoom;
    public List<string> roomMessagesInRoomUI = new List<string>();


    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("map", "silo");
        roomoptions = roomOptions;
    }
    // Start is called before the first frame update
    void Start()
    {
       Connect();
        inputfieldUsername.characterLimit = 12;
    }

    public void Connect()
    {
        
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            //PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    public override void OnConnectedToMaster()
    {
        isConnectedToMasterServer = true;
        print("Connecté au Serveur Maitre, bienvenue sur le jeu ! Choisissez un mode");
        base.OnConnectedToMaster();
    }
   
    public void ConnectToServerOrJoinLobby()
    {
        if (PhotonNetwork.IsConnected && isConnectedToMasterServer && (!string.IsNullOrEmpty(inputfieldUsername.text) && !string.IsNullOrWhiteSpace(inputfieldUsername.text)))
        {
            //PhotonNetwork.JoinRandomOrCreateRoom(null,0,MatchmakingMode.FillRoom,null,null,"lastreet",roomoptions);
            PhotonNetwork.JoinLobby();
        }
    }
    private void Update()
    {
        // Vérifie si la propriété personnalisée "map" existe
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("map"))
        {
            string mapName = (string)PhotonNetwork.CurrentRoom.CustomProperties["map"];
            Debug.Log("La carte choisie est : " + mapName);
        }
       
        canLaunchGame = PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 2;

        buttonPlayGame.interactable = canLaunchGame;
        
    }

    public void RefreshPlayersInRoom()
    {
        listOfRoomsUI.SetActive(false);
        currentRoomUI.SetActive(true);
        roomList.Clear();
        roomList = PhotonNetwork.PlayerList.ToList();
        roomPlayerUsernameList.Clear();
        
        for (int i = 0; i < roomList.Count; i++)
        {
            var index = i;
            if (roomList[i].IsMasterClient)
            {
                roomPlayerUsernameList.Add($"{roomList[i].NickName} (Host)");
                playersInRoomsUI[i].text = $"{roomList[i].NickName} (Host)";
            }
            else
            {
                roomPlayerUsernameList.Add(roomList[i].NickName);
                playersInRoomsUI[i].text = roomList[i].NickName;
            }
        }

        if (roomList.Count < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            for (int i = roomList.Count; i < playersInRoomsUI.Length; i++)
            {
                playersInRoomsUI[i].text = "Waiting for player...";
            }  
        }
    }

    public override void OnJoinedRoom()
    {
        RefreshPlayersInRoom();
        RefreshMessagesInMenuRoom();
        print($"Vous avez rejoins une room, version du jeu : {gameVersion}");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        RefreshPlayersInRoom();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
       
        base.OnPlayerLeftRoom(otherPlayer);
        RefreshPlayersInRoom();
    }
    
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        var rezz = new ExitGames.Client.Photon.Hashtable();
        rezz.Add("Mama", "Sita");
        PhotonNetwork.CurrentRoom.SetCustomProperties(rezz);

        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 3000;
        createRoomUI.SetActive(false);
        RefreshPlayersInRoom();
    }

   

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomNames.Clear();
        roomMaxPlayerNumbers.Clear();
        roomPlayerNumbers.Clear();
        for (int i = 0; i < contentRooms.transform.childCount; i++)
        {
            Destroy(contentRooms.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != 0)
            {
                roomNames.Add(roomList[i].Name);
                roomMaxPlayerNumbers.Add(roomList[i].MaxPlayers.ToString());
                roomPlayerNumbers.Add(roomList[i].PlayerCount.ToString());
                
                var currentRoompref = Instantiate(roomPref, contentRooms.transform);
                currentRoompref.transform.Find("Count").GetComponent<TextMeshProUGUI>().text = $"{roomPlayerNumbers[i]} / {roomMaxPlayerNumbers[i]}";
                currentRoompref.transform.Find("NameOfRoom").GetComponent<TextMeshProUGUI>().text = $"{roomNames[i]}";
                var roomname = roomList[i].Name;
                currentRoompref.GetComponentInChildren<Button>().onClick.AddListener(() => { PhotonNetwork.JoinRoom(roomname); });
            }
            
        }

        base.OnRoomListUpdate(roomList);
    }
  

    public override void OnJoinedLobby()
    {
        PhotonNetwork.LocalPlayer.NickName = inputfieldUsername.text;
        var ezeez = " : ";
        int azz = PhotonNetwork.LocalPlayer.NickName.Length + ezeez.Length;
        inputfieldForMessageInroomUI.characterLimit = 150 - azz;
        playMenuUI.SetActive(false);
        listOfRoomsUI.SetActive(true);
        print("Vous êtes dans le lobby");
        base.OnJoinedLobby();
    }

    #region ui
    public void CreateRoomUI()
    {
        listOfRoomsUI.SetActive(false);
        createRoomUI.SetActive(true);
    } 
    
    public void CreateRoom()
    {
        var nameofroom = inputFieldRoomName.text.ToString();
        if (!string.IsNullOrEmpty(nameofroom) && !string.IsNullOrWhiteSpace(nameofroom))
        {
            PhotonNetwork.CreateRoom(nameofroom, roomoptions);
            print("room name is ok, create room...");
        }
      
    }
    public void SendMessageInRoomMenu()
    {
        if(Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(inputfieldForMessageInroomUI.text) && !string.IsNullOrWhiteSpace(inputfieldForMessageInroomUI.text))
        {
            var index = 9999;
            for (int i = 0; i < 9999; i++)
            {
                index = i;
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("MessagesInRoom" + i))
                {
                    
                }
                else
                {
                    break;
                }
            }

            ExitGames.Client.Photon.Hashtable messageToSend = PhotonNetwork.CurrentRoom.CustomProperties;
            messageToSend.Add("MessagesInRoom" + index, $"{PhotonNetwork.LocalPlayer.NickName} : {inputfieldForMessageInroomUI.text}");
            PhotonNetwork.CurrentRoom.SetCustomProperties(messageToSend);
            inputfieldForMessageInroomUI.text = string.Empty;
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        print(propertiesThatChanged.ToString());
        RefreshMessagesInMenuRoom();
    }

    public void RefreshMessagesInMenuRoom()
    {
        DestroyMessagesInRoomMenu();
        roomMessagesInRoomUI.Clear();

        foreach (var property in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            if (property.Key.ToString().Contains("MessagesInRoom"))
            {
                roomMessagesInRoomUI.Add(property.Value.ToString());
                var messageInst = Instantiate(messageInRoomUIPref, contentMessagesInRoom.transform);
                messageInst.GetComponentInChildren<TextMeshProUGUI>().text = property.Value.ToString();
                print("il contient la clé message" + property.Key.ToString());
            };
        }
    }

    public void DestroyMessagesInRoomMenu()
    {
        if (contentMessagesInRoom.transform.childCount > 0)
        {
            for (int i = 0; i < contentMessagesInRoom.transform.childCount; i++)
            {
                Destroy(contentMessagesInRoom.transform.GetChild(i).gameObject);
            }
        }
    }

    public void LaunchGame()
    {
       
        var ezzeze = PhotonNetwork.CurrentRoom.CustomProperties.Keys;
        var newhashtable = new ExitGames.Client.Photon.Hashtable();
        foreach (var key in ezzeze)
        {
            if(key.ToString().Contains("MessagesInRoom"))
            {

            }
            else
            {
                object valueofkey = null;
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out valueofkey);
                newhashtable.Add(key.ToString(), valueofkey);
            }
        }
       
        PhotonNetwork.CurrentRoom.SetCustomProperties(newhashtable);

        PhotonNetwork.LoadLevel("Silo");
    }
    #endregion

}
