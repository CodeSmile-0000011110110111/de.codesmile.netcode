// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Tools;
using System;
using System.Collections.Generic;
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

		private readonly Dictionary<UInt64, Byte[]> m_ClientPayloads = new();
		public IReadOnlyDictionary<UInt64, Byte[]> ClientPayloads => m_ClientPayloads;

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
			netMan.ConnectionApprovalCallback = OnConnectionApprovalRequest;
		}

		private void OnConnectionApprovalRequest(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			// needs to be done here since approval request for host runs before OnServerStarted!
			ClearPayloadsOnFirstConnection();

			var clientId = request.ClientNetworkId;
			var payload = request.Payload;
			m_ClientPayloads[clientId] = payload;

			Debug.Log($"=> ConnectionApprovalRequest: Client {clientId}, payload: '{payload?.GetString()}'");

			response.Approved = true;
			response.Reason = $"{nameof(NetworkSessionState)} always approves";
			response.CreatePlayerObject = true;
		}

		private void OnServerStarted()
		{
			Debug.Log("=> Server Started");

			var netSceneManager = NetworkManager.Singleton.SceneManager;
			netSceneManager.OnSceneEvent += OnServerSceneEvent;

			netSceneManager.LoadScene(m_LoadSceneWhenServerStarts.SceneName, LoadSceneMode.Single);
		}

		private void OnServerStopped(Boolean isHost)
		{
			Debug.Log($"=> {(isHost ? "Host Server" : "Server")} Stopped");

			if (isHost == false && m_LoadSceneWhenServerStops != null)
			{
				Debug.Log($"=> Loading offline scene: {m_LoadSceneWhenServerStops.SceneName}");
				SceneManager.LoadScene(m_LoadSceneWhenServerStarts.SceneName, LoadSceneMode.Single);
			}
		}

		private void OnClientStarted()
		{
			Debug.Log("=> Client Started");

			var netSceneManager = NetworkManager.Singleton.SceneManager;
			netSceneManager.OnSceneEvent += OnClientSceneEvent;
		}

		private void OnClientStopped(Boolean isHost)
		{
			Debug.Log($"=> {(isHost ? "Host Client" : "Client")} Stopped");

			if (m_LoadSceneWhenClientDisconnects != null)
			{
				Debug.Log($"=> Loading offline scene: {m_LoadSceneWhenServerStops.SceneName}");
				SceneManager.LoadScene(m_LoadSceneWhenClientDisconnects.SceneName, LoadSceneMode.Single);
			}
		}

		private void OnServerSceneEvent(SceneEvent sceneEvent)
		{
			// Debug.Log($"=> Server: {ToString(sceneEvent)}");
		}

		private void OnClientSceneEvent(SceneEvent sceneEvent)
		{
			// Debug.Log($"=> Client: {ToString(sceneEvent)}");
		}

		private void OnConnectionEvent(NetworkManager netMan, ConnectionEventData data) =>
			Debug.Log($"=> Connection Event: {data.EventType}, clientId={data.ClientId}");

		private void OnTransportFailure() => Debug.LogWarning("=> TRANSPORT FAILURE");

		private void ClearPayloadsOnFirstConnection()
		{
			if (NetworkManager.Singleton.ConnectedClients.Count == 0)
				m_ClientPayloads.Clear();
		}

		private String ToString(SceneEvent sceneEvent) =>
			$"'{sceneEvent.SceneName}' {sceneEvent.LoadSceneMode}Scene{sceneEvent.SceneEventType} (client: {sceneEvent.ClientId})";
	}
}
