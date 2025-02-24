using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ushort id;
    public string userName = "Player";
    public int color = 0;

    [SerializeField]
    public SpriteRenderer baseColor;
    [SerializeField]
    public Transform pivot;
    [SerializeField]
    public Transform handHolder;
    [SerializeField]
    public SpriteRenderer[] hands = new SpriteRenderer[2];

    public void SetSpawnInfo(ushort _id, string _userName, int _color)
    {
        id = _id;
        userName = _userName;
        color = _color;

        baseColor.color = GameManager.instance.playerColors[color];
        foreach(SpriteRenderer _hand in hands)
        {
            _hand.color = GameManager.instance.playerColors[color];
        }
    }

    public void ChargePush()
    {
        handHolder.localPosition = new Vector3(0f, -0.25f, 0f);
    }

    public void ExecutePush()
    {
        handHolder.localPosition = new Vector3(0f, 0.5f, 0f);
    }

    public void ResetPush()
    {
        handHolder.localPosition = new Vector3(0f, 0f, 0f);
    }
}
