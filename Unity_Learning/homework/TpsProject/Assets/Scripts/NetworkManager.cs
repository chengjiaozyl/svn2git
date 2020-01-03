using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//这个类用于管理从底层传递过来的数据的封装，即传递来的数据统一用底层的方式实现封装
public class NetworkManager : MonoBehaviour
{
    public InputField ScoreInputField;
    public InputField HealthInputField;

    private NetworkCore networkCore;


    void Start()
    {
        networkCore = GetComponent<NetworkCore>();

    }

    void Update()
    {

    }



    public void OnLoginButton()
    {
        Debug.Log("Login...........................");
        InputField passwordInputField = GameObject.Find("PasswordInputField").GetComponent<InputField>();
        InputField usernameInputField = GameObject.Find("UsernameInputField").GetComponent<InputField>();
        print(usernameInputField.text);
        print(passwordInputField.text);
        networkCore.Login(usernameInputField.text, passwordInputField.text);
    }
    /*
    public void OnSendButton()
    {
        int score = int.Parse(ScoreInputField.text);
        int health = int.Parse(HealthInputField.text);
        networkCore.SendGameData(score, health);
    }

    public void OnQuitButton()
    {
        int score = int.Parse(ScoreInputField.text);
        int health = int.Parse(HealthInputField.text);
        networkCore.SendGameData(score, health);
        Application.Quit();
    }
    */

    public void SyncGameData(int bulletCount, int chargerBulletCount, float playerHealth, string weaponInfo)
    {
        networkCore.SendGameData(bulletCount.ToString(), chargerBulletCount.ToString(), 
            playerHealth.ToString(), weaponInfo);
    }
}
