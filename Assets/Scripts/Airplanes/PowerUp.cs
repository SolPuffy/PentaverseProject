using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using Mirror;
using Unity.VisualScripting;

[CreateAssetMenu(menuName = "FishyBusiness/PowerUp")]
public class PowerUp : ScriptableObject
{
    [SerializeField]
    public _PowerUpType PowerUpType;
    public int PowerUpIconIndex;
    [SerializeField]
    [Range(1,100)]
    public int PowerUpRequiredWeight;
    public int AvailableQuanity = 5;
    public bool IsRetroactive = false;
    [TextArea(10,10)]
    public string PatternStrike_Pattern;

    public async Task OnUse(object Parameter,int powerupSlot, int indexOfPlayer)
    {
        switch((int)PowerUpType)
        {
            case 0: { PatternStrikeTrigger((Vector3Int)Parameter);break; }
            case 1: { ShieldBattery((int)Parameter); break; }
            case 2: { SpawnClone((int)Parameter);break; }
            case 3: { Misfire((int)Parameter);break; }
            default:break;
        }
        ServerActions.Instance.PlanesPlayers[indexOfPlayer].takePlayerPowerup(powerupSlot);
        
        await Task.Yield();
    }
    /*
     *  aDiff.text.Length - aDiff.text.Replace _(Environment.NewLine, string.Empty).Length;
     */

    private async void ShieldBattery(int UseAtIndex)
    {
        ServerActions.Instance.PlayersList[UseAtIndex].isShielded = true;
        await Task.Yield();
    }    
    private async void SpawnClone(int UseAtIndex)
    {
        await ServerActions.Instance.AttemptToArrangePlayers(true, UseAtIndex);
        //await Task.Yield();
    }
    private async void Misfire(int UseAtIndex)
    {
        ServerActions.Instance.PlayersList[UseAtIndex].Misfire = true;
        await Task.Yield();
    }
    private async void PatternStrikeTrigger(Vector3Int UseAtLocation)
    {
        List<CoordsStructure> PatternPointsToStrike = new List<CoordsStructure>();
        List<Vector3Int> PatternCoordsToTiles = new List<Vector3Int>();
        await PatternDecoder(PatternPointsToStrike);
        await BuildDecodedArray(PatternPointsToStrike,PatternCoordsToTiles,UseAtLocation);
        await ServerActions.Instance.VerifyAndUpdatePattern(PatternCoordsToTiles.ToArray());
        debugCoords(PatternCoordsToTiles.ToArray());
        await ServerActions.Instance.PatternCalledOnTileLocation(PatternCoordsToTiles.ToArray());
        debugCoords(PatternCoordsToTiles.ToArray());
    }    
    private async Task PatternDecoder(List<CoordsStructure> structure)
    {
        string[] lines = Regex.Split(PatternStrike_Pattern, "\\n");
        if(lines[0].Length % 2 == 0 || lines.Length % 2 == 0)
        {
            Debug.Log("Powerup : Pattern strike cannot have even ranges.");
            return;
        }
        int centerOffset = lines[0].Length / 2;
        for (int i = -centerOffset; i < centerOffset + 1; i++)
        {
            for (int j = -centerOffset; j < centerOffset + 1; j++)
            {
                if (lines[i+centerOffset][j+centerOffset] == 'X' || lines[i + centerOffset][j + centerOffset] == 'x')
                {
                    CoordsStructure coords = new CoordsStructure();
                    coords.X = i;
                    coords.Y = j;
                    structure.Add(coords);
                }
            }
        }
        await Task.Yield();
    }
    
    private void debugCoords(Vector3Int[] newCoords)
    {
        string localString = "";
        for(int i=0;i<newCoords.Length;i++)
        {
            localString += newCoords.ToString() + " ";
        }
        Debug.Log(localString);
    }    
    private async Task BuildDecodedArray(List<CoordsStructure> structure,List<Vector3Int> targeting,Vector3Int center)
    {
        for (int i = 0; i < structure.Count; i++)
        {
            targeting.Add(center + new Vector3Int(structure[i].X, structure[i].Y));
        }
        await Task.Yield();
    }
}
public enum _PowerUpType
{
    PatternStrike = 0,
    ShieldBattery = 1,
    Clone = 2,
    Misfire = 3
}