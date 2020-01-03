using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tps_EnemySight : MonoBehaviour
{
    public float fieldOfViwAngle = 110f;
    public bool playerInSight;
    public Vector3 playerPosition;
    //默认位置，当玩家位置和默认位置相同，则表示未发现玩家，不同则表示发现了玩家
    public Vector3 resetPosition = Vector3.zero;

    private NavMeshAgent nav;
    private SphereCollider col;
    private Animator anim;
    private GameObject player;
    private Tps_PlayerHealth playerHealth;
    private HashIDs hash;
    private Tps_PlayerControl playerControl;

    private void Start()
    {
        nav = this.GetComponent<NavMeshAgent>();
        col = GetComponentInChildren<SphereCollider>();
        anim = this.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag(Tags.player);
        playerHealth = player.GetComponent<Tps_PlayerHealth>();
        playerControl = player.GetComponent<Tps_PlayerControl>();
        Tps_ActionController.PlayerShootEvent += ListenPlayer;
        hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInSight = false;
            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            if (angle < fieldOfViwAngle * 0.5f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
                {
                    if (hit.collider.gameObject == player)
                    {
                        playerInSight = true;
                        playerPosition = player.transform.position;
                    }
                }
            }
            if (playerControl.State == PlayerState.Walk || playerControl.State == PlayerState.Run)
            {
                ListenPlayer();
            }
        }
    }

    private void Update()
    {
        if (playerHealth.hp > 0)
        {

            anim.SetBool(hash.playerInSightBool, playerInSight);
        
        }
        else
            anim.SetBool(hash.playerInSightBool, false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInSight = false;
        }
    }

    private void ListenPlayer()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= col.radius)
            playerPosition = player.transform.position;
    }

    private void OnDestroy()
    {
        Tps_ActionController.PlayerShootEvent -= ListenPlayer;
    }
}
