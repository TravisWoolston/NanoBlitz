using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIMissileDisplay : MonoBehaviour
{
    public float missiles = 0;
    public TMP_Text MissileDisplayText;


    // Update is called once per frame
    void Update()
    {
        MissileDisplayText.text = "Missiles: " + missiles;
    }
}
