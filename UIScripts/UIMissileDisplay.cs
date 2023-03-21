using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIMissileDisplay : MonoBehaviour
{
    private float missiles = 0;
    public TMP_Text MissileDisplayText;
    public GameObject player;
    PlayerController playerC;
    
    void Start() { 
        playerC = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        missiles = playerC.missiles;
        MissileDisplayText.text = "Missiles: " + missiles;
    }
}
