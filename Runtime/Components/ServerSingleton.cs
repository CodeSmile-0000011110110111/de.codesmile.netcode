// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Singleton NetworkBehaviour that must only spawn on the server. Throws an exception if this component is spawned
	///     on a client (other than host).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DisallowMultipleComponent]
	public abstract class ServerSingleton<T> : NetworkBehaviour where T : ServerSingleton<T>
	{
		private static T s_Instance;
		public static T Singleton => s_Instance;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsServer == false)
				throw new InvalidOperationException("ServerSingleton must only spawn on Server");

			s_Instance = this as T;
		}

		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();

			s_Instance = null;
		}
	}
}
