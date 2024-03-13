// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace CodeSmile.Netcode.Extensions
{
	public static class NetworkManagerExt
	{
		private static List<Action> s_SingletonReadyCallbacks;

		public static Boolean IsServerOrHost(this NetworkManager netMan) => netMan.IsServer || netMan.IsHost;

		public static UnityTransport GetTransport(this NetworkManager netMan) => netMan.GetComponent<UnityTransport>();

		/// <summary>
		///     Invokes the callback if or when the NetworkManager.Singleton has been assigned. Relies on the NetworkManager's
		///     internal event Action OnSingletonReady to be made public by editing the NetworkManager.cs (v1.8+).
		/// </summary>
		/// <example>
		///     Usage: call this in either the Awake or OnEnable method. The callback should only be used to subscribe to
		///     NetworkManager events raised early such as OnServerStarted or OnClientStarted which you may possibly miss
		///     depending on the event execution order.
		/// </example>
		/// <remarks>
		///     The callback action will be invoked either right away in case NetworkManager.Singleton is already non-null,
		///     otherwise it will be called the moment it is assigned via NetworkManager's OnEnable method.
		///     By using this method you do not need to rely on putting scripts in the Script Execution Order.
		/// </remarks>
		/// <remarks>
		///     Issue request to make OnSingletonReady public:
		///     https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/2386
		/// </remarks>
		/// <param name="callback">The action to be invoked when NetworkManager.Singleton has been assigned.</param>
		public static void InvokeWhenSingletonReady(Action callback)
		{
			if (NetworkManager.Singleton != null)
			{
				callback?.Invoke();
				return;
			}

			s_SingletonReadyCallbacks ??= new List<Action>();
			s_SingletonReadyCallbacks.Add(callback);
			NetworkManager.OnSingletonReady -= InvokeSingletonReadyCallbacks;
			NetworkManager.OnSingletonReady += InvokeSingletonReadyCallbacks;
		}

		private static void InvokeSingletonReadyCallbacks()
		{
			NetworkManager.OnSingletonReady -= InvokeSingletonReadyCallbacks;

			foreach (var callback in s_SingletonReadyCallbacks)
				callback?.Invoke();

			s_SingletonReadyCallbacks = null;
		}
	}
}
