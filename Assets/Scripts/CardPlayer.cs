using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardPlayer : NetworkBehaviour
{
    public static CardPlayer localPlayer;
    [SyncVar] public int playerIndex = 20;
    [SyncVar] public string Nome = "P";
    List<CardPlayer> cardPlayers = new List<CardPlayer>();

    public override void OnStopClient()
    {
        Debug.Log($"Client {name}, index {playerIndex} Stopped  ");
        //ClientDisconnect();
    }

    public override void OnStopServer()
    {
        Debug.Log($"Client {name}, index {playerIndex} Stopped on Server");        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
       
        if((players.Length - 1) == 0)
        {
            Debug.Log($"No Players Left. Resetting ...");
            HitSlapRazboi.instance.ResetScene();
        }       
    }

    public override void OnStartServer()
    {
        Debug.Log($"Client {name} connected on Server");        
    }

    [ClientRpc]
    public void SetupDone()
    {
        HitSlapRazboi.instance.InititalSetupDone = true;
    }
   
    private void Start()
    {        
        if (isLocalPlayer)
        {           
            localPlayer = this;           
            if (!HitSlapRazboi.instance.InititalSetupDone)
            {               
                {
                    SetPlayerIndex();                                       
                }
            }
            else
            {              
                NetworkManager.singleton.StopClient();                  
            }            
        }
    }   
    [ClientRpc]
    public void EndGame()
    {
        HitSlapRazboi.EndGame.Invoke();
       // HitSlapRazboi.instance.ExecuteEndGame();
    }
    
    [ClientRpc]
    public void CheckTurn(int index)
    {
        //HitSlapRazboi.instance.CheckUIButtons(index);
        HitSlapRazboi.CheckUI.Invoke(index);
    }

    [Command]
    public void HitCards()
    {        
        if(playerIndex != HitSlapRazboi.instance.IndexOfActivePlayer) { Debug.LogWarning($"WrongHit turn {name}"); return; }
        Debug.Log($"hitting cards {name} with index {playerIndex}");
        HitSlapRazboi.instance.HitCards(playerIndex);
        ChangeDecks(HitSlapRazboi.instance.PlayerDecks, HitSlapRazboi.instance.Players);
    }

    [Command]
    public void SlapCards()
    {
        Debug.Log($"Trying to slap cards {name} with index {playerIndex}");
        HitSlapRazboi.instance.SlapCards(playerIndex, out bool Success, out int timeSpan);

        ChangeDecks(HitSlapRazboi.instance.PlayerDecks, HitSlapRazboi.instance.Players);

        if(Success) { RegisterWinningSlap(Nome, timeSpan ); }
    }

    [ClientRpc]
    void RegisterWinningSlap ( string Name, int reactionTime )
    {
        Debug.Log($"{Name} successfully slapped with reaction time : {reactionTime.ToString()}ms");
        HitSlapRazboi.SlapSuccess.Invoke(Name, reactionTime);
    }
    [Command]
    public void BuildDeck()
    {
        DeckControllerRazboi.instance.BuildDeck();
    }
    
    public void AddDeck()
    {
        HitSlapRazboi.instance.PlayerDecks.Add(new List<CardValueType>());
        HitSlapRazboi.instance.SlapsLeft.Add(HitSlapRazboi.instance.InitialSlapConter);
        HitSlapRazboi.instance.Players.Add(gameObject);
        ChangeDecks(HitSlapRazboi.instance.PlayerDecks, HitSlapRazboi.instance.Players);
    }

    //SCUUFED  
   

    [ClientRpc]
    public void ChangeDecks(List<List<CardValueType>> listOfDecks, List<GameObject> players) 
    {
        HitSlapRazboi.instance.PlayerDecks = listOfDecks;
        HitSlapRazboi.instance.Players = players;
        Debug.Log(ShowDecksCount(HitSlapRazboi.instance.PlayerDecks));
    }

    string ShowDecksCount(List<List<CardValueType>> listOfDecks)
    {
        string t = "";
        foreach(List<CardValueType> CardList in listOfDecks)
        {
            t += $"Deck {listOfDecks.IndexOf(CardList)} has {CardList.Count} cards. ";
        }

        return t;

    }
    [TargetRpc]
    public void DC()
    {
        NetworkManager.singleton.StopClient();
    }
    
    [Command]
    void SetPlayerIndex()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject p in Players)
        {
            cardPlayers.Add(p.GetComponent<CardPlayer>());
        }
       
        playerIndex = cardPlayers.IndexOf(this);        

        if (playerIndex > 4) 
        {
            DC(); 
        }
        else
        {
            Debug.Log($"Setting {name} index to : " + playerIndex);
            
            SetName();            
            AddDeck();
        }

      
    }    

    void SetName()
    {
        Nome = "P " + (playerIndex + 1).ToString();
        Debug.Log($"Setting {name} name to : " + Nome);
    }
}
