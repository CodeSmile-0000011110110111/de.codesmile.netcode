// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeSmile.Netcode.Components
{
	public class NetworkOneTimeTaskBehaviour : NetworkBehaviour
	{
		[SerializeField]
		private OnTaskPerformed m_OnTaskPerformed = OnTaskPerformed.DestroyComponent;

		protected void TaskPerformed()
		{
			switch (m_OnTaskPerformed)
			{
				case OnTaskPerformed.DestroyComponent:
					Destroy(this);
					break;
				case OnTaskPerformed.DestroyGameObject:
					Destroy(gameObject);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(m_OnTaskPerformed));
			}
		}

		private enum OnTaskPerformed
		{
			DestroyComponent,
			DestroyGameObject,
		}
	}

}
