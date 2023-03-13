using Beamable.Extensions;
using System.Collections.Generic;
using UnityEngine;

// Because moving objects outside of the camera is like 100x faster than activating and deactivating them.
// originally used transform hierarchy to track the lists (no GC alloc), but SetParent is expensive, so
// we use intrusive linked lists to avoid allocation and set parent..
namespace Beamable.Pooling
{
	public class HidePool : MonoBehaviour
	{
		private static readonly Vector3 offscreenPosition = new Vector3(-1000, 0, -1000);
		private readonly Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

#if UNITY_EDITOR
      List<HidePoolObject> _tempList = new List<HidePoolObject>();
#endif


		public class Pool
		{
			public GameObject prefab;
			public Transform root;
			private readonly LinkedList<HidePoolObject> free = new LinkedList<HidePoolObject>();

#if UNITY_EDITOR
         public int highWaterMark;
         public int currentlyFree;
         public int reused;
#endif

			public HidePoolObject Spawn()
			{
				if (free.Count > 0)
				{
					HidePoolObject node = free.First.Value;
					free.RemoveFirst();
#if UNITY_EDITOR
               currentlyFree--;
               reused++;
#endif
					return node;
				}

				return NewInstance();
			}

			public void Recycle(HidePoolObject hpo)
			{
				free.AddFirst(hpo.pool);
#if UNITY_EDITOR
            currentlyFree++;
#endif
			}

			private HidePoolObject NewInstance()
			{
				GameObject ret = GameObject.Instantiate<GameObject>(prefab);
#if UNITY_EDITOR
            highWaterMark++;
#endif
				ret.transform.SetParent(root);
				var hpo = ret.FindOrCreate<HidePoolObject>();
				if (hpo.pool == null)
				{
					hpo.pool = new LinkedListNode<HidePoolObject>(hpo);
				}
				hpo.owner = this;
				return hpo;
			}

			public void PreSpawn(int count)
			{
				for (int i = 0; i < count; ++i)
				{
					HidePoolObject hpo = NewInstance();
					hpo.transform.position = offscreenPosition;
					free.AddFirst(hpo.pool);
#if UNITY_EDITOR
               currentlyFree++;
#endif
				}
			}
		}

		private Pool FindOrCreatePool(GameObject prefab)
		{
			Pool p = null;
			if (!_pools.TryGetValue(prefab, out p))
			{
				p = new Pool();
				p.prefab = prefab;
				p.root = new GameObject(prefab.name).transform;
				p.root.SetParent(transform);
				_pools.Add(prefab, p);
			}
			return p;

		}

		public GameObject Spawn(GameObject prefab)
		{
			return Spawn(prefab, null);
		}

		public GameObject Spawn(GameObject prefab, LinkedList<HidePoolObject> owner)
		{
			Pool p = FindOrCreatePool(prefab);
			HidePoolObject hpo = p.Spawn();
			if (owner != null)
			{
				owner.AddFirst(hpo.pool);
			}
			return hpo.gameObject;
		}

		public GameObject SpawnAt(GameObject prefab, Vector3 worldPos)
		{
			GameObject go = Spawn(prefab, null);
			go.transform.position = worldPos;
			return go;
		}

		private GameObject SpawnAt(GameObject prefab, Vector3 worldPos, LinkedList<HidePoolObject> owner)
		{
			Pool p = FindOrCreatePool(prefab);
			HidePoolObject hpo = p.Spawn();
			if (owner != null)
			{
				owner.AddFirst(hpo.pool);
			}
			hpo.transform.position = worldPos;
			return hpo.gameObject;
		}

		public GameObject SpawnAttached(GameObject prefab, Vector3 worldPos, Transform parent)
		{
			GameObject go = SpawnAt(prefab, worldPos);
			Transform t = go.transform;
			t.SetParent(parent);
			t.localRotation = Quaternion.identity;
			return go;
		}

		public GameObject SpawnAttached(GameObject prefab, Vector3 worldPos, Transform parent, LinkedList<HidePoolObject> owner)
		{
			GameObject go = SpawnAt(prefab, worldPos, owner);
			Transform t = go.transform;
			t.SetParent(parent);
			t.localRotation = Quaternion.identity;
			return go;
		}

		public void Preallocate(GameObject prefab, int count)
		{
			Pool p = FindOrCreatePool(prefab);
			p.PreSpawn(count);
		}

		public void Recycle(GameObject instance)
		{
			Recycle(instance, null);
		}

		public void Recycle(GameObject instance, LinkedList<HidePoolObject> owner)
		{
			HidePoolObject hpo = null;

#if UNITY_EDITOR
         // use list accessor to get around annoying editor only GetComponent GC alloc..
         instance.GetComponents<HidePoolObject>(_tempList);
         if (_tempList.Count > 0)
         {
            hpo = _tempList[0];
            Recycle(hpo, owner);
         }
#else
			hpo = instance.GetComponent<HidePoolObject>();
			Recycle(hpo, owner);
#endif
		}

		private void Recycle(HidePoolObject hpo, LinkedList<HidePoolObject> owner)
		{
			if (hpo.transform.parent != hpo.owner.root)
				hpo.transform.parent = hpo.owner.root;

			hpo.transform.position = offscreenPosition;
			if (owner != null)
			{
				owner.Remove(hpo.pool);
			}
			hpo.owner.Recycle(hpo);
		}

#if UNITY_EDITOR
      public Dictionary<GameObject, Pool> GetPools()
      {
         return _pools;
      }
#endif
	}
}
