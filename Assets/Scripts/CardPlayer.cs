using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardPlayer : NetworkBehaviour
{
    public static CardPlayer localPlayer;
    [SyncVar] public int playerIndex;
    List<CardPlayer> cardPlayers = new List<CardPlayer>();
    private void Start()
    {
        if (isLocalPlayer)
            localPlayer = this;
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
    }
}
