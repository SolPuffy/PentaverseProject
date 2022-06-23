using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
[Serializable]
public class ListInList
{
    public List<CardValueType> instance = new List<CardValueType>();
}
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

    public List<MeshRenderer> PlayerSpheres = new List<MeshRenderer>();
    public List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI slapCounter;
    public List<GameObject> PlayerVisualDecks = new List<GameObject>();

    public List<ListInList> PlayerDecks = new List<ListInList>();

    public List<CardValueType> CardsOnGround = new List<CardValueType>();
    public List<CardValueType> CardsLostToSlap = new List<CardValueType>();

    public List<GameObject> PlayerObjects = new List<GameObject>();

    private void Start()
    {
        AssignColors();
        DisperseCardsBetweenPlayers();
        EnableSlapIfRules();
    }
    private void DisperseCardsBetweenPlayers()
    {
        for (int i = 0; i < RefToController.AssambledDeck.Count; i++)
        {
            //Player0Deck.Add(RefToController.AssambledDeck[i]);
            //PlayerToReceiveCards++;
            PlayerDecks[i % PlayerDecks.Count].instance.Add(RefToController.AssambledDeck[i]);
        }
    }
    public void HitCards(int indexLocalPlayer)
    {
        HitButton.interactable = false;
        CardsToHit--;
        //card pile on ground, primeste top card-ul playerului care apasa butonul
        CardsOnGround.Add(PlayerDecks[indexLocalPlayer].instance[0]);
        PlayerDecks[indexLocalPlayer].instance.RemoveAt(0);
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
            NextPlayer(indexLocalPlayer);
        }
        else
        {
            if (CardsToHit < 1)
            {
                if (RoundEndTriggered)
                {
                    WinRound(indexLocalPlayer);
                }
                else
                {
                    CardsToHit = 1;
                    NextPlayer(indexLocalPlayer);
                }
            }
            else
            {
                StaySamePlayer();
            }
        }
        //
    }
    public void WinRound(int indexLocalPlayer)
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
        //Didn't hit a +10 and someone before did
        PlayerDecks[indexLocalPlayer].instance.AddRange(CardsOnGround);
        PlayerDecks[indexLocalPlayer].instance.AddRange(CardsLostToSlap);
        ShuffleDeck(indexLocalPlayer);
        CardsOnGround.Clear();
        CardsLostToSlap.Clear();
        CheckPlayerVictory(indexLocalPlayer);
        SlapsLeft = 3;
        RoundEndTriggered = false;
        IndexOfActivePlayer = IndexOfPlayerWhoTriggeredRoundEnd;
        if (IndexOfActivePlayer == 0)
        {
            HitButton.interactable = true;
            if(PlayerDecks[indexLocalPlayer].instance.Count > 0)
            {
                SlapButton.interactable = true;
            }
            return;
        }
        else
        {
            HitButton.interactable = false;
            if (PlayerDecks[indexLocalPlayer].instance.Count > 0)
            {
                SlapButton.interactable = true;
            }
        }
    }
    public void NextPlayer(int indexLocalPlayer)
    {
        IndexOfActivePlayer++;
        if (IndexOfActivePlayer > 4)
        {
            IndexOfActivePlayer = 0;
        }
        SkipPlayersWithNoCards(indexLocalPlayer);
        if (IndexOfActivePlayer == 0)
        {
            HitButton.interactable = true;
            return;
        }
        else
        {
            //HitButton.interactable = false;
        }

    }
    public void CheckPlayerVictory(int indexLocalPlayer)
    {
        if (PlayerDecks[indexLocalPlayer].instance.Count == RefToController.AssambledDeck.Count)
        {
            //HitButton.interactable = false;
            SlapButton.interactable = false;
            return;
            //Player0Wins
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
        for(int i=0;i<PlayerDecks.Count;i++)
        {
            PlayerCardCount[i].text = PlayerDecks[i].instance.Count.ToString();
        }
        hitCounter.text = CardsToHit.ToString();
        slapCounter.text = SlapsLeft.ToString();
    }
    //shuffle deck after taking cards from table
    public void ShuffleDeck(int indexLocalPlayer)
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        int cacheRandomResult2;
        for (int i = 0; i < RefToController.ShuffleToggles.ShufflesCount / 5; i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, PlayerDecks[indexLocalPlayer].instance.Count - 1);
            cacheRandomResult2 = UnityEngine.Random.Range(0, PlayerDecks[indexLocalPlayer].instance.Count - 1);

            auxShuffleValue = PlayerDecks[indexLocalPlayer].instance[cacheRandomResult];
            PlayerDecks[indexLocalPlayer].instance[cacheRandomResult] = PlayerDecks[indexLocalPlayer].instance[cacheRandomResult2];
            PlayerDecks[indexLocalPlayer].instance[cacheRandomResult2] = auxShuffleValue;
        }
    }
    public void SkipPlayersWithNoCards(int indexLocalPlayer)
    {
        if (PlayerDecks[indexLocalPlayer].instance.Count < 1)
        {
            PlayerVisualDecks[0].SetActive(false);
            SlapButton.interactable = false;
            HitButton.interactable = false;
            NextPlayer(indexLocalPlayer);
        }
    }

    public void SlapCards(int IndexOfSlappingPlayer)
    {
        if (SlapsLeft > 0)
        {
            SlapsLeft--;
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
                    SlapImage.sprite = PlayerDecks[IndexOfSlappingPlayer].instance[0].CardSprite;
                    CardsLostToSlap.Insert(0, PlayerDecks[IndexOfSlappingPlayer].instance[0]);
                    PlayerDecks[IndexOfSlappingPlayer].instance.RemoveAt(0);
                }
                catch
                {
                    SlapImage.sprite = PlayerDecks[IndexOfSlappingPlayer].instance[0].CardSprite;
                    CardsLostToSlap.Add(PlayerDecks[IndexOfSlappingPlayer].instance[0]);
                    PlayerDecks[IndexOfSlappingPlayer].instance.RemoveAt(0);
                }
                SkipPlayersWithNoCards(IndexOfSlappingPlayer);
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
