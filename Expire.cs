using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expire : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(transform.position.x);
    }

    private void OnBecameInvisible()
    {
        // Object.Destroy(this.gameObject, 1);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(transform.position.x);
    }
}
