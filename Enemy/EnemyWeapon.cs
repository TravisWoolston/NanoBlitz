using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;
public class EnemyWeapon : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireForce = 120f;
    public EnemyBasic parent;
public GameObject fireTarget;
public GameObject missilePrefab;


    // public GameObject sparkPrefab;
    void Start()
    {
        fireForce = 120f;
    }

    public void Fire()
    {
        GameObject bullet = ObjectPool.SharedInstance.GetEnemyBullet();
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            // bullet.transform.rotation = firePoint.rotation;

            bullet.SetActive(true);
            bullet
                .GetComponent<Rigidbody2D>()
                .AddForce(firePoint.up * fireForce, ForceMode2D.Impulse);
        }
        // GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);


        // Object.Destroy(this.gameObject, 1);
    }

    [ServerRpc(RequireOwnership=false)]
    public void FireMissileServerRpc()
    {
        
        // if (!IsOwner)
        //     return;
    //           if(!NetworkManager.Singleton.IsServer) return;
    // UM.Instance.spawnMissileServerRpc(transform.position, transform.rotation);
      
    //    Missile netMissileC = netMissile.GetComponent<Missile>();
    //    netMissileC.SetPlayerTarget(fireTarget);
    //     netMissileC.prefab = missilePrefab;
    //     if(!netMissile.IsSpawned) netMissile.Spawn(true);
    //      netMissile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 6000);
    }
}
