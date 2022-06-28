using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SyncListGameObject : SyncList<GameObject> { }

[System.Serializable]
public class SyncListCards : SyncList<CardValueType> { }

[System.Serializable]
public class SyncListDecks : List<SyncListCards> { }


public class HitSlapRazboi : NetworkBehaviour
{
    [SerializeField] public int InitialSlapConter = 3;

    public Dictionary<Tuple<int, string>, Sprite> CardImages = new Dictionary<Tuple<int, string>, Sprite>();

    public static HitSlapRazboi instance;   

    public readonly SyncListCards SimpleCards = new SyncListCards();

    public DeckControllerRazboi RefToController;

    public Button HitButton;
    public Button SlapButton;
    public GameObject StartGame;
    public GameObject EndGame;

    public Image SlapImage;
    public Image CardSlot0;
    public Image CardSlot1;
    public Image CardSlot2;

    public Material normalMat;
    public Material activeMat;

    public Sprite BlankSprite;

    [SyncVar] public bool InititalSetupDone = false;
    [SyncVar] public int CardsToHit;
    [SyncVar] public int IndexOfPlayerWhoTriggeredRoundEnd;
    [SyncVar] public int IndexOfActivePlayer = 0;
    [SyncVar] public int IndexOfSlappingPlayer = 0;
    public SyncList<int> SlapsLeft = new SyncList<int>();
    public bool RoundEndTriggered = false;

    public List<MeshRenderer> PlayerSpheres = new List<MeshRenderer>();
    public List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI slapCounter;
    public List<GameObject> PlayerVisualDecks = new List<GameObject>();

    public List<List<CardValueType>> PlayerDecks = new List<List<CardValueType>>();

    [SyncVar] CardValueType SlapCard = null;

    

    public SyncListCards CardsOnGround = new SyncListCards();
    public SyncListCards CardsLostToSlap = new SyncListCards();

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
        EndGame.SetActive(false);
        StartGame.SetActive(false);
        DeactivateVisualDecks();

