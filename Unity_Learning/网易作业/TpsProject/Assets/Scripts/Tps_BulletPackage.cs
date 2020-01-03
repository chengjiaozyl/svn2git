using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_BulletPackage : MonoBehaviour
{
    public AudioClip BPGrab;
    public int BulletAddAmount = 30;

    private GameObject player;
    private Tps_ActionController controller;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.player);
        controller = player.GetComponent<Tps_ActionController>();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            AudioSource.PlayClipAtPoint(BPGrab, transform.position);
            controller.AddBullet(BulletAddAmount);
            Destroy(this.gameObject);

        }
    }
}
