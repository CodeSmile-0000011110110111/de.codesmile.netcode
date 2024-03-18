// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Netcode.Extensions;
using CodeSmile.SceneTools;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Single-Loads a scene when server starts or stops or client disconnects.
	///     This moves clients into the online scene when networking starts and
	///     moves clients to the offline scene when they disconnect.
	/// </summary>
	[DisallowMultipleComponent]
	public class NetworkSceneAutoLoader : NetworkBehaviour
	{
		[Tooltip("(Server / Host) Load this network scene when server starts.")]
		[SerializeField] private SceneReference m_LoadWhenServerStarts;
		[Tooltip("(Client / Host) Load this offline scene when disconnecting.")]
		[SerializeField] private SceneReference m_LoadWhenClientDisconnects;

		private void OnValidate()
		{
			m_LoadWhenServerStarts?.OnValidate();
			m_LoadWhenClientDisconnects?.OnValidate();
		}

		private void OnEnable() => NetworkManagerExt.InvokeWhenSingletonReady(RegisterCallbacks);

		private void OnDisable() => UnregisterCallbacks();

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnServerStarted += OnServerStarted;
				netMan.OnClientStopped += OnClientStopped;
			}
		}

		private void UnregisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null)
			{
				netMan.OnServerStarted -= OnServerStarted;
				netMan.OnClientStopped -= OnClientStopped;
			}
		}

		private void OnServerStarted()
		{
			if (IsServer)
			{
				if (m_LoadWhenServerStarts != null)
					LoadNetworkScene(m_LoadWhenServerStarts.SceneName);
			}
		}

		private void OnClientStopped(Boolean isHost)
		{
			if (IsClient)
			{
				if (m_LoadWhenClientDisconnects != null)
					LoadOfflineScene(m_LoadWhenClientDisconnects.SceneName);
			}
		}

		private void LoadNetworkScene(String sceneName)
		{
			NetworkLog.LogInfo($"=> Loading network scene: {sceneName}");
			NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		}

		private void LoadOfflineScene(String sceneName) => StartCoroutine(LoadOfflineSceneAtEndOfFrame(sceneName));

		private IEnumerator LoadOfflineSceneAtEndOfFrame(String sceneName)
		{
			yield return null;

			NetworkLog.LogInfo($"=> Loading offline scene: {sceneName}");
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		}
	}
}
