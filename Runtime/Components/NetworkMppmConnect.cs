// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using CodeSmile.Netcode.Extensions;
using System;
using UnityEngine;
#if UNITY_EDITOR
using CodeSmile.SceneTools;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEditor;
#endif

namespace CodeSmile.Netcode.Components
{
	[DisallowMultipleComponent]
	public class NetworkMppmConnect : OneTimeTaskBehaviour
	{
		private const String JoinCodeFile = "RelayJoinCode.txt";
		private const String TryRelayFile = "TryRelayInEditor.txt";

		// copied from NetworkManagerEditor
		private static readonly String k_UseEasyRelayIntegrationKey =
			"NetworkManagerUI_UseRelay_" + Application.dataPath.GetHashCode();

		[Tooltip("MPPM errors on shutdown can leave the main editor state paused with the pause button not visually pressed. " +
		         "Starting playmode with virtual players in this case will have virtual players seem frozen. " +
		         "By setting this true, entering playmode will unpause the game for all virtual players.")]
		[SerializeField] private Boolean m_AutoUnpauseOnEnterPlaymode = true;

#if UNITY_EDITOR
		private void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

		private void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			// MPPM workaround to prevent final scene change when exiting playmode, this may cause virtual players
			// to throw "This cannot be used during play mode" errors as they try to load a scene during shutdown
			// due to the delay of sending scene change messages to virtual clients
			if (m_AutoUnpauseOnEnterPlaymode && state == PlayModeStateChange.EnteredPlayMode)
			{
				if (EditorApplication.isPaused)
				{
					EditorApplication.isPaused = false;
					Debug.LogWarning($"{nameof(NetworkMppmConnect)}: EditorApplication.isPaused was true entering " +
					                 "playmode => auto unpaused to let virtual players run");
				}
			}
		}

		private void Start() => TryStartMultiplayerPlaymode();

		private async void TryStartMultiplayerPlaymode()
		{
			var playerTags = CurrentPlayer.ReadOnlyTags();
			var useRelay = IsEditorRelayEnabled();
			if (playerTags.Contains("Server"))
			{
				SceneAutoLoader.DestroyAll(); // server loads scene via NetworkSessionState
				DeleteRelayJoinCodeFile();
				DeleteEditorRelayEnabledFile();

				SetEditorRelayEnabled(useRelay);
				NetworkLog.LogInfo("Multiplayer Playmode => Start SERVER");

				await Network.StartServer();
				if (useRelay)
					WriteRelayJoinCodeFile();

				NetworkLog.LogInfo("Multiplayer Playmode => SERVER did start ...");
			}
			else if (playerTags.Contains("Host"))
			{
				SceneAutoLoader.DestroyAll(); // server loads scene via NetworkSessionState
				DeleteRelayJoinCodeFile();
				DeleteEditorRelayEnabledFile();

				SetEditorRelayEnabled(useRelay);
				NetworkLog.LogInfo("Multiplayer Playmode => Start HOST");

				await Network.StartHost();

				if (useRelay)
					WriteRelayJoinCodeFile();

				NetworkLog.LogInfo("Multiplayer Playmode => HOST did start ...");
			}
			else if (playerTags.Contains("Client"))
			{
				SceneAutoLoader.DestroyAll(); // clients auto-load scene when connected
				await Task.Delay(250); // ensure a virtual client never starts before the host

				Network.UseRelayService = await WaitForEditorRelayEnabledFile();
				if (Network.UseRelayService)
				{
					NetworkLog.LogInfo("Multiplayer Playmode => CLIENT waiting for relay " +
					                   $"join code in: {RelayJoinCodeFilePath}");
					var code = await WaitForRelayJoinCodeFile();
					Network.RelayJoinCode = code;

					NetworkLog.LogInfo($"Multiplayer Playmode => CLIENT got relay join code: {code}");
				}

				NetworkLog.LogInfo("Multiplayer Playmode => Start CLIENT");
				await Network.StartClient();

				NetworkLog.LogInfo("Multiplayer Playmode => CLIENT did connect ...");
			}

			TaskPerformed();
		}

		private void SetEditorRelayEnabled(Boolean enabled)
		{
			Network.UseRelayService = enabled;
			WriteEditorRelayEnabledFile(enabled);
			if (enabled)
				NetworkLog.LogInfo("Multiplayer Playmode => Using Relay service ...");
		}

		private static Boolean IsEditorRelayEnabled() => EditorPrefs.GetBool(k_UseEasyRelayIntegrationKey, false);

		private static String EditorRelayEnabledFilePath => $"{Application.persistentDataPath}/{TryRelayFile}";

		private async Task<Boolean> WaitForEditorRelayEnabledFile()
		{
			do
			{
				await Task.Delay(200);
			} while (EditorRelayEnabledFileExists() == false);

			return ReadEditorRelayEnabledFile();
		}

		private void WriteEditorRelayEnabledFile(Boolean useRelay)
		{
			try
			{
				var path = EditorRelayEnabledFilePath;
				File.WriteAllText(path, useRelay.ToString());
				// Debug.Log($"Wrote try-relay-in-editor {useRelay} to file: {path}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private Boolean ReadEditorRelayEnabledFile()
		{
			try
			{
				var path = EditorRelayEnabledFilePath;
				var useRelay = File.ReadAllText(path);
				// Debug.Log($"Read try-relay-in-editor {useRelay} from file: {path}");
				return useRelay.Equals(true.ToString());
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			return false;
		}

		private void DeleteEditorRelayEnabledFile()
		{
			try
			{
				var path = EditorRelayEnabledFilePath;
				if (EditorRelayEnabledFileExists())
				{
					File.Delete(path);
					// Debug.Log($"Deleted try-relay-in-editor exchange file: {path}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private static Boolean EditorRelayEnabledFileExists() => File.Exists(EditorRelayEnabledFilePath);

		private static String RelayJoinCodeFilePath => $"{Application.persistentDataPath}/{JoinCodeFile}";

		private async Task<String> WaitForRelayJoinCodeFile()
		{
			do
			{
				await Task.Delay(200);
			} while (RelayJoinCodeFileExists() == false);

			return ReadRelayJoinCodeFile();
		}

		private static void WriteRelayJoinCodeFile()
		{
			try
			{
				var path = RelayJoinCodeFilePath;
				File.WriteAllText(path, Network.RelayJoinCode);
				// Debug.Log($"Wrote join code {Network.RelayJoinCode} to file: {path}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private static String ReadRelayJoinCodeFile()
		{
			String joinCode = null;
			try
			{
				var path = RelayJoinCodeFilePath;
				joinCode = File.ReadAllText(path);
				// Debug.Log($"Read join code {joinCode} from file: {path}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
			return joinCode;
		}

		private static void DeleteRelayJoinCodeFile()
		{
			try
			{
				var path = RelayJoinCodeFilePath;
				if (File.Exists(path))
				{
					File.Delete(path);
					// Debug.Log($"Deleted join code exchange file: {path}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		private static Boolean RelayJoinCodeFileExists() => File.Exists(RelayJoinCodeFilePath);
#endif
	}
}
