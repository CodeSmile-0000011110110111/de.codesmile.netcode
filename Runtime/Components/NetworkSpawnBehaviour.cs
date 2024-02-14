// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	internal enum SpawnTask
	{
		DoNothing,
		SetActive,
		SetInactive,
		Destroy,
	}

	public class NetworkSpawnBehaviour : NetworkOneTimeTaskBehaviour
	{
		[SerializeField] private SpawnTask m_LocalOwnerTask;
		[SerializeField] private SpawnTask m_RemoteOwnerTask;

		public override void OnNetworkSpawn()
		{
			PerformSpawnTask(IsOwner ? m_LocalOwnerTask : m_RemoteOwnerTask);
			TaskPerformed();
		}

		private void PerformSpawnTask(SpawnTask task)
		{
			switch (task)
			{
				case SpawnTask.DoNothing:
					break;
				case SpawnTask.SetActive:
					gameObject.SetActive(true);
					break;
				case SpawnTask.SetInactive:
					gameObject.SetActive(false);
					break;
				case SpawnTask.Destroy:
					Destroy(gameObject);
					break;
				default:
					throw new ArgumentOutOfRangeException(task.ToString());
			}
		}
	}
}
