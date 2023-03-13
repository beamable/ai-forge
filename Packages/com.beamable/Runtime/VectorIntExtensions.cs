using System;
using UnityEngine;

namespace Beamable.Server
{
	[Serializable]
	public struct Vector2IntEx
	{
		public int x;
		public int y;

		public Vector2IntEx(Vector2Int vec)
		{
			x = vec.x;
			y = vec.y;
		}

		public static Vector2Int DeserializeToVector2(string json)
		{
			Vector2IntEx tmp = JsonUtility.FromJson<Vector2IntEx>(json);
			return new Vector2Int(tmp.x, tmp.y);
		}
	}

	[Serializable]
	public struct Vector3IntEx
	{
		public int x;
		public int y;
		public int z;

		public Vector3IntEx(Vector3Int vec)
		{
			x = vec.x;
			y = vec.y;
			z = vec.z;
		}

		public static Vector3Int DeserializeToVector3(string json)
		{
			Vector3IntEx tmp = JsonUtility.FromJson<Vector3IntEx>(json);
			return new Vector3Int(tmp.x, tmp.y, tmp.z);
		}
	}
}
