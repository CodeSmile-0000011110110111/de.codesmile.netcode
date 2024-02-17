// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Disables components or game objects based on whether the network object is local or remote owned.
	/// </summary>
	public class NetworkSpawnDisableComponents : NetworkBehaviour
	{
		[SerializeField] private Boolean m_DisableMeansDestroy;

		[Tooltip("Specify components to disable on spawn, in this order, if the object is locally owned. ")]
		[SerializeField] private Component[] m_DisableIfLocalOwner;

		[Tooltip("Specify components to disable on spawn, in this order, if the object is remotely owned. ")]
		[SerializeField] private Component[] m_DisableIfRemoteOwner;

		public void Start()
		{
			// if not networked, assume it's the local owner
			if (NetworkManager == null || NetworkManager.IsListening == false)
				DisableOrDestroyComponents(m_DisableIfLocalOwner);
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			DisableOrDestroyComponents(IsOwner ? m_DisableIfLocalOwner : m_DisableIfRemoteOwner);

			Destroy(this);
		}

		private void DisableOrDestroyComponents(Component[] components)
		{
			if (components == null)
				return;

			foreach (var component in components)
			{
				if (component != null)
				{
					// Debug.Log($"NetworkSpawn disabling component: {component.GetType().Name}");

					if (component is MonoBehaviour mb)
						mb.enabled = false;
					else if (component is Collider cc)
						cc.enabled = false;
					else
						throw new ArgumentException($"unhandled type {component.GetType()}");

					if (m_DisableMeansDestroy)
						Destroy(component);
				}
			}
		}
	}
}
