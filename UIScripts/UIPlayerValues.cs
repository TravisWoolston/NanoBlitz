using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPlayerValues : MonoBehaviour
{
    public float clones = 0;
    public float missiles = 0;
    public TMP_Text AIReclaimed;
    public float rocketRate;
    public string serverKey;

    // Start is called before the first frame update
    // void Start() { }

    // Update is called once per frame
    void FixedUpdate()
    {
        AIReclaimed.text = "AI Reclaimed: " + clones + "\n" + 
        "Missiles: " + missiles + "\n" +
        "MissileMultiplier/MaxHp: " + rocketRate + "\n" +
        "Server Key: " + serverKey + "\n" +
        "\n\nControls: \nLeftShift/W: Accelerate \nA/D: Strafe \nLeftClick: Focus Fire \nRightClick: Fire Missile \nF: Consume Allies \nScrollWheel: Zoom In/Out";

    }
}
