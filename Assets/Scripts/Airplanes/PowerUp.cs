using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using Mirror;

[CreateAssetMenu(menuName = "FishyBusiness/PowerUp")]
public class PowerUp : ScriptableObject
{
    [SerializeField]
    public _PowerUpType PowerUpType;
    [SerializeField]
    [Range(1,100)]
    public int PowerUpRequiredWeight;
    public int AvailableQuanity = 5;
    [TextArea(10,10)]
    public string PatternStrike_Pattern;
    public async Task OnUse(Vector3Int UseAtLocation)
    {
        switch((int)PowerUpType)
        {
            case 0: { PatternStrikeTrigger(UseAtLocation);break; }
            case 1: { /* new stuff here*/; break; }
            default:break;
        }
        await Task.Yield();
    }
    /*
     *  aDiff.text.Length - aDiff.text.Replace _(Environment.NewLine, string.Empty).Length;
     */

    private async void PatternStrikeTrigger(Vector3Int UseAtLocation)
    {
        List<CoordsStructure> PatternPointsToStrike = new List<CoordsStructure>();
        List<Vector3Int> PatternCoordsToTiles = new List<Vector3Int>();
        await PatternDecoder(PatternPointsToStrike);
        await BuildDecodedArray(PatternPointsToStrike,PatternCoordsToTiles,UseAtLocation);
        await ServerActions.Instance.VerifyAndUpdatePattern(PatternCoordsToTiles.ToArray());
        await ServerActions.Instance.PatternCalledOnTileLocation(PatternCoordsToTiles.ToArray()); 
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
    Whatever = 1
}