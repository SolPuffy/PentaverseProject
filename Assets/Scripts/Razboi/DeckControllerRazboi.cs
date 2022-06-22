using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class DeckControllerRazboi : MonoBehaviour
{
    //J Q K A, 
    // A - deal 4
    // K - deal 3
    // Q - deal 2
    // J - deal 1

    //CurrentDeck
    public List<CardValueType> AssambledDeck = new List<CardValueType>();
    [Header("Decks")]
    public List<CardValueType> Jokers = new List<CardValueType>();
    public List<CardValueType> Hearts = new List<CardValueType>();
    public List<CardValueType> Spades = new List<CardValueType>();
    public List<CardValueType> Diamonds = new List<CardValueType>();
    public List<CardValueType> Clubs = new List<CardValueType>();
    [Header("Toggles")]
    public _DeckToggles DeckToggles;
    public _ShuffleToggles ShuffleToggles;
    public _RulesToggles SpecialRulesToggles;

    private void Awake()
    {
        CheckTogglesAndAddEnabled();
        ShuffleDeckCountTimes();
        RemoveTwoLowCards();
    }

    private void CheckTogglesAndAddEnabled()
    {
        if(DeckToggles.IncludeJokers)
        {
            AssambledDeck.AddRange(Jokers);
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
    }
    private void ShuffleDeckCountTimes()
    {
        CardValueType auxShuffleValue;
        int cacheRandomResult;
        for(int i=0;i<ShuffleToggles.ShufflesCount;i++)
        {
            cacheRandomResult = UnityEngine.Random.Range(0, AssambledDeck.Count - 1);
            auxShuffleValue = AssambledDeck[cacheRandomResult];
            AssambledDeck[cacheRandomResult] = AssambledDeck[AssambledDeck.Count - 1];
            AssambledDeck[AssambledDeck.Count - 1] = auxShuffleValue;
        }    
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