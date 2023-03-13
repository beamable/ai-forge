using System;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif


namespace Beamable.Common
{
	public static partial class Constants
	{
		public static partial class Features
		{
			public static partial class ContentManager
			{
				public const string BASE_PATH = Directories.BEAMABLE_PACKAGE_EDITOR_UI + "/Content";
				public const string COMPONENTS_PATH = BASE_PATH + "/Components";

				public const string DEFAULT_MANIFEST_ID = "global";

				public static Vector2 ConfirmationPopupSize = new Vector2(300, 120);
				public static Vector2 WindowSizeMinimum = new Vector2(400, 300);

				// Display Text
				public const string BREADCRUMBS_ALL_CONTENT_TEXT = "All Content";
				public const string BREADCRUMBS_COUNT_TEXT = " {0}/{1}";
				public const string BREADCRUMB_TOKEN_ARROW = ">";

				public const string CONTENT_TYPE_VIEW_HEADER_TEXT = "Content group/content";

				public static Vector2 CreateNewPopupWindowSize = new Vector2(160, 30);
				public const string CREATE_NEW_POPUP_WINDOW_TITLE = "Create New";
				public const string CREATE_NEW_POPUP_ADD_BUTTON_DISABLED_TEXT = "New Item";
				public const string CREATE_NEW_POPUP_ADD_BUTTON_ENABLED_TEXT = "Create: {0}";

				// Confirmation window content
				public const string CONFIRM_WINDOW_HEADER = "Confirmation";
				public const string CONFIRM_ITEM_DELETION = "Are You sure You want to delete this item?";

				public static class Download
				{
					public const string DOWNLOAD_SUMMARY_LABEL_TEXT = "summary";
					public const string DOWNLOAD_MESSAGE_TEXT = "Clicking download will overwrite any locally changed content with the published content from Beamable.";
					public const string DOWNLOAD_NO_DATA_TEXT = "There is no content to download. All your local content is up to date.";
					public const string DOWNLOAD_COMPLETE_TEXT = "All content has finished downloading.";
					public const string DOWNLOAD_LOAD_TEXT = "Getting latest server data...";
				}

				public static class Validate
				{
					public const string VALIDATE_START_MESSAGE = "Starting validation...";
					public const string VALIDATE_PROGRESS_MESSAGE = "Validating content...";
					public const string VALIDATION_COMPLETE_MESSAGE = "Validation successful.";
					public const string VALIDATION_FAILURE_MESSAGE = "Validation failed. Review the failures below.";
					public const string VALIDATE_BUTTON_START_TEXT = "Validating";
					public const string VALIDATE_BUTTON_DONE_WITH_ERRORS_TEXT = "View Failures";
					public const string VALIDATE_BUTTON_DONE_WITHOUT_ERRORS_TEXT = "Okay";
				}

				public static class Publish
				{
					public const string PUBLISH_MESSAGE_LOADING = "Preparing Content for Publish...";
					public const string PUBLISH_MESSAGE_PREVIEW = "Clicking publish will upload the following changes to {0}/{1}.";
					public const string PUBLISH_COMPLETE_MESSAGE = "All Content has been successfully published.";
					public const string PUBLISH_MESSAGE_IN_PROGRESS = "Publishing content...";
					public const string PUBLISH_NO_DATA_TEXT = "There is no content to Publish.";
				}

				public static class Reset
				{
					public const string RESET_CONTENT_MESSAGE_PREVIEW = "This operation will remove all your local changes. You can't undo this operation. Are you sure you want to proceed?";
					public const string RESET_CONTENT_COMPLETE_MESSAGE = "All Local Content has been successfully synchronized.";
				}

				public static class ContentList
				{
					public const string CONTENT_LIST_HEADER_TEXT = "Object Name                       Path                                     Tag";
					public const string CONTENT_LIST_CREATE_ITEM = "Create";
					public const string CONTENT_LIST_DELETE_ITEM = "Delete Item";
					public const string CONTENT_LIST_DELETE_ITEMS = "Delete Items";
					public const string CONTENT_LIST_RENAME_ITEM = "Rename Item";
					public const string CONTENT_LIST_DOWNLOAD_ITEM = "Download Item";
					public const string CONTENT_LIST_REVERT_ITEM = "Revert Item";
					public const string CONTENT_LIST_DOWNLOAD_ITEMS = "Download Items";
				}

				public static class ActionNames
				{
					public const string DOWNLOAD_CONTENT = "Download Content";
					public const string VALIDATE_CONTENT = "Validate Content";
					public const string PUBLISH_CONTENT = "Publish Content";
					public const string ARCHIVE_MANIFESTS = "Archive Manifests";
					public const string REMOVE_LOCAL_CONTENT = "Reset Content";
				}

				/// <summary>
				/// Creates a name with NO SPACES. Spaces are not allowed by the backend - srivello
				/// </summary>
				/// <param name="type"></param>
				/// <returns></returns>
				public static string GET_NAME_FOR_NEW_CONTENT_FILE_BY_TYPE(Type type)
				{
					return string.Format("New_{0}", type.Name);
				}
			}
		}
	}
}
