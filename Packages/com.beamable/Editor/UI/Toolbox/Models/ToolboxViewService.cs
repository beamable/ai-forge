using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Realms;
using Beamable.Editor.Modules.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Toolbox;

namespace Beamable.Editor.Toolbox.Models
{
	public interface IToolboxViewService
	{
		event Action<List<RealmView>> OnAvailableRealmsChanged;
		event Action<RealmView> OnRealmChanged;
		event Action<IWidgetSource> OnWidgetSourceChanged;
		event Action OnQueryChanged;
		event Action<EditorUser> OnUserChanged;
		event Action<IEnumerable<AnnouncementModelBase>> OnAnnouncementsChanged;

		List<RealmView> Realms { get; }
		RealmView CurrentRealm { get; }
		EditorUser CurrentUser { get; }
		IWidgetSource WidgetSource { get; }
		ToolboxQuery Query { get; }
		string FilterText { get; }
		IEnumerable<AnnouncementModelBase> Announcements { get; }
		void Initialize();
		void UseDefaultWidgetSource();
		void AddAnnouncement(AnnouncementModelBase announcementModel);
		void RemoveAnnouncement(AnnouncementModelBase announcementModel);
		bool IsSpecificAnnouncementCurrentlyDisplaying(Type type);
		void SetQuery(string filter);
		void SetQuery(ToolboxQuery query);
		IEnumerable<Widget> GetFilteredWidgets();
		Promise<List<RealmView>> RefreshAvailableRealms();
		void Destroy();
		void SetQueryTag(WidgetTags tags, bool shouldHaveTag);
		void SetOrientationSupport(WidgetOrientationSupport orientation, bool shouldHaveOrientation);
		void SetSupportStatus(SupportStatus status, bool shouldHaveStatus, bool disableOther);
	}

	public class ToolboxViewService : IToolboxViewService
	{
		const string FILTER_TEXT_KEY = "ToolboxViewFilterText";
		public event Action<List<RealmView>> OnAvailableRealmsChanged;
		public event Action<RealmView> OnRealmChanged;
		public event Action<IWidgetSource> OnWidgetSourceChanged;
		public event Action OnQueryChanged;
		public event Action<EditorUser> OnUserChanged;
		public event Action<IEnumerable<AnnouncementModelBase>> OnAnnouncementsChanged;

		public List<RealmView> Realms { get; private set; }
		public RealmView CurrentRealm { get; private set; }
		public EditorUser CurrentUser { get; private set; }
		public IWidgetSource WidgetSource { get; private set; }
		public ToolboxQuery Query { get; private set; }

		public string FilterText
		{
			get => SessionState.GetString(FILTER_TEXT_KEY, string.Empty);
			private set => SessionState.SetString(FILTER_TEXT_KEY, value);
		}

		private List<AnnouncementModelBase> _announcements = new List<AnnouncementModelBase>();
		public IEnumerable<AnnouncementModelBase> Announcements => _announcements;


		public ToolboxViewService()
		{
			WidgetSource = new EmptyWidgetSource();
		}

		public void AddAnnouncement(AnnouncementModelBase announcementModel)
		{
			_announcements.Add(announcementModel);
			OnAnnouncementsChanged?.Invoke(Announcements);
		}

		public void RemoveAnnouncement(AnnouncementModelBase announcementModel)
		{
			_announcements.Remove(announcementModel);
			OnAnnouncementsChanged?.Invoke(Announcements);
		}

		public bool IsSpecificAnnouncementCurrentlyDisplaying(Type type)
		{
			return Announcements.Any(announcement => announcement.GetType() == type);
		}

		public void UseDefaultWidgetSource()
		{
			WidgetSource = AssetDatabase.LoadAssetAtPath<WidgetSource>($"{BASE_PATH}/Models/toolboxData.asset");
			Query = ToolboxQuery.Parse(FilterText);
			OnWidgetSourceChanged?.Invoke(WidgetSource);
		}

		// TODO: Add a method that creates a widgetSource derived from networking.

		public void SetQuery(string filter)
		{
			var oldFilterText = FilterText;
			var nextQuery = ToolboxQuery.Parse(filter);
			Query = nextQuery;
			FilterText = filter;
			if (!string.Equals(oldFilterText, FilterText))
			{
				OnQueryChanged?.Invoke();
			}
		}

