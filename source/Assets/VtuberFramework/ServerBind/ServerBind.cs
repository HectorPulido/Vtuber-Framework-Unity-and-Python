using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CielaSpike;
using UnityEngine;
using Newtonsoft.Json;

[RequireComponent(typeof(VTuberMovement))]
public class ServerBind : MonoBehaviour
{
    public VTuberMovement vtuberMovement;
    public string ipAdress;
    public int port;
    bool connected = false;
    bool stayConnected = true;
    IPAddress ip;
    TcpClient client;
    NetworkStream networkStream;

    IEnumerator Start()
    {
        if (vtuberMovement == null)
        {
            vtuberMovement = GetComponent<VTuberMovement>();
        }

        while (!connected)
        {
            yield return new WaitForSeconds(2);
            this.StartCoroutineAsync(StartServer());
        }

    }

    IEnumerator StartServer()
    {
        yield return Ninja.JumpBack;

        //Start the connection
        ip = IPAddress.Parse(ipAdress);
        client = new TcpClient();
        client.Connect(ip, port);
        networkStream = client.GetStream();

        connected = true;

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

            try
            {
                int[][] resultArray = JsonConvert.DeserializeObject<int[][]>(data);
                vtuberMovement.ShowTrackingData(resultArray);
            }
            catch (System.Exception)
            {
                continue;
            }


            yield return Ninja.JumpBack;
        }
    }

}
