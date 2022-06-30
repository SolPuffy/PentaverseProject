using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

[System.Serializable]
public class SyncListObjects : SyncList<GameObject> { }

[System.Serializable]
public class SyncListCards : SyncList<CardValueType> { }

[System.Serializable]
public class SyncListDecks : List<List<CardValueType>> { }


public class HitSlapRazboi : NetworkBehaviour
{
    public static HitSlapRazboi instance;

    public int InitialSlapConter = 3;    
    public static UnityEvent<int>  CheckUI;
    public static UnityEvent  EndGame;
    public static UnityEvent<string, int> SlapSuccess;
    public DeckControllerRazboi RefToController;
    public bool RoundEndTriggered = false;
    public List<List<CardValueType>> PlayerDecks = new List<List<CardValueType>>();
    float LastHitTime;
    float SlapTime;    

    [SyncVar] public bool InititalSetupDone = false;
    [SyncVar] public int CardsToHit;
    [SyncVar] public int IndexOfPlayerWhoTriggeredRoundEnd;
    [SyncVar] public int IndexOfActivePlayer = 0;
    [SyncVar] public int IndexOfSlappingPlayer = 0;
    [SyncVar] public CardValueType SlapCard = null;
    public SyncList<int> SlapsLeft = new SyncList<int>();    
    public SyncListCards CardsOnGround = new SyncListCards();
    public SyncListCards CardsLostToSlap = new SyncListCards();
    public SyncListObjects Players = new SyncListObjects();

