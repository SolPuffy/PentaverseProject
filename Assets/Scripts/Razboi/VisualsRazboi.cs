using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class VisualsRazboi : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI hitCounter;
    [SerializeField] TextMeshProUGUI slapCounter;
    [SerializeField] GameObject EndGame;
    [SerializeField] GameObject SlapPanel;
    [Header("PlayerSpots")]
    [SerializeField] Color PlayerDefaultColor;
    [SerializeField] Color ActivePlayerColor;
    [SerializeField] List<int> correctOrder = new List<int>();
    [SerializeField] List<Image> PlayerPortrait = new List<Image>();    
    [SerializeField] List<TextMeshProUGUI> PlayerName = new List<TextMeshProUGUI>();
    [SerializeField] List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();
    [SerializeField] List<GameObject> PlayerVisualDecks = new List<GameObject>();  
    [Header("Cards")]    
    [SerializeField] Image SlapImage;
    [SerializeField] Image CardSlot0;
    [SerializeField] Image CardSlot1;
    [SerializeField] Image CardSlot2;
    [SerializeField] Sprite BlankSprite;
    [SerializeField] List<Sprite> CardImages = new List<Sprite>();
    [Header("UIButtons")]
    [SerializeField] Button HitButton;
    [SerializeField] Button SlapButton;    
    [SerializeField] GameObject StartGame;
    TextMeshProUGUI SlapName;
    TextMeshProUGUI ReactionTxt;
    Animator SlapAnimator;


    private void Awake()
    {
        if(Application.isBatchMode) { Destroy(this); }
        EndGame.SetActive(false);
        StartGame.SetActive(false);
        SlapPanel.SetActive(false);
        if (HitSlapRazboi.CheckUI == null) { HitSlapRazboi.CheckUI = new UnityEvent<int>(); }
        if (HitSlapRazboi.EndGame == null) { HitSlapRazboi.EndGame = new UnityEvent(); }
        if (HitSlapRazboi.SlapSuccess == null) { HitSlapRazboi.SlapSuccess = new UnityEvent<string, int>(); }

        HitSlapRazboi.CheckUI.AddListener(CheckUIButtons);
        HitSlapRazboi.EndGame.AddListener(ExecuteEndGame);
        HitSlapRazboi.SlapSuccess.AddListener(SuccessSlap);
        HitSlapRazboi.SlapAnimation.AddListener(SlapAnimation);
        StartCoroutine(WaitForLocal());
        SlapName = SlapPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ReactionTxt = SlapPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        SlapAnimator = GameObject.Find("Glass").GetComponent<Animator>();

    }

    void Update()
    {
        if(HitSlapRazboi.instance == null ) { return; }

        DeactivateVisualDecks();
        for (int i = 0; i < HitSlapRazboi.instance.CardCount.Count; i++)
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
        CheckSlapButton();
    }

    IEnumerator WaitForLocal()
    {
        Debug.Log("waiting for local");
        while (CardPlayer.localPlayer == null || CardPlayer.localPlayer.playerIndex > 10)
        {            
            yield return null;
        }
        Debug.Log("I have local player and index");
        StartCoroutine(WaitForStart());
    }

    IEnumerator WaitForStart()
    {
        while(!HitSlapRazboi.instance.InititalSetupDone)
        {
            if (CardPlayer.localPlayer.playerIndex == 0)
            {
                Debug.Log("I am HOST");
                StartGame.SetActive(true);
            }
            else
            {
                StartGame.SetActive(false);
            }
            yield return null;
        }
    }

    void CalculateCorrectOrder(List<int> _correctOrder)
    {
        _correctOrder.Clear();
        for (int i = 0; i < HitSlapRazboi.instance.CardCount.Count; i++)
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
            PlayerCardCount[correctOrder.IndexOf(i)].text = HitSlapRazboi.instance.CardCount[i].ToString();
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
            //PlayerName[i].text = "P" + (correctOrder[i] + 1).ToString();
            PlayerName[correctOrder.IndexOf(i)].text = HitSlapRazboi.instance.PlayerNames[i];
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

    void CheckUIButtons(int indexACtivePlayer)
    {
        Debug.Log($"Checking Turn my index: {CardPlayer.localPlayer.playerIndex} with {indexACtivePlayer}");
        if (indexACtivePlayer == CardPlayer.localPlayer.playerIndex)
        {
            HitButton.interactable = true;
        }
        else
        {
            HitButton.interactable = false;
        }
    }

    void ExecuteEndGame()
    {
        HitButton.gameObject.SetActive(false);
        SlapButton.gameObject.SetActive(false);
        EndGame.SetActive(true);
    }

    void CheckSlapButton()
    {
        SlapButton.interactable = false;
        if (!HitSlapRazboi.instance.InititalSetupDone) { return; }

        try
        {
            if (HitSlapRazboi.instance.SlapsLeft[CardPlayer.localPlayer.playerIndex] > 0)
            {
                SlapButton.interactable = true;
            }
        }
        catch { Debug.LogWarning("probably no localplayer yet"); }
        
    }

    void SuccessSlap(string Name, int ReactionTime)
    {
        StopAllCoroutines();

        SlapName.text = Name;
        ReactionTxt.text = ReactionTime.ToString();

        StartCoroutine(StopSlapPanel(5));
    }
    public void SlapAnimation()
    {

        SlapAnimator.Play("Glass_Shaking");
    }

    IEnumerator StopSlapPanel(float WaitTime)
    {
        SlapPanel.SetActive(true);
        yield return new WaitForSeconds(WaitTime);
        SlapPanel.SetActive(false);
    }

 
}
