using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireForce = 120f;
    // SinglePooledDynamicSpawner missileNetSpawn;
    // public NetworkObjectPool netPool;
    // public GameObject missilePrefab;

    // public GameObject sparkPrefab;
    void Start()
    {
        fireForce = 80f;
    }

    public void Fire()
    {
        GameObject bullet = ObjectPool.SharedInstance.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;

            bullet.SetActive(true);
            bullet
                .GetComponent<Rigidbody2D>()
                .AddForce(firePoint.up * fireForce, ForceMode2D.Impulse);
        }
    }
    public void FireMissile(GameObject fireTarget)
    {
        GameObject missile = ObjectPool.SharedInstance.GetMissile();
        if (missile != null)
        {
            missile.transform.position = firePoint.position;
            missile.transform.rotation = transform.rotation;

            
            // missile.GetComponent<NetworkObject>().Spawn();
            missile.GetComponent<Missile>().SetPlayerTarget(fireTarget);
            missile.SetActive(true);
            
            // missile.GetComponent<NetworkObject>().Spawn();
            missile.GetComponent<Rigidbody2D>().velocity += (Vector2)(firePoint.up * 100);
            //  missile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 1000);
        }
    }
    // public void FireMissile(PlayerController playerC)
    // {
    //     // GameObject missile = ObjectPool.SharedInstance.GetMissile();
    //     // missile.SetActive(true);
       
    //     NetworkObject netMissile = missileNetSpawn.Instantiate(
    //         NetworkManager.Singleton.LocalClientId,
    //         firePoint.position,
    //         transform.rotation
    //     );
    //     // netMissile.GetComponent<NetworkObject>().Spawn(null, true);

    //     if (netMissile != null)
    //     {
    //         // netMissile.transform.position = firePoint.position;
    //         // netMissile.transform.rotation = transform.rotation;

    //         netMissile.GetComponent<Missile>().SetPlayerTarget(playerC);
    //         // missile.GetComponent<NetworkObject>().Spawn();

    //         // missile.GetComponent<NetworkObject>().Spawn();
    //         netMissile.GetComponent<Rigidbody2D>().velocity += (Vector2)(firePoint.up * 100);
    //         //  missile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 1000);
    //     }
    // }

    public void FireEM()
    {
        GameObject missile = ObjectPool.SharedInstance.GetEM();
        if (missile != null)
        {
            missile.transform.position = firePoint.position;
            missile.transform.rotation = transform.rotation;

            // missile.GetComponent<Missile>().SetTarget(fireTarget);

            missile.SetActive(true);
            missile.GetComponent<Rigidbody2D>().velocity += (Vector2)(firePoint.up * 100);
            //  missile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 1000);
        }
    }
}
