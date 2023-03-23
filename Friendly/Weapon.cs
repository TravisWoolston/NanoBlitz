using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Networking;

public class Weapon : NetworkBehaviour
{
    public GameObject bulletPrefab;
    [SerializeField] private GameObject missilePrefab;
    public Transform firePoint;
    public float fireForce = 120f;
    public PlayerController parent;
    public GameObject fireTarget;
    public bool hasMissiles = true;

    [SerializeField]
    private List<GameObject> spawnedMissiles = new List<GameObject>();

    // SinglePooledDynamicSpawner missileNetSpawn;
    // public NetworkObjectPool netPool;
   

    // public GameObject sparkPrefab;
    public override void OnNetworkSpawn() {
        parent = transform.parent.gameObject.GetComponent<PlayerController>();
    }
    void Start()
    {
  
         
         parent = transform.parent.gameObject.GetComponent<PlayerController>();
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
        if (!IsOwner)
            return;
        GameObject missile = ObjectPool.SharedInstance.GetMissile();
        if (missile != null)
        {
            missile.transform.position = firePoint.position;
            missile.transform.rotation = transform.rotation;

            missile.GetComponent<Missile>().SetPlayerTarget(fireTarget);
            missile.SetActive(true);

            missile.GetComponent<Rigidbody2D>().velocity += (Vector2)(firePoint.up * 100);
        }
    }
    void FixedUpdate(){
        if(!hasMissiles) return;
        if(parent.fireTarget != null)
        fireTarget = parent.fireTarget;
        else
        fireTarget = UM.Instance.GetClosestGameObject(UM.Instance.enemies, transform.position);
    }
[ServerRpc(RequireOwnership=false)]
    public void FireMissileServerRpc()
    {
        // if (!IsOwner)
        //     return;
              if(!NetworkManager.Singleton.IsServer) return;
        NetworkObject netMissile = UM.Instance.spawnMissileServerRpc(missilePrefab, transform);
      
       Missile netMissileC = netMissile.GetComponent<Missile>();
       netMissileC.SetPlayerTarget(fireTarget);
        netMissileC.prefab = missilePrefab;
        if(!netMissile.IsSpawned) netMissile.Spawn(true);
         netMissile.GetComponent<Rigidbody2D>().AddForce(firePoint.up * 6000);
    }
[ServerRpc]
public void DestroyServerRpc() {
    GameObject toDestroy = spawnedMissiles[0];
    toDestroy.GetComponent<NetworkObject>().Despawn();
    spawnedMissiles.Remove(toDestroy);
    // Destroy(toDestroy);
    
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
