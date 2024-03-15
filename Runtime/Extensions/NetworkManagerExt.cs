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
		///     Usage: call this in either the Awake or OnEnable method. In Start, NetworkManager.Singleton is already non-null.
		///		The callback need only be used to subscribe to NetworkManager events that may be raised instantly after
		///		StartServer, StartHost or StartClient are called from a component's OnEnable method.
		///		This mainly concerns the OnServerStarted and OnClientStarted events.
		///     By using this event handler you do not need to put scripts in the Script Execution Order nor worry about
		///		component execution order shifting since the order of component execution is not guaranteed.
		/// </example>
		/// <remarks>
		///     The callback action will be invoked directly in case NetworkManager.Singleton is already non-null.
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
