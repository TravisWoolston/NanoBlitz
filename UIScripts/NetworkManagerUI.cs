using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField]
    private Button serverBtn;

    [SerializeField]
    private Button hostBtn;

    [SerializeField]
    private Button clientBtn;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        });
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            NetworkObjectPool.Singleton.InitializePool();
            // NetworkManager.Singleton.SceneManager.LoadScene("Overworld", LoadSceneMode.Single);
        });
        clientBtn.onClick.AddListener(() =>
        {

            NetworkManager.Singleton.StartClient();
            NetworkObjectPool.Singleton.InitializePool();
        });
    }
}
