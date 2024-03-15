// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	[DisallowMultipleComponent]
	public abstract class ServerSingleton<T> : NetworkBehaviour where T : ServerSingleton<T>
	{
		private static T s_Instance;
		public static T Singleton => s_Instance;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsServer == false)
				throw new InvalidOperationException("ServerSingleton should only spawn on Server");

			s_Instance = this as T;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			s_Instance = null;
		}
	}
}
