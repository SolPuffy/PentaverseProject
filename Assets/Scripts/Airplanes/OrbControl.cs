using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbControl : MonoBehaviour
{
    public Image[] OrbSlots;
    public Sprite[] OrbSprites;
    public GameObject[] PlayerSlots;
    public Image[] LocalPlayer;

    private static Image[] accessToSlots = new Image[5];
    private static Sprite[] accessToSprites = new Sprite[5];
    public static GameObject[] accessPlayerSlots = new GameObject[5];
    public static  Image[] acessLocalPlayer = new Image[5];



    private void Awake()
    {        
        accessToSlots = OrbSlots;
        accessToSprites = OrbSprites;
        accessPlayerSlots = PlayerSlots;
        acessLocalPlayer = LocalPlayer;
    }

    private void Start()
    {
        ChangeOrbDefault();
    }

    // To Change colors at runtime
    // accessToOrbs[0] = Green  | Player's turn
    // accessToOrbs[1] = Gray   | NOT Player's turn (is still alive)
    // accessToOrbs[2] = Red    | Player is eliminated
    // accessToOrbs[3] = Black  | Player is disconnected
    // accessToOrbs[4] = Purple | Player is "Misfired"
    public static void ChangeOrbCustom(int Index,int OrbType)
    {
        accessToSlots[Index].sprite = accessToSprites[OrbType];
        
    }
    //To Change colors at game restart back to normal state (all players to gray, player 0 to green)
    public static void ChangeOrbDefault()
    {
        for(int i=0;i<5;i++)
        {
            accessToSlots[i].sprite = accessToSprites[1]; 
        }
        accessToSlots[0].sprite = accessToSprites[0];
    }
}
