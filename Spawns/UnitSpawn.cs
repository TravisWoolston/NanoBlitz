using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawn : NetworkBehaviour
{
    public GameObject enemyBasicPrefab;
    public float spawnTimer = 0;
    public float spawn = 15;
    private bool spawned= false;

    public Transform firePoint;
    public float fireForce = 10f;
    public GameObject basicEnemyPrefab;
    public GameObject enemyCptPrefab;
    UM uM;
        //    public override void OnNetworkSpawn() {
        //     if (this.gameObject.tag == "EnemyCptSpawn")
        //     {
        //         uM.spawnEnemyCptServerRpc(enemyCptPrefab, transform);
        //     }
        //     else
        //     {
        //         uM.spawnEnemyBasicServerRpc(basicEnemyPrefab, transform);
        //     }
        //    }

    void Start()
    {
        uM = UM.Instance;
                            if (this.gameObject.tag == "EnemyCptSpawn")
        {
            spawn = 120;
        }
        else if (this.gameObject.tag == "EnemyHHSpawn"){
            spawn = 30;
        }
        else
        {
            spawn = 5;
        }
            if (this.gameObject.tag == "EnemyCptSpawn")
            {
                uM.spawnEnemyCptServerRpc(transform.position, transform.rotation);
            }
            else
            {
                uM.spawnEnemyBasicServerRpc(transform.position, transform.rotation);
            }

    }
    
void spawnUnit(GameObject unitPrefab) {
    NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(unitPrefab, firePoint.position, transform.rotation);

        unitToSpawn.GetComponent<EnemyBasic>().prefab = unitPrefab;
        if(!unitToSpawn.IsSpawned) unitToSpawn.Spawn(true);
}

    void FixedUpdate()
    {
        if(!NetworkManager.Singleton.IsServer) return;
        if (UM.Instance.playerArray.Length == 0) {
            return;
        }
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawn || !spawned)
        {
            spawned = true;
            if (this.gameObject.tag == "EnemyCptSpawn")
            {
                uM.spawnEnemyCptServerRpc(transform.position, transform.rotation);
            }
            else if(this.gameObject.tag == "EnemyHHSpawn"){
                spawn = 200 - GameObject.FindGameObjectsWithTag("Clone").Length;
                uM.spawnEnemyHHServerRpc(transform.position, transform.rotation);
            }
            else
            {
                uM.spawnEnemyBasicServerRpc(transform.position, transform.rotation);
                spawn = 5;
            }

            spawnTimer = 0;
                    
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
