using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CamMovement : MonoBehaviour
{
//    public override void OnNetworkSpawn() {
//     if(!IsOwner) {
//         Debug.Log("is owner");
//         Destroy(this);
//     }
//    }

    public Transform player;
    float targets = 0;
    float clones = 0;
    public float startSize = 90f;
    public float targetSize = 100f;
    public float size = 0;
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    public Camera cam;
    private float zoomRate = .05f;
    private float scaleRate;
    public GameObject VG;
    public float maxZoom = 0;
public float mainCamRef;


    void Start()
    {
        scaleRate = zoomRate / 30;
        if (this.gameObject.tag == "MiniMap")
        {
            cam.GetComponent<Camera>().orthographicSize = 600;
        }
        else
        {
            size = startSize;
            Camera.main.orthographicSize = size;
        }
        if(this.gameObject.tag == "RenderCam"){
            mainCamRef = GameObject.Find("Main Camera").GetComponent<CamMovement>().size;

        }
    }

public void SetPlayerTransform(Transform playerT){
player = playerT;
}
    void LateUpdate()
    {
        // targetSize = 110 + maxZoom;
        if(player == null) return;
        zoomRate = .05f * maxZoom/10;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            targetSize -= 1;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // forward
        {
            targetSize += 1;
        }
        if (this.gameObject.tag != "MiniMap")
        {
            if (size < targetSize && size < 110 + maxZoom)
            {
                size += zoomRate;
                                if (this.gameObject.tag == "MainCamera")

                VG.transform.localScale += new Vector3(scaleRate, scaleRate, 0);
               
                cam.orthographicSize = size;
            }
            if (size > targetSize && size > 1)
            {
                size -= zoomRate;
                if (this.gameObject.tag == "MainCamera")
                VG.transform.localScale -= new Vector3(scaleRate, scaleRate, 0);
                cam.orthographicSize = size;
            }
            // clones = PlayerController.Instance.allies;
            // if (clones > targets && Camera.main.orthographicSize < 120)
            // {
            //     targets = clones;
            //     targetSize = startSize + targets / 3;
            //     // Camera.main.orthographicSize = 35 + targets/4;
            // }
        }
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, 0) + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            .25f
        );
        //     Vector3 objPos = Camera.main.WorldToScreenPoint(VGRef.transform.position);
        // float scaleFactor = Mathf.Min(Screen.width, Screen.height) /10;
        // VGRef.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }
}