		public void SetQuery(ToolboxQuery query)
		{
			var oldFilterText = FilterText;
			Query = query;
			FilterText = query.ToString();

			if (!string.Equals(oldFilterText, FilterText))
			{
				OnQueryChanged?.Invoke();
			}
		}

		public IEnumerable<Widget> GetFilteredWidgets()
		{
			for (var i = 0; i < WidgetSource.Count; i++)
			{
				var widget = WidgetSource.Get(i);
				if (Query != null && !Query.Accepts(widget)) continue;

				yield return widget;
			}
		}

		public Promise<List<RealmView>> RefreshAvailableRealms()
		{
			return BeamEditorContext.Default.ServiceScope.GetService<RealmsService>().GetRealms().Then(realms =>
			{
				Realms = realms;
				OnAvailableRealmsChanged?.Invoke(realms);
			});
		}

		public void Initialize()
		{
			RefreshAvailableRealms().Error(exception =>
			{
				if (exception is RequesterException)
				{
					RefreshAvailableRealms().Error(Debug.LogException);
				}
			});

			var api = BeamEditorContext.Default;
			api.OnRealmChange += HandleRealmChanged;
			CurrentUser = api.CurrentUser;
			OnUserChanged?.Invoke(CurrentUser);
			CurrentRealm = api.CurrentRealm;
			OnRealmChanged?.Invoke(CurrentRealm);
		}

		private void HandleRealmChanged(RealmView realm)
		{
			CurrentRealm = realm;
			OnRealmChanged?.Invoke(realm);
		}

		public void Destroy()
		{
			OnAvailableRealmsChanged = null;
			var api = BeamEditorContext.Default;
			api.OnRealmChange -= HandleRealmChanged;
		}

		public void SetQueryTag(WidgetTags tags, bool shouldHaveTag)
		{
			var hasOrientation = (Query?.HasTagConstraint ?? false) &&
								 Query.FilterIncludes(tags);
			var nextQuery = new ToolboxQuery(Query);

			if (hasOrientation && !shouldHaveTag)
			{
				nextQuery.TagConstraint = nextQuery.TagConstraint & ~tags;
				nextQuery.HasTagConstraint = nextQuery.TagConstraint > 0;
			}
			else if (!hasOrientation && shouldHaveTag)
			{
				nextQuery.TagConstraint |= tags;
				nextQuery.HasTagConstraint = true;
			}
			SetQuery(nextQuery);

		}

		public void SetOrientationSupport(WidgetOrientationSupport orientation, bool shouldHaveOrientation)
		{
			var hasOrientation = (Query?.HasOrientationConstraint ?? false) &&
								 Query.FilterIncludes(orientation);
			var nextQuery = new ToolboxQuery(Query);

			if (hasOrientation && !shouldHaveOrientation)
			{
				nextQuery.OrientationConstraint = nextQuery.OrientationConstraint & ~orientation;
				nextQuery.HasOrientationConstraint = nextQuery.OrientationConstraint > 0;
			}
			else if (!hasOrientation && shouldHaveOrientation)
			{
				nextQuery.OrientationConstraint |= orientation;
				nextQuery.HasOrientationConstraint = true;
			}
			SetQuery(nextQuery);
		}

		public void SetSupportStatus(SupportStatus status, bool shouldHaveStatus, bool disableOther)
		{
			var hasOrientation = (Query?.HasSupportConstraint ?? false) &&
								 Query.FilterIncludes(status);
			var nextQuery = new ToolboxQuery(Query);

			if (hasOrientation && !shouldHaveStatus)
			{
				nextQuery.SupportStatusConstraint &= ~status;
				nextQuery.HasSupportConstraint = nextQuery.SupportStatusConstraint > 0;
			}
			else if (!hasOrientation && shouldHaveStatus)
			{
				nextQuery.SupportStatusConstraint = disableOther ? status : nextQuery.SupportStatusConstraint | status;
				nextQuery.HasSupportConstraint = true;
			}
			SetQuery(nextQuery);
		}
	}
}

