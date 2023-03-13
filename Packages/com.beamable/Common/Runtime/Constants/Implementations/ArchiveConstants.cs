using UnityEngine;

namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class Archive
			{
				public const string ARCHIVE_WINDOW_HEADER = "Archive";
				public const string ARCHIVE_WINDOW_TEXT = "If you archive this service, then it will be disabled the next time you deploy. " +
														  "You may delete the files associated with this service. " +
														  "Before you delete the files, please make sure you have a backup in case you need to restore the service later. " +
														  "If this service has never been deployed, once it is archived and deployed, it won’t be recoverable.";
				public const string DELETE_ALL_FILES_TEXT = "Delete All Files";
				public static readonly Vector2 ARCHIVE_WINDOW_SIZE = new Vector2(470, 250);

			}
		}
	}
}
