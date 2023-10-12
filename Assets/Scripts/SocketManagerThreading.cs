using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;


public enum Protocol
{
    TCP,
    UDP
}

// Class that manages network input. Can use both UDP and TCP protocols. Uses C# native threading to handle async user input.
public class SocketManagerThreading : MonoBehaviour
{
    // Change this in the editor
    [SerializeField]
    private Protocol protocol = Protocol.UDP;
    [SerializeField]
    private int port = 8999;

    private TcpListener _server;
    private UdpClient _udpClient;
    private IPEndPoint _endPoint;
    private Thread th;
    // Variable used to stop loop once Unity shuts down (not sure if necessary)
    private bool running = true;

    // Object to be used as mutex to access the below variables, that are also accessed asynchronously by worker thread
    public System.Object obj;

    // Received coordinates
    public double x = 0.0;
    public double y = 0.0;
    // Is the user currently touching the screen
    public bool touching = false;


    void Start()
    {
        obj = new System.Object();
        if (protocol == Protocol.TCP) {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            th = new Thread(RecieveCoordinatesTCP);
        }
        else {
            _endPoint = new IPEndPoint(IPAddress.Any, 0);
            _udpClient = new UdpClient(port);
            th = new Thread(ReceiveCoordinatesUDP);
        }

        th.IsBackground = true;
        th.Start();
    }


    private void OnDestroy() {
        _server?.Stop();
        _udpClient?.Close();
        running = false;
        th.Abort();
    }

    private void RecieveCoordinatesTCP() {
        while (running) {
            try
            {
                using TcpClient client = _server.AcceptTcpClient();
                Debug.Log("Connected!");

                while (client.Connected) {
                    NetworkStream stream = client.GetStream();
                    byte[] received = new byte[17];
                    stream.Read(received, 0, 17);
                    parseReceived(received);

                }
            }
            catch (SocketException e) {
                Debug.Log($"Thread probably interrupted. No worries, though! {e}");
            }
            finally {
                Debug.Log("Disconnected!");
            }
        }
    }

    private void ReceiveCoordinatesUDP() {
        while (running) {
            try
            {
                byte[] received = _udpClient.Receive(ref _endPoint);
                parseReceived(received);
            }
            catch (SocketException e) {
                Debug.Log($"Thread probably interrupted. No worries, though! {e}");
            }
        }
    }

    // Get the 17 Bytes if data and parse them into the coordinates and the information about the user touch
    private void parseReceived(byte[] received) {
        // Instantiate byte arrays for x, y and action (i.e pressing or not pressing)
        byte[] xData = new byte[8];
        byte[] yData = new byte[8];
        byte[] actionData = new byte[1];

        // Seperate data
        Array.Copy(received, 0, xData, 0, 8);
        Array.Copy(received, 8, yData, 0, 8);
        actionData[0] = received[16];

        // Lock object and parse data
        lock (obj) {
            x = Double.Parse(Encoding.UTF8.GetString(xData));
            y = - Double.Parse(Encoding.UTF8.GetString(yData));

            string data = Encoding.UTF8.GetString(actionData);
            if (data == "D" || data == "M")
                touching = true;
            else
                touching = false;
        }
    }
}