        if (NetworkManager.singleton == null)
        {
            SceneManager.LoadScene("ConnectScene");
        }        
    }
    public void DeactivateVisualDecks()
    {
        for (int i = 0; i < 5; i++)
        {
            PlayerVisualDecks[i].SetActive(false);
            PlayerSpheres[i].gameObject.SetActive(false);
            PlayerCardCount[i].gameObject.SetActive(false);
        }
    }
    public void ActivateVisualDecks(int index)
    {
        PlayerVisualDecks[index].SetActive(true);
        PlayerSpheres[index].gameObject.SetActive(true);
        PlayerCardCount[index].gameObject.SetActive(true);
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
        DeckControllerRazboi.instance.AssambledDeck.Clear();
        StopAllCoroutines();
        StartCoroutine(Setup());
    }
    private void Start()
    {
        if (!Application.isBatchMode)
            FillMegaDictionary();

        AssignColors();
        EnableSlapIfRules();
        if (Application.isBatchMode)
        {
            Debug.Log("I am SERVER");
            StartCoroutine(Setup());
        }
        else 
        {
           StartCoroutine(WaitForLocal());
        }
        //
        //
    }

    IEnumerator WaitForLocal()
    {
        Debug.Log("waiting for local");
        while(CardPlayer.localPlayer == null || CardPlayer.localPlayer.playerIndex > 10)
        {
            //Debug.Log(CardPlayer.localPlayer.playerIndex);
            yield return null;
        }
        Debug.Log("I have local player and index");
        if (CardPlayer.localPlayer.playerIndex == 0)
        {
            Debug.Log("I am HOST");
            //CardPlayer.localPlayer.BuildDeck();
            StartGame.SetActive(true);
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
    private void Update()
    {
        if(Application.isBatchMode) { return; }
        AssignColors();
        CardCountUpdate();
        CardsOnGroundVisual();
        DeactivateVisualDecks();
        for(int i = 0; i < PlayerDecks.Count; i++)
        {
            ActivateVisualDecks(i);
        }
    }
    #region HandlePlayerInput


    public void HitCards(int indexLocalPlayer)
    {
        if (!InititalSetupDone) return;
        if (indexLocalPlayer != IndexOfActivePlayer) return;
        ServerBackup.AddHitToList(indexLocalPlayer);
        //Print Deck of player HIT
        /*
        string _string = $"Deck for player {indexLocalPlayer} :";
        foreach (CardValueType card in PlayerDecks[indexLocalPlayer])
        {
            _string += card.CardValue.ToString() + "; ";
        }
        Debug.Log(_string);
        */

        //HitButton.interactable = false;
        CardsToHit--;
        //card pile on ground, primeste top card-ul playerului care apasa butonul
        CardsOnGround.Add(PlayerDecks[indexLocalPlayer][0]);
        PlayerDecks[indexLocalPlayer].RemoveAt(0);
       
        //check if card > 10 , Yes = trigger round end, No = continue
        if (CardsOnGround[CardsOnGround.Count - 1].CardValue > 10 /* 9 */)
        {
            IndexOfPlayerWhoTriggeredRoundEnd = indexLocalPlayer;
            switch (CardsOnGround[CardsOnGround.Count - 1].CardValue)
            {
                //case 10: { CardsToHit = 1; break; }
                case 12: { CardsToHit = 1; break; }
                case 13: { CardsToHit = 2; break; }
                case 14: { CardsToHit = 3; break; }
                case 15: { CardsToHit = 4; break; }
            }
            RoundEndTriggered = true;
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
    public void SlapCards(int IndexOfSlappingPlayer)
    {
        if (!InititalSetupDone) return;
        if (SlapsLeft[IndexOfSlappingPlayer] <= 0) return;
        if (PlayerDecks[IndexOfSlappingPlayer].Count <= 0) return;

        ServerBackup.AddSlapToList(IndexOfSlappingPlayer);
        instance.IndexOfSlappingPlayer = IndexOfSlappingPlayer;
        SlapsLeft[IndexOfSlappingPlayer]--; 
            if (CheckSlapRules())
            {
                //successfully slapped, take cards wait for bots (delay && conditions to be removed for actual players)
                SlapButton.interactable = false;
                HitButton.interactable = false;
                RoundEndTriggered = true;
                IndexOfPlayerWhoTriggeredRoundEnd = IndexOfSlappingPlayer;
                WinRound(IndexOfSlappingPlayer);
            }
            else
            {
                //lose 1 card, continue game
                try
                {
                    //SlapImage.sprite = CardImages[new Tuple<int, string>(PlayerDecks[IndexOfSlappingPlayer][0].CardValue, PlayerDecks[IndexOfSlappingPlayer][0].CardType)];
                    SlapCard = PlayerDecks[IndexOfSlappingPlayer][0];
                    CardsLostToSlap.Insert(0, PlayerDecks[IndexOfSlappingPlayer][0]);
                    PlayerDecks[IndexOfSlappingPlayer].RemoveAt(0);                    
                }
                catch
                {
                    //SlapImage.sprite = CardImages[new Tuple<int, string>(PlayerDecks[IndexOfSlappingPlayer][0].CardValue, PlayerDecks[IndexOfSlappingPlayer][0].CardType)];
                    SlapCard = PlayerDecks[IndexOfSlappingPlayer][0];
                    CardsLostToSlap.Add(PlayerDecks[IndexOfSlappingPlayer][0]);
                    PlayerDecks[IndexOfSlappingPlayer].RemoveAt(0);                   
                }
                SkipPlayersWithNoCards(IndexOfSlappingPlayer);
            }
           
    }
    #endregion
    #region Visuals

    public void ExecuteEndGame()
    {
        HitButton.gameObject.SetActive(false);
        SlapButton.gameObject.SetActive(false);
        EndGame.SetActive(true);
    }
    public void AssignColors()
    {        
        for (int i = 0; i < PlayerSpheres.Count; i++)
        {
            PlayerSpheres[i].material = normalMat;
        }
        if (InititalSetupDone)
        {            
            PlayerSpheres[IndexOfActivePlayer].material = activeMat;
        }
    }
    public void CardCountUpdate()
    {
        for (int i = 0; i < PlayerDecks.Count; i++)
        {
            PlayerCardCount[i].text = PlayerDecks[i].Count.ToString();
        }
        hitCounter.text = CardsToHit.ToString();
        try { slapCounter.text = SlapsLeft[CardPlayer.localPlayer.playerIndex].ToString(); }
        catch { Debug.Log("slap error. IGNORE ME"); }
        
    }

    public void CheckUIButtons(int indexACtivePlayer)
    {
        if(indexACtivePlayer == CardPlayer.localPlayer.playerIndex)
        {
            HitButton.interactable = true;
        }
        else
        {
            HitButton.interactable = false;
        }
    }
    public void CardsOnGroundVisual()
    {
        try
        {
            CardSlot2.sprite = BlankSprite;
            CardSlot1.sprite = BlankSprite;
            CardSlot0.sprite = BlankSprite;
            SlapImage.sprite = BlankSprite;
        }
        catch
        { }

        switch (CardsOnGround.Count)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    CardSlot2.sprite = CardImages[new Tuple<int, string>(CardsOnGround[CardsOnGround.Count - 1].CardValue, CardsOnGround[CardsOnGround.Count - 1].CardType)];
                    break;
                }
            case 2:
                {
                    CardSlot2.sprite = CardImages[new Tuple<int, string>(CardsOnGround[CardsOnGround.Count - 1].CardValue, CardsOnGround[CardsOnGround.Count - 1].CardType)];
                    CardSlot1.sprite = CardImages[new Tuple<int, string>(CardsOnGround[CardsOnGround.Count - 2].CardValue, CardsOnGround[CardsOnGround.Count - 2].CardType)];
                    break;
                }
                //over 3
            default:
                {
                    CardSlot2.sprite = CardImages[new Tuple<int, string>(CardsOnGround[CardsOnGround.Count - 1].CardValue, CardsOnGround[CardsOnGround.Count - 1].CardType)];
                    CardSlot1.sprite = CardImages[new Tuple<int, string>(CardsOnGround[CardsOnGround.Count - 2].CardValue, CardsOnGround[CardsOnGround.Count - 2].CardType)];
                    CardSlot0.sprite = CardImages[new Tuple<int, string>(CardsOnGround[CardsOnGround.Count - 3].CardValue, CardsOnGround[CardsOnGround.Count - 3].CardType)];
                    break;
                }                
        }

        if(SlapCard != null)
        {
            SlapImage.sprite = CardImages[new Tuple<int, string>(SlapCard.CardValue, SlapCard.CardType)];
        }
    }
    #endregion
    #region Other
    private void DisperseCardsBetweenPlayers()
    {
        //SCUUUFED
        Debug.Log($"Deck count : {PlayerDecks.Count}");
        for (int i = 0; i < RefToController.AssambledDeck.Count; i++)
        {           
            PlayerDecks[i % PlayerDecks.Count].Add(RefToController.AssambledDeck[i]);
        }

        Debug.Log($"created {PlayerDecks[PlayerDecks.Count - 1].Count}");
        /*
        string _string = $"Deck for player {PlayerDecks.Count - 1} :";
        foreach (CardValueType card in PlayerDecks[PlayerDecks.Count - 1])
        {
            _string += card.CardValue.ToString() + "; ";
        }
        Debug.Log(_string);
        */

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
    public void EnableSlapIfRules()
    {
        if (RefToController.SpecialRulesToggles.KQ_or_QK || RefToController.SpecialRulesToggles.Last2AddTo10 || RefToController.SpecialRulesToggles.TwoEqualInARow || RefToController.SpecialRulesToggles.TwoSandwichOne)
        {
            SlapButton.interactable = true;
        }
        else
        {
            SlapButton.interactable = false;
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
    public void SkipPlayersWithNoCards(int indexLocalPlayer)
    {
        if (PlayerDecks[indexLocalPlayer].Count < 1)
        {
            PlayerVisualDecks[0].SetActive(false);
            SlapButton.interactable = false;
            HitButton.interactable = false;
            NextPlayer();
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

    void FillMegaDictionary()
    {
        Debug.Log($"TEste before dictionary {CardImages.Count}");
        // OMEGA SCUFFED
        CardImages.Add(new Tuple<int, string>(15, "Heart"),
            DeckControllerRazboi.instance.HeartsImages[0]);
        CardImages.Add(new Tuple<int, string>(2, "Heart"), DeckControllerRazboi.instance.HeartsImages[1]);
        CardImages.Add(new Tuple<int, string>(3, "Heart"), DeckControllerRazboi.instance.HeartsImages[2]);
        CardImages.Add(new Tuple<int, string>(4, "Heart"), DeckControllerRazboi.instance.HeartsImages[3]);
        CardImages.Add(new Tuple<int, string>(5, "Heart"), DeckControllerRazboi.instance.HeartsImages[4]);
        CardImages.Add(new Tuple<int, string>(6, "Heart"), DeckControllerRazboi.instance.HeartsImages[5]);
        CardImages.Add(new Tuple<int, string>(7, "Heart"), DeckControllerRazboi.instance.HeartsImages[6]);
        CardImages.Add(new Tuple<int, string>(8, "Heart"), DeckControllerRazboi.instance.HeartsImages[7]);
        CardImages.Add(new Tuple<int, string>(9, "Heart"), DeckControllerRazboi.instance.HeartsImages[8]);
        CardImages.Add(new Tuple<int, string>(10, "Heart"), DeckControllerRazboi.instance.HeartsImages[9]);
        CardImages.Add(new Tuple<int, string>(12, "Heart"), DeckControllerRazboi.instance.HeartsImages[10]);
        CardImages.Add(new Tuple<int, string>(13, "Heart"), DeckControllerRazboi.instance.HeartsImages[11]);
        CardImages.Add(new Tuple<int, string>(14, "Heart"), DeckControllerRazboi.instance.HeartsImages[12]);

        CardImages.Add(new Tuple<int, string>(15, "Spade"), DeckControllerRazboi.instance.SpadesImages[0]);
        CardImages.Add(new Tuple<int, string>(2, "Spade"), DeckControllerRazboi.instance.SpadesImages[1]);
        CardImages.Add(new Tuple<int, string>(3, "Spade"), DeckControllerRazboi.instance.SpadesImages[2]);
        CardImages.Add(new Tuple<int, string>(4, "Spade"), DeckControllerRazboi.instance.SpadesImages[3]);
        CardImages.Add(new Tuple<int, string>(5, "Spade"), DeckControllerRazboi.instance.SpadesImages[4]);
        CardImages.Add(new Tuple<int, string>(6, "Spade"), DeckControllerRazboi.instance.SpadesImages[5]);
        CardImages.Add(new Tuple<int, string>(7, "Spade"), DeckControllerRazboi.instance.SpadesImages[6]);
        CardImages.Add(new Tuple<int, string>(8, "Spade"), DeckControllerRazboi.instance.SpadesImages[7]);
        CardImages.Add(new Tuple<int, string>(9, "Spade"), DeckControllerRazboi.instance.SpadesImages[8]);
        CardImages.Add(new Tuple<int, string>(10, "Spade"), DeckControllerRazboi.instance.SpadesImages[9]);
        CardImages.Add(new Tuple<int, string>(12, "Spade"), DeckControllerRazboi.instance.SpadesImages[10]);
        CardImages.Add(new Tuple<int, string>(13, "Spade"), DeckControllerRazboi.instance.SpadesImages[11]);
        CardImages.Add(new Tuple<int, string>(14, "Spade"), DeckControllerRazboi.instance.SpadesImages[12]);

        CardImages.Add(new Tuple<int, string>(15, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[0]);
        CardImages.Add(new Tuple<int, string>(2, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[1]);
        CardImages.Add(new Tuple<int, string>(3, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[2]);
        CardImages.Add(new Tuple<int, string>(4, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[3]);
        CardImages.Add(new Tuple<int, string>(5, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[4]);
        CardImages.Add(new Tuple<int, string>(6, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[5]);
        CardImages.Add(new Tuple<int, string>(7, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[6]);
        CardImages.Add(new Tuple<int, string>(8, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[7]);
        CardImages.Add(new Tuple<int, string>(9, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[8]);
        CardImages.Add(new Tuple<int, string>(10, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[9]);
        CardImages.Add(new Tuple<int, string>(12, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[10]);
        CardImages.Add(new Tuple<int, string>(13, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[11]);
        CardImages.Add(new Tuple<int, string>(14, "Diamond"), DeckControllerRazboi.instance.DiamondsImages[12]);

        CardImages.Add(new Tuple<int, string>(15, "Club"), DeckControllerRazboi.instance.ClubsImages[0]);
        CardImages.Add(new Tuple<int, string>(2, "Club"), DeckControllerRazboi.instance.ClubsImages[1]);
        CardImages.Add(new Tuple<int, string>(3, "Club"), DeckControllerRazboi.instance.ClubsImages[2]);
        CardImages.Add(new Tuple<int, string>(4, "Club"), DeckControllerRazboi.instance.ClubsImages[3]);
        CardImages.Add(new Tuple<int, string>(5, "Club"), DeckControllerRazboi.instance.ClubsImages[4]);
        CardImages.Add(new Tuple<int, string>(6, "Club"), DeckControllerRazboi.instance.ClubsImages[5]);
        CardImages.Add(new Tuple<int, string>(7, "Club"), DeckControllerRazboi.instance.ClubsImages[6]);
        CardImages.Add(new Tuple<int, string>(8, "Club"), DeckControllerRazboi.instance.ClubsImages[7]);
        CardImages.Add(new Tuple<int, string>(9, "Club"), DeckControllerRazboi.instance.ClubsImages[8]);
        CardImages.Add(new Tuple<int, string>(10, "Club"), DeckControllerRazboi.instance.ClubsImages[9]);
        CardImages.Add(new Tuple<int, string>(12, "Club"), DeckControllerRazboi.instance.ClubsImages[10]);
        CardImages.Add(new Tuple<int, string>(13, "Club"), DeckControllerRazboi.instance.ClubsImages[11]);
        CardImages.Add(new Tuple<int, string>(14, "Club"), DeckControllerRazboi.instance.ClubsImages[12]);

        Debug.Log($"Omega useless Dictionary count : {CardImages.Count}");
    }
    #endregion

    private void OnDestroy()
    {
        InititalSetupDone = false;
    }
}
