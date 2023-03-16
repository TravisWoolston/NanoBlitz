using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawn : MonoBehaviour
{
    public GameObject enemyBasicPrefab;
    public float spawnTimer = 0;
    public float spawn = 15;
    public Transform firePoint;
    public float fireForce = 10f;

    void Start()
    {
        if (this.gameObject.tag == "EnemyCptSpawn")
        {
            spawn = 100;
        }
        else
        {
            spawn = 8;
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawn)
        {
            if (this.gameObject.tag == "EnemyCptSpawn")
            {
                GameObject minion = ObjectPool.SharedInstance.GetEnemyCpt();
                if (minion != null)
                {
                    minion.transform.position = firePoint.position;
                    minion.SetActive(true);
                }
            }
            else
            {
                GameObject minion = ObjectPool.SharedInstance.GetBasicEnemy();
                if (minion != null)
                {
                    minion.transform.position = firePoint.position;
                    minion.SetActive(true);
                }
            }

            spawnTimer = 0;
            if (spawn > 4)
                if (this.gameObject.tag == "EnemyCptSpawn")
                    spawn -= PlayerController.Instance.allies / 4;
                else
                    spawn -= PlayerController.Instance.allies / 100;
        }
        if (this.gameObject.tag == "EnemyCptSpawn")
        {
            if (spawn <= 30)
            {
                spawn = 30;
            }
        }
        else if (spawn <= 1)
        {
            spawn = 1;
        }
    }
}
