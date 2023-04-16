using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class UM : NetworkBehaviour
{
    public float VGZ = 10;
    public static UM Instance;
    public static UM Singleton
    {
        get { return Instance; }
    }
    public float AICount;
    public VectorGrid VG;
    public float currentTask;
    private float timer = 0;

    public GameObject enemyBasicPrefab;
    public GameObject enemyCptPrefab;
    public GameObject clonePrefab;
    public GameObject missilePrefab;
    public GameObject EMPrefab;
    public GameObject bulletPrefab;
    public GameObject enemyBulletPrefab;
    public GameObject playerTrail;
    public GameObject trail;
    public GameObject enemyTrail;
    public GameObject enemyHHPrefab;
    public GameObject sparkPrefab;
    public GameObject explosionPrefab;
    public bool updateNeeded = false;
    private float delayTime = .7f;
    public float VGForce = .03f;
    public float VGRadius = .03f;
    public float xGrav = 0;
    public float yGrav = 0;
    public AudioClip[] explosions;
    private AudioClip exp;

    public int allies = 0;
    public GameObject[] allyArray;
    public Transform[] allyArrayTransform;
    public GameObject[] enemies;
    public GameObject[] captains;
    public GameObject player;
    public GameObject[] cloneArray;
    public GameObject[] playerArray;
    public GameObject[] combinedAllies;
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public Color[] colorArray;
    Camera[] playerCams;
    public string gameMode = "PVP";
    public float sparkTimer = 0;
    public List<NetworkObject> activeSparks = new List<NetworkObject>();
    void Awake()
    {
         {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

    }
    }

    void Start()
    {
        playerArray = GameObject.FindGameObjectsWithTag("Player");
        // colorArray = [color1, color2, color3, color4];
        AICount = 1;
        currentTask = 0;
        InvokeRepeating("UpdateGlobal", 0f, 1f);
        VGForce = .03f;
        VGRadius = .03f;
        Physics2D.gravity = new Vector2(xGrav, yGrav);
        cloneArray = GameObject.FindGameObjectsWithTag("Clone");
    }
    // [ServerRpc(RequireOwnership=false)]
    public void UpdateEnemyTeam(GameObject go){
        for(int i = 0; i < playerArray.Length; i++){
            playerArray[i].GetComponent<PlayerController>().enemyDic.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go);
    }
    }
    public void RemoveUpdateEnemyTeam(GameObject go){
        for(int i = 0; i < playerArray.Length; i++){
            playerArray[i].GetComponent<PlayerController>().enemyDic.Remove(go.GetComponent<NetworkObject>().NetworkObjectId, out go);
    }
    }
