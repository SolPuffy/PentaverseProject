using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

[System.Serializable]
public class decks
{
    public List<int> cards = new List<int>();
}
public class SimmulateGame : MonoBehaviour
{
    [HideInInspector] public int rulesSetting = 0;
    List<int> cards = new List<int> { 15, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14 };
    List<decks> deck = new List<decks>();
    List<int> groundCards = new List<int>();
    float[] playersBaseSlappingDelays = { 0, 0, 0, 0, 0 };
    float[] timedSlappingDelays = { 0, 0, 0, 0, 0 };
    bool[] playerIsEliminated = { false, false, false, false, false };
    int indexOfCurrentTurn = 0;
    int indexOfExpectedWin = 0;
    int cardsToHit = 1;
    bool GameInProgress = true;
    bool roundEnd = false;
    bool slapped = false;
    float timeElapsed = 0;

    [HideInInspector] public float A;
    [HideInInspector] public float B;
    [HideInInspector] public float C;
    [HideInInspector] public float D;
    [HideInInspector] public float E;

    [HideInInspector] public int RandomTickA;
    [HideInInspector] public float RandomTickB;


    public async Task Simmulate()
    {
        var CToken = new CancellationTokenSource();
        CancellationToken ct = CToken.Token;

        await Initialization();
        await PlayTheGame();
    }

    private async Task Initialization()
    {
        await PrepareDeck();
        await SetSlappingDelays();
        await CreateEmptyDecks();
        await SpreadCards();
    }

