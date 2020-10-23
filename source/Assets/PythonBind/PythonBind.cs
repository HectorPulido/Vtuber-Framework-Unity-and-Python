using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using CielaSpike;
using UnityEngine;
using Newtonsoft.Json;

public class PythonBind : MonoBehaviour
{
	public VTuberMovement vtuberMovement;
    public string ipAdress;
    public int port;

    void Start()
    {
        this.StartCoroutineAsync(StartServer());
    }

    public void SendData(string Text)
    {
        if (networkStream == null)
        {
            Debug.LogError("Client is not connected to the server");
            return;
        }

        byte[] buffer = Encoding.ASCII.GetBytes(Text);
        networkStream.Write(buffer, 0, buffer.Length);
    }

    bool stayConnected = true;
    IPAddress ip;
    TcpClient client;
    NetworkStream networkStream;
    IEnumerator StartServer()
    {
        yield return Ninja.JumpBack;

        //Start the connection
        ip = IPAddress.Parse(ipAdress);
        client = new TcpClient();
        client.Connect(ip, port);
        networkStream = client.GetStream();

        //Connection stabished, lets get some data
        yield return Ninja.JumpToUnity;
        this.StartCoroutineAsync(ReceiveData(client));
        yield return Ninja.JumpBack;

        //We must stay connected
        while (stayConnected)
        {
            yield return null;
        }

        //The connection will broke
        client.Client.Shutdown(SocketShutdown.Send);
        networkStream.Close();
        client.Close();

        yield return Ninja.JumpToUnity;
    }

    IEnumerator ReceiveData(TcpClient client)
    {
        NetworkStream ns = client.GetStream();
        byte[] receivedBytes = new byte[1024];
        int byte_count;
        while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
        {
            yield return Ninja.JumpToUnity;
            var data = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
            int[][] resultArray = JsonConvert.DeserializeObject<int[][]>(data);
            vtuberMovement.ShowTrackingData(resultArray);
            yield return Ninja.JumpBack;
        }
    }

}
