using Beamable.Api.Analytics;
using Beamable.Api.Auth;
using Beamable.Api.Groups;
using Beamable.Api.Mail;
using Beamable.Api.Payments;
using Beamable.Api.Sessions;
using Beamable.Api.Stats;
using Beamable.Common;
using Beamable.Common.Api.Groups;
using Beamable.Common.Api.Mail;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Dependencies;
using Beamable.Console;
using Beamable.ConsoleCommands;
using Beamable.Coroutines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace Beamable.Api
{
	[BeamableConsoleCommandProvider]
	public class PlatformConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		private BeamableConsole Console => _provider.GetService<BeamableConsole>();
		private CoroutineService CoroutineService => _provider.GetService<CoroutineService>();
		private IPlatformService PlatformService => _provider.GetService<IPlatformService>();
		private StatsService StatsService => _provider.GetService<StatsService>();


		[Preserve]
		public PlatformConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand("IDFA", "print advertising identifier", "IDFA")]
		private string PrintAdvertisingIdentifier(string[] args)
		{
			Application.RequestAdvertisingIdentifierAsync((id, trackingEnabled, error) =>
				Console.Log($"AdId = {id}\nTrackingEnabled={trackingEnabled}\nError = {error}"));

			return String.Empty;
		}

		[BeamableConsoleCommand("RESET", "Dispose the context, clear the access token, and reload the scene given as an argument, OR the current scene", "RESET <scene index or name>")]
		protected string Reset(params string[] args)
		{

			Beam.ClearAndStopAllContexts()
				.FlatMap(_ => Beam.ResetToScene(args.Length == 1 ? args[0] : null))
				.Then(_ =>
				{
					Debug.Log("Reset complete");
				});
			return "Resetting...";
		}

		[BeamableConsoleCommand(new[] { "RESTART", "FR" }, "Clear access tokens, then Restart the game as if it had just been launched", "FORCE-RESTART")]
		public string Restart(params string[] args)
		{
			Beam.ClearAndStopAllContexts()
				.FlatMap(_ => Beam.ResetToScene("0"))
				.Then(_ =>
				{
					Debug.Log("Restart complete");
				});
			return "Restarting to scene 0...";
		}

		/// <summary>
		/// Send a local notification test at some delay.
		/// </summary>
		[BeamableConsoleCommand(new[] { "LOCALNOTE", "LN" }, "Send a local notification. Default delay is 10 seconds.", "LOCALNOTE [<delay> [<title> [<body>]]]")]
		private string LocalNotificationCommand(params string[] args)
		{
			var title = "Test Notification Message Title";
			var message =
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
			var delay = 10;
			if (args.Length >= 1)
			{
				int.TryParse(args[0], out delay);
			}
			if (args.Length >= 2)
			{
				title = args[1];
			}
			if (args.Length >= 3)
			{
				message = args[2];
			}
			var customData = new Dictionary<string, string> { { "evt", "test" }, { "foo", "123" } };
			var notification = _provider.GetService<INotificationService>();

			string channel = "test";

			notification.CreateNotificationChannel(channel, "Test", "Test notifications of regular importance.");
			notification.ScheduleLocalNotification(channel, "DBCONSOLE", 0, title, message,
												   TimeSpan.FromSeconds(delay), false, customData);
			return string.Format("Scheduled notification for {0} seconds in the future.", delay);
		}

		[BeamableConsoleCommand("GCCOLLECT", "Do a GC Collect and Unload Unused Assets", "GCCOLLECT")]
		private static string GCCollect(params string[] args)
		{
			Profiler.BeginSample("Memory collect test");
			Profiler.BeginSample("GC.Collect");
			System.GC.Collect();
			Profiler.EndSample();
			Profiler.BeginSample("Resources.UnloadUnusedAssets");
			Resources.UnloadUnusedAssets();
			Profiler.EndSample();
			Profiler.EndSample();
			return "";
		}

		[BeamableConsoleCommand(new[] { "TIMESCALE", "TS" }, "Sets the current timescale", "TIMESCALE <value> | variable")]
		private string Timescale(params string[] args)
		{
			if (args.Length < 1)
			{
				return Console.Help("TIMESCALE");
			}

			float timescale = 1;
			CoroutineService.StopCoroutine("VariableTimescale");
			if (args[0] == "variable")
			{
				CoroutineService.StartCoroutine("VariableTimescale");
				return "variable timescale";
			}
			else if (float.TryParse(args[0], out timescale))
			{
				Time.timeScale = timescale;
				return "setting timescale to " + timescale;
			}

			return "unknown timescale";
		}

		private IEnumerator VariableTimescale()
		{
			while (true)
			{
				Time.timeScale = (float)Mathf.Sqrt(UnityEngine.Random.Range(0f, 20.0f));
				yield return null;
			}
		}


		[BeamableConsoleCommand("SUBSCRIBER_DETAILS", "Query subscriber details", "SUBSCRIBER_DETAILS")]
		public string SubscriberDetails(string[] args)
		{
			_provider.GetService<IPubnubNotificationService>().GetSubscriberDetails().Then(rsp =>
			{
				Console.Log(
					rsp.authenticationKey + " " +
					rsp.customChannelPrefix + " " +
					rsp.gameGlobalNotificationChannel + " " +
					rsp.gameNotificationChannel + " " +
					rsp.playerChannel + " " +
					rsp.playerForRealmChannel + " " +
					rsp.subscribeKey
				);
			}).Error(err =>
			{
				Console.Log("Failed: " + err.ToString());
			});
			return "";
		}

		[BeamableConsoleCommand("DBID", "Show current player DBID", "DBID")]
		private string ShowDBID(params string[] args)
		{
			return PlatformService.User.id.ToString();
		}

		[BeamableConsoleCommand("HEARTBEAT", "Get heartbeat of a user", "HEARTBEAT <dbid>")]
		string GetHeartbeat(params string[] args)
		{
			if (args.Length != 1)
			{
				return "Requires dbid";
			}
			var dbid = long.Parse(args[0]);
			_provider.GetService<ISessionService>().GetHeartbeat(dbid)
				.Then(rsp => { Console.Log(rsp.ToString()); })
				.Error(err => { Console.Log(String.Format("Error:", err)); });

			return "Querying...";
		}

		/**
         * Login to a previously registered account with the given username and password.
         */
		[BeamableConsoleCommand("LOGIN_ACCOUNT", "Log in to the DBID designated by the given username and password", "LOGIN_ACCOUNT <email> <password>")]
		string LoginAccount(params string[] args)
		{
			if (args.Length < 2)
			{
				return "Requires both an email and a password.";
			}
			var email = args[0];
			var password = args[1];
			var ctx = _provider.GetService<BeamContext>();
			var auth = _provider.GetService<IAuthService>();
			auth.Login(email, password)
				.RecoverWith(ex =>
				{
					if (ex is PlatformRequesterException code && code.Error.error == "UnableToMergeError")
					{
						Debug.LogWarning("The current account is already associated with an email");
						return auth.Login(email, password, false);
					}

					throw ex;
				})
				.Then(rsp =>
				{
					Debug.Log("Email and Password are valid. Now applying token to context.");
					ctx.ChangeAuthorizedPlayer(rsp).Then(_ =>
					{
						Debug.Log("Successfully reloaded context with new user.");
					});
				});

			return "Logging in...";
		}

		/**
         * Get the counts of the mailbox
         */
		[BeamableConsoleCommand("MAIL_GET", "Get mailbox messages", "MAIL_GET <category>")]
		string GetMail(params string[] args)
		{
			if (args.Length < 1)
			{
				return "Requires category";
			}

			var mail = _provider.GetService<MailService>();
			mail.GetMail(args[0]).Then(rsp =>
			{
				for (int i = 0; i < rsp.result.Count; i++)
				{
					var next = rsp.result[i];
					Console.Log("[" + next.id + "]");
					Console.Log("FROM: " + next.senderGamerTag);
					Console.Log(next.subject);
					Console.Log("(" + next.rewards.items.Count + " items)");
					Console.Log("(" + next.rewards.currencies.Count + " currencies)");
					Console.Log(next.body);
					Console.Log("");
				}
				Console.Log("DONE");
			}).Error(err =>
			{
				Console.Log(String.Format("Error:", err));
			});
			return "Querying...";
		}

		[BeamableConsoleCommand("MAIL_SEND", "Send a message via the mail system.", "MAIL_SEND <receiver> <body>")]
		string SendMail(params string[] args)
		{
			var mail = _provider.GetService<MailService>();
			var userId = _provider.GetService<IPlatformService>().UserId;
			if (args.Length < 2)
			{
				return "Requires receiver and body";
			}

			var receiver = long.Parse(args[0]);
			var body = args[1];
			var request = new MailSendRequest();
			request.Add(new MailSendEntry
			{
				senderGamerTag = userId,
				receiverGamerTag = receiver,
				category = "test",
				subject = "test message",
				body = body
			});
			mail.SendMail(request).Then(rsp =>
			{
				Console.Log(JsonUtility.ToJson(rsp));
			}).Error(err =>
			{
				Console.Log($"Error: {err}");
			});
			return "Mail sent!";
		}

		/**
         * Update a mail in the mailbox
         */
		[BeamableConsoleCommand("MAIL_UPDATE", "Update a mail", "MAIL_UPDATE <id> <state> <acceptAttachments>")]
		string UpdateMail(params string[] args)
		{
			if (args.Length < 2)
			{
				return "Requires mailId and state";
			}

			string mailId = args[0];
			string stateStr = args[1];
			MailState state = (MailState)Enum.Parse(typeof(MailState), stateStr);
			bool acceptAttachments = args.Length >= 3;

			MailUpdateRequest updates = new MailUpdateRequest();
			updates.Add(long.Parse(mailId), state, acceptAttachments, "");
			_provider.GetService<MailService>().Update(updates).Then(rsp =>
			{
				Console.Log(JsonUtility.ToJson(rsp));
			}).Error(err =>
			{
				Console.Log(String.Format("Error:", err));
			});
			return "Updating...";
		}

		/**
         * Registers the current DBID to the given username and password.
         */
		[BeamableConsoleCommand("REGISTER_ACCOUNT", "Registers this DBID with the given email and password", "REGISTER_ACCOUNT <email> <password>")]
		string RegisterAccount(params string[] args)
		{
			if (args.Length < 2)
			{
				return "Requires both an email and a password.";
			}
			var email = args[0];
			var password = args[1];
			var auth = _provider.GetService<IAuthService>();
			auth.RegisterDBCredentials(email, password)
				.Then(rsp => { Console.Log(String.Format("Successfully registered user {0}", email)); })
				.Error(err => { Console.Log(err.ToString()); });

			return "Registering user: " + email;
		}

		[BeamableConsoleCommand("TOKEN", "Show current access token", "TOKEN")]
		private string ShowToken(params string[] args)
		{
			return _provider.GetService<BeamContext>().AccessToken.Token;
		}

		[BeamableConsoleCommand("EXPIRE_TOKEN", "Expires the current access token to trigger the refresh flow", "EXPIRE_TOKEN")]
		public string ExpireAccessToken(params string[] args)
		{
			var platform = _provider.GetService<BeamContext>();
			platform.AccessToken.ExpireAccessToken();
			var _ = Beam.ResetToScene(args.Length == 1 ? args[0] : null);
			return "Access Token is now expired. Restarting.";
		}

		[BeamableConsoleCommand("CORRUPT_TOKEN", "Corrupts the current access token to trigger the refresh flow", "CORRUPT_TOKEN")]
		public string CorruptAccessToken(params string[] args)
		{
			var platform = _provider.GetService<BeamContext>();
			platform.AccessToken.CorruptAccessToken();
			return "Access Token has been corrupted.";
		}

		[BeamableConsoleCommand("TEST-ANALYTICS", "Run 1000 events to test batching/load", "TEST-ANALYTICS")]
		public string TestAnalytics(params string[] args)
		{
			var evt = new SampleCustomEvent("lorem ipsum dolar set amet", "T-T-T-Test the base!");

			var analytics = _provider.GetService<AnalyticsTracker>();
			analytics.TrackEvent(evt);
			for (var i = 0; i < 1000; ++i)
			{
				analytics.TrackEvent(evt);
			}
			return "Analytics Sent";
		}

		[BeamableConsoleCommand("IAP_BUY", "Invokes the real money transaction flow to purchase the given item_symbol.", "IAP_BUY <listing> <sku>")]
		string IAPBuy(params string[] args)
		{
			if (args.Length != 2)
			{
				return "Requires: <listing> <sku>";
			}

			_provider.GetService<IBeamablePurchaser>().StartPurchase(args[0], args[1])
					 .Then((txn) => { Console.Log("Purchase Complete: " + txn.Txid); })
					 .Error((err) => { Console.Log("Purchase Failed: " + err.ToString()); });

			return "Purchasing item: " + args[0];
		}

		[BeamableConsoleCommand("IAP_PENDING", "Displays pending transactions", "IAP_PENDING")]
		string IAPPending(params string[] args)
		{
			return PlayerPrefs.GetString("pending_purchases");
		}

		[BeamableConsoleCommand("IAP_UNFULFILLED", "Display unfulfilled purchases", "IAP_UNFULFILLED")]
		string IAPUnfulfilled(params string[] args)
		{
			return PlayerPrefs.GetString("unfulfilled_transactions");
		}

		/**
         * Get the group info of a user
         */
		[BeamableConsoleCommand("GROUP_USER", "Query a user for group info", "GROUP_USER <dbid>")]
		string GetGroupUser(params string[] args)
		{
			long gamerTag;
			var groups = _provider.GetService<GroupsService>();
			if (args.Length < 1)
			{
				gamerTag = PlatformService.User.id;
			}
			else
			{
				gamerTag = long.Parse(args[0]);
			}
			groups.GetUser(gamerTag)
				.Then(rsp => { Console.Log(JsonUtility.ToJson(rsp)); })
				.Error(err => { Console.Log(String.Format("Error:", err)); });
			return "Querying...";
		}

		[BeamableConsoleCommand("GROUP_LEAVE", "Leave the current group", "GROUP_LEAVE")]
		string GroupLeave(params string[] args)
		{
			long gamerTag = PlatformService.UserId;
			var groups = _provider.GetService<GroupsService>();
			groups.GetUser(gamerTag)
				.FlatMap<GroupMembershipResponse>(userRsp =>
				{
					long group = 0;
					if (userRsp.member.guild.Count > 0)
					{
						group = userRsp.member.guild[0].id;
					}
					return groups.LeaveGroup(group);
				})
				.Error(err => { Console.Log(String.Format("Error:", err)); });
			return "Querying...";
		}



		/**
         * View stats for a user
         */
		[BeamableConsoleCommand("GET_STATS", "Get stats for some user", "GET_STATS <domain> <access> <type> <id>")]
		string GetStats(params string[] args)
		{
			if (args.Length != 4)
			{
				return "Requires: <DOMAIN> <ACCESS> <TYPE> <ID>";
			}


			StatsService.GetStats(args[0], args[1], args[2], long.Parse(args[3]))
				.Then(rsp =>
				{
					foreach (var next in rsp)
					{
						Console.Log(String.Format("{0} = {1}", next.Key, next.Value));
					}
					Console.Log("Done");
				})
				.Error(err => { Console.Log(String.Format("Error:", err)); });
			return "Querying...";
		}

		/**
         * Set stat for a user
         */
		[BeamableConsoleCommand("SET_STAT", "Sets client stat for self", "SET_STAT <access> <key> <value>")]
		string SetStat(params string[] args)
		{
			if (args.Length != 3)
			{
				return "Requires: <ACCESS> <KEY> <VALUE>";
			}

			Dictionary<string, string> stats = new Dictionary<string, string>();
			stats.Add(args[1], args[2]);
			StatsService.SetStats(args[0], stats)
				.Then(rsp => Console.Log("Done"))
				.Error(err => Console.Log(String.Format("Error:", err)));
			return "Querying...";
		}

		[BeamableConsoleCommand("SET_TIME", "Sets the override time. If no time is specified, then there will be no override", "SET_TIME <time>")]
		string SetTime(params string[] args)
		{
			var platform = PlatformService;
			if (args.Length == 0)
			{
				platform.TimeOverride = null;
				return "Clearing Override Time";
			}

			try
			{
				platform.TimeOverride = args[0];
				return String.Format("Setting Override: {0}", platform.TimeOverride);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				return "Invalid Time";
			}
		}

		[BeamableConsoleCommand("list_contexts", "list active beam context", "list_contexts")]
		string ListContexts(string[] args)
		{
			StringBuilder builder = new StringBuilder();
			foreach (BeamContext beamContext in BeamContext.All)
			{
				builder.Append(beamContext.PlayerId);
				builder.Append(string.IsNullOrEmpty(beamContext.PlayerCode) ? "\n" : $" with player code: {beamContext.PlayerCode}\n");
			}
			return builder.ToString();
		}

		[BeamableConsoleCommand("set_console_context", "set active beam console context", "set_console_context <id_or_player_code>")]
		string SetActiveContext(string[] args)
		{
			if (args.Length == 0)
			{
				return "Requires context id as parameter";
			}

			if (!long.TryParse(args[0], out var result))
			{
				var contextWithPlayerCode = BeamContext.All.FirstOrDefault(context => context.PlayerCode == args[0]);
				if (contextWithPlayerCode != null)
				{
					ConsoleFlow.Instance.ChangePlayerContext(contextWithPlayerCode?.PlayerCode ?? "");
					return $"Console BeamContext successfully set to existing playerCode=[{contextWithPlayerCode.PlayerCode}]";
				}
				else
				{
					ConsoleFlow.Instance.ChangePlayerContext(args[0]);
					return $"Console BeamContext successfully set to new playerCode=[{args[0]}]";
				}
			}

			if (BeamContext.All.All(context => context.PlayerId != result))
			{
				return $"Cannot find BeamContext with PlayerId: {result}, \n" +
					   $"valid values are: {string.Join(",", BeamContext.All.Select(context => context.PlayerId.ToString()).ToArray())}";
			}
			var contextWithPlayerId = BeamContext.All.FirstOrDefault(context => context.PlayerId == result);
			ConsoleFlow.Instance.ChangePlayerContext(contextWithPlayerId.PlayerCode);
			return $"Console BeamContext successfully set to {result}";
		}

		[BeamableConsoleCommand("get_console_context", "get active beam console context", "get_console_context")]
		string GetActiveContext(string[] args)
		{
			return _provider.GetService<BeamContext>().PlayerId.ToString();
		}
	}
}
