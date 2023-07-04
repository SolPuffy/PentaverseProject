using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using UnityEngine.UIElements;

public class UILobby : MonoBehaviour {

    public static UILobby instance;

    bool GameSelected = false;
    bool NameEntered  = false;
    bool Connecting   = false;
    [SerializeField] InputField Name;
    [SerializeField] GameObject KillMe;
    [SerializeField] ScrollView GamesList;

    [Header("Host Join")]
    [SerializeField] GameObject HostGroup;
    [SerializeField] InputField joinMatchInput;
    
    [SerializeField] List<Selectable> lobbySelectables = new List<Selectable> ();
    [SerializeField] Canvas lobbyCanvas;  


    [Header ("Lobby")]
    [SerializeField] Transform UIPlayerParent;
    [SerializeField] GameObject UIPlayerPrefab;
    //[SerializeField] Text matchIDText;               

    GameObject localPlayerLobbyUI;

    void Awake () {
        instance = this;       
    }       

    public void ReturnToMenu()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StopClient();
    }
    public void HostPublic () {
        

        CardPlayer.localPlayer.HostGame (true);
    }

    public void HostPrivate () {
        

        CardPlayer.localPlayer.HostGame (false);
    }

    public void HostSuccess (bool success, string matchID) {
        if (success) {            
            if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
            localPlayerLobbyUI = SpawnPlayerUIPrefab (CardPlayer.localPlayer);
            //matchIDText.text = matchID;            
        } else {
            
        }
    }

    public void Join () {
        lobbySelectables.ForEach (x => x.interactable = false);

        CardPlayer.localPlayer.JoinGame (joinMatchInput.text.ToUpper ());
    }

    public void JoinSuccess (bool success, string matchID) {
        if (success) {            
            if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
            localPlayerLobbyUI = SpawnPlayerUIPrefab (CardPlayer.localPlayer);
            //matchIDText.text = matchID;
        } else {
            
        }
    }

    public void DisconnectGame () {
        if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
        CardPlayer.localPlayer.DisconnectGame ();
        
    }

    public GameObject SpawnPlayerUIPrefab (CardPlayer player) {
        GameObject newUIPlayer = Instantiate (UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer> ().SetPlayer (player);
        newUIPlayer.transform.SetSiblingIndex (player.playerIndex - 1);

        return newUIPlayer;
    }

    public void ReorderUIPrefabs()
    {

    }

    public void BeginGame () 
    {
        PressStart();
    } 

    public void PressStart()
    {
        
    }              
}