public void UpdateTeams(PlayerController playerC, GameObject go){
    for(int i = 0; i < playerArray.Length; i++){
        if(i != playerC.playerID.Value){
            playerArray[i].GetComponent<PlayerController>().enemyDic.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go);
        }
        else {
            playerArray[i].GetComponent<PlayerController>().allyDic.Add(go.GetComponent<NetworkObject>().NetworkObjectId, go);
        }
    }
}
public void RemoveUpdateTeams(PlayerController playerC, GameObject go){
    for(int i = 0; i < playerArray.Length; i++){
        if(i != playerC.playerID.Value){
            playerArray[i].GetComponent<PlayerController>().enemyDic.Remove(go.GetComponent<NetworkObject>().NetworkObjectId);
        }
        else {
            playerArray[i].GetComponent<PlayerController>().allyDic.Remove(go.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }
}
    public void Explosion(Transform splodePlace)
    {
        int indexE = UnityEngine.Random.Range(0, explosions.Length);
        exp = explosions[indexE];

        AudioSource.PlayClipAtPoint(exp, splodePlace.position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void moveServerRpc(int ID, Vector3 thrustPower)
    {
        Rigidbody2D rb = playerArray[ID].GetComponent<Rigidbody2D>();
        rb.AddForce(thrustPower);
    }

    public LineRenderer hpDrain;
 
    [ServerRpc(RequireOwnership = false)]
    public void UpgradeServerRpc(int ID, Vector3 start, Vector3 end)
    {
        
        PlayerController player = playerArray[ID].GetComponent<PlayerController>();
        // player.targetObject.GetComponent<PlayerCopy>().hp-=.002f;
        // player.targetObject
        //         .GetComponent<Rigidbody2D>()
        //         .AddForce((start - end) * 500);
        player.hpDrain.SetPosition(0, start);
        player.hpDrain.SetPosition(1, end);
        UpgradeClientRpc(ID, start, end);
    }

    [ClientRpc]
    public void UpgradeClientRpc(int ID, Vector3 start, Vector3 end)
    {
        PlayerController player = playerArray[ID].GetComponent<PlayerController>();
        // player.targetObject.GetComponent<PlayerCopy>().hp-=.002f;
        // player.targetObject
        //         .GetComponent<Rigidbody2D>()
        //         .AddForce((start - end) * 500);
        player.hpDrain.SetPosition(0, start);
        player.hpDrain.SetPosition(1, end);
    }

    [ServerRpc(RequireOwnership = false)]
    public void rotateServerRpc(int ID, Quaternion targetRotation)
    {
        Transform rbTransform = playerArray[ID].GetComponent<Rigidbody2D>().transform;
        rbTransform.rotation = Quaternion.RotateTowards(rbTransform.rotation, targetRotation, 3);
    }

    [ServerRpc(RequireOwnership = false)]
    public void scaleServerRpc(int ID, Vector3 targetScale)
    {
        Transform rbTransform = playerArray[ID].GetComponent<Rigidbody2D>().transform;
        rbTransform.localScale = targetScale;
    }

    public GameObject GetClosestPlayerGameObject(Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in playerArray)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                closestGameObject = gameObject;
            }
        }
        return closestGameObject;
    }


    [ServerRpc]
    public void spawnEnemyBasicServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            enemyBasicPrefab,
            objVector,
            objQuat
        );

        unitToSpawn.GetComponent<EnemyBasic>().prefab = enemyBasicPrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
    }

    [ServerRpc]
    public void spawnEnemyCptServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            enemyCptPrefab,
            objVector,
            objQuat
        );

        unitToSpawn.GetComponent<EnemyBasic>().prefab = enemyCptPrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
    }
  [ServerRpc]
    public void spawnEnemyHHServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            enemyHHPrefab,
            objVector,
            objQuat
        );

        unitToSpawn.GetComponent<EnemyBasic>().prefab = enemyHHPrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
    }

    [ServerRpc]
    public void spawnCloneServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            clonePrefab,
            objVector,
            objQuat
        );

        unitToSpawn.GetComponent<PlayerCopy>().prefab = clonePrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void spawnMissileServerRpc(
        Vector3 objVector,
        Quaternion objQuat,
        ServerRpcParams serverRpcParams = default
    )
    {
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            missilePrefab,
            objVector,
            objQuat
        );

        unitToSpawn.GetComponent<Missile>().prefab = missilePrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
        Missile netMissileC = unitToSpawn.GetComponent<Missile>();

        PlayerController parent = playerArray[
            serverRpcParams.Receive.SenderClientId
        ].GetComponent<PlayerController>();
        // Debug.Log("missile parent: " + parent.fireTarget + " " + parent.enemyDic);
        netMissileC.SetPlayerTarget(parent.fireTarget, parent.enemyDic);
        unitToSpawn.GetComponent<Rigidbody2D>().AddForce(parent.firePoint.up * 6000);
    }

    public EnemyBasic cptParent;
    EnemyBasic parent;

    public void spawnEM(Vector3 objVector, Quaternion objQuat, NetworkObject NW)
    {
        cptParent = NW.GetComponent<EnemyBasic>();
        spawnEMServerRpc(objVector, objQuat);
    }

    [ServerRpc(RequireOwnership = false)]
    public void spawnEMServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            EMPrefab,
            objVector,
            objQuat
        );

        unitToSpawn.GetComponent<Missile>().prefab = EMPrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
        Missile netMissileC = unitToSpawn.GetComponent<Missile>();

        netMissileC.SetTarget(cptParent.fireTarget);
        unitToSpawn.GetComponent<Rigidbody2D>().AddForce(cptParent.firePoint.up * 6000);
    }
