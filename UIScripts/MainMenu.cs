using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static string HostType = null;
    public static string ServerKey;
    public static float volume = .75f;
    public Slider volumeSlider;
    public MusicVolumeController mVC;
    void awake(){
        mVC = mVC.GetComponent<MusicVolumeController>();
        volume = PlayerPrefs.GetFloat("volume");
        volumeSlider.value = volume;
    }
public void StartSurvival(string hostType){
    HostType = hostType;
    SceneManager.LoadScene("NB multiplayer");
}
public void SetServerKey(string serverKey){
    ServerKey = serverKey;
}
public void JoinGame(string hostType, string serverKey){
    HostType = "Client";
    SceneManager.LoadScene("NB multiplayer");
}
public void QuitGame(){
    Application.Quit();
}
public void UpdateVolume(){
    mVC.musicPlayer.volume = volumeSlider.value;
    PlayerPrefs.SetFloat("volume", volumeSlider.value);
}
}
