using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] TMP_Text gameMessageTM;
    [SerializeField] TMP_Text fusionSessionNameTM;
    [SerializeField] NetworkRunner currentRunner;

    void OnEnable()
    {
        currentRunner.AddCallbacks (this);
    }

    void OnDisable()
    {
        currentRunner.RemoveCallbacks (this);
    }

    void Start()
    {
        ConnectToGame();
    }

    void ConnectToGame()
    {  
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef (scene);
        }
        
        currentRunner.StartGame (
            new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = "tic",
                PlayerCount = 2,
                Scene = sceneInfo,
                SceneManager = currentRunner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            }
        );
    }

    public void OnPlayerJoined (NetworkRunner runner, PlayerRef player)
    {
        fusionSessionNameTM.text = $"Session Name: {runner.SessionInfo.Name}";
    }

    public void OnPlayerLeft (NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnConnectedToServer (NetworkRunner runner)
    {
    }
    
#region unused
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer (NetworkRunner runner, NetDisconnectReason reason)
    {
        gameMessageTM.text = "Host connection lost. Please close the game.";
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
         
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
         
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
         
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
         
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
         
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
         
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
         
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
#endregion
}
