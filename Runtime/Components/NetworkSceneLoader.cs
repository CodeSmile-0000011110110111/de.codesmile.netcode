// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.Tools;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeSmile
{
	public class NetworkSceneLoader : OneTimeTaskBehaviour
	{
		[SerializeField] private SceneReference m_OfflineScene;
		[SerializeField] private SceneReference m_OnlineScene;

		public SceneReference OfflineScene => m_OfflineScene;
		public SceneReference OnlineScene => m_OnlineScene;

		private bool m_LoadOfflineScene;
		private bool m_LoadOnlineScene;

		public void ScheduleLoadOfflineScene()
		{
			m_LoadOfflineScene = true;
			m_LoadOnlineScene = false;
		}
		public void ScheduleLoadOnlineScene()
		{
			m_LoadOfflineScene = false;
			m_LoadOnlineScene = true;
		}

		private void Start()
		{
			if (String.IsNullOrWhiteSpace(m_OfflineScene.SceneName))
				throw new ArgumentException($"Offline scene not assigned in {nameof(NetworkSceneLoader)}");
			if (String.IsNullOrWhiteSpace(m_OnlineScene.SceneName))
				throw new ArgumentException($"Online scene not assigned in {nameof(NetworkSceneLoader)}");
		}

		private void OnValidate()
		{
			m_OfflineScene.OnValidate();
			m_OnlineScene.OnValidate();
		}

		private void LateUpdate()
		{
			if (m_LoadOfflineScene)
				LoadScene(m_OfflineScene.SceneName);
		}

		private void LoadScene(string sceneName)
		{
			// clients (launched from command line) automatically load a scene when connected
			var networkManager = NetworkManager.Singleton;
			Debug.Log(
				$"NET State: listen={networkManager.IsListening}, server={networkManager.IsServer}, host={networkManager.IsHost}, client={networkManager.IsClient}");

			// if (networkManager.IsListening && networkManager.IsClient)
			// {
			// 	Debug.Log("Client waiting for connection ...");
			// 	TaskPerformed();
			// 	return;
			// }

			if (m_LoadOfflineScene)
			{
				Debug.Log($"Loading offline scene: {sceneName}");
				//SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
			}
			else if (m_LoadOnlineScene)
			{
				Debug.Log($"Loading online scene: {sceneName}");
				//networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
			}

			m_LoadOfflineScene = false;
			m_LoadOnlineScene = false;
		}
	}
}
