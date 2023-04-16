using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private float expiration = 0;
    public GameObject prefab;
    private UM uM;
    // public bool boosted = false;
    public Rigidbody2D rb;
    private Transform rbTransform;
    public int teamID = -1;
// void OnNetworkSpawn(){
// rbTransform = rb.transform;
// }
    void Start()
    {
        rbTransform = rb.transform;
        uM = UM.Instance;
    }

    // void OnEnable()
    // {
    //     // boosted = false;
    // }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // if (this.gameObject.tag == "Bullet")
        // {
        //     if (col.gameObject.tag == "EnemyBasic")
        //         despawnClientRpc();
        // NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        //     // if(col.gameObject.tag == "Clone"){
        //     //         gameObject.SetActive(false);

        //     // }
        // }
        // if (this.gameObject.tag == "EnemyBullet")
        // {
        //     if (col.gameObject.tag == "Clone")
        //         despawnClientRpc();
        // NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        // }
        

    if (NetworkObject.IsSpawned && gameObject.tag != col.gameObject.tag){
        // Debug.Log(rbTransform + " " + Quaternion.LookRotation(-rb.velocity.normalized));
        uM.spawnSparkServerRpc(rbTransform.position, Quaternion.LookRotation(-rb.velocity.normalized));
despawnServerRpc();
    }
        
        // despawnClientRpc();
    }

    [ClientRpc]
    public void despawnClientRpc()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void despawnServerRpc()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, prefab);
        despawnClientRpc();
    }

    // void Update(){
    //     transform.Translate(rbTransform.up * Time.deltaTime * 50);
    // }
    void FixedUpdate()
    {
        // if (!boosted)
        // {
        //     // rb.AddForce(rb.transform.up * 2000);
        //     boosted = true;
        // }

        if (rbTransform.position.z != -1)
        {
            rbTransform.position = new Vector3(rbTransform.position.x, rbTransform.position.y, -1);
        }
        expiration += Time.deltaTime;
        if (expiration > 5)
        {
            expiration = 0;

            //  despawnClientRpc();

            despawnServerRpc();
        }
    }
}
