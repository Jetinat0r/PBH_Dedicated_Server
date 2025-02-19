using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public enum ClientToServerId : ushort
{
    requestDisconnect = 0,
    joinInfo = 1,
    playerPosRot = 2,
    shoveStart = 3,
    shoveExecute = 4,
}

public enum ServerToClientId : ushort
{
    disconnectWithReason = 0,
    playerSpawnInfo = 1,
    playerPosRot = 2,
    playerShoveStart = 3,
    playerShoveExecute = 4,
}

public class ServerManager : MonoBehaviour
{
    Server server;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        server.Update();
    }

    private void HandleIncomingConnection(Connection _pendingConnection, Message _connectMessage)
    {
        if (isMatchInProgress)
        {
            Message _disconnectReason = Message.Create();
            _disconnectReason.AddString("Can't join match in progress!");
            server.Reject(_pendingConnection, _disconnectReason);
            return;
        }

        server.Accept(_pendingConnection);
        //TODO: Try to sort out name & color right here
    }

    private void OnClientConnected(object _sender, ServerConnectedEventArgs _e)
    {
        
    }
}
