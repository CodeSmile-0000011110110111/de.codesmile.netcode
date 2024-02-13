// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;

namespace CodeSmile.Netcode.Components
{
	public class NetworkPlayerDebugName : NetworkBehaviour
	{
		private void Awake()
		{
#if !DEBUG
			Destroy(this);
#endif
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			SetPlayerDebugName();
			UnityEngine.Debug.Log($"Player spawn: {name}");
		}

		public override void OnNetworkDespawn()
		{
			UnityEngine.Debug.Log($"Player despawn: {name}");
			base.OnNetworkDespawn();
		}

		private void SetPlayerDebugName()
		{
			var local = IsLocalPlayer ? "(LOCAL)" : "";
			var clientId = $"ClId: {NetworkManager.LocalClientId}";
			var serverId = $"SvId: {NetworkManager.ServerClientId}";
			var ownerId = $"OwId: {OwnerClientId}";
			name = $"{name} ({clientId}, {serverId}, {ownerId}) {local}".Replace("(Clone)", "");
		}
	}
}
