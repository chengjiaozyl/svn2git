using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_Weapon_g36 : Tps_Weapon
{
    public AudioClip WeaponGrab;

    private GameObject player;
    private Tps_ActionController controller;
    private Tps_PlayerInventory playerInventory;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.player);
        controller = player.GetComponent<Tps_ActionController>();
        playerInventory = player.GetComponent<Tps_PlayerInventory>();
        WeaponName = "Tps_Weapon_g36";

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            AudioSource.PlayClipAtPoint(WeaponGrab, transform.position);
            if (!playerInventory.HasWeapon(this))
            {
                controller.AddWeaponType(this);
                this.transform.parent = GameObject.FindGameObjectWithTag(Tags.weaponContainer).transform;
                this.transform.position = GameObject.FindGameObjectWithTag(Tags.weaponContainer).transform.position;
                this.gameObject.SetActive(false);
            }


        }
    }
}
