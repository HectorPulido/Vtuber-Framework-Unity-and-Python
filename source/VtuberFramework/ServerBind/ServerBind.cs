using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CielaSpike;
using UnityEngine;
using Newtonsoft.Json;

public struct ImageData
{
    public int[][] pose_data;
    public string emotion_data;
    public float[] pose_depth_data;
}

[RequireComponent(typeof(VTuberMovement))]
public class ServerBind : MonoBehaviour
{
    public VTuberMovement vtuberMovement;
    public FaceHandler faceHandler;
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
            print("Trying to connect...");
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
                ImageData resultArray = JsonConvert.DeserializeObject<ImageData>(data);
                vtuberMovement.ShowTrackingData(resultArray);
                faceHandler.ChangeEmotion(resultArray.emotion_data);
            }
            catch (System.Exception)
            {
                Debug.Log("Json Parse failed");
                continue;
            }

            yield return Ninja.JumpBack;
        }
    }

}
