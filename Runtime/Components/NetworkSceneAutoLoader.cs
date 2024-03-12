// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.SceneTools;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile.Netcode.Components
{
	[DisallowMultipleComponent]
	public class NetworkSceneAutoLoader : MonoBehaviour
	{
		[SerializeField] private SceneReference m_LoadWhenServerStarts;
		[SerializeField] private SceneReference m_LoadWhenServerStops;
		[SerializeField] private SceneReference m_LoadWhenHostOrClientDisconnects;
		private Boolean m_CallbacksRegistered;

		private void OnValidate()
		{
			m_LoadWhenServerStarts?.OnValidate();
			m_LoadWhenServerStops?.OnValidate();
			m_LoadWhenHostOrClientDisconnects?.OnValidate();
		}

		private void Start() => RegisterCallbacks();

		private void OnEnable() => RegisterCallbacks();

		private void OnDisable() => UnregisterCallbacks();

		private void RegisterCallbacks()
		{
			var netMan = NetworkManager.Singleton;
			if (netMan != null && m_CallbacksRegistered == false)
			{
				m_CallbacksRegistered = true;
				netMan.OnServerStarted += OnServerStarted;
				netMan.OnServerStopped += OnServerStopped;
				netMan.OnClientStopped += OnClientStopped;
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
				netMan.OnClientStopped -= OnClientStopped;
			}
		}

		private void OnServerStarted()
		{
			if (m_LoadWhenServerStarts != null)
				LoadNetworkScene(m_LoadWhenServerStarts.SceneName);
		}

		private void OnServerStopped(Boolean isHost)
		{
			if (isHost == false && m_LoadWhenServerStops != null)
				LoadOfflineScene(m_LoadWhenServerStops.SceneName);
		}

		private void OnClientStopped(Boolean isHost)
		{
			if (m_LoadWhenHostOrClientDisconnects != null)
				LoadOfflineScene(m_LoadWhenHostOrClientDisconnects.SceneName);
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
