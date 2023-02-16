using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayReplay : MonoBehaviour
{   
    [SerializeField] GameObject SlapPanel;
    [SerializeField] SpriteRenderer Table;
    [SerializeField] float TimeBetweenActions_seconds = 0.2f;
    [SerializeField] float TimeBetweenWinRounds_seconds = 1f;
    public TMP_InputField inputIndex;
    TextMeshProUGUI SlapName;
    TextMeshProUGUI ReactionTxt;
    Animator SlapAnimator;
    AudioSource SlapSoundEffect;

    [Header("PlayerSpots")]
    [SerializeField] List<Sprite> TableCurrentTurn = new List<Sprite>();
    [SerializeField] List<TextMeshProUGUI> PlayerName = new List<TextMeshProUGUI>();    
    [SerializeField] List<GameObject> PlayerVisualDecks = new List<GameObject>();
    [SerializeField] ControllerImageCounter[] playerCardImages = new ControllerImageCounter[5];
    [SerializeField] List<TextMeshProUGUI> PlayerCardCount = new List<TextMeshProUGUI>();


    [Header("Cards")]
    [SerializeField] Image SlapImage;
    [SerializeField] Image CardSlot0;
    [SerializeField] Image CardSlot1;
    [SerializeField] Image CardSlot2;
    [SerializeField] Sprite BlankSprite;
    [SerializeField] List<Sprite> CardImages = new List<Sprite>();

    BackupData ReplayData;
    

    private void Awake()
    {
        SlapPanel.SetActive(false);


        SlapName = SlapPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ReactionTxt = SlapPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        SlapAnimator = GameObject.Find("Glass").GetComponent<Animator>();
        SlapSoundEffect = GameObject.Find("Table").GetComponent<AudioSource>();

        SlapImage.sprite = BlankSprite;
        CardSlot0.sprite = BlankSprite;
        CardSlot1.sprite = BlankSprite;
        CardSlot2.sprite = BlankSprite;

        DeactivateVisualDecks();
    }
    public void StartReplay()
    {
        SoundManager.instance.Sounds[UnityEngine.Random.Range(1, 3)].source.Play();
        StartCoroutine(PLAY());        
    }

    IEnumerator PLAY()
    {
        foreach(Action act in ReplayData.Actions)
        {           
            if (act.actionType =="Hit")
            {
                yield return ActionHit();
            }
            else
            {
                yield return ActionSlap(act.playerName, act.SlapSuccessful, act.SlapResponseTime);
            }

           
            if (act.WonRound) yield return new WaitForSeconds(TimeBetweenWinRounds_seconds);

            ShowCurrentState(act.CurrentGameState.CardCount, act.CurrentGameState.CardsonGroundIndexes, act.CurrentGameState.TurnIndex);
        }       
       
    }

    IEnumerator ActionHit()
    {
        SoundManager.instance.Sounds[UnityEngine.Random.Range(6, 10)].source.Play();
        yield return new WaitForSeconds (TimeBetweenActions_seconds);
    }

    IEnumerator ActionSlap(string PlayerName, bool Success, int ReactionTime)
    {
        SlapAnimation();

        if (Success)
            SuccessSlap(PlayerName, ReactionTime);
        yield return new WaitForSeconds(TimeBetweenActions_seconds);
    }
    

    public async void inputFieldToData()
    {
        Debug.Log("Trying to find file replay : " + inputIndex.text);
        inputIndex.gameObject.SetActive(false);
        ReplayData = await ServerBackup.RetrieveDataHoldFromServer(inputIndex.text);
        
        StartReplay();
    }    
    
    private void ShowCurrentState(int[] CardCount, int[] CardsonGroundIndexes, int TurnIndex)
    {      
        //Show Current Turn
        Table.sprite = TableCurrentTurn[TurnIndex];

        //CardCount Visuals
        DeactivateVisualDecks();
        for (int i = 0; i < CardCount.Length; i++)
        {
            PlayerCardCount[i].gameObject.SetActive(true);
            PlayerVisualDecks[i].SetActive(true);
            UpdateVisualsForIndex(i, CardCount[i]);
        }

        //CardsonGround Visuals
        if (CardsonGroundIndexes[0] == -1) { SlapImage.sprite = BlankSprite; } else { SlapImage.sprite = CardImages[CardsonGroundIndexes[0]]; }
        if (CardsonGroundIndexes[1] == -1) { CardSlot0.sprite = BlankSprite; } else { CardSlot0.sprite = CardImages[CardsonGroundIndexes[1]]; }
        if (CardsonGroundIndexes[2] == -1) { CardSlot1.sprite = BlankSprite; } else { CardSlot1.sprite = CardImages[CardsonGroundIndexes[2]]; }
        if (CardsonGroundIndexes[3] == -1) { CardSlot2.sprite = BlankSprite; } else { CardSlot2.sprite = CardImages[CardsonGroundIndexes[3]]; }
    }   

    void SuccessSlap(string Name, int ReactionTime)
    {
        SlapName.text = Name;
        ReactionTxt.text = ReactionTime.ToString();        
    }

    void SlapAnimation()
    {
        SlapSoundEffect.Play(0);
        SlapAnimator.Play("Glass_Shaking");
    }

    public void TogglePanel()
    {
        SlapPanel.SetActive(!SlapPanel.activeSelf);        
    }

    void UpdateVisualsForIndex(int Index, int CardCount)
    {
        string localCardCount = CardCount.ToString();
        switch (localCardCount.Length)
        {
            case 1:
                {
                    playerCardImages[Index].cardIndexes[0] = 0;
                    playerCardImages[Index].cardIndexes[1] = Int32.Parse(localCardCount[0].ToString());
                    break;
                }
            case 2:
                {
                    playerCardImages[Index].cardIndexes[0] = Int32.Parse(localCardCount[0].ToString());
                    playerCardImages[Index].cardIndexes[1] = Int32.Parse(localCardCount[1].ToString());
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
            playerCardImages[Index].ImageConstruction[i].sprite =
                playerCardImages[Index].CardsCollection[playerCardImages[Index].cardIndexes[i]];
        }
    }

    void DeactivateVisualDecks()
    {
        for (int i = 0; i < PlayerVisualDecks.Count; i++)
        {
            PlayerVisualDecks[i].SetActive(false);
            PlayerCardCount[i].gameObject.SetActive(false);
        }
    }
}
