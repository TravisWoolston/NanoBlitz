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


    private void Awake()
    {
        serverKeyInput.text = "Enter Key";
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        });
        hostBtn.onClick.AddListener(() =>
        {
             GetComponent<ServerRelay>().CreateRelay();
            // NetworkManager.Singleton.StartHost();
            NetworkObjectPool.Singleton.InitializePool();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        });

        clientBtn.onClick.AddListener(() =>
        {
            GetComponent<ServerRelay>().JoinRelay();
            // NetworkManager.Singleton.StartClient();
            NetworkObjectPool.Singleton.InitializePool();
        });
                localHostBtn.onClick.AddListener(() =>
        {
            //  GetComponent<ServerRelay>().CreateRelay();
            NetworkManager.Singleton.StartHost();
            NetworkObjectPool.Singleton.InitializePool();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        });
        localJoinBtn.onClick.AddListener(() =>
        {
            // GetComponent<ServerRelay>().JoinRelay();
            NetworkManager.Singleton.StartClient();
            NetworkObjectPool.Singleton.InitializePool();
        });
    }
}