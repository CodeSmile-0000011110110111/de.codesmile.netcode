// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace CodeSmile.Netcode.Extensions
{
	public static class NetworkManagerExt
	{
		public static Boolean IsServerOrHost(this NetworkManager netMan) => netMan.IsServer || netMan.IsHost;

		public static UnityTransport GetTransport(this NetworkManager netMan) => netMan.GetComponent<UnityTransport>();
	}
}
