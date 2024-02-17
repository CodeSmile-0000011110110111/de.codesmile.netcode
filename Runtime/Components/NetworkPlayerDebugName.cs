// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Netcode;
using UnityEngine;

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
			Debug.Log($"Player spawn: {name}");
		}

		public override void OnNetworkDespawn()
		{
			Debug.Log($"Player despawn: {name}");
			base.OnNetworkDespawn();
		}

		private void SetPlayerDebugName()
		{
			var local = IsOwner ? " LOCAL" : " remote";
			var ownerId = $"Owner: {OwnerClientId}";
			name = $"{name} ({ownerId}){local}".Replace("(Clone)", "");
		}
	}
}
