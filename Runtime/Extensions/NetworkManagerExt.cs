// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeSmile.Netcode.Extensions
{
	public static class NetworkManagerExt
	{
		private static List<Action> OnSingletonReadyCallbacks;

		/// <summary>
		///     Shorthand for calling: NetworkManager.Singleton.GetComponent<UnityTransport>()
		/// </summary>
		/// <param name="netMan"></param>
		/// <returns>The UnityTransport component.</returns>
		public static UnityTransport GetTransport(this NetworkManager netMan) => netMan.GetComponent<UnityTransport>();

		/// <summary>
		///     Let's you subscribe to NetworkManager's internal OnSingletonReady event.
		///     Will continue to work even if OnSingletonReady may become public in the future.
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
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			// singleton already initialized? => direct Invoke
			if (NetworkManager.Singleton != null)
			{
				callback.Invoke();
				return;
			}

			// one-time init of list and singleton ready event
			if (OnSingletonReadyCallbacks == null)
			{
				OnSingletonReadyCallbacks = new List<Action>();
				Subscribe(GetSingletonReadyEvent(), InvokeSingletonReadyCallbacksOnce);
			}

			// put it in the queue to be called later
			OnSingletonReadyCallbacks.Add(callback);
		}

		private static void InvokeSingletonReadyCallbacksOnce()
		{
			Unsubscribe(GetSingletonReadyEvent(), InvokeSingletonReadyCallbacksOnce);

			foreach (var callback in OnSingletonReadyCallbacks)
				callback.Invoke();

			OnSingletonReadyCallbacks = null;
		}

		private static void Subscribe(EventInfo readyEvent, Action eventHandler) =>
			readyEvent.GetAddMethod(true).Invoke(readyEvent, new Object[] { eventHandler });

		private static void Unsubscribe(EventInfo readyEvent, Action eventHandler) =>
			readyEvent.GetRemoveMethod(true).Invoke(readyEvent, new Object[] { eventHandler });

		private static EventInfo GetSingletonReadyEvent()
		{
			const String ReadyEventName = "OnSingletonReady";

			var readyEvent = typeof(NetworkManager).GetEvent(ReadyEventName, BindingFlags.Static | BindingFlags.NonPublic);

			// try to get the public version, because this may be made public in the future (was internal in Netcode 1.8.1)
			if (readyEvent == null)
				readyEvent = typeof(NetworkManager).GetEvent(ReadyEventName, BindingFlags.Static | BindingFlags.Public);

			if (readyEvent == null)
			{
				throw new MissingMemberException("NetworkManager does not have the 'OnSingletonReady' event. " +
				                                 "This may indicate that an unsupported Netcode package version is installed.");
			}

			return readyEvent;
		}

#if UNITY_EDITOR
		[InitializeOnLoadMethod] private static void ResetStaticFields() => OnSingletonReadyCallbacks = null;
#endif
	}
}
