using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Mirror;

public class DeckControllerRazboi : NetworkBehaviour
{
    //J Q K A, 
    // A - deal 4
    // K - deal 3
    // Q - deal 2
    // J - deal 1

    //CurrentDeck
    public readonly SyncListCards AssambledDeck = new SyncListCards();
    [Header("Decks")]
    public List<CardValueType> Jokers = new List<CardValueType>();
    public List<CardValueType> Hearts = new List<CardValueType>();
    public List<CardValueType> Spades = new List<CardValueType>();
    public List<CardValueType> Diamonds = new List<CardValueType>();
    public List<CardValueType> Clubs = new List<CardValueType>();
    public List<Sprite> HeartsImages = new List<Sprite>();
    public List<Sprite> ClubsImages = new List<Sprite>();
    public List<Sprite> SpadesImages = new List<Sprite>();
    public List<Sprite> DiamondsImages = new List<Sprite>();
    [Header("Toggles")]
    public _DeckToggles DeckToggles;
    public _ShuffleToggles ShuffleToggles;
    public _RulesToggles SpecialRulesToggles;
    [SyncVar] public int cardcount;
    [SyncVar] public bool done = false;
    public static DeckControllerRazboi instance;

    private void Awake()
    {
        instance = this;        
    }

    public void BuildDeck() 
    {
        Debug.Log("Building deck");
        CheckTogglesAndAddEnabled();
    }    
    private void CheckTogglesAndAddEnabled()
    {
        if(DeckToggles.IncludeJokers)
        {
            //AssambledDeck.AddRange(Jokers);
        }    
        if(DeckToggles.IncludeHearts)
        {
            AssambledDeck.AddRange(Hearts);
        }
        if (DeckToggles.IncludeSpades)
        {
            AssambledDeck.AddRange(Spades);
        }
        if (DeckToggles.IncludeDiamonds)
        {
            AssambledDeck.AddRange(Diamonds);
        }
        if (DeckToggles.IncludeClubs)
        {
            AssambledDeck.AddRange(Clubs);
        }
        ShuffleDeckCountTimes();
    }
    private void ShuffleDeckCountTimes()
    {
        if(ShuffleToggles.DoShuffling)
        {
            CardValueType auxShuffleValue;
            int cacheRandomResult;
            int cacheRandomResult2;
            for (int i = 0; i < ShuffleToggles.ShufflesCount; i++)
            {
                cacheRandomResult = UnityEngine.Random.Range(0, AssambledDeck.Count - 1);
                cacheRandomResult2 = UnityEngine.Random.Range(0, AssambledDeck.Count - 1);

                auxShuffleValue = AssambledDeck[cacheRandomResult];

                AssambledDeck[cacheRandomResult] = AssambledDeck[cacheRandomResult2];
                AssambledDeck[cacheRandomResult2] = auxShuffleValue;
            }
        }

        RemoveTwoLowCards();

    }
    private void RemoveTwoLowCards()
    {
        int cardsToRemove = 2;
        for (int i = 0; i < AssambledDeck.Count; i++)
        {
            if (AssambledDeck[i].CardValue < 10)
            {
                AssambledDeck.RemoveAt(i);
                cardsToRemove--;
            }
            if (cardsToRemove < 1)
            {
                break;
            }
        }
        ShowDeck();
        List<CardValueType> SyncToLocalList = new List<CardValueType>();
        AssambledDeck.CopyTo(SyncToLocalList);
        ServerBackup.BackupDeck(SyncToLocalList);
        done = true;
    }    
    public void ShowDeck()
    {
        string _string = "";
        foreach (CardValueType card in AssambledDeck)
        {
            _string += card.CardValue.ToString() + "; ";
        }
        Debug.Log(_string);
        cardcount = AssambledDeck.Count;
    }
    private void OnDestroy()
    {
        done = false;
    }
}
[Serializable]
public struct _DeckToggles
{
    public bool IncludeJokers;
    public bool IncludeHearts;
    public bool IncludeSpades;
    public bool IncludeDiamonds;
    public bool IncludeClubs;
}
[Serializable]
public struct _ShuffleToggles
{
    public bool DoShuffling;
    public int ShufflesCount;
}
[Serializable]
public struct _RulesToggles
{
    public bool Last2AddTo10;
    public bool TwoEqualInARow;
    public bool TwoSandwichOne;
    public bool KQ_or_QK;
}