[ServerRpc(RequireOwnership = false)]
    public void spawnSparkServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        // if (!NetworkManager.Singleton.IsServer)
        //     return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            sparkPrefab,
            objVector,
            objQuat
        );
        ParticleSystem unitToSpawnC = unitToSpawn.GetComponent<ParticleSystem>();
        unitToSpawnC.Play();

        if (!unitToSpawn.IsSpawned)
        {
            unitToSpawn.Spawn(true); 
            activeSparks.Add(unitToSpawn);
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void despawnSparkServerRpc(){
        NetworkObject nextSpark = activeSparks[0];
        GameObject prefab;
        if(nextSpark.tag == "Spark"){
            prefab = sparkPrefab;
        } else {
            prefab = explosionPrefab;
        }
NetworkObjectPool.Singleton.ReturnNetworkObject(nextSpark, prefab);
nextSpark.Despawn();
activeSparks.RemoveAt(0);
    
    }
[ServerRpc(RequireOwnership = false)]
    public void spawnExplosionServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        // if (!NetworkManager.Singleton.IsServer)
        //     return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            explosionPrefab,
            objVector,
            objQuat
        );
        ParticleSystem unitToSpawnC = unitToSpawn.GetComponent<ParticleSystem>();
        unitToSpawnC.Play();

        if (!unitToSpawn.IsSpawned)
        {
            unitToSpawn.Spawn(true); 
            activeSparks.Add(unitToSpawn);
        }

    }
//     [ServerRpc(RequireOwnership = false)]
//     public void despawnExplosionServerRpc(){
//         NetworkObject nextSpark = activeSparks[0];
// NetworkObjectPool.Singleton.ReturnNetworkObject(nextSpark, explosionPrefab);
// nextSpark.Despawn();
// activeSparks.RemoveAt(0);
    
//     }
    [ServerRpc(RequireOwnership = false)]
    public void spawnBulletServerRpc(Vector3 objVector, Quaternion objQuat, int teamID)
    {
        // if (!NetworkManager.Singleton.IsServer)
        //     return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            bulletPrefab,
            objVector,
            objQuat
        );
        Bullet unitToSpawnC = unitToSpawn.GetComponent<Bullet>();
        unitToSpawnC.prefab = bulletPrefab;
        unitToSpawnC.teamID = teamID;

        if (!unitToSpawn.IsSpawned)
        {
            Rigidbody2D rb = unitToSpawn.GetComponent<Rigidbody2D>();
            unitToSpawn.Spawn(true);
            rb.AddForce(rb.transform.up * 4000);
        }

    }

    [ClientRpc]
    public void spawnBulletClientRpc(Vector3 objVector, Quaternion objQuat, int teamID)
    {
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            bulletPrefab,
            objVector,
            objQuat
        );
        Bullet unitToSpawnC = unitToSpawn.GetComponent<Bullet>();
        unitToSpawnC.prefab = bulletPrefab;
        unitToSpawnC.teamID = teamID;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
        Rigidbody2D rb = unitToSpawn.GetComponent<Rigidbody2D>();
        rb.AddForce(rb.transform.up * 2000);
    }

    [ServerRpc(RequireOwnership = false)]
    public void spawnEnemyBulletServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            enemyBulletPrefab,
            objVector,
            objQuat
        );
        unitToSpawn.GetComponent<Bullet>().prefab = enemyBulletPrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
        Rigidbody2D rb = unitToSpawn.GetComponent<Rigidbody2D>();
        rb.AddForce(rb.transform.up * 2000);
    }

    [ClientRpc]
    public void spawnEnemyBulletClientRpc(Vector3 objVector, Quaternion objQuat)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            enemyBulletPrefab,
            objVector,
            objQuat
        );
        unitToSpawn.GetComponent<Bullet>().prefab = enemyBulletPrefab;
        if (!unitToSpawn.IsSpawned)
            unitToSpawn.Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void spawnTrailServerRpc(Vector3 objVector, Quaternion objQuat)
    {
        NetworkObject unitToSpawn = NetworkObjectPool.Singleton.GetNetworkObject(
            trail,
            objVector,
            objQuat
        );
    }

    GameObject missileToDespawn;
    NetworkObject missileToDespawnNW;

    public void despawn(GameObject unitPrefab)
    {
        missileToDespawn = unitPrefab;
        missileToDespawnNW = unitPrefab.GetComponent<NetworkObject>();
        //  NetworkObjectPool.Singleton.ReturnNetworkObject(missileToDespawnNW, unitPrefab);
        despawnServerRpc();
        // despawnClientRpc();
    }

    [ClientRpc]
    public void despawnClientRpc()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(missileToDespawnNW, missileToDespawn);
    }

    [ServerRpc(RequireOwnership = false)]
    public void despawnServerRpc()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(missileToDespawnNW, missileToDespawn);
    }

    public Transform GetClosestTransform(GameObject[] gameObjects, Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        foreach (GameObject gameObject in gameObjects)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                // break;
            }
        }
        return closestTransform;
    }

    public GameObject GetClosestGameObject(GameObject[] gameObjects, Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in gameObjects)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                closestGameObject = gameObject;
            }
        }
        return closestGameObject;
    }