    CardPlayer firstPlayer { get 
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if(players.Length > 0)
            {
                return players[0].GetComponent<CardPlayer>();
            }
            else
            {
                return null;
            }
        } 
        set { } }

    
    private void Awake()
    {
        instance = this;
    }   
    public void ResetScene()
    {
        ServerBackup.CleanDataHold();
        InititalSetupDone = false;
        DeckControllerRazboi.instance.done = false;
        IndexOfActivePlayer = 0;
        CardsOnGround.Clear();
        CardsLostToSlap.Clear();
        SlapCard = null;
        SlapsLeft.Clear();
        //SlapsLeft = 3;
        CardsToHit = 1;
        PlayerDecks.Clear();
        Players.Clear();
        DeckControllerRazboi.instance.AssambledDeck.Clear();
        StopAllCoroutines();
        StartCoroutine(Setup());
    }
    private void Start()
    {         
        if (Application.isBatchMode)
        {
            Debug.Log("I am SERVER");
            StartCoroutine(Setup());
        }      
    }

    

    IEnumerator Setup()
    {
        // Server is waiting until a player calls CardPlayer.localPlayer.BuildDeck();
        while (!DeckControllerRazboi.instance.done)
        {           
            yield return null;
        }
        Debug.Log("dispersing cards");
        IndexOfActivePlayer = 0;
        DisperseCardsBetweenPlayers();
        SlapCard = null;        
    }    
    #region HandlePlayerInput


    public void HitCards(int indexLocalPlayer)
    {
        if (!InititalSetupDone) return;
        if (indexLocalPlayer != IndexOfActivePlayer) return;

        //start measure Time
        LastHitTime = Time.realtimeSinceStartup;
       

        ServerBackup.AddHitToList(indexLocalPlayer);         
        CardsToHit--;

        //card pile on ground, primeste top card-ul playerului care apasa butonul
        CardsOnGround.Add(PlayerDecks[indexLocalPlayer][0]);
        PlayerDecks[indexLocalPlayer].RemoveAt(0);
       
        //check if card > 10 , Yes = trigger round end, No = continue
        if (CardsOnGround[CardsOnGround.Count - 1].CardValue > 10 /* 9 */)
        {
            RoundEndTriggered = true;
            IndexOfPlayerWhoTriggeredRoundEnd = indexLocalPlayer;

            switch (CardsOnGround[CardsOnGround.Count - 1].CardValue)
            {
                //case 10: { CardsToHit = 1; break; }
                case 12: { // J is a free pass
                        CardsToHit = 1;
                        RoundEndTriggered = false;
                        break;
                    }
                case 13: { CardsToHit = 1; break; }
                case 14: { CardsToHit = 2; break; }
                case 15: { CardsToHit = 3; break; }
            }
            
            NextPlayer();
        }
        else
        {
            if (CardsToHit < 1)
            {
                if (RoundEndTriggered)
                {
                    WinRound(IndexOfPlayerWhoTriggeredRoundEnd);
                }
                else
                {
                    CardsToHit = 1;
                    NextPlayer();
                }
            }
            else
            {
                StaySamePlayer();
            }
        }
        //
    }
    public void SlapCards(int IndexOfSlappingPlayer, out bool Success , out int ReactionTime )
    {
        Success = false;
        ReactionTime = 0;
        if (!InititalSetupDone) return;
        if (SlapsLeft[IndexOfSlappingPlayer] <= 0) return;
        if (PlayerDecks[IndexOfSlappingPlayer].Count <= 0) return;

        ServerBackup.AddSlapToList(IndexOfSlappingPlayer);
        instance.IndexOfSlappingPlayer = IndexOfSlappingPlayer;
        SlapsLeft[IndexOfSlappingPlayer]--;

        SlapTime = Time.realtimeSinceStartup;
        //Debug.Log(SlapTime);
        ReactionTime = Mathf.RoundToInt((SlapTime - LastHitTime) * 1000);

        if (CheckSlapRules())
            {                
                WinRound(IndexOfSlappingPlayer);
                Success = true;               
                Debug.Log($"Slap result : {Success.ToString()}  {ReactionTime.ToString()}");
            }
            else
            {
                //lose 1 card, continue game
                Success = false;   
            
                SlapCard = PlayerDecks[IndexOfSlappingPlayer][0];
                CardsLostToSlap.Add(PlayerDecks[IndexOfSlappingPlayer][0]);
                PlayerDecks[IndexOfSlappingPlayer].RemoveAt(0);   

                Debug.Log($"Slap result : {Success.ToString()}  {ReactionTime.ToString()}");
            }
           
    }
    #endregion
    #region Visuals

    
   
    

    
   
    #endregion
    #region Other
    private void DisperseCardsBetweenPlayers()
    {
        
        for (int i = 0; i < RefToController.AssambledDeck.Count; i++)
        {           
            PlayerDecks[i % PlayerDecks.Count].Add(RefToController.AssambledDeck[i]);
        }   
        

        //SCUUUFED Update Decks
        Debug.Log("Finished Setying up decks for players.  Starting Game");
        InititalSetupDone = true;
        firstPlayer.SetupDone();
        firstPlayer.ChangeDecks(PlayerDecks);
        firstPlayer.CheckTurn(IndexOfActivePlayer);       

    }

    public void WinRound(int indexLocalPlayer)
    {
        CardsToHit = 1;
        //Didn't hit a +10 and someone before did
        PlayerDecks[indexLocalPlayer].AddRange(CardsOnGround);
        PlayerDecks[indexLocalPlayer].AddRange(CardsLostToSlap);
        SlapCard = null;
        ShuffleDeck(indexLocalPlayer);
        CardsOnGround.Clear();
        CardsLostToSlap.Clear();
        
        for(int i = 0; i < SlapsLeft.Count; i++)
        {
            SlapsLeft[i] = InitialSlapConter;
        }
        RoundEndTriggered = false;
        IndexOfActivePlayer = IndexOfPlayerWhoTriggeredRoundEnd;

        firstPlayer.CheckTurn(IndexOfActivePlayer);

        CheckPlayerVictory(indexLocalPlayer);
    }

    void IncrementActivePlayer()
    {
        IndexOfActivePlayer++;
        if (IndexOfActivePlayer >= PlayerDecks.Count)
        {
            IndexOfActivePlayer = 0;
        }
    }
    public void NextPlayer()
    {
        IncrementActivePlayer();

        int whilecounter = 0;
        while (PlayerDecks[IndexOfActivePlayer].Count == 0)
        {
            IncrementActivePlayer();
            whilecounter++;
            if(whilecounter > 10)
            {
                Debug.LogWarning("no valid decks found");
                return;                
            }
        }
        firstPlayer.CheckTurn(IndexOfActivePlayer); 
    }
    public void CheckPlayerVictory(int indexLocalPlayer)
    {
        if (PlayerDecks[indexLocalPlayer].Count == RefToController.AssambledDeck.Count)
        {
            firstPlayer.EndGame();
            ServerBackup.PerformServerBackup();
            return;           
        }
    }

    public void StaySamePlayer()
    {
        if (PlayerDecks[IndexOfActivePlayer].Count == 0)
            NextPlayer();
        else
        {
            firstPlayer.CheckTurn(IndexOfActivePlayer);
        }
    }
    
    //shuffle deck after taking cards from table
    public void ShuffleDeck(int indexLocalPlayer)
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, PlayerDecks[indexLocalPlayer].Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, PlayerDecks[indexLocalPlayer].Count - 1);

            auxShuffleValue = PlayerDecks[indexLocalPlayer][cacheRandomResult];
            PlayerDecks[indexLocalPlayer][cacheRandomResult] = PlayerDecks[indexLocalPlayer][cacheRandomResult2];
            PlayerDecks[indexLocalPlayer][cacheRandomResult2] = auxShuffleValue;
        }
    }   

    public bool CheckSlapRules()
    {
        try
        {
            if (RefToController.SpecialRulesToggles.Last2AddTo10 && (CardsOnGround[CardsOnGround.Count - 1].CardValue + CardsOnGround[CardsOnGround.Count - 2].CardValue == 10))
            {
                return true;
            }
            if (RefToController.SpecialRulesToggles.TwoEqualInARow && (CardsOnGround[CardsOnGround.Count - 1].CardValue == CardsOnGround[CardsOnGround.Count - 2].CardValue))
            {
                return true;
            }
            if (RefToController.SpecialRulesToggles.KQ_or_QK && (CardsOnGround[CardsOnGround.Count - 1].CardValue == 14 && CardsOnGround[CardsOnGround.Count - 2].CardValue == 13) || (CardsOnGround[CardsOnGround.Count - 1].CardValue == 13 && CardsOnGround[CardsOnGround.Count - 2].CardValue == 14))
            {
                return true;
            }
            if (RefToController.SpecialRulesToggles.JQ_or_QJ && (CardsOnGround[CardsOnGround.Count - 1].CardValue == 12 && CardsOnGround[CardsOnGround.Count - 2].CardValue == 13) || (CardsOnGround[CardsOnGround.Count - 1].CardValue == 13 && CardsOnGround[CardsOnGround.Count - 2].CardValue == 12))
            {
                return true;
            }
        }
        catch
        {
            Debug.Log("rule w/ 2 cards overflow");
            //Failed since there aren't atleast 2 cards on the table
        }
        try
        {
            if (RefToController.SpecialRulesToggles.TwoSandwichOne && (CardsOnGround[CardsOnGround.Count - 1].CardValue == CardsOnGround[CardsOnGround.Count - 3].CardValue))
            {
                return true;
            }
        }
        catch
        {
            Debug.Log("rule w/ 3 cards overflow");
            //Failed since there aren't atleast 3 cards on the table
        }
        return false;
    }
   
    #endregion

    private void OnDestroy()
    {
        InititalSetupDone = false;
        CheckUI.RemoveAllListeners();
        EndGame.RemoveAllListeners();
        SlapSuccess.RemoveAllListeners();
    }
}