    private async Task PrepareDeck()
    {
        int aux;
        for (int i = 0; i < 1000; i++)
        {
            aux = cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)];
            cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)] = cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)];
            cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)] = aux;
        }
        await TrimTwoLowCards();
    }
    private async Task RefreshPlayerCards()
    {
        int aux;
        for (int i = 0; i < 1000; i++)
        {
            aux = deck[indexOfExpectedWin].cards[(int)Mathf.Min((new System.DateTime().Millisecond* 0.05f), 51)];
            deck[indexOfExpectedWin].cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)] = deck[indexOfExpectedWin].cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)];
            deck[indexOfExpectedWin].cards[(int)Mathf.Min((new System.DateTime().Millisecond * 0.05f), 51)] = aux;
        }
        await Task.Yield();
    }
    private async Task SetSlappingDelays()
    {
        timedSlappingDelays[0] = playersBaseSlappingDelays[0] = A;
        timedSlappingDelays[1] = playersBaseSlappingDelays[1] = B;
        timedSlappingDelays[2] = playersBaseSlappingDelays[2] = C;
        timedSlappingDelays[3] = playersBaseSlappingDelays[3] = D;
        timedSlappingDelays[4] = playersBaseSlappingDelays[4] = E;
        await Task.Yield();
    }
    private async Task TrimTwoLowCards()
    {
        int aux = 0;
        while (cards.Count > 50)
        {
            if (cards[aux] < 12)
            {
                cards.RemoveAt(aux);
            }
            aux++;
        }
        await Task.Yield();
    }
    private async Task CreateEmptyDecks()
    {
        for (int i = 0;i<5;i++)
        {
            deck.Add(new decks());
        }
        await Task.Yield();
    }
    private async Task SpreadCards()
    {
        for (int i=0;i<50;i++)
        {
            switch(i%5)
            {
                case 0: { deck[0].cards.Add(cards[i]);break; }
                case 1: { deck[1].cards.Add(cards[i]);break; }
                case 2: { deck[2].cards.Add(cards[i]);break; }
                case 3: { deck[3].cards.Add(cards[i]);break; }
                case 4: { deck[4].cards.Add(cards[i]);break; }

                default: { Debug.Log("erlol"); break; }
            }
        }
        await Task.Yield();
    }
    private async Task PlayTheGame()
    {
        SimulationCollection.SetTimeStart(DateTime.Now.ToString("fff"));
        while(GameInProgress)
        {
            timeElapsed++;
            await CheckVictory();
            for(int i=0;i<cardsToHit;i++)
            {
                if (deck[indexOfCurrentTurn].cards.Count < 1)
                {
                    playerIsEliminated[indexOfCurrentTurn] = true;
                    break;
                }
                await AddCardsToPile();
                deck[indexOfCurrentTurn].cards.RemoveAt(0);
                SimulationCollection.CollectHit();
                if (await Hit(groundCards[groundCards.Count - 1]) > 10)
                {
                    await AmountOfCardsToHit();
                    break;
                }
                else
                {
                    if(i == cardsToHit - 1 && roundEnd || slapped)
                    {
                        if(slapped)
                        {
                            SimulationCollection.CollectSlap();
                            slapped = false;
                        }
                        await AwardCards(); 
                        groundCards.Clear();
                        await ModifyTurn(false);
                        roundEnd = false;
                    }
                    else
                    {
                        cardsToHit = 1;
                    }
                }
            }
            await ModifyTurn(true);
        }
    }
    private async Task AmountOfCardsToHit()
    {
        switch(rulesSetting)
        {
            case 0:
                {
                    switch (groundCards[groundCards.Count - 1])
                    {
                        case 15: { cardsToHit = 4; break; }
                        case 12: { cardsToHit = 1; break; }
                        case 13: { cardsToHit = 2; break; }
                        case 14: { cardsToHit = 3; break; }
                    }roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break;
                }
            case 1:
                {
                    switch (groundCards[groundCards.Count - 1])
                    {
                        case 15: { cardsToHit = 3; roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break; }
                        case 12: { cardsToHit = 1; roundEnd = false; break; }
                        case 13: { cardsToHit = 1; roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break; }
                        case 14: { cardsToHit = 2; roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break; }
                    }break;
                }
            case 2:
                {
                    switch (groundCards[groundCards.Count - 1])
                    {
                        case 15: { cardsToHit = 4; roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break; }
                        case 12: { cardsToHit = 1; roundEnd = false; break; }
                        case 13: { cardsToHit = 2; roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break; }
                        case 14: { cardsToHit = 3; roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break; }
                    }break;
                }
            case 3:
                {
                    switch (groundCards[groundCards.Count - 1])
                    {
                        case 15: { cardsToHit = 4; break; }
                        case 12: { cardsToHit = 4; break; }
                        case 13: { cardsToHit = 4; break; }
                        case 14: { cardsToHit = 4; break; }
                    }roundEnd = true; indexOfExpectedWin = indexOfCurrentTurn; break;
                }
        }
        await Task.Yield();
    }
    private async Task AwardCards()
    {
        deck[indexOfExpectedWin].cards.AddRange(groundCards);
        await RefreshPlayerCards();
    }
    private async Task AddCardsToPile()
    {
        groundCards.Add(deck[indexOfCurrentTurn].cards[0]);
        await Task.Yield();
    }
    private async Task<int> Hit(int cardValueDrawn)
    {
        if (SlapRules())
        {
            return await TickDownSlapOcassion(cardValueDrawn);
        }
        else
        {
            return cardValueDrawn;
        }
    }
    private async Task<int> TickDownSlapOcassion(int cardValueDrawn)
    {
        for (int i=0;i<RandomTickA; i++)
        {
            await TickPlayersDelays();
            if (await CheckDelaysForTriggers())
            {
                roundEnd = true;
                return 0;
            }
            else
            {
                //nothing
            }
        }
        return cardValueDrawn;
        //for random small time tick down all slap timers by random
    }    
    private bool SlapRules()
    {
        if (groundCards.Count > 1 && (groundCards[groundCards.Count-1] == groundCards[groundCards.Count - 2] || //2InARow
            (groundCards[groundCards.Count - 1] == 13 && groundCards[groundCards.Count - 2] == 12 || groundCards[groundCards.Count - 1] == 12 && groundCards[groundCards.Count - 2] == 13) || //QJ and JQ
            (groundCards[groundCards.Count - 1] == 13 && groundCards[groundCards.Count - 2] == 14 || groundCards[groundCards.Count - 1] == 14 && groundCards[groundCards.Count - 2] == 13) //QK and KQ
            ))
        {
            return true;
        }
        else if(groundCards.Count > 2 && (groundCards[groundCards.Count - 1] == groundCards[groundCards.Count - 3])) //Sandwich
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private async Task TickPlayersDelays()
    {
        for(int i = 0;i<5;i++)
        {
            if (!playerIsEliminated[i])
            {
                timedSlappingDelays[i] -= RandomTickB;
            }
        }
        await Task.Yield();
    }
    private async Task<bool> CheckDelaysForTriggers()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!playerIsEliminated[i] && timedSlappingDelays[i] <= 0)
            {
                indexOfExpectedWin = i;
                await ModifyTurn(false);
                slapped = true;
                await ResetDelaysAndIncrementWinning();
                return true;
            }
        }
        return false;
    }
    private async Task ResetDelaysAndIncrementWinning()
    {
        playersBaseSlappingDelays[indexOfExpectedWin] += 0.15f;
        for(int i = 0;i<5;i++)
        {
            if (!playerIsEliminated[i])
            {
                timedSlappingDelays[i] = playersBaseSlappingDelays[i];
            }
        }    
        await Task.Yield();
    }

    private async Task ModifyTurn(bool normalCross)
    {
        if (normalCross)
        {
            if (indexOfCurrentTurn + 1 > 4)
            {
                indexOfCurrentTurn = 0;
            }
            else
            {
                indexOfCurrentTurn++;
            }
        }    
        else
        {
            if(indexOfCurrentTurn - 1 < 0)
            {
                indexOfCurrentTurn = 4;
            }
            else
            {
                indexOfCurrentTurn--;
            }
        }
        await Task.Yield();
    }

    private async Task CheckVictory()
    {
        int playersElimiated = 0;
        int markPossibleWin = 0;
        for(int i=0;i<5;i++)
        {
            if(playerIsEliminated[i])
            {
                playersElimiated++;
            }
            else
            {
                markPossibleWin = i;
            }
        }    
        if(playersElimiated == 4)
        {
            GameInProgress = false;
            SimulationCollection.SetTimeEnd(DateTime.Now.ToString("fff"));
            SimulationCollection.CollectTime(timeElapsed);
        }
        await Task.Yield();
    }
}
