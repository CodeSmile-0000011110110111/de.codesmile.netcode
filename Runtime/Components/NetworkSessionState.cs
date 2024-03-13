// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	[DisallowMultipleComponent]
	public class NetworkSessionState : MonoBehaviour
	{
		private readonly Dictionary<UInt64, Byte[]> m_ClientPayloads = new();

		private Boolean m_CallbacksRegistered;

		public IReadOnlyDictionary<UInt64, Byte[]> ClientPayloads => m_ClientPayloads;

		private void OnEnable() => NetworkManagerExt.InvokeWhenSingletonReady(RegisterCallbacks);

		private void OnDisable() => UnregisterCallbacks();

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null && m_CallbacksRegistered == false)
			{
				m_CallbacksRegistered = true;
				netMan.OnServerStarted += OnServerStarted;
				netMan.OnServerStopped += OnServerStopped;
				netMan.OnClientStarted += OnClientStarted;
				netMan.OnClientStopped += OnClientStopped;
				netMan.OnConnectionEvent += OnConnectionEvent;
				netMan.OnTransportFailure += OnTransportFailure;
				netMan.ConnectionApprovalCallback += OnConnectionApprovalRequest;
			}
		}

		private void UnregisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				m_CallbacksRegistered = false;
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnServerStopped -= OnServerStopped;
				netMan.OnClientStarted -= OnClientStarted;
				netMan.OnClientStopped -= OnClientStopped;
				netMan.OnConnectionEvent -= OnConnectionEvent;
				netMan.OnTransportFailure -= OnTransportFailure;
				netMan.ConnectionApprovalCallback -= OnConnectionApprovalRequest;
			}
		}

		private void OnConnectionApprovalRequest(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			// needs to be done here since approval request for host runs before OnServerStarted!
			ClearPayloadsOnFirstConnection();

			var clientId = request.ClientNetworkId;
			var payload = request.Payload;
			m_ClientPayloads[clientId] = payload;

			NetworkLog.LogInfo($"=> ConnectionApprovalRequest: Client {clientId}, " +
			                   $"payload: '{payload?.GetString()}' ({payload.Length} bytes)");

			response.Approved = true;
			response.Reason = $"{nameof(NetworkSessionState)} always approves";
			response.CreatePlayerObject = true;
		}

		private void OnServerStarted()
		{
			NetworkLog.LogInfo("=> Server Started");

			// var netSceneManager = NetworkManager.Singleton.SceneManager;
			// netSceneManager.OnSceneEvent += OnServerSceneEvent;
		}

		private void OnClientStarted()
		{
			NetworkLog.LogInfo("=> Client Started");

			// var netSceneManager = NetworkManager.Singleton.SceneManager;
			// netSceneManager.OnSceneEvent += OnClientSceneEvent;
		}

		private void OnServerStopped(Boolean isHost) =>
			NetworkLog.LogInfo($"=> {(isHost ? "Server (Host)" : "Server")} Stopped");

		private void OnClientStopped(Boolean isHost) =>
			NetworkLog.LogInfo($"=> {(isHost ? "Client (Host)" : "Client")} Stopped");

		private void OnServerSceneEvent(SceneEvent sceneEvent)
		{
			//NetworkLog.LogInfo($"=> Server: {ToString(sceneEvent)}");
		}

		private void OnClientSceneEvent(SceneEvent sceneEvent)
		{
			//NetworkLog.LogInfo($"=> Client: {ToString(sceneEvent)}");
		}

		private void OnConnectionEvent(NetworkManager netMan, ConnectionEventData data) =>
			NetworkLog.LogInfo($"=> Connection Event: {data.EventType}, clientId={data.ClientId}");

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
