using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIAIReclaimed : MonoBehaviour
{
    private float clones = 0;
    public TMP_Text AIReclaimed;

    // Start is called before the first frame update
    // void Start() { }

    // Update is called once per frame
    void FixedUpdate()
    {
        clones = UM.Instance.allies;
        AIReclaimed.text = "AI Reclaimed: " + clones;
    }
}
