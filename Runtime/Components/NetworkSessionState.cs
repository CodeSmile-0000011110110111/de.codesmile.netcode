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
		[SerializeField] private Int32 m_MaxPayloadBytes = 1024;

		private readonly Dictionary<UInt64, Byte[]> m_ClientPayloads = new();

		public IReadOnlyDictionary<UInt64, Byte[]> ClientPayloads => m_ClientPayloads;

		private void OnEnable() => NetworkManagerExt.InvokeWhenSingletonReady(RegisterCallbacks);

		private void OnDisable() => UnregisterCallbacks();

		private Boolean PayloadSizeTooBig(NetworkManager.ConnectionApprovalRequest request,
			NetworkManager.ConnectionApprovalResponse response)
		{
			var payloadLength = request.Payload.Length;
			var tooBig = payloadLength > m_MaxPayloadBytes;
			if (tooBig)
			{
				response.Approved = false;
				response.Reason = "payload too big";
				NetworkLog.LogWarning(
					$"possible DOS attack by client {request.ClientNetworkId}, payload too big: {payloadLength}");
			}

			return tooBig;
		}

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
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
			if (PayloadSizeTooBig(request, response))
				return;

			// needs to be done here since approval request for host runs before OnServerStarted!
			ClearPayloadsOnFirstConnection();

			var clientId = request.ClientNetworkId;
			var payload = request.Payload;
			m_ClientPayloads[clientId] = payload;

			NetworkLog.LogInfo($"=> ConnectionApprovalRequest: Client {clientId}, " +
			                   $"payload: '{payload?.GetString()}' ({payload.Length} bytes)");

			response.Approved = true;
			response.Reason = $"{nameof(NetworkSessionState)} approves";
			response.CreatePlayerObject = true;
		}

		private void OnServerStarted() => NetworkLog.LogInfo("=> Server Started");
		private void OnClientStarted() => NetworkLog.LogInfo("=> Client Started");

		private void OnServerStopped(Boolean isHost) =>
			NetworkLog.LogInfo($"=> {(isHost ? "Server (Host)" : "Server")} Stopped");

		private void OnClientStopped(Boolean isHost) =>
			NetworkLog.LogInfo($"=> {(isHost ? "Client (Host)" : "Client")} Stopped");

		private void OnConnectionEvent(NetworkManager netMan, ConnectionEventData data) =>
			NetworkLog.LogInfo($"=> Connection Event: {data.EventType}, clientId={data.ClientId}");

		private void OnTransportFailure() => Debug.LogWarning("=> TRANSPORT FAILURE");

		private void ClearPayloadsOnFirstConnection()
		{
			if (NetworkManager.Singleton.ConnectedClients.Count == 0)
				m_ClientPayloads.Clear();
		}
	}
}
