using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardPlayer : NetworkBehaviour
{
    public static CardPlayer localPlayer;
    [SyncVar] public int playerIndex = 20;
    [SyncVar] public string Nome = "P";
    [SyncVar] public bool HasEntered = false;
    //List<CardPlayer> cardPlayers = new List<CardPlayer>();
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
    public override void OnStopClient()
    {
        Debug.Log($"Client {name}, index {playerIndex} Stopped  ");
        //ClientDisconnect();
    }

    public override void OnStopServer()
    {
        Debug.Log($"Client {name}, index {playerIndex} Stopped on Server");        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if ((players.Length - 1) == 0)
        {
            Debug.Log($"No Players Left. Resetting ...");
            HitSlapRazboi.instance.ResetScene();
            return;
        }

        if (!HasEntered)
        {
            Debug.Log("Player tried to join with no space left");
        }
        else
        {
            HitSlapRazboi.instance.RemovePlayer(playerIndex);
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

    [Command]
    public void RequestConsoleUpdate(string input)
    {
        DisplayConsoleOut(input);
    }
    [ClientRpc]
    private void DisplayConsoleOut(string input)
    {
        ScriptToConsoleOut.UpdateConsole(input);
    }
   
    [TargetRpc]
    public void InstantIndexUpdate(int newIndex)
    {
        Debug.Log($"Updating index to {newIndex}");
        playerIndex = newIndex;
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
        HitSlapRazboi.instance.HitCards(playerIndex,Nome);
        //ChangeDecks(HitSlapRazboi.instance.PlayerDecks, HitSlapRazboi.instance.Players);
    }

    [Command]
    public void SlapCards()
    {
        Debug.Log($"Trying to slap cards {name} with index {playerIndex}");
        HitSlapRazboi.instance.SlapCards(playerIndex,Nome, out bool Success, out int timeSpan);

       // ChangeDecks(HitSlapRazboi.instance.PlayerDecks, HitSlapRazboi.instance.Players);

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
        HitSlapRazboi.instance.CardCount.Add(0);
        HitSlapRazboi.instance.Players.Add(this);
        playerIndex = HitSlapRazboi.instance.Players.IndexOf(this);       
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
        
        if (Players.Length > 4) 
        {
            DC(); 
        }
        else
        {
            HasEntered = true;
            AddDeck();
            SetName();
            Debug.Log($"Setting {name} index to : " + playerIndex);     
        }      
    }    

    void SetName()
    {
        Nome = "P " + (playerIndex + 1).ToString();
        HitSlapRazboi.instance.PlayerNames.Add(Nome);
        Debug.Log($"Setting {name} name to : " + Nome);
    }
}
