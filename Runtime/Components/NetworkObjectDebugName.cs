// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	[DisallowMultipleComponent]
	public class NetworkObjectDebugName : NetworkBehaviour
	{
		[Tooltip("If true, replaces 'Network' prefix with 'Local' or 'Remote' to clearly mark ownership. Does nothing" +
		         "if the object isn't prefixed accordingly.")]
		[SerializeField] private Boolean m_ReplaceNetworkPrefix;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			SetDebugName();
		}

		protected override void OnOwnershipChanged(UInt64 previous, UInt64 current) => SetDebugName();

		protected virtual void SetDebugName()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			name = GetDebugName(name);
#endif
		}

		protected virtual string GetDebugName(string originalName)
		{
			var objName = originalName.Replace("(Clone)", "");

			// the extra characters are meant to keep the original string length
			if (m_ReplaceNetworkPrefix && objName.StartsWith("Network"))
				objName = $"{(IsOwner ? ">Local " : "<Remote")}{objName.Substring("Network".Length)}";

			var owner = IsOwner ? "(OWNER)" : "";
			return $"[{OwnerClientId}|{NetworkObjectId}] {objName} {owner}";
		}
	}
}
