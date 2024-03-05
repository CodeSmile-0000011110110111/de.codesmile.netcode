// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	public class NetworkDespawnTimer : NetworkBehaviour
	{
		[SerializeField] private Single m_SecondsTillDespawn = 3f;

		private Single m_TargetTime;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsOwner)
				StartCoroutine(DespawnWhenTimeOut());
		}

		private IEnumerator DespawnWhenTimeOut()
		{
			m_TargetTime = Time.time + m_SecondsTillDespawn;

			yield return new WaitUntil(() => Time.time > m_TargetTime);

			GetComponent<NetworkObject>().Despawn();
		}
	}
}
