#if UNITY_2021_2_OR_NEWER
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Utils
{
	public class BasicPoolSystem<T> : MonoBehaviour where T : PoolableBehaviour
	{
		public IObjectPool<T> Pool => pool;

		[SerializeField] private T prefab;
		[SerializeField] private int baseSize = 10;
		[SerializeField] private int maxSize = 50;
		[SerializeField] private Transform parent;
		private IObjectPool<T> pool;

		public void SetTransformParent(Transform newParent) => parent = newParent;
		public void SetPrefab(T newPrefab)
		{
			prefab = newPrefab;
			pool = new ObjectPool<T>(createFunc: CreatePooled, actionOnGet: GetPooled, actionOnRelease: ReleasePooled, actionOnDestroy: DestroyPooled, collectionCheck: true, defaultCapacity: baseSize, maxSize: maxSize);
		}
		
		private void Awake()
		{
			if(prefab != null)
			{
				SetPrefab(prefab);
			}
		}

		private void DestroyPooled(T obj)
		{
			Destroy(obj.gameObject);
		}

		private void ReleasePooled(T obj)
		{
			obj.gameObject.SetActive(false);
		}

		private void GetPooled(T obj)
		{
			obj.gameObject.SetActive(true);
			obj.ResetValues();
		}

		private T CreatePooled()
		{
			var item = parent == null ? Instantiate(prefab) : Instantiate(prefab, parent);
			item.ReleaseAction = () => pool.Release(item);
			item.ResetValues();

			return item;
		}
	}
}
#endif
