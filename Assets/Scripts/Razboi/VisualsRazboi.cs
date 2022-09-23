using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

[System.Serializable]
public class ControllerImageCounter
{
    public Sprite[] CardsCollection = new Sprite[10];
    public Image[] ImageConstruction = new Image[2];
    public int[] cardIndexes = new int[2];
}

public class VisualsRazboi : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI hitCounter;
    [SerializeField] TextMeshProUGUI slapCounter;
    [SerializeField] GameObject EndGame;
    [SerializeField] GameObject WIN;
    [SerializeField] GameObject LOSE;

    [SerializeField] GameObject SlapPanel;
    [Header("PlayerSpots")]
    [SerializeField] Color PlayerDefaultColor;
    [SerializeField] Color ActivePlayerColor;
    public ControllerImageCounter[] playerCardImages = new ControllerImageCounter[5];
    [SerializeField] List<int> correctOrder = new List<int>();
    [SerializeField] List<Sprite> PlayerPortrait = new List<Sprite>();    
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
    [SerializeField] SpriteRenderer Table;
    TextMeshProUGUI SlapName;
    TextMeshProUGUI ReactionTxt;
    Animator SlapAnimator;
    AudioSource SlapSoundEffect;


    private void Awake()
    {
        if(Application.isBatchMode) { Destroy(this); }
        EndGame.SetActive(false);
        WIN.SetActive(false);
        LOSE.SetActive(false);
        StartGame.SetActive(false);
        SlapPanel.SetActive(false);
        if (HitSlapRazboi.CheckUI == null) { HitSlapRazboi.CheckUI = new UnityEvent<int>(); }
        if (HitSlapRazboi.EndGame == null) { HitSlapRazboi.EndGame = new UnityEvent<List<string>>(); }
        if (HitSlapRazboi.SlapSuccess == null) { HitSlapRazboi.SlapSuccess = new UnityEvent<string, int>(); }
        if (HitSlapRazboi.SlapAnimation == null) { HitSlapRazboi.SlapAnimation = new UnityEvent(); }
        if (HitSlapRazboi.HitCard == null) { HitSlapRazboi.HitCard = new UnityEvent(); }
        if (HitSlapRazboi.StartGame == null) { HitSlapRazboi.StartGame = new UnityEvent(); }

        HitSlapRazboi.CheckUI.AddListener(CheckUIButtons);
        HitSlapRazboi.EndGame.AddListener(ExecuteEndGame);
        HitSlapRazboi.SlapSuccess.AddListener(SuccessSlap);
        HitSlapRazboi.SlapAnimation.AddListener(SlapAnimation);
        HitSlapRazboi.HitCard.AddListener(HitEvent);
        HitSlapRazboi.StartGame.AddListener(StartEvent);

        StartCoroutine(WaitForLocal());
        SlapName = SlapPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ReactionTxt = SlapPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        SlapAnimator = GameObject.Find("Glass").GetComponent<Animator>();
        SlapSoundEffect = GameObject.Find("Table").GetComponent<AudioSource>();       

    }

    void Update()
    {
        if(HitSlapRazboi.instance == null ) { return; }

        DeactivateVisualDecks();
        for (int i = 0; i < HitSlapRazboi.instance.CardCount.Count; i++)
        {
            ActivateVisualDecks(i);
        }
       
       
        CardCountUpdate();
        CardsOnGroundVisual();
        AssignColors();
        SetNames();
        CheckSlapButton();
    }

    public void UpdateVisualsForIndex(int Index)
    {
        string localCardCount = HitSlapRazboi.instance.CardCount[Index].ToString();
        switch (localCardCount.Length)
        {
            case 1:
                {
                    playerCardImages[Index].cardIndexes[0] = 0;
                    playerCardImages[Index].cardIndexes[1] = (int)localCardCount[0];
                    break;
                }
            case 2:
                {
                    playerCardImages[Index].cardIndexes[0] = (int)localCardCount[0];
                    playerCardImages[Index].cardIndexes[1] = (int)localCardCount[1];
                    break;
                }
            default:
                {
                    playerCardImages[Index].cardIndexes[0] = 0;
                    playerCardImages[Index].cardIndexes[1] = 0;
                    Debug.Log("PANICA PANICA PANICA");
                    break;
                }
        }

        for (int i = 0; i < 2; i++)
        {
            playerCardImages[Index].ImageConstruction[i].sprite = playerCardImages[Index].CardsCollection[playerCardImages[Index].cardIndexes[i]];
        }
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
                //Debug.Log("I am HOST");
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
        try
        {
            for (int i = 0; i < correctOrder.Count; i++)
            {
                PlayerCardCount[correctOrder.IndexOf(i)].text = HitSlapRazboi.instance.CardCount[i].ToString();
                UpdateVisualsForIndex(i);
            }
            hitCounter.text = HitSlapRazboi.instance.CardsToHit.ToString();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
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
        try
        {
            if (HitSlapRazboi.instance.InititalSetupDone)
            {
                Table.sprite = PlayerPortrait[correctOrder.IndexOf(HitSlapRazboi.instance.IndexOfActivePlayer)];
            }
        }

        catch(Exception e)
        {
            Debug.Log(e);
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
            //PlayerPortrait[i].gameObject.SetActive(false);
            PlayerCardCount[i].gameObject.SetActive(false);
        }
    }
    public void ActivateVisualDecks(int index)
    {
        PlayerVisualDecks[index].SetActive(true);
        //PlayerPortrait[index].gameObject.SetActive(true);
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

    void ExecuteEndGame(List<string> winOrder)
    {
        HitButton.gameObject.SetActive(false);
        SlapButton.gameObject.SetActive(false);
        EndGame.SetActive(true);
        SoundManager.instance.Sounds[10].source.Stop();
        if(CardPlayer.localPlayer.Nome == winOrder[0])
        {
            SoundManager.instance.Sounds[3].source.Play();
            WIN.SetActive(true);
        }
        else
        {
            LOSE.SetActive(true);
            SoundManager.instance.Sounds[5].source.Play();
        }
        
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
        //StopAllCoroutines();

        SlapName.text = Name;
        ReactionTxt.text = ReactionTime.ToString();

        //StartCoroutine(StopSlapPanel(5));
    }
    void SlapAnimation()
    {
        SlapSoundEffect.Play(0);
        SlapAnimator.Play("Glass_Shaking");
    }

    IEnumerator StopSlapPanel(float WaitTime)
    {
        SlapPanel.SetActive(true);
        yield return new WaitForSeconds(WaitTime);
        SlapPanel.SetActive(false);
    }

    void HitEvent()
    {
        SoundManager.instance.Sounds[UnityEngine.Random.Range(6, 10)].source.Play();
    }

    void StartEvent()
    {
        SoundManager.instance.Sounds[UnityEngine.Random.Range(1, 3)].source.Play();
        try
        { CalculateCorrectOrder(correctOrder); }
        catch { Debug.LogWarning("temp Error"); }

    }

    public void ToggleConsole()
    {
        SlapPanel.SetActive(!SlapPanel.activeSelf);
    }
}
