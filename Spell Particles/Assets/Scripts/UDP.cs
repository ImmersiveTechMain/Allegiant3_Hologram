using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class UDP : MonoBehaviour
{
    static Thread listeningThread;
    static bool InitializeOnce = true;
    public static int DefaultWritePort { private set; get; }
    public static int DefaultListenPort { private set; get; }
    public static string DefaultWriteIP { private set; get; }

    static bool hasBeenIntialized = false;

    Queue<string> receivedMessages = new Queue<string>();
    public static List<string> receivedMessagesHistory = new List<string>();
    public static List<string> sentMessagesHistory = new List<string>();

    public string ip = "10.199.199.100";
    public string hostName = "255.255.255.10";
    public int port = 3000;
    public int writePort = 2500;
    public static bool threadRunning { private set; get; }

    public delegate void MessageCallback(string message);
    public static MessageCallback onMessageReceived = delegate (string message) { };

    static UdpClient listener;

    private void Start() {
        Beging();
    }

    // Use this for initialization
    public void Beging() {
        if (InitializeOnce && hasBeenIntialized) { return; }
        DefaultListenPort = port;
        DefaultWritePort = writePort;
        DefaultWriteIP = ip;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize() {
        threadRunning = false;
        if (listeningThread != null) { listeningThread.Abort(); yield return null; }
        if (listener != null) { listener.Close(); yield return null; }

        bool allGood = false;

        while (!allGood) {
            if (Application.isPlaying) {
                try {
                    listener = new UdpClient(port);
                    listeningThread = new Thread(Listen);
                    listeningThread.Name = "UDP Listening Thread";
                    listeningThread.Start();
                    allGood = true;
                    hasBeenIntialized = true;
                } catch (System.Exception e) {
                    Debug.Log(e);
                    allGood = false;
                }
            }
            yield return null;
        }
    }

    public static string Write(string message, string ip = null, int port = -1) {
        try {
            UdpClient sender = new UdpClient();
            sender.Connect(ip == null ? DefaultWriteIP : ip, port <= 0 ? DefaultWritePort : port);
            byte[] parsed = System.Text.Encoding.ASCII.GetBytes(message);
            sender.Send(parsed, parsed.Length);
            sender.Close();
            sentMessagesHistory.Add(message);
            Debug.Log("\"" + message + "\" HAS BEEN SENT");
            return "Sent";

        } catch (System.Exception e) {
            Debug.Log("Error at sending: ".ToUpper() + e.Message);
            return "Error at sending: ".ToUpper() + e.Message;
        }
    }

    public void Broadcast(string message, string hostName = null, int port = -1) {
        UdpClient sender = new UdpClient();
        sender.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        byte[] parsed = System.Text.Encoding.ASCII.GetBytes(message);
        sender.Send(parsed, parsed.Length, hostName == null ? this.hostName : hostName, port <= 0 ? this.port : port);
    }


    private void Update() {
        if (receivedMessages.Count > 0) {
            string message = receivedMessages.Dequeue();
            onMessageReceived(message);
            Debug.Log("Message Received - \"" + message + "\"");
        }
    }

    void Listen() {
        threadRunning = true;
        while (threadRunning) {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            byte[] answer = listener.Receive(ref ip);
            OnByteReceived(answer);
        }
        threadRunning = false;

    }

    void OnByteReceived(byte[] receivedMessageBytes) {
        string receivedMessage = System.Text.Encoding.ASCII.GetString(receivedMessageBytes);
        receivedMessage = receivedMessage.Trim('\0');
        receivedMessagesHistory.Add(receivedMessage);
        receivedMessages.Enqueue(receivedMessage);
    }



    void OnApplicationQuit() {
        threadRunning = false;
        if (listener != null) { listener.Close(); }
        if (listeningThread != null) {
            listeningThread.Abort();
        }
    }
}


