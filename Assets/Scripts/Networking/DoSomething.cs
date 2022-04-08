using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

/// <summary>
/// TEST CODE
/// </summary>
public class DoSomething : MonoBehaviour
{
    [MessageHandler((ushort)ServerToClientId.testMessage)]
    public static void ActuallyIrrelevantFunctionName(Message message)
    {
        string sendString = message.GetString();
        int sendInt = message.GetInt();

        // ...
        Debug.Log("retrieved data: " + sendString + ", " + sendInt);
    }
}
