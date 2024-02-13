// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Tools;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.Netcode.Components
{
	public class NetworkSessionState : MonoBehaviour
	{
		[SerializeField] private SceneReference m_LoadSceneWhenServerStarted;

		private void Awake() => DontDestroyOnLoad(gameObject);

		private void OnValidate() => m_LoadSceneWhenServerStarted?.OnValidate();

		private void Start()
		{
			if (String.IsNullOrWhiteSpace(m_LoadSceneWhenServerStarted?.SceneName))
				throw new ArgumentException($"Server start scene not assigned in {nameof(NetworkSessionState)}");

			// since the session state is not supposed to be destroyed until application quits,
			// there is no need to unsubscribe the event handlers
			var netMan = NetworkManager.Singleton;
			netMan.OnServerStarted += OnServerStarted;
			netMan.OnServerStopped += OnServerStopped;
			netMan.OnClientStarted += OnClientStarted;
			netMan.OnClientStopped += OnClientStopped;
			netMan.OnConnectionEvent += OnConnectionEvent;
			netMan.OnTransportFailure += OnTransportFailure;
		}

		private void OnServerStarted()
		{
			Debug.Log("=> Server Started");
			Debug.Log($"=> Loading scene: {m_LoadSceneWhenServerStarted.SceneName}");
			NetworkManager.Singleton.SceneManager.LoadScene(m_LoadSceneWhenServerStarted.SceneName, LoadSceneMode.Single);
		}

		private void OnServerStopped(Boolean isHost) => Debug.Log($"=> Server Stopped, host={isHost}");

		private void OnClientStarted() => Debug.Log("=> Client Started");

		private void OnClientStopped(Boolean isHost) => Debug.Log($"=> Client Stopped, host={isHost}");

		private void OnConnectionEvent(NetworkManager netMan, ConnectionEventData data) =>
			Debug.Log($"=> {data.EventType}, clientId={data.ClientId}");

		private void OnTransportFailure() => Debug.LogWarning("=> TRANSPORT FAILURE");
	}
}
