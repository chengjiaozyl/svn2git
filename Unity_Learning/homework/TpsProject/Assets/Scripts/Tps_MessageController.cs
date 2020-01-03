using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//单例，用于作为主线程和网络线程的消息中心，游戏逻辑中需要发送消息时，将消息发送至该类单例，该类负责包装成消息格式放置到待发送队列
public class Tps_MessageController : MonoBehaviour
{
    public int bulletCount;
    public int chargerBulletCount;
    public string weaponInfo;
    public float hp;

    public GameObject wayPoint;
    public GameObject enermy;

    private Tps_ActionController actionController;
    private Tps_PlayerHealth playerHealth;

    private bool isLogin=false;
    private bool times = false;
    private int activeTimes = 0;

    private Tps_PlayerControl playerControl;
    private Tps_TpCamera cam;
    private NetworkManager networkManager;

    private Tps_EnemyCreate enemyCreate;
    private Tps_EnemyAI enemyAI;
    private Tps_EnemyAnimation enemyAnimation;
    private Tps_EnemyShoot enemyShoot;
    private Tps_EnemySight enemySight;

    private Transform WayPoint_01;

    private Stack<Dictionary<string, string>> RcvInfoList;

    private Stack<Dictionary<string,string>> SndInfoList;


    private static Tps_MessageController instance;
    private Tps_MessageController() { }
    public static Tps_MessageController Instance
    {
        get
        {
            if (instance == null)
                instance = new Tps_MessageController();
            return instance;
        }
    }

    private void Start()
    {
        actionController = this.GetComponent<Tps_ActionController>();
        playerControl = this.GetComponent<Tps_PlayerControl>();
        cam = this.GetComponentInChildren<Tps_TpCamera>();
        playerHealth = this.GetComponent<Tps_PlayerHealth>();
        networkManager = GameObject.FindGameObjectWithTag(Tags.networkManager).GetComponent<NetworkManager>();

        enemyCreate = GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyCreate>();
        enemyAI = GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyAI>();
        enemyAnimation = GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyAnimation>();
        enemyShoot = GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyShoot>();
        enemySight = GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemySight>();

        WayPoint_01 = GameObject.FindGameObjectWithTag(Tags.wayPoint).GetComponent<Transform>();
    }

    private void OnGUI()
    {
        
        if (!isLogin&&!times)
        {
            print("1");
            cam.enabled = false;
            actionController.enabled = false;
            playerControl.enabled = false;

            enemyAI.enabled = false;
            enemyAnimation.enabled = false;
            enemyShoot.enabled = false;
            enemySight.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            times = true;
        }
        else if(isLogin)
        {
            //print("2");
            GameObject.FindGameObjectWithTag(Tags.player).GetComponent<Tps_PlayerControl>().enabled = true;
            GameObject.FindGameObjectWithTag(Tags.player).GetComponent<Tps_ActionController>().enabled = true;
            GameObject.FindGameObjectWithTag(Tags.mainCamera).GetComponent<Tps_TpCamera>().enabled = true;

            GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyAI>().enabled = true;
            GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyAnimation>().enabled = true;
            GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyShoot>().enabled = true;
            GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemySight>().enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (activeTimes==0)
            {
                activeTimes++;
                GameObject.FindGameObjectWithTag(Tags.loginCanvas).SetActive(false);
            }

            ParseRcvData();
        }
        /*
        GUI.BeginGroup(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 100, 600, 600));
        GUI.Box(new Rect(0, 0, 250, 200), editTitle);

        GUI.Label(new Rect(30, 160, 150, 30), editTips);
        if (GUI.Button(new Rect(20, 120, 60, 35), "Login"))
        {
            if (editUsername == "Jeter" && editPassword == "123")
            {
                editTips = "Login success";
                Destroy(this);
            }
            else
            {
                editTips = "login failuer";
            }
        }

        GUI.Label(new Rect(20, 40, 100, 50), "账号:");
        GUI.Label(new Rect(20, 80, 100, 50), "密码:");

        editUsername = GUI.TextField(new Rect(60, 40, 150, 30), editUsername, 15);
        editPassword = GUI.PasswordField(new Rect(60, 80, 150, 30), editPassword, "*"[0], 15);

        GUI.EndGroup();
        */
    }

