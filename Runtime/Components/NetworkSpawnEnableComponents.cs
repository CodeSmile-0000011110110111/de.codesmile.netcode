// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	/// <summary>
	///     Enables components or game objects based on whether the network object is local or remote owned.
	/// </summary>
	public class NetworkSpawnEnableComponents : NetworkOneTimeTaskBehaviour
	{
		[Tooltip("Specify components to enable on spawn, in this order, if the object is locally owned. ")]
		[SerializeField] private Component[] m_EnableIfLocalOwner;

		[Tooltip("Specify components to enable on spawn, in this order, if the object is remotely owned. ")]
		[SerializeField] private Component[] m_EnableIfRemoteOwner;

		private void Start()
		{
			// always enable when not networked
			if (NetworkManager == null || NetworkManager.IsListening == false)
				EnableComponents(m_EnableIfLocalOwner);
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			EnableComponents(IsLocalPlayer ? m_EnableIfLocalOwner : m_EnableIfRemoteOwner);

			TaskPerformed();
		}

		private void EnableComponents(Component[] components)
		{
			if (components == null)
				return;

			foreach (var component in components)
			{
				if (component != null)
				{
					Debug.Log($"NetworkSpawn enabling component: {component.GetType().Name}");

					if (component is MonoBehaviour mb)
						mb.enabled = true;
					else if (component is Collider cc)
						cc.enabled = true;
					else
						throw new ArgumentException($"unhandled type {component.GetType()}");
				}
			}
		}
	}
}
