using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardPlayer : NetworkBehaviour
{
    public static CardPlayer localPlayer;
    [SyncVar] public int playerIndex = 20;
    List<CardPlayer> cardPlayers = new List<CardPlayer>();
    
    private void Start()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
            AddDeck();
            SetPlayerIndex();
        }
    }
    [ClientRpc]
    public void CheckTurn(int index)
    {
        HitSlapRazboi.instance.CheckUIButtons(index);
    }

    [Command]
    public void HitCards()
    {
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

    [Command]
    public void AddDeck()
    {
        HitSlapRazboi.instance.PlayerDecks.Add(new List<CardValueType>());
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

    
    [Command]
    public void SetPlayerIndex()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject p in Players)
        {
            cardPlayers.Add(p.GetComponent<CardPlayer>());
        }
       
        playerIndex = cardPlayers.IndexOf(this);
        Debug.Log("Setting index to : " + playerIndex);
    }
}
