using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_BloodPackage : MonoBehaviour
{
    public AudioClip BPGrab;
    public float BPAmount = 40.0f;

    private GameObject player;
    private Tps_PlayerInventory playerInventory;
    private Tps_PlayerHealth playerHealth;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.player);
        playerInventory = player.GetComponent<Tps_PlayerInventory>();
        playerHealth = player.GetComponent<Tps_PlayerHealth>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            AudioSource.PlayClipAtPoint(BPGrab, transform.position);
            float currentHp = playerHealth.hp + BPAmount;
            if (currentHp > 100)
                currentHp = 100;
            playerHealth.hp = currentHp;
            Destroy(this.gameObject);

        }
    }
}
