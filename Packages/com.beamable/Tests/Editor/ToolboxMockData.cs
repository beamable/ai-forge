using Beamable.Editor.Toolbox.Models;
using Beamable.Editor.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beamable.Editor.Tests
{
	public class ToolboxMockData : IWidgetSource
	{
		public ToolboxMockData()
		{
			Widget[] w =
			{
				//Admin Flow
				new Widget() {
					Name = "Admin Flow",
					Description = "UI for game commands and cheats",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT | WidgetOrientationSupport.LANDSCAPE,
					Tags = WidgetTags.FLOW | WidgetTags.ADMIN,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Currency HUD
				new Widget() {
					Name = "Currency HUD",
					Description = "UI for virtual currency",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT | WidgetOrientationSupport.LANDSCAPE,
					Tags =  WidgetTags.COMPONENT | WidgetTags.SHOP | WidgetTags.INVENTORY | WidgetTags.CURRENCY,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Account HUD
				new Widget() {
					Name = "Account HUD",
					Description = "UI to open login flow",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT | WidgetOrientationSupport.LANDSCAPE,
					Tags =  WidgetTags.COMPONENT | WidgetTags.ACCOUNTS,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Account Management Flow
				new Widget() {
					Name = "Account Management Flow",
					Description = "Allows users to manage account",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT,
					Tags =  WidgetTags.FLOW | WidgetTags.ACCOUNTS,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Leaderboard Flow
				new Widget() {
					Name = "Leaderboard Flow",
					Description = "Allow user to manage leaderboard",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT,
					Tags =  WidgetTags.FLOW | WidgetTags.LEADERBOARDS,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Leaderboard Flow (NEW)
				new Widget() {
					Name = "Leaderboard Flow (NEW)",
					Description = "Allow user to manage leaderboard",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT,
					Tags =  WidgetTags.FLOW | WidgetTags.LEADERBOARDS,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Announcement Flow
				new Widget() {
					Name = "Announcement Flow",
					Description = "Allow user to manage announcements",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT,
					Tags =  WidgetTags.FLOW | WidgetTags.INVENTORY | WidgetTags.CURRENCY,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Inventory Flow
				new Widget() {
					Name = "Inventory Flow",
					Description = "Allow user to manage inventory",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT,
					Tags =  WidgetTags.FLOW | WidgetTags.ADMIN,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Tournament Flow
				new Widget() {
					Name = "Tournament Flow",
					Description = "Allow user set up a recurring tournament",
					OrientationSupport = WidgetOrientationSupport.LANDSCAPE,
					Tags =  WidgetTags.FLOW | WidgetTags.LEADERBOARDS,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

				//Store Flow
				new Widget() {
					Name = "Store Flow",
					Description = "Allow user to shop",
					OrientationSupport = WidgetOrientationSupport.PORTRAIT,
					Tags =  WidgetTags.FLOW | WidgetTags.SHOP,
					Icon = new Texture2D(128, 128),
					Prefab = new GameObject()
				},

			};

			Widgets = w.ToList();

		}

		public List<Widget> Widgets = new List<Widget>();

		public int Count => Widgets.Count();

		public Widget Get(int index)
		{
			return Widgets[index];
		}

	}
}
