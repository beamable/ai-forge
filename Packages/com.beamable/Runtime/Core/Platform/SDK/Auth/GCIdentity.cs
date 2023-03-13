using System.Runtime.InteropServices;

namespace GCIdentityPlugin
{
	public class GCIdentity
	{
#if UNITY_IOS && !UNITY_EDITOR
      [DllImport("__Internal")]
      private static extern void _GenerateIdentity(string gameObjectName);
#else
		private static void _GenerateIdentity(string gameObjectName) { }
#endif

		public static void GenerateIdentity(string gameObjectName)
		{
			_GenerateIdentity(gameObjectName);
		}

		public static string[] ParseIdentity(string identity)
		{
			return identity.Split(';');
		}
	}
}
