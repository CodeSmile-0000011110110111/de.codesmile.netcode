// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	[RequireComponent(typeof(NetworkObject))]
	[DisallowMultipleComponent]
	public class NetworkObjectVisibility : NetworkBehaviour
	{
		[SerializeField] private Visibility m_Visibility;

		private void Awake()
		{
			var netObject = GetComponent<NetworkObject>();
			netObject.CheckObjectVisibility = clientId => m_Visibility switch
			{
				Visibility.AllClients => true,
				Visibility.NonOwnerClients => clientId != OwnerClientId,
				Visibility.OnlyOwnerClient => clientId == OwnerClientId,
				Visibility.NoClients => false,
				_ => throw new ArgumentOutOfRangeException(nameof(m_Visibility), m_Visibility.ToString()),
			};

			enabled = false;
		}

		private enum Visibility
		{
			AllClients,
			NoClients,
			NonOwnerClients,
			OnlyOwnerClient,
		}
	}
}
