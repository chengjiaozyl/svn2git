using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_EnemyCreate : MonoBehaviour
{
    private Transform enemy;

    private void Start()
    {
        enemy = this.GetComponent<Transform>();
    }

    public void SetEnemyCreatePosition(Vector3 position, Quaternion rotation)
    {
        print("enter enemy Create");
        print(enemy.position.x + "..." + enemy.position.y + "...." + enemy.position.z);
        enemy.position=position;
        print("1_");
        enemy.rotation = rotation;
    }
}