    //
    private void ParseRcvData()
    {
        while (RcvInfoList.Count > 0)
        {
            Dictionary<string, string> data = RcvInfoList.Pop();
            string code = data["code"];
            switch (code)
            {
                case "msg":
                    //CloseConnection();
                    //msg表示异常消息，发生了错误，需要关闭连接，将关闭连接消息添加到发送消息队列
                    Dictionary<string, string> infoDict = new Dictionary<string,string>(){ { "code", "close_connection" } };
                    SndInfoList.Push(infoDict);
                    break;
                case "login_successful":
                    //SendInitDataRequest();
                    //登陆成功，根据服务器传递的初始消息对游戏对象进行初始化
                    SetupPlayerInitData(data);
                    break;
                case "game_player_data":
                    /*
                     * 正常情况下服务器在登陆后不会发送存储的玩家信息给客户端，但是如果客户端掉线或者断网，不需要重新登陆，
                     * 但是需要重新发送玩家信息给客户端用于初始化
                     * 考虑断线重连需要重新登陆的情况，可以取消这种状态                    
                     * 
                    */
                    SetupPlayerInitData(data);
                    break;
                case "game_enermy_data":
                    //根据服务器传来的信息创建敌人
                    CreateEnermy(data);
                    break;
                case "game_other_player_data":
                    //多人在线时，其它玩家的信息
                    break;
            }
        }  
    }
    
    private void SetupPlayerInitData(Dictionary<string, string> DataDict)
    {
        print("SetupPlayerInitData");
        actionController.chargerBulletCount = int.Parse(DataDict["chargerBulletCount"]);
        print("2");
        actionController.SetBullet(int.Parse(DataDict["bulletCount"]), actionController.chargerBulletCount);
        print("3");
        actionController.weaponInfo = DataDict["weaponInfo"];
        print("4");
        actionController.SetWeaponInfo(actionController.weaponInfo);
        print("5");
        playerHealth.hp = float.Parse(DataDict["playerHealth"]);
        if (playerHealth.hp <= 0f)
            playerHealth.hp = 100f;
        print("6");
        CreateEnermy(DataDict);
        print("7");
        isLogin = true;
        print("end set up");

    }
    
    private void CreateEnermy(Dictionary<string, string> DataDict)
    {
        print("_1");
        print(DataDict["wayPoint1X"]+"....."+ DataDict["wayPoint2X"]+"........" + DataDict["wayPoint3X"]);
        Vector3 point1 = new Vector3(float.Parse(DataDict["wayPoint1X"]), 1, float.Parse(DataDict["wayPoint1Z"]));
        Vector3 point2 = new Vector3(float.Parse(DataDict["wayPoint2X"]), 1, float.Parse(DataDict["wayPoint2Z"]));
        Vector3 point3 = new Vector3(float.Parse(DataDict["wayPoint3X"]), 1, float.Parse(DataDict["wayPoint3Z"]));
        print("_2");
        //print(WayPoint_01.position.x+"..."+WayPoint_01.position.y+"...."+WayPoint_01.position.z);
        //WayPoint_01.position = point1;
        print("_3");
        //GameObject.Find("WayPoint_02").transform.position = point1;
        enemyCreate.SetEnemyCreatePosition(point1, Quaternion.identity);
        print("_4");
        //enemy.position = point1;
        
        
        //Instantiate(wayPoint, point1, Quaternion.identity);
        //Instantiate(wayPoint, point2, Quaternion.identity);
        //Instantiate(wayPoint, point3, Quaternion.identity);
        
        //Instantiate(enermy, point1, Quaternion.identity);
    }

    public void AddRcvInfoIntoQueue(Dictionary<string, string> info) => RcvInfoList.Push(info);

    public void AddSndInfoIntoQueue(Dictionary<string, string> info) => SndInfoList.Push(info);

    public Dictionary<string,string> GetSndInfo()
    {
        return SndInfoList.Pop();
    }

    public int SndInfoListCount
    {
        get { return SndInfoList.Count; }
    }

}
