using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_TpInput : MonoBehaviour
{
    public bool LockCusor
    {
        get { return Cursor.lockState == CursorLockMode.Locked ? true : false; }
        set
        {
            Cursor.visible = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private Tps_PlayerParameter parameter;
    private Tps_Input input;

    private void Start()
    {
        LockCusor = true;
        parameter = this.GetComponent<Tps_PlayerParameter>();
        input = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<Tps_Input>();


    }
    private void Update()
    {
        InitialInput();
    }
    private void InitialInput()
    {
        parameter.inputMoveVector = new Vector2(input.GetAxis("Horizontal"),input.GetAxis("Vertical"));
        parameter.inputSmoothLook = new Vector2(input.GetAxisRaw("Mouse X"), input.GetAxisRaw("Mouse Y"));
        parameter.inputCrouch = input.GetButton("Crouch");
        parameter.inputJump = input.GetButton("Jump");
        parameter.inputSprint = input.GetButton("Sprint");
        parameter.inputFire = input.GetButton("Fire");
        parameter.inputReload = input.GetButton("Reload");
        parameter.inputFirstWeapon = input.GetButton("FirstWeapon");
        parameter.inputSecondWeapon = input.GetButton("SecondWeapon");
        parameter.inputThirdWeapon = input.GetButton("ThirdWeapon");

    }
}
