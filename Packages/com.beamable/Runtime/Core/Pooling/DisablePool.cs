using Beamable.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Pooling
{
	public class DisablePool : MonoBehaviour
	{
		public readonly Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();

		#region Spawn
		public GameObject Spawn(GameObject prefab)
		{
			List<GameObject> pool;
			if (pools.TryGetValue(prefab, out pool))
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (!pool[i].activeSelf)
					{
						pool[i].gameObject.SetActive(true);
						return pool[i];
					}
				}

				var newInstance = Object.Instantiate(prefab);
				pool.Add(newInstance);
				return newInstance;
			}
			else
			{
				pool = new List<GameObject>();
				var newInstance = Object.Instantiate(prefab);
				pool.Add(newInstance);

				pools.Add(prefab, pool);

				return newInstance;
			}
		}

		public GameObject SpawnOnParent(GameObject prefab, Transform parent)
		{
			var instance = Spawn(prefab);
			instance.transform.SetParent(parent, false);
			return instance;
		}

		public GameObject SpawnAtPosition(GameObject prefab, Vector3 position)
		{
			var instance = Spawn(prefab);
			instance.transform.position = position;
			return instance;
		}

		//LocalPosition is an optimization of position because no conversion to/from world space has to happen.
		//if you can use this one its better...
		public GameObject SpawnAtLocalPosition(GameObject prefab, Vector3 position)
		{
			var instance = Spawn(prefab);
			instance.transform.localPosition = position;
			return instance;
		}
		#endregion

		#region SpawnByType
		public T Spawn<T>(T prefab) where T : MonoBehaviour
		{
			return Spawn(prefab.gameObject).GetComponent<T>();
		}

		public T SpawnOnParent<T>(T prefab, Transform parent) where T : MonoBehaviour
		{
			return SpawnOnParent(prefab.gameObject, parent).GetComponent<T>();
		}

		public T SpawnAtPosition<T>(T prefab, Vector3 position) where T : MonoBehaviour
		{
			return SpawnAtPosition(prefab.gameObject, position).GetComponent<T>();
		}

		public T SpawnAtLocalPosition<T>(T prefab, Vector3 position) where T : MonoBehaviour
		{
			return SpawnAtLocalPosition(prefab.gameObject, position).GetComponent<T>();
		}
		#endregion

		#region Recycle
		public virtual void RecycleAllChildren(Transform t)
		{
			if (t != null)
			{
				t.SetActiveAllChildren(false);
			}
		}

		public virtual void Recycle(GameObject instance)
		{
			instance.SetActive(false);
		}
		#endregion
	}

}
