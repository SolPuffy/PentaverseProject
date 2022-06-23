using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HitSlapRazboi : MonoBehaviour
{
    public DeckControllerRazboi RefToController;

    public Button HitButton;
    public Button SlapButton;

    public Image SlapImage;
    public Image CardSlot0;
    public Image CardSlot1;
    public Image CardSlot2;

    public Material normalMat;
    public Material activeMat;

    public Sprite BlankSprite;

    public int CardsToHit;
    public int IndexOfPlayerWhoTriggeredRoundEnd;
    public int IndexOfActivePlayer = 0;
    public int SlapsLeft = 3;
    public bool RoundEndTriggered = false;
    public bool GameIsBotPlayable = true;

    public List<MeshRenderer> PlayerSpheres = new List<MeshRenderer>();
    public List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI slapCounter;
    public List<GameObject> PlayerVisualDecks = new List<GameObject>();

    public List<CardValueType> Player0Deck = new List<CardValueType>();
    public List<CardValueType> Player1Deck = new List<CardValueType>();
    public List<CardValueType> Player2Deck = new List<CardValueType>();
    public List<CardValueType> Player3Deck = new List<CardValueType>();
    public List<CardValueType> Player4Deck = new List<CardValueType>();

    public List<CardValueType> CardsOnGround = new List<CardValueType>();
    public List<CardValueType> CardsLostToSlap = new List<CardValueType>();

    private void Start()
    {
        AssignColors();
        DisperseCardsBetweenPlayers();
        EnableSlapIfRules();
    }
    private void DisperseCardsBetweenPlayers()
    {
        int PlayerToReceiveCards = 0;
        for (int i = 0; i < RefToController.AssambledDeck.Count; i++)
        {
            switch (PlayerToReceiveCards)
            {
                case 0:
                    {
                        Player0Deck.Add(RefToController.AssambledDeck[i]);
                        PlayerToReceiveCards++;
                        break;
                    }
                case 1:
                    {
                        Player1Deck.Add(RefToController.AssambledDeck[i]);
                        PlayerToReceiveCards++;
                        break;
                    }
                case 2:
                    {
                        Player2Deck.Add(RefToController.AssambledDeck[i]);
                        PlayerToReceiveCards++;
                        break;
                    }
                case 3:
                    {
                        Player3Deck.Add(RefToController.AssambledDeck[i]);
                        PlayerToReceiveCards++;
                        break;
                    }
                case 4:
                    {
                        Player4Deck.Add(RefToController.AssambledDeck[i]);
                        PlayerToReceiveCards++;
                        break;
                    }
                default:
                    {
                        Player0Deck.Add(RefToController.AssambledDeck[i]);
                        PlayerToReceiveCards = 1;
                        break;
                    }
            }
        }
    }
    public void HitCards()
    {
        HitButton.interactable = false;
        CardsToHit--;
        switch (IndexOfActivePlayer)
        {
            case 0:
                {
                    CardsOnGround.Add(Player0Deck[0]);
                    Player0Deck.RemoveAt(0);
                    break;
                }
            case 1:
                {
                    CardsOnGround.Add(Player1Deck[0]);
                    Player1Deck.RemoveAt(0);
                    break;
                }
            case 2:
                {
                    CardsOnGround.Add(Player2Deck[0]);
                    Player2Deck.RemoveAt(0);
                    break;
                }
            case 3:
                {
                    CardsOnGround.Add(Player3Deck[0]);
                    Player3Deck.RemoveAt(0);
                    break;
                }
            case 4:
                {
                    CardsOnGround.Add(Player4Deck[0]);
                    Player4Deck.RemoveAt(0);
                    break;
                }
        }
        //display card on table and shift others to left if case allows
        switch (CardsOnGround.Count)
        {
            case 1:
                {
                    CardSlot2.sprite = CardsOnGround[CardsOnGround.Count - 1].CardSprite;
                    break;
                }
            case 2:
                {
                    CardSlot2.sprite = CardsOnGround[CardsOnGround.Count - 1].CardSprite;
                    CardSlot1.sprite = CardsOnGround[CardsOnGround.Count - 2].CardSprite;
                    break;
                }
            default:
                {
                    CardSlot2.sprite = CardsOnGround[CardsOnGround.Count - 1].CardSprite;
                    CardSlot1.sprite = CardsOnGround[CardsOnGround.Count - 2].CardSprite;
                    CardSlot0.sprite = CardsOnGround[CardsOnGround.Count - 3].CardSprite;
                    break;
                }
        }
        //
        //check if card > 10 , Yes = trigger round end, No = continue
        if (CardsOnGround[CardsOnGround.Count - 1].CardValue > 10 /* 9 */)
        {
            IndexOfPlayerWhoTriggeredRoundEnd = IndexOfActivePlayer;
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
                    LoseRound();
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
    public async void LoseRound()
    {
        await Task.Delay(1000);
        try
        {
            CardSlot2.sprite = BlankSprite;
            CardSlot1.sprite = BlankSprite;
            CardSlot0.sprite = BlankSprite;
            SlapImage.sprite = BlankSprite;
        }
        catch
        { }
        //Didn't hit a +10 and someone before did
        switch (IndexOfPlayerWhoTriggeredRoundEnd)
        {
            case 0: { Player0Deck.AddRange(CardsOnGround); Player0Deck.AddRange(CardsLostToSlap);
                    ShuffleDeck0(); break; }
            case 1: { Player1Deck.AddRange(CardsOnGround); Player1Deck.AddRange(CardsLostToSlap);
                    ShuffleDeck1(); break; }
            case 2: { Player2Deck.AddRange(CardsOnGround); Player2Deck.AddRange(CardsLostToSlap);
                    ShuffleDeck2(); break; }
            case 3: { Player3Deck.AddRange(CardsOnGround); Player3Deck.AddRange(CardsLostToSlap);
                    ShuffleDeck3(); break; }
            case 4: { Player4Deck.AddRange(CardsOnGround); Player4Deck.AddRange(CardsLostToSlap);
                    ShuffleDeck4(); break; }
        }
        CardsOnGround.Clear();
        CardsLostToSlap.Clear();
        CheckPlayerVictory();
        SlapsLeft = 3;
        RoundEndTriggered = false;
        IndexOfActivePlayer = IndexOfPlayerWhoTriggeredRoundEnd;
        if (IndexOfActivePlayer == 0)
        {
            HitButton.interactable = true;
            if(Player0Deck.Count > 0)
            {
                SlapButton.interactable = true;
            }
            return;
        }
        else
        {
            HitButton.interactable = false;
            if (Player0Deck.Count > 0)
            {
                SlapButton.interactable = true;
            }
            TriggerBot();
        }
    }
    public async void NextPlayer()
    {
        IndexOfActivePlayer++;
        if (IndexOfActivePlayer > 4)
        {
            IndexOfActivePlayer = 0;
        }
        SkipPlayersWithNoCards();
        if (IndexOfActivePlayer == 0)
        {
            await Task.Delay(1000);
            HitButton.interactable = true;
            return;
        }
        else
        {
            HitButton.interactable = false;
            TriggerBot();
        }

    }
    public void CheckPlayerVictory()
    {
        if (Player0Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsBotPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;
            return;
            //Player0Wins
        }
        if (Player1Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsBotPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;
            return;
            //Player1Wins
        }
        if (Player2Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsBotPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;
            return;
            //Player2Wins
        }
        if (Player3Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsBotPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;
            return;
            //Player3Wins
        }
        if (Player4Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsBotPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;
            return;
            //Player4Wins
        }
    }
    private void Update()
    {
        AssignColors();
        CardCountUpdate();
    }
    public void StaySamePlayer()
    {
        //DoNothingIfUrNotABot
        if (IndexOfActivePlayer > 0)
        {
            TriggerBot();
        }
        else
        {

            HitButton.interactable = true;
        }
    }
    public void EnableSlapIfRules()
    {
        if(RefToController.SpecialRulesToggles.KQ_or_QK || RefToController.SpecialRulesToggles.Last2AddTo10 || RefToController.SpecialRulesToggles.TwoEqualInARow || RefToController.SpecialRulesToggles.TwoSandwichOne)
        {
            SlapButton.interactable = true;
        }   
        else
        {
            SlapButton.interactable = false;
        }
    }
    public void AssignColors()
    {
        for (int i = 0; i < PlayerSpheres.Count; i++)
        {
            PlayerSpheres[i].material = normalMat;
        }
        PlayerSpheres[IndexOfActivePlayer].material = activeMat;
    }
    public void CardCountUpdate()
    {
        PlayerCardCount[0].text = Player0Deck.Count.ToString();
        PlayerCardCount[1].text = Player1Deck.Count.ToString();
        PlayerCardCount[2].text = Player2Deck.Count.ToString();
        PlayerCardCount[3].text = Player3Deck.Count.ToString();
        PlayerCardCount[4].text = Player4Deck.Count.ToString();

        hitCounter.text = CardsToHit.ToString();
        slapCounter.text = SlapsLeft.ToString();
    }
    public void ShuffleDeck0()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player0Deck.Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, Player0Deck.Count - 1);

            auxShuffleValue = Player0Deck[cacheRandomResult];
            Player0Deck[cacheRandomResult] = Player0Deck[cacheRandomResult2];
            Player0Deck[cacheRandomResult2] = auxShuffleValue;
        }
    }
    public void ShuffleDeck1()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player1Deck.Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, Player1Deck.Count - 1);

            auxShuffleValue = Player1Deck[cacheRandomResult];

            Player1Deck[cacheRandomResult] = Player1Deck[cacheRandomResult2];
            Player1Deck[cacheRandomResult2] = auxShuffleValue;
        }
    }
    public void ShuffleDeck2()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player2Deck.Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, Player2Deck.Count - 1);

            auxShuffleValue = Player2Deck[cacheRandomResult];

            Player2Deck[cacheRandomResult] = Player2Deck[cacheRandomResult2];
            Player2Deck[cacheRandomResult2] = auxShuffleValue;
        }
    }
    public void ShuffleDeck3()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player3Deck.Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, Player3Deck.Count - 1);

            auxShuffleValue = Player3Deck[cacheRandomResult];

            Player3Deck[cacheRandomResult] = Player3Deck[cacheRandomResult2];
            Player3Deck[cacheRandomResult2] = auxShuffleValue;
        }
    }
    public void ShuffleDeck4()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player4Deck.Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, Player4Deck.Count - 1);

            auxShuffleValue = Player4Deck[cacheRandomResult];

            Player4Deck[cacheRandomResult] = Player4Deck[cacheRandomResult2];
            Player4Deck[cacheRandomResult2] = auxShuffleValue;
        }
    }
    public void SkipPlayersWithNoCards()
    {
        switch(IndexOfActivePlayer)
        {
            case 0: { 
                    if (Player0Deck.Count < 1)
                    {
                        PlayerVisualDecks[0].SetActive(false);
                        SlapButton.interactable = false;
                        HitButton.interactable = false;
                        NextPlayer();
                    } 
                    break; }
            case 1:
                {
                    if (Player1Deck.Count < 1)
                    {
                        PlayerVisualDecks[1].SetActive(false);
                        NextPlayer();
                    }
                    break;
                }
            case 2:
                {
                    if (Player2Deck.Count < 1)
                    {
                        PlayerVisualDecks[2].SetActive(false);
                        NextPlayer();
                    }
                    break;
                }
            case 3:
                {
                    if (Player3Deck.Count < 1)
                    {
                        PlayerVisualDecks[3].SetActive(false);
                        NextPlayer();
                    }
                    break;
                }
            case 4:
                {
                    if (Player4Deck.Count < 1)
                    {
                        PlayerVisualDecks[4].SetActive(false);
                        NextPlayer();
                    }
                    break;
                }
        }
    }
    public async void TriggerBot()
    {
        await Task.Delay(1000);
        if (GameIsBotPlayable)
        {
            try
            {
                HitCards();
            }
            catch
            {
                SkipPlayersWithNoCards();
            }
        }
        
    }

    public async void SlapCards(int IndexOfSlappingPlayer)
    {
        if (SlapsLeft > 0)
        {
            SlapsLeft--;
            if (CheckSlapRules())
            {
                //successfully slapped, take cards wait for bots (delay && conditions to be removed for actual players)
                GameIsBotPlayable = false;
                SlapButton.interactable = false;
                HitButton.interactable = false;
                RoundEndTriggered = true;
                IndexOfPlayerWhoTriggeredRoundEnd = IndexOfSlappingPlayer;
                LoseRound();
                await Task.Delay(1000);
                GameIsBotPlayable = true;
            }
            else
            {
                //lose 1 card, continue game
                try
                {
                    switch (IndexOfSlappingPlayer)
                    {
                        case 0:
                            {
                                SlapImage.sprite = Player0Deck[0].CardSprite;
                                CardsLostToSlap.Insert(0, Player0Deck[0]);
                                Player0Deck.RemoveAt(0);
                                break;
                            }
                        case 1:
                            {
                                SlapImage.sprite = Player1Deck[0].CardSprite;
                                CardsLostToSlap.Insert(0, Player1Deck[0]);
                                Player1Deck.RemoveAt(0);
                                break;
                            }
                        case 2:
                            {
                                SlapImage.sprite = Player2Deck[0].CardSprite;
                                CardsLostToSlap.Insert(0, Player2Deck[0]);
                                Player2Deck.RemoveAt(0);
                                break;
                            }
                        case 3:
                            {
                                SlapImage.sprite = Player3Deck[0].CardSprite;
                                CardsLostToSlap.Insert(0, Player3Deck[0]);
                                Player3Deck.RemoveAt(0);
                                break;
                            }
                        case 4:
                            {
                                SlapImage.sprite = Player4Deck[0].CardSprite;
                                CardsLostToSlap.Insert(0, Player4Deck[0]);
                                Player4Deck.RemoveAt(0);
                                break;
                            }
                    }
                }
                catch
                {
                    switch (IndexOfSlappingPlayer)
                    {
                        case 0:
                            {
                                SlapImage.sprite = Player0Deck[0].CardSprite;
                                CardsLostToSlap.Add(Player0Deck[0]);
                                Player0Deck.RemoveAt(0);
                                break;
                            }
                        case 1:
                            {
                                SlapImage.sprite = Player1Deck[0].CardSprite;
                                CardsLostToSlap.Add(Player1Deck[0]);
                                Player1Deck.RemoveAt(0);
                                break;
                            }
                        case 2:
                            {
                                SlapImage.sprite = Player2Deck[0].CardSprite;
                                CardsLostToSlap.Add(Player2Deck[0]);
                                Player2Deck.RemoveAt(0);
                                break;
                            }
                        case 3:
                            {
                                SlapImage.sprite = Player3Deck[0].CardSprite;
                                CardsLostToSlap.Add(Player3Deck[0]);
                                Player3Deck.RemoveAt(0);
                                break;
                            }
                        case 4:
                            {
                                SlapImage.sprite = Player4Deck[0].CardSprite;
                                CardsLostToSlap.Add(Player4Deck[0]);
                                Player4Deck.RemoveAt(0);
                                break;
                            }
                    }
                }
                SkipPlayersWithNoCards();
            }
        }
        if (SlapsLeft < 1)
        {
            SlapButton.interactable = false;
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
}
