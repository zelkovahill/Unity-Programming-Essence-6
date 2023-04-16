using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    // 플레이어 목록과 준비 여부를 표시할 UI 텍스트
    public Text lobbyText;

    // 게임을 시작하기 위해 필요한 최소한의 준비된 플레이어 수
    private const int MinimumReadyCountToStartGame = 2;

    // 플레이어 준비 상태를 저장하는 딕셔너리
    private Dictionary<ulong, bool> _clientsInLobbyReadyStateDictionary
        = new Dictionary<ulong, bool>();

    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    // OnNetworkSpawn은 NetworkBehaviour가 생성될 때 호출됨
    // 여기서는 플레이어 목록을 초기화하고, 서버에서는 클라이언트들이 접속했는지 감지하는 콜백을 등록함
    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        // 먼저 자기 자신을 등록
        _clientsInLobbyReadyStateDictionary.Add(NetworkManager.LocalClientId, false);

        // 만약 우리가 서버(호스트)라면, 클라이언트들이 접속했는지 콜백을 통해 감지하고 관리해야함
        if (IsServer)
        {
            // 클라이언트가 접속하거나 떠났을 때 호출되는 콜백을 등록
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectedCallback;

            // OnLoadComplete는 클라이언트가 어떤 씬을 로드하는데 성공했을때 실행되는 콜백 
            // 서버는 OnLoadComplete를 모든 클라이언트들에 대해서 듣는다는 것에 주의!
            NetworkManager.SceneManager.OnLoadComplete += OnClientSceneLoadComplete;
        }

        //Update our lobby
        UpdateLobbyText();
    }

    // OnNetworkDespawn은 NetworkBehaviour가 파괴될 때 호출됨
    // 여기서는 서버가 네트워크 매니저에게 등록한 콜백을 해제하는데 사용
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
            NetworkManager.SceneManager.OnLoadComplete -= OnClientSceneLoadComplete;
        }
    }

    // 클라이언트가 어떤 씬을 로드하는데 성공했을때 실행할 처리
    private void OnClientSceneLoadComplete(ulong clientid, string scenename, LoadSceneMode loadscenemode)
    {
        if (IsServer)
        {
            if (!_clientsInLobbyReadyStateDictionary.ContainsKey(clientid))
            {
                _clientsInLobbyReadyStateDictionary.Add(clientid, false);
                UpdateLobbyText();
            }

            UpdateAndCheckPlayersInLobby();
        }
    }

    // 로비의 텍스트를 갱신
    private void UpdateLobbyText()
    {
        var stringBuilder = new StringBuilder();
        foreach (var clientLobbyStatusPair in _clientsInLobbyReadyStateDictionary)
        {
            var clientId = clientLobbyStatusPair.Key;
            var isReady = clientLobbyStatusPair.Value;

            if (isReady)
            {
                stringBuilder.AppendLine($"PLAYER_{clientId} : READY");
            }
            else
            {
                stringBuilder.AppendLine($"PLAYER_{clientId} : NOT READY");
            }
        }

        lobbyText.text = stringBuilder.ToString();
    }

    // 클라이언트들의 준비 상태를 갱신하고, 게임을 시작할 수 있는지 확인
    private void UpdateAndCheckPlayersInLobby()
    {
        var enoughPlayer = _clientsInLobbyReadyStateDictionary.Count >= MinimumReadyCountToStartGame;
        var allReady = true;
        foreach (var clientReadyStatePair in _clientsInLobbyReadyStateDictionary)
        {
            var clientId = clientReadyStatePair.Key;
            var isReady = clientReadyStatePair.Value;

            SendClientReadyStatusUpdatesClientRpc(clientId, isReady);

            if (!isReady)
            {
                allReady = false;
            }
        }

        if (enoughPlayer && allReady)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.SceneManager.OnLoadComplete -= OnClientSceneLoadComplete;
            NetworkManager.SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }
    }


    // 클라이언트가 연결되었을때 실행할 콜백
    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!_clientsInLobbyReadyStateDictionary.ContainsKey(clientId))
            {
                _clientsInLobbyReadyStateDictionary.Add(clientId, false);
            }

            UpdateLobbyText();
            UpdateAndCheckPlayersInLobby();
        }
    }

    // 클라이언트 접속이 끊겼을 때 실행할 콜백
    private void OnClientDisconnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (_clientsInLobbyReadyStateDictionary.ContainsKey(clientId))
            {
                _clientsInLobbyReadyStateDictionary.Remove(clientId);
                UpdateLobbyText();
            }
        }
    }

    // 서버가 다른 클라이언트들에게 어떤 클라이언트의 준비 상태가 변경됨을 알리는 RPC 메서드
    [ClientRpc]
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    {
        if (IsServer)
        {
            return;
        }

        _clientsInLobbyReadyStateDictionary[clientId] = isReady;
        UpdateLobbyText();
    }


    // 클라이언트가 준비 버튼을 눌렀을때 실행하는 메서드
    public void SetPlayerIsReady()
    {
        _clientsInLobbyReadyStateDictionary[NetworkManager.Singleton.LocalClientId] = true;
        if (IsServer)
        {
            UpdateAndCheckPlayersInLobby();
        }
        else
        {
            OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        UpdateLobbyText();
    }

    // 클라이언트가 준비 버튼을 눌렀음을 서버에게 알리기 위한 RPC 메서드
    [ServerRpc(RequireOwnership = false)]
    private void OnClientIsReadyServerRpc(ulong clientid)
    {
        if (_clientsInLobbyReadyStateDictionary.ContainsKey(clientid))
        {
            _clientsInLobbyReadyStateDictionary[clientid] = true;
            UpdateAndCheckPlayersInLobby();
            UpdateLobbyText();
        }
    }
}