using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientForPlayer : MonoBehaviour
{
    Quaternion quaternion;
    private void Start()
    {
        quaternion = transform.rotation;
        Debug.Log($"Rotation  : {quaternion.eulerAngles.x}, {quaternion.eulerAngles.y}, {quaternion.eulerAngles.z}");
    }
    void Update()
    {
        try
        {
            transform.rotation = Quaternion.Euler(quaternion.eulerAngles.x, 72 * CardPlayer.localPlayer.playerIndex, quaternion.eulerAngles.z);
        }
        catch { }
    }
}
