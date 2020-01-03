using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MyJsonUtil;
using System;

public class NetworkCore : MonoBehaviour
{
    public string serverAddress = "127.0.0.1";
    public int serverPort = 5000;
    //public string username = "vivid";
    //public string password = "888888";

    
    private TcpClient _client;
    private NetworkStream _stream;
    private Thread _thread;
    private byte[] _buffer = new byte[1024]; //用于接收消息的缓存
    private string receiveMsg = "";
    private bool isConnected = false;

    private Tps_MessageController messageController;

    void Start()
    {
        messageController = Tps_MessageController.Instance;
    }


    private void Update()
    {
        
    }

    public void Login(string username, string password)
    {
        
        SetupConnection();
        Dictionary<string, string> dict = new Dictionary<string, string>()
        {
            { "code","login"},
            { "username",username},
            { "password",password}
        };
        SendData(Encode(dict));
        
    }


    //用于传递游戏数据给服务器用于同步状态信息的方法
    public void SendGameData(string bulletCount, string chargerBulletCount, string playerHealth, string weaponInfo)
    {
        if (!isConnected)
            return;
        Dictionary<string, string> dict = new Dictionary<string, string>()
        {
            { "code","gds"},
            { "bulletCount",bulletCount},
            { "chargerBulletCount",chargerBulletCount},
            { "playerHealth",playerHealth},
            { "weaponInfo",weaponInfo}
        };
        SendData(Encode(dict));
    }

    //用于发送敌人生命值
    public void SendEnermyHealth(string health)
    {
        if (!isConnected)
            return;
        Dictionary<string, string> dict = new Dictionary<string, string>()
        {
            { "code","ged"},
            { "health",health}
        };
        SendData(Encode(dict));
    }


    private void GetSndInfoFromMessageController()
    { 
        while(messageController.SndInfoListCount>0)
        {
            Dictionary<string,string> info = messageController.GetSndInfo();
            /*
            string code = info["code"];
            switch(code)
            {
                case "login":
                    Login(info["username"], info["password"]);
                    break;
                case "gds":
                    SendGameData(info["bulletCount"], info["chargerBulletCount"], info["playerHealth"], info["weaponInfo"]);
                    break;
                case "ged":
                    SendEnermyHealth(info["enermyHealth"]);
                    break;
                case "close_connection":
                    CloseConnection();
                    break;
            }
            */
            SendData(Encode(info));
        
        }
    }

    private void SetupConnection()
    {
        try
        {
            _thread = new Thread(ReceiveData);
            _thread.IsBackground = true;
            _client = new TcpClient(serverAddress, serverPort);
            _stream = _client.GetStream();
            _thread.Start();
            isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            CloseConnection();
        }
    }

    //在接收到服务器发来的消息之后，Json接口解析后，把消息放到消息队列中，不作其它处理
    private void ReceiveData()
    {
        if (!isConnected)
            return;
        int numberOfBytesRead = 0;
        while (isConnected && _stream.CanRead)
        {
            try
            {
                numberOfBytesRead = _stream.Read(_buffer, 0, _buffer.Length);
                receiveMsg = Encoding.ASCII.GetString(_buffer, 0, numberOfBytesRead);
                _stream.Flush();
                Dictionary<string, string> rcvMsgDict = Decode(receiveMsg);
                print("收到消息为:"+rcvMsgDict["code"]);
                messageController.AddRcvInfoIntoQueue(rcvMsgDict);
                //ParseData(rcvMsgDict);
                receiveMsg = "";
            }
            catch (Exception e)
            {
                CloseConnection();
                Debug.Log(e.Message);
            }
        }
    }
    /*
    private void ParseData(Dictionary<string, string> rcvMsgDict)
    {
        string code = rcvMsgDict["code"];
        switch (code)
        {
            case "msg":
                CloseConnection();
                break;
            case "login_successful":
                SendInitDataRequest();
                break;
            case "game_player_data":
                print("set up playerinit data");
                //messageController.SetupPlayerInitData(rcvMsgDict);
                break;
            case "game_enermy_data":
                //messageController.CreateEnermy(rcvMsgDict);
                break;
        }
    }
    */
    private void SendInitDataRequest()
    {
        print("send init data request");
        Dictionary<string, string> dict = new Dictionary<string, string>()
        {
            { "code","gdr"},
            { "bulletCount",""},
            { "chargerBulletCount",""},
            { "playerHealth",""},
            { "weaponInfo",""},
            { "username",""}
        };
        SendData(Encode(dict));
    }

    private void SendData(string msgToSend)
    {
        byte[] bytesToSend = Encoding.ASCII.GetBytes(msgToSend);
        if (_stream.CanWrite)
        {
            _stream.Write(bytesToSend, 0, bytesToSend.Length);
        }
    }

    private void CloseConnection()
    {
        if (isConnected)
        {
            _thread.Interrupt();
            _stream.Close();
            _client.Close();
            isConnected = false;
            receiveMsg = "";
        }
    }

    //编码成json格式，用"\r\n"作为分隔符
    string Encode(Dictionary<string, string> dict)
    {
        string json = Json.Encode(dict);
        string header = "\r\n" + json.Length.ToString() + "\r\n";
        string result = header + json;
        return result;
    }

    //解码json格式数据
    Dictionary<string, string> Decode(string raw)
    {
        string payload_str = "";
        string raw_leftover = raw;
        if (raw.Substring(0, 2).Equals("\r\n"))
        {
            int index = raw.IndexOf("\r\n", 2);
            int payload_length = int.Parse(raw.Substring(2, index - 2 + 1));
            if (raw.Length >= index + 2 + payload_length)
            {
                payload_str = raw.Substring(index + 2, payload_length);
                raw_leftover = raw.Substring(index + 2 + payload_length);
            }
        }
        return Json.Decode<Dictionary<string, string>>(payload_str);
    }
}
