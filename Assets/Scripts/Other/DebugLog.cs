using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugLog : MonoBehaviour
{
    static public TMP_Text debugLogText;
    static private List<string> messages;

    private void Start()
    {
        messages = new List<string>();
        debugLogText = GetComponent<TMP_Text>();
    }

    static public void Log(string message)
    {
        int maxLines = 6;

        messages.Add(message);

        if(messages.Count >= maxLines) { messages.RemoveAt(0); }

        string logText = "";

        foreach(string messageText in messages)
        {
            logText += messageText;
            logText += "\n";
        }

        debugLogText.text = logText;
    }
}
