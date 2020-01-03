using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PasswordInput : MonoBehaviour
{
    private void Start()
    {

        transform.GetComponent<InputField>().onValueChanged.AddListener(ChangeValue);
        transform.GetComponent<InputField>().onEndEdit.AddListener(EndValue);
    }

    public void ChangeValue(string input)
    {
        //Debug.Log("正在输入：" + input);
    }
    public void EndValue(string input)
    {
        //Debug.Log("输入内容：" + input);
    }
}
