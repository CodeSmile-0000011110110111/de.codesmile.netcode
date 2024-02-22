// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;

namespace CodeSmile.Netcode.Components
{
	public class NetworkObjectDebugName : NetworkBehaviour
	{
		protected String m_Name;

		private void Awake()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			m_Name = name.Replace("(Clone)", "");
#else
			enabled = false;
			Destroy(this);
#endif
		}

		public override void OnNetworkSpawn() => SetDebugName();

		protected override void OnOwnershipChanged(UInt64 previous, UInt64 current) => SetDebugName();

		protected virtual void SetDebugName()
		{
			var owner = IsOwner ? "(OWNER)" : "";
			name = $"[{OwnerClientId}|{NetworkObjectId}] {m_Name} {owner}";
		}
	}
}
