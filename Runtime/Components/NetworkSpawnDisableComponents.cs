// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	public class NetworkSpawnDisableComponents : NetworkBehaviour
	{
		[SerializeField] private Boolean m_DisableMeansDestroy;

		[Tooltip("Specify components to disable on spawn, in this order, if the object is locally owned. ")]
		[SerializeField] private Component[] m_IfLocallyOwned;

		[Tooltip("Specify components to disable on spawn, in this order, if the object is remotely owned. ")]
		[SerializeField] private Component[] m_IfRemotelyOwned;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			DisableOrDestroyComponents(IsLocalPlayer ? m_IfLocallyOwned : m_IfRemotelyOwned);

			Destroy(this);
		}

		private void DisableOrDestroyComponents(Component[] components)
		{
			foreach (var component in components)
			{
				if (component != null)
				{
					if (m_DisableMeansDestroy)
						Destroy(component);
					else
					{
						if (component is MonoBehaviour mb)
							mb.enabled = false;
						else if (component is Collider cc)
							cc.enabled = false;
						else
							throw new ArgumentException($"unhandled type {component.GetType()}");
					}
				}
			}
		}
	}
}
