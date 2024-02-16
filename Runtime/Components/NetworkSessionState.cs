// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Tools;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.Netcode.Components
{
	public class NetworkSessionState : MonoBehaviour
	{
		[SerializeField] private SceneReference m_LoadSceneWhenServerStarts;
		[SerializeField] private SceneReference m_LoadSceneWhenServerStops;
		[SerializeField] private SceneReference m_LoadSceneWhenClientDisconnects;

		private void OnValidate()
		{
			m_LoadSceneWhenServerStarts?.OnValidate();
			m_LoadSceneWhenServerStops?.OnValidate();
			m_LoadSceneWhenClientDisconnects?.OnValidate();
		}

		// Important to init Session State in OnEnable since that runs before Start.
		// This guarantees that Session State hooks into NetworkManager events before NM starts listening.
		// Note that in Awake() the NetworkManager.Singleton is generally still null.
		private void OnEnable()
		{
			if (String.IsNullOrWhiteSpace(m_LoadSceneWhenServerStarts?.SceneName))
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
			Debug.Log($"=> Loading scene: {m_LoadSceneWhenServerStarts.SceneName}");
			NetworkManager.Singleton.SceneManager.LoadScene(m_LoadSceneWhenServerStarts.SceneName, LoadSceneMode.Single);
		}

		private void OnServerStopped(Boolean isHost)
		{
			Debug.Log($"=> {(isHost ? "Host Server" : "Server")} Stopped");

			if (isHost == false && m_LoadSceneWhenServerStops != null)
			{
				Debug.Log($"=> Loading scene: {m_LoadSceneWhenServerStops.SceneName}");
				SceneManager.LoadScene(m_LoadSceneWhenServerStarts.SceneName, LoadSceneMode.Single);
			}
		}

		private void OnClientStarted() => Debug.Log("=> Client Started");

		private void OnClientStopped(Boolean isHost)
		{
			Debug.Log($"=> {(isHost ? "Host Client" : "Client")} Stopped");

			if (m_LoadSceneWhenClientDisconnects != null)
			{
				Debug.Log($"=> Loading scene: {m_LoadSceneWhenServerStops.SceneName}");
				SceneManager.LoadScene(m_LoadSceneWhenClientDisconnects.SceneName, LoadSceneMode.Single);
			}
		}

		private void OnConnectionEvent(NetworkManager netMan, ConnectionEventData data) =>
			Debug.Log($"=> {data.EventType}, clientId={data.ClientId}");

		private void OnTransportFailure() => Debug.LogWarning("=> TRANSPORT FAILURE");
	}
}
