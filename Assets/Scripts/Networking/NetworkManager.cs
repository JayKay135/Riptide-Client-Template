using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using TMPro;
using UnityEngine;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;


/// <summary>
/// Always make sure, that these enums are identical on the server as on the client side
/// </summary>
public enum ServerToClientId : ushort
{
    testMessage = 1
}

public enum ClientToServerId : ushort
{
    hololensConnected = 1,
    log = 2,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }

        }
    }

    public Client Client { get; private set; }

    [SerializeField] private String ip;
    [SerializeField] private ushort port;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += ClientDisconnnected;
        Client.Disconnected += DidDisconnect;

        Connect();
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    /// <summary>
    /// Lets this client try to connect to the riptide server
    /// </summary>
    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    /// <summary>
    /// Callback on successfull connection to the riptide server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DidConnect(object sender, EventArgs e)
    {
        // resend already created logs
        foreach (Log log in LogStack.GetLogs())
        {
            Message newMessage = Message.Create(MessageSendMode.reliable, ClientToServerId.log);
            newMessage.AddInt(log.LogType);
            newMessage.AddString(log.Message);
            Singleton.Client.Send(newMessage);
        }
        LogStack.ClearLogs();


        Message isHololensMessage = Message.Create(MessageSendMode.reliable, ClientToServerId.hololensConnected);
        Singleton.Client.Send(isHololensMessage);


        /// TEST LOGS -> REMOVE
        LogManager.LogData("This is a normal log", LogManager.LogTypeApp.Normal);
        LogManager.LogData("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.", LogManager.LogTypeApp.Warning);
        LogManager.LogData("This is an error", LogManager.LogTypeApp.Error);        
        LogManager.LogData("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.", LogManager.LogTypeApp.Warning);
        LogManager.LogData("This is a normal log", LogManager.LogTypeApp.Normal);
        LogManager.LogData("This is an error", LogManager.LogTypeApp.Error);
    }

    /// <summary>
    /// Callback on failed connection attempt to the riptide server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FailedToConnect(object sender, EventArgs e)
    {
        
    }

    /// <summary>
    /// Callback when another connected client left the riptide network
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClientDisconnnected(object sender, ClientDisconnectedEventArgs e)
    {
        
    }

    /// <summary>
    /// Callback on disconnection invoked by the riptide server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DidDisconnect(object sender, EventArgs e)
    {
        
    }
}
