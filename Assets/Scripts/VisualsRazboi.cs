using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class VisualsRazboi : MonoBehaviour
{    
    [SerializeField] List<Sprite> CardImages = new List<Sprite>(); 
    [SerializeField] List<Image> PlayerPortrait = new List<Image>();
    [SerializeField] Color PlayerDefaultColor;
    [SerializeField] Color ActivePlayerColor;   
    [SerializeField] List<TextMeshProUGUI> PlayerName = new List<TextMeshProUGUI>();
    [SerializeField] List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();
    [SerializeField] List<GameObject> PlayerVisualDecks = new List<GameObject>();
    [SerializeField] List<int> correctOrder = new List<int>();
    [SerializeField] TextMeshProUGUI hitCounter;
    [SerializeField] TextMeshProUGUI slapCounter;
    [SerializeField] Image SlapImage;
    [SerializeField] Image CardSlot0;
    [SerializeField] Image CardSlot1;
    [SerializeField] Image CardSlot2;
    [SerializeField] Sprite BlankSprite;
    private void Awake()
    {
        if(Application.isBatchMode) { Destroy(this); }        
    }

    void Update()
    {
        if(HitSlapRazboi.instance == null ) { return; }

        DeactivateVisualDecks();
        for (int i = 0; i < HitSlapRazboi.instance.PlayerDecks.Count; i++)
        {
            ActivateVisualDecks(i);
        }
        try
        { CalculateCorrectOrder(correctOrder); }
        catch { Debug.LogWarning("temp Error"); }
       
        CardCountUpdate();
        CardsOnGroundVisual();
        AssignColors();
        SetNames();        
    }

    void CalculateCorrectOrder(List<int> _correctOrder)
    {
        _correctOrder.Clear();
        for (int i = 0; i < HitSlapRazboi.instance.PlayerDecks.Count; i++)
        {
            _correctOrder.Add(0);
        }
        _correctOrder[0] = CardPlayer.localPlayer.playerIndex;

        for (int i = 1; i < _correctOrder.Count; i++)
        {
            _correctOrder[i] = (_correctOrder[0] + i) % _correctOrder.Count;
        }
    }

    public void CardCountUpdate()
    {
        for (int i = 0; i < correctOrder.Count; i++)
        {
            PlayerCardCount[correctOrder.IndexOf(i)].text = HitSlapRazboi.instance.PlayerDecks[i].Count.ToString();
        }
        hitCounter.text = HitSlapRazboi.instance.CardsToHit.ToString();
        try { slapCounter.text = HitSlapRazboi.instance.SlapsLeft[CardPlayer.localPlayer.playerIndex].ToString(); }
        catch { Debug.Log("slap error. IGNORE ME"); }

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

        switch (HitSlapRazboi.instance.CardsOnGround.Count)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    CardSlot2.sprite = CardImages[HitSlapRazboi.instance.CardsOnGround[HitSlapRazboi.instance.CardsOnGround.Count - 1].CardSpriteIndex];
                    break;
                }
            case 2:
                {
                    CardSlot2.sprite = CardImages[HitSlapRazboi.instance.CardsOnGround[HitSlapRazboi.instance.CardsOnGround.Count - 1].CardSpriteIndex];
                    CardSlot1.sprite = CardImages[HitSlapRazboi.instance.CardsOnGround[HitSlapRazboi.instance.CardsOnGround.Count - 2].CardSpriteIndex];
                    break;
                }
            //over 3
            default:
                {
                    CardSlot2.sprite = CardImages[HitSlapRazboi.instance.CardsOnGround[HitSlapRazboi.instance.CardsOnGround.Count - 1].CardSpriteIndex];
                    CardSlot1.sprite = CardImages[HitSlapRazboi.instance.CardsOnGround[HitSlapRazboi.instance.CardsOnGround.Count - 2].CardSpriteIndex];
                    CardSlot0.sprite = CardImages[HitSlapRazboi.instance.CardsOnGround[HitSlapRazboi.instance.CardsOnGround.Count - 3].CardSpriteIndex];
                    break;
                }
        }

        if (HitSlapRazboi.instance.SlapCard != null)
        {
            SlapImage.sprite = CardImages[HitSlapRazboi.instance.SlapCard.CardSpriteIndex];
        }
    }
    void AssignColors()
    {
        foreach(Image portrait in PlayerPortrait)
        {
            portrait.color = PlayerDefaultColor;
        }
        
        if (HitSlapRazboi.instance.InititalSetupDone)
        {
            PlayerPortrait[correctOrder.IndexOf(HitSlapRazboi.instance.IndexOfActivePlayer)].color = ActivePlayerColor;
        }
    }
    void SetNames()
    {
        for(int i = 0; i < correctOrder.Count; i++)
        {
            PlayerName[i].text = "P" + (correctOrder[i] + 1).ToString();
        }
    }    
    public void DeactivateVisualDecks()
    {
        for (int i = 0; i < 5; i++)
        {
            PlayerVisualDecks[i].SetActive(false);
            PlayerPortrait[i].gameObject.SetActive(false);
            PlayerCardCount[i].gameObject.SetActive(false);
        }
    }
    public void ActivateVisualDecks(int index)
    {
        PlayerVisualDecks[index].SetActive(true);
        PlayerPortrait[index].gameObject.SetActive(true);
        PlayerCardCount[index].gameObject.SetActive(true);
    }
}
