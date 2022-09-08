using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;

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
    public float DelayBeforeResetBoard = 1;
    public int SwitchCaseDeckRules = 0;
    public static UnityEvent<int>  CheckUI;
    public static UnityEvent  EndGame;
    public static UnityEvent<string, int> SlapSuccess;
    public static UnityEvent SlapAnimation;
    public static UnityEvent HitCard;
    public static UnityEvent StartGame;
    public DeckControllerRazboi RefToController;
    public bool RoundEndTriggered = false;
    public List<List<CardValueType>> PlayerDecks = new List<List<CardValueType>>();
    public List<CardPlayer> Players = new List<CardPlayer>();
    public List<CardValueType> CardsLostToSlap = new List<CardValueType>();
    int IndexOfPlayerWhoTriggeredRoundEnd;    
    float LastHitTime;
    float SlapTime;
    bool StopInputAtRoundWin = false;
    [SyncVar] public bool InititalSetupDone = false;
    [SyncVar] public int CardsToHit;    
    [SyncVar] public int IndexOfActivePlayer = 0;    
    [SyncVar] public CardValueType SlapCard;
    public SyncList<int> SlapsLeft = new SyncList<int>();   
    public SyncListCards CardsOnGround = new SyncListCards();
    

    public SyncList<int> CardCount = new SyncList<int>();
    public SyncList<string> PlayerNames = new SyncList<string>();




    public CardPlayer firstPlayer { get 
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
        SlapCard = null;
    }   
   
    private void Start()
    {         
        if (Application.isBatchMode)
        {
            Debug.Log("I am SERVER");
            StartCoroutine(Setup());
        }      
    }

    private void Update()
    {
        if (Application.isBatchMode)
        {
            UpdateValuesForVisuals();
        }
    }

    IEnumerator Setup()
    {
        // Server is waiting until a player calls CardPlayer.localPlayer.BuildDeck();
        while (!DeckControllerRazboi.instance.done)
        {           
            yield return null;
        }
        ServerBackup.RegisterStartTime();
        Debug.Log("dispersing cards");
        IndexOfActivePlayer = 0;
        DisperseCardsBetweenPlayers();
        SlapCard = null;
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
        CardCount.Clear();
        PlayerNames.Clear();
        //SlapsLeft = 3;
        CardsToHit = 1;
        PlayerDecks.Clear();
        Players.Clear();
        DeckControllerRazboi.instance.AssambledDeck.Clear();
        StopAllCoroutines();
        StartCoroutine(Setup());
    }

    void UpdateValuesForVisuals()
    {
        for(int i = 0; i < PlayerDecks.Count; i++ )
        {
            CardCount[i] = PlayerDecks[i].Count;            
        }
    }
    #region HandlePlayerInput


    public void HitCards(int indexLocalPlayer,string playerName)
    {
        if (!InititalSetupDone) return;
        if (StopInputAtRoundWin) return;
        if (indexLocalPlayer != IndexOfActivePlayer) return;

        //start measure Time
        LastHitTime = Time.realtimeSinceStartup;
       

        ServerBackup.AddHitToList(indexLocalPlayer,playerName);         
        CardsToHit--;

        //card pile on ground, primeste top card-ul playerului care apasa butonul
        CardsOnGround.Add(PlayerDecks[indexLocalPlayer][0]);
        PlayerDecks[indexLocalPlayer].RemoveAt(0);

        //CONSOLE OUT
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut($"Player {indexLocalPlayer} hit card {CardsOnGround[CardsOnGround.Count - 1].CardValue}_{CardsOnGround[CardsOnGround.Count - 1].CardType}");

        //
        //check if card > 10 , Yes = trigger round end, No = continue
        if (CardsOnGround[CardsOnGround.Count - 1].CardValue > 10 /* 9 */)
        {
            RoundEndTriggered = true;
            IndexOfPlayerWhoTriggeredRoundEnd = indexLocalPlayer;

            SwitchInSwitchlol(CardsOnGround[CardsOnGround.Count - 1].CardValue);
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
    #region SwitchInSwitchlol
    private void SwitchInSwitchlol(int checkValue)
    {
        switch(SwitchCaseDeckRules)
        {
            case 0:
                {
                    //Default Behavior
                    switch (checkValue)
                    {
                        //case 10: { CardsToHit = 1; break; }
                        case 12:
                            { // J is a free pass
                                CardsToHit = 1;
                                //RoundEndTriggered = false;
                                break;
                            }
                        case 13: { CardsToHit = 2; break; }
                        case 14: { CardsToHit = 3; break; }
                        case 15: { CardsToHit = 4; break; }
                    }
                    break;
                }
            case 1:
                {
                    //Requested 12 pass Behavior
                    switch (checkValue)
                    {
                        //case 10: { CardsToHit = 1; break; }
                        case 12:
                            { // J is a free pass
                                CardsToHit = 1;
                                RoundEndTriggered = false;
                                break;
                            }
                        case 13: { CardsToHit = 1; break; }
                        case 14: { CardsToHit = 2; break; }
                        case 15: { CardsToHit = 3; break; }
                    }
                    break;
                }
            case 2:
                {
                    //Requested 12 pass Behavior + Normal others
                    switch (checkValue)
                    {
                        //case 10: { CardsToHit = 1; break; }
                        case 12:
                            { // J is a free pass
                                CardsToHit = 1;
                                RoundEndTriggered = false;
                                break;
                            }
                        case 13: { CardsToHit = 2; break; }
                        case 14: { CardsToHit = 3; break; }
                        case 15: { CardsToHit = 4; break; }
                    }
                    break;
                }
            case 3:
                {
                    //BulletGame Behavior
                    switch (checkValue)
                    {
                        //case 10: { CardsToHit = 1; break; }
                        case 12:
                            { // J is a free pass
                                CardsToHit = 4;
                                break;
                            }
                        case 13: { CardsToHit = 4; break; }
                        case 14: { CardsToHit = 4; break; }
                        case 15: { CardsToHit = 4; break; }
                    }
                    break;
                }
            default:
                {
                    //Default Behavior
                    switch (checkValue)
                    {
                        //case 10: { CardsToHit = 1; break; }
                        case 12:
                            { // J is a free pass
                                CardsToHit = 1;
                                //RoundEndTriggered = false;
                                break;
                            }
                        case 13: { CardsToHit = 2; break; }
                        case 14: { CardsToHit = 3; break; }
                        case 15: { CardsToHit = 4; break; }
                    }
                    break;
                }
        }
    }
    #endregion
    public void SlapCards(int IndexOfSlappingPlayer,string PlayerName, out bool Success , out int ReactionTime )
    {
        Success = false;
        ReactionTime = 0;
        if (!InititalSetupDone) return;
        if (StopInputAtRoundWin) return;
        if (SlapsLeft[IndexOfSlappingPlayer] <= 0) return;
        if (PlayerDecks[IndexOfSlappingPlayer].Count <= 0) return;
        if (CardsOnGround.Count < 2) return;
  
        SlapsLeft[IndexOfSlappingPlayer]--;

        SlapTime = Time.realtimeSinceStartup;
        //Debug.Log(SlapTime);
        ReactionTime = Mathf.RoundToInt((SlapTime - LastHitTime) * 1000);
        if (CheckSlapRules())
            {
            Success = true;
            WinRound(IndexOfSlappingPlayer);
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

        //CONSOLE OUTPUT
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut($"Player at Index: {IndexOfSlappingPlayer}, slapped in {ReactionTime}ms, Success={Success}\n");
        string consoleOut = "Last (up to) 3 cards on ground on Slap:\n";
        for (int i = 0; i < Math.Min(CardsOnGround.Count, 3); i++)
        {
            consoleOut += $"CardIndex: {CardsOnGround[CardsOnGround.Count - i - 1].CardSpriteIndex}, CardValue: {CardsOnGround[CardsOnGround.Count - i - 1].CardValue}, CardType: {CardsOnGround[CardsOnGround.Count - i - 1].CardType}\n";
        }
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut(consoleOut);
        //

        ServerBackup.AddSlapToList(IndexOfSlappingPlayer, PlayerName, Success, ReactionTime);
    }
    #endregion
    #region Visuals







    #endregion
    #region Other
    public void UpdateRules(int input)
    {
        SwitchCaseDeckRules = input;
        string localString;
        switch(input)
        {
            case 0: { localString = "Card rules changed to Default"; break; }
            case 1: { localString = "Card rules changed to 12IsPass"; break; }
            case 2: { localString = "Card rules changed to Hybrid"; break; }
            case 3: { localString = "Card rules changed to Bullet"; break; }
            default: { localString = "LMAO"; Debug.Log("big error lol"); break; }
        }
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut(localString);

    }
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
        //firstPlayer.ChangeDecks(PlayerDecks, Players);
        firstPlayer.CheckTurn(IndexOfActivePlayer);       

    }

    public async void WinRound(int indexLocalPlayer)
    {
        StopInputAtRoundWin = true;
        await Task.Delay(System.Convert.ToInt32(DelayBeforeResetBoard * 1000));
        CardsToHit = 1;
        //Didn't hit a +10 and someone before did
        PlayerDecks[indexLocalPlayer].AddRange(CardsOnGround);
        PlayerDecks[indexLocalPlayer].AddRange(CardsLostToSlap);
        SlapCard = null;
        //SlapCard.CardSpriteIndex = 52;
        ShuffleDeck(indexLocalPlayer);
        //CONSOLE OUTPUT
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut($"Index of round winner:{indexLocalPlayer}, gained {CardsOnGround.Count + CardsLostToSlap.Count} cards");

        //
        CardsOnGround.Clear();
        CardsLostToSlap.Clear();
        for(int i = 0; i < SlapsLeft.Count; i++)
        {
            SlapsLeft[i] = InitialSlapConter;
        }
        
        StopInputAtRoundWin = false;
        RoundEndTriggered = false;
        IndexOfActivePlayer = indexLocalPlayer;

        firstPlayer.CheckTurn(indexLocalPlayer);

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
        Debug.Log("Next Player");
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
        //CONSOLE OUTPUT
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut($"UpdatePlayerTurn_Index:{IndexOfActivePlayer}");
        //
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
        Debug.Log("Stay same player");
        if (IndexOfActivePlayer >= PlayerDecks.Count)
        {
            IndexOfActivePlayer = 0;
        }

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

    public void RemovePlayer(int index)
    {
        Debug.Log($"Removing player with index {index}");
        SlapsLeft.RemoveAt(index);
        CardCount.RemoveAt(index);
        Players.RemoveAt(index);
        PlayerNames.RemoveAt(index);

        //split cards
        if (PlayerDecks[index].Count <= 0)
        {
            PlayerDecks.RemoveAt(index);
        }
        else 
        {
            List<CardValueType> tempList = PlayerDecks[index];
            PlayerDecks.RemoveAt(index);
            for (int i = 0; i < tempList.Count; i++)
            {
                if(PlayerDecks[i % PlayerDecks.Count].Count > 0)
                    PlayerDecks[i % PlayerDecks.Count].Add(tempList[i]);
            }
        }

        //rename indexes
        foreach(CardPlayer player in Players)
        {
            int newIndex = Players.IndexOf(player);
            player.playerIndex = newIndex;
            player.InstantIndexUpdate(newIndex);
        }

        //RefreshTurn
        if(index < IndexOfActivePlayer) {IndexOfActivePlayer--;}
        StaySamePlayer();
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