public GameObject GetClosestEnemyGameObjectDic(Dictionary<ulong, GameObject> _enemies, Vector3 position)
{
    // GameObject closestNeutral = GetClosestEnemyGameObject(position); 
//    if(gameMode != "PVP") return closestNeutral;
    float distance = Mathf.Infinity;
    Transform closestTransform = null;
    float closestDistance = float.MaxValue;
    GameObject closestGameObject = null;
    foreach (KeyValuePair<ulong, GameObject> kvp in _enemies)
    {
        Transform transform = kvp.Value.transform;
        distance = (transform.position - position).sqrMagnitude;
        if (distance < closestDistance)
        {
            closestTransform = transform;
            closestDistance = distance;
            closestGameObject = kvp.Value;
        }
    }
    
    // Transform transformN = closestNeutral.transform;
    // distance = (transformN.position - position).sqrMagnitude;
    // if(distance < closestDistance){
    //     return closestNeutral;
    // }
    // else
    return closestGameObject;
}
    public GameObject GetClosestEnemyGameObject(Vector3 position)
    {
        float distance = Mathf.Infinity;
        Transform closestTransform = null;
        float closestDistance = float.MaxValue;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in enemies)
        {
            Transform transform = gameObject.transform;
            distance = (transform.position - position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestTransform = transform;
                closestDistance = distance;
                closestGameObject = gameObject;
            }
        }
        return closestGameObject;
    }

    public void AddGridForce(
        Vector3 position,
        float magnitude,
        float radius,
        Color rippleColor,
        bool hasColor
    )
    {
        position.z = VGZ;
        VG.AddGridForce(position, magnitude, radius, rippleColor, hasColor);
    }

    private void UpdateGlobal()
    {
        cloneArray = GameObject.FindGameObjectsWithTag("Clone");
        combinedAllies = new GameObject[cloneArray.Length + playerArray.Length];
        Array.Copy(playerArray, combinedAllies, playerArray.Length);
        Array.Copy(cloneArray, 0, combinedAllies, playerArray.Length, cloneArray.Length);
        allyArray = combinedAllies;
        captains = GameObject.FindGameObjectsWithTag("EnemyCaptain");
        enemies = GameObject.FindGameObjectsWithTag("BasicEnemy");
        allies = allyArray.Length;
        GameObject[] combinedEnemies = new GameObject[captains.Length + enemies.Length];
        Array.Copy(captains, combinedEnemies, captains.Length);
        Array.Copy(enemies, 0, combinedEnemies, captains.Length, enemies.Length);
        enemies = combinedEnemies;
        allyArrayTransform = new Transform[allies];
        for (int i = 0; i < allies; i++)
        {
            allyArrayTransform[i] = allyArray[i].transform;
        }
    }
    public int GetPlayerID()
    {
        playerArray = GameObject.FindGameObjectsWithTag("Player");

        return playerArray.Length - 1;
    }

    void FixedUpdate()
    {
        playerArray = GameObject.FindGameObjectsWithTag("Player");
        VGZ = VG.transform.position.z;
        int activeSparksCount = activeSparks.Count;
        if(activeSparksCount > 0){
            sparkTimer+=Time.deltaTime * activeSparksCount;
        }
        if(sparkTimer> 1){
            sparkTimer = 0;
            despawnSparkServerRpc();
        }
        
    }

    [ClientRpc]
    void UMClientRpc()
    {
        UM.Instance = this;
    }
}
