using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

namespace MirrorBasics {

    [System.Serializable]
    public class Match {
        public string matchID;
        public bool publicMatch;
        public bool inMatch;
        public bool matchFull;
        public SyncListGameObject players = new SyncListGameObject ();

        public Match (string matchID, GameObject player, bool publicMatch) {
            matchFull = false;
            inMatch = false;
            this.matchID = matchID;
            this.publicMatch = publicMatch;
            players.Add (player);
        }

        public Match () { }
    }

    [System.Serializable]
    public class SyncListGameObject : SyncList<GameObject> { }

    [System.Serializable]
    public class SyncListMatch : SyncList<Match> { }

    public class MatchMaker : NetworkBehaviour {

        public static MatchMaker instance;
        [SyncVar] public int gamesStarted = 0;
        [SyncVar] public int tutStarted = 0;
        public SyncListMatch matches = new SyncListMatch ();
        public SyncList<string> matchIDs = new SyncList<string>();

        [SerializeField] int maxMatchPlayers = 4;

        void Start () {
            Application.targetFrameRate = 30;
            instance = this;
            Debug.Log("Lobby Loaded");
        }


        public bool HostGame (string _matchID, GameObject _player, bool publicMatch, out int playerIndex) {
            playerIndex = -1;

            if (!matchIDs.Contains (_matchID)) {
                matchIDs.Add (_matchID);
                Match match = new Match (_matchID, _player, publicMatch);
                matches.Add (match);
                Debug.Log ($"Match generated");
                _player.GetComponent<CardPlayer> ().currentMatch = match;
                playerIndex = 1;
                _player.GetComponent<CardPlayer>().isHost = true;
                return true;
            } else {
                Debug.Log ($"Match ID already exists");
                return false;
            }
        }

        public bool JoinGame (string _matchID, GameObject _player, out int playerIndex) {
            playerIndex = -1;

            if (matchIDs.Contains (_matchID)) {

                for (int i = 0; i < matches.Count; i++) {
                    if (matches[i].matchID == _matchID) {
                        if (!matches[i].inMatch && !matches[i].matchFull) {
                            matches[i].players.Add (_player);
                            _player.GetComponent<CardPlayer> ().currentMatch = matches[i];
                            _player.GetComponent<CardPlayer>().isHost = false;
                            playerIndex = matches[i].players.Count;

                            if (matches[i].players.Count == maxMatchPlayers) {
                                matches[i].matchFull = true;
                            }

                            break;
                        } else {
                            return false;
                        }
                    }
                }

                Debug.Log ($"Match joined");
                return true;
            } else {
                Debug.Log ($"Match ID does not exist");
                return false;
            }
        }

        public bool JoinMidGame(string _matchID, GameObject _player, out int playerIndex)
        {
            playerIndex = -1;

            if (matchIDs.Contains(_matchID))
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    if (matches[i].matchID == _matchID)
                    {
                        if (!matches[i].matchFull)
                        {
                            matches[i].players.Add(_player);
                            _player.GetComponent<CardPlayer>().currentMatch = matches[i];
                            _player.GetComponent<CardPlayer>().isHost = false;
                            playerIndex = matches[i].players.Count;

                            if (matches[i].players.Count == maxMatchPlayers)
                            {
                                matches[i].matchFull = true;
                            }

                            break;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                Debug.Log($"Match joined");
                return true;
            }
            else
            {
                Debug.Log($"Match ID does not exist");
                return false;
            }
        }


        public void BeginGame (string _matchID, string Map, int _difficulty) {
            List<GameObject> objectsTodelete = new List<GameObject>();
            List<GameObject> newObjectstoAdll = new List<GameObject>();
            for (int i = 0; i < matches.Count; i++) {
                if (matches[i].matchID == _matchID) {
                    matches[i].inMatch = true;
                    foreach (var player in matches[i].players)
                    {
                        CardPlayer _player = player.GetComponent<CardPlayer> ();
                        if(_player.CharacterSelected == 1)
                        {
                            _player.StartGame(Map, _difficulty);
                        }
                        /*
                        else
                        {
                            GameObject toDelete;
                            GameObject toAdd;
                            int _selectedChar = _player.CharacterSelected;
                            Debug.Log($"Spawning new player char : {_selectedChar}");
                            ReplacePlayer(player.GetComponent<NetworkIdentity>().connectionToClient, NetworkManager.singleton.GetComponent<NetworkManager>().CharacterPrefabs[_selectedChar-1], out toDelete, out toAdd).StartGame(Map, _difficulty);
                            objectsTodelete.Add(toDelete);
                            newObjectstoAdll.Add(toAdd);
                        }      
                        */
                    }
                    foreach (var newPlayer in newObjectstoAdll)
                    {
                        matches[i].players.Add(newPlayer);                      
                    }
                    foreach (var oldPlayer in objectsTodelete)
                    {                                         
                        NetworkServer.Destroy(oldPlayer);                        
                    }
                    gamesStarted++;
                    break;
                }
            }
        }

        CardPlayer ReplacePlayer(NetworkConnectionToClient conn, GameObject newPrefab, out GameObject toDelete, out GameObject toAdd)
        {
            // Cache a reference to the current player object
            toDelete = conn.identity.gameObject;
            CardPlayer oldPlayerComp = toDelete.GetComponent<CardPlayer>();
            // Instantiate the new player object and broadcast to clients
            NetworkServer.ReplacePlayerForConnection(conn, Instantiate(newPrefab));
            toAdd = conn.identity.gameObject;
            CardPlayer _newPlayer = toAdd.GetComponent<CardPlayer>();
            toAdd.GetComponent<NetworkMatch>().matchId = toDelete.GetComponent<NetworkMatch>().matchId;            
            _newPlayer.matchID      = oldPlayerComp.matchID;
            _newPlayer.playerIndex  = oldPlayerComp.playerIndex;
            _newPlayer.currentMatch = oldPlayerComp.currentMatch;
            if (_newPlayer.playerIndex == 1)
                _newPlayer.isHost = true;
            return _newPlayer;                     
        }

        public static string GetRandomMatchID () {
            string _id = string.Empty;
            for (int i = 0; i < 5; i++) {
                int random = UnityEngine.Random.Range (0, 36);
                if (random < 26) {
                    _id += (char) (random + 65);
                } else {
                    _id += (random - 26).ToString ();
                }
            }
            Debug.Log ($"Random Match ID: {_id}");
            return _id;
        }

        public void PlayerDisconnected (CardPlayer player, string _matchID) {
            for (int i = 0; i < matches.Count; i++) {
                if (matches[i].matchID == _matchID) {
                    int playerIndex = matches[i].players.IndexOf (player.gameObject);
                    matches[i].players.RemoveAt (playerIndex);
                    Debug.Log ($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining");

                    if (matches[i].players.Count == 0) {
                        Debug.Log ($"No more players in Match. Terminating {_matchID}");
                        matches.RemoveAt (i);
                        matchIDs.Remove (_matchID);
                    }
                    break;
                }
            }
        }        

    }

    public static class MatchExtensions {
        public static Guid ToGuid (this string id) {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider ();
            byte[] inputBytes = Encoding.Default.GetBytes (id);
            byte[] hashBytes = provider.ComputeHash (inputBytes);

            return new Guid (hashBytes);
        }
    }

}