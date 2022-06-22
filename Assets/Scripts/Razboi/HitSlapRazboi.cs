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
    public bool RoundEndTriggered = false;
    public bool GameIsPlayable = true;

    public List<MeshRenderer> PlayerSpheres = new List<MeshRenderer>();
    public List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();
    public List<GameObject> PlayerVisualDecks = new List<GameObject>();

    public List<CardValueType> Player0Deck = new List<CardValueType>();
    public List<CardValueType> Player1Deck = new List<CardValueType>();
    public List<CardValueType> Player2Deck = new List<CardValueType>();
    public List<CardValueType> Player3Deck = new List<CardValueType>();
    public List<CardValueType> Player4Deck = new List<CardValueType>();

    public List<CardValueType> CardsOnGround = new List<CardValueType>();

    private void Start()
    {
        AssignColors();
        DisperseCardsBetweenPlayers();
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
            case 0: { Player0Deck.AddRange(CardsOnGround); ShuffleDeck0(); break; }
            case 1: { Player1Deck.AddRange(CardsOnGround); ShuffleDeck1(); break; }
            case 2: { Player2Deck.AddRange(CardsOnGround); ShuffleDeck2(); break; }
            case 3: { Player3Deck.AddRange(CardsOnGround); ShuffleDeck3(); break; }
            case 4: { Player4Deck.AddRange(CardsOnGround); ShuffleDeck4(); break; }
        }
        CardsOnGround.Clear();
        CheckPlayerVictory();
        RoundEndTriggered = false;
        IndexOfActivePlayer = IndexOfPlayerWhoTriggeredRoundEnd;
        if (IndexOfActivePlayer == 0)
        {
            HitButton.interactable = true;
            return;
        }
        else
        {
            HitButton.interactable = false;
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
            GameIsPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;

            //Player0Wins
        }
        if (Player1Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;

            //Player1Wins
        }
        if (Player2Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;

            //Player2Wins
        }
        if (Player3Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;

            //Player3Wins
        }
        if (Player4Deck.Count == RefToController.AssambledDeck.Count)
        {
            GameIsPlayable = false;
            HitButton.interactable = false;
            SlapButton.interactable = false;

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
    }
    public void ShuffleDeck0()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player0Deck.Count - 1);
            auxShuffleValue = Player0Deck[cacheRandomResult];
            Player0Deck[cacheRandomResult] = Player0Deck[Player0Deck.Count - 1];
            Player0Deck[Player0Deck.Count - 1] = auxShuffleValue;
        }
    }
    public void ShuffleDeck1()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player1Deck.Count - 1);
            auxShuffleValue = Player1Deck[cacheRandomResult];
            Player1Deck[cacheRandomResult] = Player1Deck[Player1Deck.Count - 1];
            Player1Deck[Player1Deck.Count - 1] = auxShuffleValue;
        }
    }
    public void ShuffleDeck2()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player2Deck.Count - 1);
            auxShuffleValue = Player2Deck[cacheRandomResult];
            Player2Deck[cacheRandomResult] = Player2Deck[Player2Deck.Count - 1];
            Player2Deck[Player2Deck.Count - 1] = auxShuffleValue;
        }
    }
    public void ShuffleDeck3()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player3Deck.Count - 1);
            auxShuffleValue = Player3Deck[cacheRandomResult];
            Player3Deck[cacheRandomResult] = Player2Deck[Player3Deck.Count - 1];
            Player3Deck[Player3Deck.Count - 1] = auxShuffleValue;
        }
    }
    public void ShuffleDeck4()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, Player4Deck.Count - 1);
            auxShuffleValue = Player4Deck[cacheRandomResult];
            Player4Deck[cacheRandomResult] = Player4Deck[Player4Deck.Count - 1];
            Player4Deck[Player4Deck.Count - 1] = auxShuffleValue;
        }
    }
    public void SkipPlayersWithNoCards()
    {
        switch(IndexOfActivePlayer)
        {
            case 0: { 
                    if (Player0Deck.Count < 1)
                    {
                        NextPlayer();
                    } 
                    break; }
            case 1:
                {
                    if (Player1Deck.Count < 1)
                    {
                        NextPlayer();
                    }
                    break;
                }
            case 2:
                {
                    if (Player2Deck.Count < 1)
                    {
                        NextPlayer();
                    }
                    break;
                }
            case 3:
                {
                    if (Player3Deck.Count < 1)
                    {
                        NextPlayer();
                    }
                    break;
                }
            case 4:
                {
                    if (Player4Deck.Count < 1)
                    {
                        NextPlayer();
                    }
                    break;
                }
        }
    }
    public async void TriggerBot()
    {
        await Task.Delay(1000);
        if (GameIsPlayable)
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
    
    public void SlapCards()
    {

    }
}
