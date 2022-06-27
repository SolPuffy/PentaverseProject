using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardPlayer : NetworkBehaviour
{
    public static CardPlayer localPlayer;
    [SyncVar] public int playerIndex = 20;
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

        //ServerDisconnect();
    }

    public override void OnStartServer()
    {
        Debug.Log($"Client {name} connected on Server");        
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
        HitSlapRazboi.instance.ExecuteEndGame();
    }
    
    [ClientRpc]
    public void CheckTurn(int index)
    {
        HitSlapRazboi.instance.CheckUIButtons(index);
    }

    [Command]
    public void HitCards()
    {
        Debug.Log("hitting cards");
        HitSlapRazboi.instance.HitCards(playerIndex);
        ChangeDecks(HitSlapRazboi.instance.PlayerDecks);
    }

    [Command]
    public void SlapCards()
    {
        HitSlapRazboi.instance.SlapCards(playerIndex);
        ChangeDecks(HitSlapRazboi.instance.PlayerDecks);
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
        ChangeDecks(HitSlapRazboi.instance.PlayerDecks);
    }

    //SCUUFED  
   

    [ClientRpc]
    public void ChangeDecks(List<List<CardValueType>> listOfDecks) 
    {
        HitSlapRazboi.instance.PlayerDecks = listOfDecks;
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
    public void SetPlayerIndex()
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
            Debug.Log("Setting index to : " + playerIndex);
            AddDeck();
        }

      
    }    
}
