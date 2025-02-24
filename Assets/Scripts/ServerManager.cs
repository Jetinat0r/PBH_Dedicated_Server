using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using System.Linq;

public enum ClientToServerId : ushort
{
    requestDisconnect = 0,
    playerPosRot,
    pushStart,
    pushExecute,
    playerPushReturn,
    placeBulletPattern,
}

public enum ServerToClientId : ushort
{
    disconnectWithReason = 0,
    playerSpawnInfo,
    playerPosRot,
    playerPushStart,
    playerPushExecute,
    playerPushReturn,
    spawnBulletPattern,
}

public class ServerManager : MonoBehaviour
{
    //Maximum length in chars a player's name can be
    //  The server can return a name longer due to adding identifiers (e.g. Player1, Player2)
    //  This length ensures that with the server's additions the name is not too long for the display field
    public const int PLAYER_NAME_MAX_LENGTH = 10;

    public static ServerManager instance;

    public Server server;
    [SerializeField]
    private ushort port = 7777;
    [SerializeField]
    public ushort maxClientCount = 4;
    [SerializeField]
    //TODO: Move to different place
    public bool isMatchInProgress = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void InitializeRiptide()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        string[] _cmdLineArgs = Environment.GetCommandLineArgs();

        foreach(string _arg in _cmdLineArgs)
        {
            if (_arg.StartsWith("-port="))
            {
                string _portString = _arg.Substring(6);
                if(ushort.TryParse(_portString, out ushort _newPort))
                {
                    port = _newPort;
                }
                else
                {
                    Debug.LogError($"Port argument [{_arg}] found, but port was invalid!");
                }
            }
            else if (_arg.StartsWith("-max-players="))
            {
                string _maxPlayersString = _arg.Substring(13);
                if (ushort.TryParse(_maxPlayersString, out ushort _newMaxPlayers))
                {
                    if(_newMaxPlayers < 2)
                    {
                        Debug.LogError("Player counts less than 2 are not supported!");
                        continue;
                    }

                    maxClientCount = _newMaxPlayers;
                }
                else
                {
                    Debug.LogError($"Max Players argument [{_arg}] found, but number was invalid!");
                }
            }
        }

        server = new Server();
        server.Start(port, maxClientCount);

        server.HandleConnection += HandleIncomingConnection;
        server.ClientConnected += OnClientConnected;

        GameManager.instance.SubscribeToPlayerDisconnect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        server.Update();
    }

    private void RejectConnectionForReason(Connection _pendingConnection, string _reason)
    {
        Message _disconnectReason = Message.Create();
        _disconnectReason.AddString(_reason);
        server.Reject(_pendingConnection, _disconnectReason);
    }

    private void HandleIncomingConnection(Connection _pendingConnection, Message _connectMessage)
    {
        if (isMatchInProgress)
        {
            RejectConnectionForReason(_pendingConnection, "Can't join match in progress!");
            return;
        }

        string _playerName = _connectMessage.GetString().Trim();
        int _playerColor = _connectMessage.GetInt();

        //Reject sneaky players who try to input bad names
        if(_playerName.Length <= 0 || _playerName.Length > PLAYER_NAME_MAX_LENGTH)
        {
            RejectConnectionForReason(_pendingConnection, $"Invalid player name received!");
            return;
        }

        //Reject sneaky players who try to use a color that doesn't exist
        if(_playerColor < 0 || _playerColor >= GameManager.instance.playerColors.Count)
        {
            RejectConnectionForReason(_pendingConnection, $"Invalid player color received!");
            return;
        }

        server.Accept(_pendingConnection);
        
        //Sort out name & color right here
        Message _assignNewPlayerInfo = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawnInfo);
        _assignNewPlayerInfo.AddUShort(_pendingConnection.Id);

        string _nameDeduper = "";
        int _nameDedupeCounter = 0;
        while(GameManager.playerList.Any((_existingPlayer) => _existingPlayer.Value.userName == _playerName + _nameDeduper))
        {
            _nameDedupeCounter++;
            _nameDeduper = _nameDedupeCounter.ToString();
        }
        _playerName += _nameDeduper;

        if(server.ClientCount > GameManager.instance.playerColors.Count)
        {
            //If we have more players than colors, cycle through the list
            _playerColor = _pendingConnection.Id % GameManager.instance.playerColors.Count;
        }
        else
        {
            //Try to give the player the color they want, else give them the first available color
            while(GameManager.playerList.Any((_existingPlayer) => _existingPlayer.Value.color == _playerColor))
            {
                _playerColor++;
                _playerColor %= GameManager.instance.playerColors.Count;
            }
        }

        _assignNewPlayerInfo.AddString(_playerName);
        _assignNewPlayerInfo.AddInt(_playerColor);

        //Alert existing players that someone new has joined
        server.SendToAll(_assignNewPlayerInfo);

        //Inform the new player about existing players
        foreach(KeyValuePair<ushort, Player> _existingPlayer in GameManager.playerList)
        {
            Message _sendExistingPlayer = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawnInfo);
            _sendExistingPlayer.AddUShort(_existingPlayer.Key);
            _sendExistingPlayer.AddString(_existingPlayer.Value.userName);
            _sendExistingPlayer.AddInt(_existingPlayer.Value.color);

            server.Send(_sendExistingPlayer, _pendingConnection.Id);
        }

        //Setup reference gameobject
        GameManager.instance.CreateNewPlayer(_pendingConnection.Id, _playerName, _playerColor);
    }

    private void OnClientConnected(object _sender, ServerConnectedEventArgs _e)
    {
        
    }
}
