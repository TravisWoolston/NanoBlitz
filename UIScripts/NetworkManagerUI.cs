using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField]
    private Button serverBtn;

    [SerializeField]
    private Button hostBtn;

    [SerializeField]
    private Button clientBtn;
    [SerializeField]
    private Button localHostBtn;
    [SerializeField]
    private Button localJoinBtn;
    [SerializeField]
    private InputField serverKeyInput;
    public string hostType;
    private void Awake()
    {
        hostType = MainMenu.HostType;
        serverKeyInput.text = "Enter Key";
        serverBtn.onClick.AddListener(() =>
        {
            StartServer();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        });
        if(hostType == "RelayHost"){
            // GetComponent<ServerRelay>().SignIn();
            CreateRelay();
        }
        hostBtn.onClick.AddListener(() =>
        {
             CreateRelay();
        });
        if(hostType == "Client"){
            // GetComponent<ServerRelay>().SignIn();
            JoinRelay();
        }
        clientBtn.onClick.AddListener(() =>
        {
            JoinRelay();
        });
        if(hostType == "LocalHost"){
            LocalHost();
        }
                localHostBtn.onClick.AddListener(() =>
        {
           LocalHost();
        });
        localJoinBtn.onClick.AddListener(() =>
        {
           LocalJoin();
        });
        Debug.Log("Host type: " + MainMenu.HostType);
        
    }
    public void StartServer(){
            NetworkManager.Singleton.StartServer();
        }
        public void CreateRelay(){
            GetComponent<ServerRelay>().CreateRelay();
            // NetworkManager.Singleton.StartHost();
            NetworkObjectPool.Singleton.InitializePool();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        }
        public void JoinRelay(){
            GetComponent<ServerRelay>().JoinRelay(MainMenu.ServerKey);
            // NetworkManager.Singleton.StartClient();
            NetworkObjectPool.Singleton.InitializePool();
        }
        public void LocalHost(){
             //  GetComponent<ServerRelay>().CreateRelay();
            NetworkManager.Singleton.StartHost();
            NetworkObjectPool.Singleton.InitializePool();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        }
        public void LocalJoin(){
             // GetComponent<ServerRelay>().JoinRelay();
            
            NetworkManager.Singleton.StartClient();
            NetworkObjectPool.Singleton.InitializePool();
        }
}