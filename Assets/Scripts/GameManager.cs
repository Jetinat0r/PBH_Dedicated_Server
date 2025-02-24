using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<ushort, Player> playerList = new Dictionary<ushort, Player>();
    [SerializeField]
    public List<Color> playerColors = new List<Color>();
    [SerializeField]
    public List<Transform> playerSpawnPositions = new List<Transform>();

    [SerializeField]
    public Player playerPrefab;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    public void SubscribeToPlayerDisconnect()
    {
        ServerManager.instance.server.ClientDisconnected += OnPlayerDisconnect;
    }

    public void CreateNewPlayer(ushort _id, string _userName, int _color)
    {
        Transform _spawnTransform = playerSpawnPositions[_id % playerSpawnPositions.Count];
        Player _newPlayer = Instantiate(playerPrefab, _spawnTransform.position, _spawnTransform.rotation);
        _newPlayer.SetSpawnInfo(_id, _userName, _color);
        playerList.Add(_id, _newPlayer);
    }

    public void OnPlayerDisconnect(object _sender, ServerDisconnectedEventArgs _e)
    {
        Debug.Log($"Player {_e.Client.Id} has disconnected!");
        if(playerList.TryGetValue(_e.Client.Id, out Player _disconnectedPlayer))
        {
            Destroy(_disconnectedPlayer.gameObject);
            playerList.Remove(_e.Client.Id);
        }
        else
        {
            Debug.LogWarning($"Can't remove disconnected player {_e.Client.Id} because they don't exist!");
        }
    }

    #region Messages
    [MessageHandler((ushort)ClientToServerId.playerPosRot)]
    public static void RecievePlayerPosRot(ushort _fromClientId, Message _message)
    {
        if(playerList.TryGetValue(_fromClientId, out Player _player))
        {
            Vector3 _playerPos = _message.GetVector3();
            Quaternion _playerRot = _message.GetQuaternion();

            _player.transform.SetPositionAndRotation(_playerPos, _playerRot);

            Message _forwardPlayerPosRot = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerPosRot);
            _forwardPlayerPosRot.AddUShort(_fromClientId);
            _forwardPlayerPosRot.AddVector3(_playerPos);
            _forwardPlayerPosRot.AddQuaternion(_playerRot);

            ServerManager.instance.server.SendToAll(_forwardPlayerPosRot, _fromClientId);
        }
        else
        {
            Debug.LogWarning($"Received PosRot from client {_fromClientId} that does not exist!");
        }
    }

    [MessageHandler((ushort)ClientToServerId.pushStart)]
    public static void RecievePlayerPushStart(ushort _fromClientId, Message _message)
    {
        if (playerList.TryGetValue(_fromClientId, out Player _player))
        {
            _player.ChargePush();

            Message _playerPushStart = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerPushStart);
            _playerPushStart.AddUShort(_fromClientId);

            ServerManager.instance.server.SendToAll(_playerPushStart, _fromClientId);
        }
        else
        {
            Debug.LogWarning($"Received Push Start from client {_fromClientId} that does not exist!");
        }
    }

    [MessageHandler((ushort)ClientToServerId.pushExecute)]
    public static void RecievePlayerPushExecute(ushort _fromClientId, Message _message)
    {
        if (playerList.TryGetValue(_fromClientId, out Player _player))
        {
            _player.ExecutePush();

            Message _playerPushExecute = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerPushExecute);
            _playerPushExecute.AddUShort(_fromClientId);

            ServerManager.instance.server.SendToAll(_playerPushExecute, _fromClientId);
        }
        else
        {
            Debug.LogWarning($"Received Push Execute from client {_fromClientId} that does not exist!");
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerPushReturn)]
    public static void RecievePlayerPushReturn(ushort _fromClientId, Message _message)
    {
        if (playerList.TryGetValue(_fromClientId, out Player _player))
        {
            _player.ResetPush();

            Message _playerPushReturn = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerPushReturn);
            _playerPushReturn.AddUShort(_fromClientId);

            ServerManager.instance.server.SendToAll(_playerPushReturn, _fromClientId);
        }
        else
        {
            Debug.LogWarning($"Received Push Return from client {_fromClientId} that does not exist!");
        }
    }
    #endregion
}
