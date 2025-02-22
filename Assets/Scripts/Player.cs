using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ushort id;
    public string userName = "Player";
    public int color = 0;

    public void SetSpawnInfo(ushort _id, string _userName, int _color)
    {
        id = _id;
        userName = _userName;
        color = _color;
    }
}
