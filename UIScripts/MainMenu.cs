using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static string HostType;
    public static string ServerKey;
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
}
