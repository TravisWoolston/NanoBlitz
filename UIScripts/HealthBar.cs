using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    PlayerController playerController;

    public Slider slider;
    public Gradient gradient;
    float maxHP;
    float hp;
    void start(){
        slider.value = slider.maxValue;
    }
    public void SetMaxHealth(float hp)
    {
       slider.maxValue = hp;
    //    slider.value = hp;

    }
    public void SetHealth(float hp) {
        slider.value = hp;
    }

}
