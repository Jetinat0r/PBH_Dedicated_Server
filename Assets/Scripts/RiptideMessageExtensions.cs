using Riptide;
using UnityEngine;

public static class RiptideMessageExtensions
{
    public static Message Add(this Message _message, Vector2Int _value) => AddVector2Int(_message, _value);

    public static Message AddVector2Int(this Message _message, Vector2Int _value)
    {
        return _message.AddInt(_value.x).AddInt(_value.y);
    }

    public static Vector2Int GetVector2Int(this Message _message)
    {
        return new Vector2Int(_message.GetInt(), _message.GetInt());
    }

    public static Message Add(this Message _message, Vector3Int _value) => AddVector3Int(_message, _value);

    public static Message AddVector3Int(this Message _message, Vector3Int _value)
    {
        return _message.AddInt(_value.x).AddInt(_value.y).AddInt(_value.z);
    }

    public static Vector3Int GetVector3Int(this Message _message)
    {
        return new Vector3Int(_message.GetInt(), _message.GetInt(), _message.GetInt());
    }
}
