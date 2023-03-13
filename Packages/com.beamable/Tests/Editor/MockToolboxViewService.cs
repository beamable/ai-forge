using Beamable.Common;
using Beamable.Common.Api.Realms;
using Beamable.Editor.Modules.Account;
using Beamable.Editor.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.Tests
{
	public class MockToolboxViewService : IToolboxViewService
	{
		public List<RealmView> Realms { get; private set; }

		public RealmView CurrentRealm { get; private set; }

		public EditorUser CurrentUser { get; private set; }

		public IWidgetSource WidgetSource { get; private set; }

		public ToolboxQuery Query { get; private set; }

		public string FilterText { get; private set; }

		private List<AnnouncementModelBase> _announcements = new List<AnnouncementModelBase>();
		public IEnumerable<AnnouncementModelBase> Announcements { get; }

		public event Action<List<RealmView>> OnAvailableRealmsChanged;
		public event Action<RealmView> OnRealmChanged;
		public event Action<IWidgetSource> OnWidgetSourceChanged;
		public event Action OnQueryChanged;
		public event Action<EditorUser> OnUserChanged;
		public event Action<IEnumerable<AnnouncementModelBase>> OnAnnouncementsChanged;

		public MockToolboxViewService()
		{
			WidgetSource = new EmptyWidgetSource();

			UseDefaultWidgetSource();
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

		public IEnumerable<Widget> GetFilteredWidgets()
		{
			for (var i = 0; i < WidgetSource.Count; i++)
			{
				var widget = WidgetSource.Get(i);
				if (Query != null && !Query.Accepts(widget)) continue;

				yield return widget;
			}
		}

		public void Initialize()
		{
			RefreshAvailableRealms();

			var api = BeamEditorContext.Default;
			api.OnRealmChange += API_OnRealmChanged;
			CurrentUser = api.CurrentUser;
			OnUserChanged?.Invoke(CurrentUser);
			CurrentRealm = api.CurrentRealm;
			OnRealmChanged?.Invoke(CurrentRealm);
		}

		private void API_OnRealmChanged(RealmView realm)
		{
			CurrentRealm = realm;
			OnRealmChanged?.Invoke(realm);
		}

		public bool IsSpecificAnnouncementCurrentlyDisplaying(Type type)
		{
			return Announcements.Any(announcement => announcement.GetType() == type);
		}

		public Promise<List<RealmView>> RefreshAvailableRealms()
		{
			var api = BeamEditorContext.Default;
			return api.ServiceScope.GetService<RealmsService>().GetRealms().Then(realms =>
			{
				Realms = realms;
				OnAvailableRealmsChanged?.Invoke(realms);
			}).Error(err => api.Logout());
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

		public void UseDefaultWidgetSource()
		{
			//ACTUAL DISK IMPLEMENTATION
			//WidgetSource = AssetDatabase.LoadAssetAtPath<WidgetSource>($"{BASE_PATH}/Models/toolboxData.asset");

			//MOCK LOCAL DISK IMPLEMENTATION
			WidgetSource = new ToolboxMockData();
			OnWidgetSourceChanged?.Invoke(WidgetSource);
		}
	}
}

