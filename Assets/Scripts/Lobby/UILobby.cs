using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace MirrorBasics {

    public class UILobby : MonoBehaviour {

        public static UILobby instance;

        [Header("Host Join")]
        [SerializeField] GameObject HostGroup;
        [SerializeField] InputField joinMatchInput;
        [SerializeField] List<Selectable> lobbySelectables = new List<Selectable> ();
        [SerializeField] Canvas lobbyCanvas;
        [SerializeField] GameObject ChooseDifficulty;
        [SerializeField] GameObject ChooseMap;


        [Header ("Lobby")]
        [SerializeField] Transform UIPlayerParent;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;               

        GameObject localPlayerLobbyUI;

        void Start () {
            instance = this;
            lobbyCanvas.enabled = false;           
            HostGroup.SetActive(true);
            lobbySelectables.ForEach(x => x.interactable = true);
            ChooseDifficulty.SetActive(false);
            ChooseMap.SetActive(false);
        }       

        public void ReturnToMenu()
        {
            GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StopClient();
        }
        public void HostPublic () {
            lobbySelectables.ForEach (x => x.interactable = false);

            CardPlayer.localPlayer.HostGame (true);
        }

        public void HostPrivate () {
            lobbySelectables.ForEach (x => x.interactable = false);

            CardPlayer.localPlayer.HostGame (false);
        }

        public void HostSuccess (bool success, string matchID) {
            if (success) {
                lobbyCanvas.enabled = true;
                HostGroup.SetActive(false);
                if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
                localPlayerLobbyUI = SpawnPlayerUIPrefab (CardPlayer.localPlayer);
                matchIDText.text = matchID;
                //beginGameButton.SetActive (true);
            } else {
                lobbySelectables.ForEach (x => x.interactable = true);
            }
        }

        public void Join () {
            lobbySelectables.ForEach (x => x.interactable = false);

            CardPlayer.localPlayer.JoinGame (joinMatchInput.text.ToUpper ());
        }

        public void JoinSuccess (bool success, string matchID) {
            if (success) {
                lobbyCanvas.enabled = true;
                HostGroup.SetActive(false);
                if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
                localPlayerLobbyUI = SpawnPlayerUIPrefab (CardPlayer.localPlayer);
                matchIDText.text = matchID;
            } else {
                lobbySelectables.ForEach (x => x.interactable = true);
            }
        }

        public void DisconnectGame () {
            if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
            CardPlayer.localPlayer.DisconnectGame ();

            lobbyCanvas.enabled = false;
            HostGroup.SetActive(true);
            lobbySelectables.ForEach (x => x.interactable = true);
            ChooseDifficulty.SetActive(false);
            ChooseMap.SetActive(false);
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
            ChooseDifficulty.SetActive(true);
        }              
    }
}