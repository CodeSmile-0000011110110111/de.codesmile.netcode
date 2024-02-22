// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;

namespace CodeSmile.Netcode
{
	public abstract class ServerSingleton<T> : NetworkBehaviour where T: ServerSingleton<T>
	{
		private static T s_Instance;
		public static T Singleton => s_Instance;

		protected virtual void Awake()
		{
			if (s_Instance != null)
				throw new InvalidOperationException($"{GetType()} instance already exists!");

			s_Instance = this as T;

		}

		public override void OnDestroy() => s_Instance = null;

	}
}
