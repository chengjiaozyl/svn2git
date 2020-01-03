using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tps_EnemyAI : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float chaseWaitTime = 5f;
    public float patrolWaitTime = 1f;
    public Transform[] patrolWayPoint;

    private Tps_EnemySight enemySight;
    private NavMeshAgent nav;
    private Transform player;
    private Tps_PlayerHealth playerHealth;
    private float chaseTimer;
    private float patrolTimer;
    private int wayPointIndex;

    private void Start()
    {
        enemySight = this.GetComponent<Tps_EnemySight>();
        nav = this.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag(Tags.player).transform;
        playerHealth = player.GetComponent<Tps_PlayerHealth>();

    }

    private void Update()
    {
        if (enemySight.playerInSight && playerHealth.hp > 0f)
        {
            Shooting();
        }
        else if (enemySight.playerPosition != enemySight.resetPosition && playerHealth.hp > 0f)
            Chasing();
        else
            Patrolling();
    }

    private void Shooting()
    {
        nav.SetDestination(transform.position);
    }

    private void Chasing()
    {
        Vector3 sightDeltaPos = enemySight.playerPosition - transform.position;

        if (sightDeltaPos.sqrMagnitude > 4f)
            nav.destination = enemySight.playerPosition;

        nav.speed = chaseSpeed;

        if (nav.remainingDistance < nav.stoppingDistance)
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseWaitTime)
            {
                enemySight.playerPosition = enemySight.resetPosition;
                chaseTimer = 0f;
            }

        }
        else
            chaseTimer = 0;
    }

    private void Patrolling()
    {
        nav.speed = patrolSpeed;

        if (nav.destination == enemySight.resetPosition || nav.remainingDistance < nav.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                if (wayPointIndex == patrolWayPoint.Length - 1)
                    wayPointIndex = 0;
                else
                    wayPointIndex++;
                patrolTimer = 0;
            }
        }
        else
            patrolTimer = 0;
        nav.destination = patrolWayPoint[wayPointIndex].position;
    }
}
