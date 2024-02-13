// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;

namespace CodeSmile.Netcode
{
	/// <summary>
	///     Launching Server/Host/Client with optional Relay allocation.
	///     Set the static public properties before calling the Server/Host/Client methods.
	/// </summary>
	public static class NetworkStart
	{
		/// <summary>
		///     Set to true before starting to use Unity's Relay service for connections.
		/// </summary>
		public static Boolean UseRelayService { get; set; }

		/// <summary>
		///     Clients must assign their join code before starting. Server/Host assign the join code after starting, which
		///     should be provided to the user in some way (eg shown in a GUI label or copied to clipboard).
		/// </summary>
		public static String RelayJoinCode { get; set; }

		/// <summary>
		///     Defaults to 'dtls'.
		/// </summary>
		public static String RelayConnectionType { get; set; }

		/// <summary>
		///     Maximum connections accepted by the relay service.
		/// </summary>
		/// <remarks>
		///     As of February 2024 the limit is 100 connections. See: https://docs.unity.com/ugs/manual/relay/manual/limitations
		/// </remarks>
		public static Int32 RelayMaxConnections { get; set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStaticFields()
		{
			UseRelayService = false;
			RelayJoinCode = "";
			RelayConnectionType = "dtls";
			RelayMaxConnections = 4;
		}


		/// <summary>
		///     Start a network session as Server. Set UseRelayService before calling this to enable relay.
		/// </summary>
		public static async Task Server()
		{
			if (UseRelayService)
				RelayJoinCode = await AcquireRelayJoinCode(RelayMaxConnections, RelayConnectionType);

			NetworkManager.Singleton.StartServer();
		}

		/// <summary>
		///     Start a network session as Host. Set UseRelayService before calling this to enable relay.
		/// </summary>
		public static async Task Host()
		{
			if (UseRelayService)
				RelayJoinCode = await AcquireRelayJoinCode(RelayMaxConnections, RelayConnectionType);

			NetworkManager.Singleton.StartHost();
		}

		/// <summary>
		///     Start a network session as Client. Set UseRelayService and RelayJoinCode before calling this to enable relay.
		/// </summary>
		public static async Task Client()
		{
			if (UseRelayService)
				await JoinWithRelayCode(RelayJoinCode);

			NetworkManager.Singleton.StartClient();
		}

		/// <summary>
		///     Authenticates the player with Unity's AuthenticationService anonymously. Automatically called when
		///     starting with relay service enabled.
		/// </summary>
		public static async Task AuthenticateAnonymously()
		{
			await UnityServices.InitializeAsync();

			if (AuthenticationService.Instance.IsSignedIn == false)
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}

		private static async Task<String> AcquireRelayJoinCode(Int32 maxConnections, String connectionType)
		{
			await AuthenticateAnonymously();

			var createAlloc = await RelayService.Instance.CreateAllocationAsync(maxConnections);
			GetTransport().SetRelayServerData(new RelayServerData(createAlloc, connectionType));
			var joinCode = await RelayService.Instance.GetJoinCodeAsync(createAlloc.AllocationId);

			Debug.Log($"Relay join code is: {joinCode}");
			return joinCode;
		}

		private static async Task JoinWithRelayCode(String joinCode)
		{
			await AuthenticateAnonymously();

			Debug.Log($"Joining Relay with code: {joinCode}");
			var joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
			GetTransport().SetRelayServerData(new RelayServerData(joinAlloc, RelayConnectionType));
		}

		private static UnityTransport GetTransport() => NetworkManager.Singleton.GetComponent<UnityTransport>();
	}
}
