// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;
using System;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Multiplayer.Playmode;
using UnityEditor;
#endif

namespace CodeSmile.Netcode.Components
{
	public class NetworkMppmConnect : OneTimeTaskBehaviour
	{
		private const String JoinCodeFile = "RelayJoinCode.txt";
		private const String TryRelayFile = "TryRelayInEditor.txt";

		// copied from NetworkManagerEditor
		private static readonly String k_UseEasyRelayIntegrationKey =
			"NetworkManagerUI_UseRelay_" + Application.dataPath.GetHashCode();

#if UNITY_EDITOR
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
				Debug.Log("Multiplayer Playmode => Start SERVER");

				await Network.StartServer();
				if (useRelay)
					WriteRelayJoinCodeFile();

				Debug.Log("Multiplayer Playmode => SERVER is running ...");
			}
			else if (playerTags.Contains("Host"))
			{
				SceneAutoLoader.DestroyAll(); // server loads scene via NetworkSessionState
				DeleteRelayJoinCodeFile();
				DeleteEditorRelayEnabledFile();

				SetEditorRelayEnabled(useRelay);
				Debug.Log("Multiplayer Playmode => Start HOST");

				await Network.StartHost();
				if (useRelay)
					WriteRelayJoinCodeFile();

				Debug.Log("Multiplayer Playmode => HOST is running ...");
			}
			else if (playerTags.Contains("Client"))
			{
				SceneAutoLoader.DestroyAll(); // clients auto-load scene when connected
				await Task.Delay(500); // ensure a client never starts before the host

				Network.UseRelayService = await WaitForEditorRelayEnabledFile();
				if (Network.UseRelayService)
				{
					Debug.Log("Multiplayer Playmode => CLIENT waiting for relay " +
					          $"join code in: {RelayJoinCodeFilePath}");
					var code = await WaitForRelayJoinCodeFile();
					Network.RelayJoinCode = code;

					Debug.Log($"Multiplayer Playmode => CLIENT got relay join code: {code}");
				}

				Debug.Log("Multiplayer Playmode => Start CLIENT");
				await Network.StartClient();

				Debug.Log("Multiplayer Playmode => CLIENT connected ...");
			}

			TaskPerformed();
		}

		private void SetEditorRelayEnabled(Boolean enabled)
		{
			Network.UseRelayService = enabled;
			WriteEditorRelayEnabledFile(enabled);
			if (enabled)
				Debug.Log("Multiplayer Playmode => Using Relay service ...");
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
				Debug.Log($"Wrote try-relay-in-editor {useRelay} to file: {path}");
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
				Debug.Log($"Read try-relay-in-editor {useRelay} from file: {path}");
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
					Debug.Log($"Deleted try-relay-in-editor exchange file: {path}");
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
				Debug.Log($"Wrote join code {Network.RelayJoinCode} to file: {path}");
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
				Debug.Log($"Read join code {joinCode} from file: {path}");
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
					Debug.Log($"Deleted join code exchange file: {path}");
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
