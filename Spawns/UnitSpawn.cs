using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawn : NetworkBehaviour
{
    public GameObject enemyBasicPrefab;
    public float spawnTimer = 0;
    public float spawn = 15;
    public Transform firePoint;
    public float fireForce = 10f;
    public GameObject basicEnemyPrefab;
    public GameObject enemyCptPrefab;
    UM uM;
    void Start()
    {
        uM = UM.Instance;
        if (this.gameObject.tag == "EnemyCptSpawn")
        {
            spawn = 100;
        }
        else
        {
            spawn = 8;
        }
    }
    
void spawnUnit(GameObject unitPrefab) {
    NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(unitPrefab, firePoint.position, transform.rotation);

        unitToSpawn.GetComponent<EnemyBasic>().prefab = unitPrefab;
        if(!unitToSpawn.IsSpawned) unitToSpawn.Spawn(true);
}

    void Update()
    {
        if (UM.Instance.playerArray.Length == 0) {
            return;
        }
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawn)
        {
            if (this.gameObject.tag == "EnemyCptSpawn")
            {
                uM.spawnEnemyCptServerRpc(enemyCptPrefab, transform);
                // if (minion != null)
                // {
                //     minion.transform.position = 
                //     minion.SetActive(true);
                // }
            }
            else
            {
                uM.spawnEnemyBasicServerRpc(basicEnemyPrefab, transform);
                // if (minion != null)
                // {
                //     minion.transform.position = firePoint.position;
                //     minion.SetActive(true);
                // }
            }

            spawnTimer = 0;
            if (spawn > 4)
                if (this.gameObject.tag == "EnemyCptSpawn")
                    spawn -= UM.Instance.allies / 4;
                else
                    spawn -= UM.Instance.allies / 100;
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
