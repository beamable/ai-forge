using Beamable.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using static Beamable.Common.Constants.Features.Toolbox.EditorPrefsKeys;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Beamable.Editor.Environment
{
	public class BeamablePackageMeta
	{
		public bool IsPackageAvailable;
		public string VersionNumber;
	}

	public static class BeamablePackageUpdateMeta
	{
		public static event Action OnPackageUpdated;

		public static bool IsInstallationIgnored { get; set; }
		public static string CurrentVersionNumber { get; set; }
		public static string CurrentServerVersionNumber { get; set; }

		public static string NewestVersionNumber => EditorPrefs.GetString(NEWEST_VERSION_NUMBER);
		public static string NewestServerVersionNumber => EditorPrefs.GetString(NEWEST_SERVER_VERSION_NUMBER);

		public static void SetCurrentVersionNumber(string versionNumber, bool skipEvent = false)
		{
			CurrentVersionNumber = versionNumber;
			if (!skipEvent)
				OnPackageUpdated?.Invoke();
		}
		public static void SetNewestVersionNumber(string versionNumber)
		{
			EditorPrefs.SetString(NEWEST_VERSION_NUMBER, versionNumber);
			IsInstallationIgnored = false;
		}
		public static void SetNewestServerVersionNumber(string versionNumber)
		{
			EditorPrefs.SetString(NEWEST_SERVER_VERSION_NUMBER, versionNumber);
		}
	}

	public static class BeamablePackages
	{
		public const string BeamablePackageName = "com.beamable";
		public const string ServerPackageName = "com.beamable.server";

		public const string SESSION_KEY_UPDATE_SERVER_REQUIRED = "beam-update-server-package-requested";

		private static Dictionary<string, Action> _packageToWindowInitialization = new Dictionary<string, Action>();

		private static Promise<BeamablePackageMeta> _serverPackageMetaPromise;

		public static Promise<BeamablePackageMeta> ServerPackageMeta =>
			_serverPackageMetaPromise ?? (_serverPackageMetaPromise = GetServerPackage());


		static BeamablePackages()
		{
			var _ = ServerPackageMeta;
			var __ = CheckForPendingUpdates();
		}

		private static async Promise CheckForPendingUpdates()
		{
			if (IsServerUpdateRequested)
			{
				var isUpdated = await IsServerPackageUpdated();
				if (!isUpdated)
				{
					await UpdateBeamablePackageServer();
				}
			}
		}

		private static bool IsServerUpdateRequested
		{
			get => SessionState.GetBool(SESSION_KEY_UPDATE_SERVER_REQUIRED, false);
			set => SessionState.SetBool(SESSION_KEY_UPDATE_SERVER_REQUIRED, value);
		}

		public static void ShowServerWindow()
		{
			if (!_packageToWindowInitialization.TryGetValue(ServerPackageName, out var windowInitializer))
			{
				BeamableLogger.LogError("Beamable server not installed.");
			}

			windowInitializer?.Invoke();
		}

		public static Promise<PackageInfo> GetPackageInfo(string packageName)
		{
			var listReq = Client.List(true);
			var promise = new Promise<PackageInfo>();

			void Check()
			{
				if (!listReq.IsCompleted)
				{
					EditorApplication.delayCall += Check;
					return;
				}

				var isSuccess = listReq.Status == StatusCode.Success;
				if (!isSuccess)
				{
					promise.CompleteError(new Exception("Unable to list local packages. " + listReq.Error.message));
				}

				var package = listReq.Result.FirstOrDefault(p => p.name.Equals(packageName));
				promise.CompleteSuccess(package);
			}

			EditorApplication.delayCall += Check;
			return promise;
		}

		public static Promise<BeamablePackageMeta> GetServerPackage()
		{
			var req = Client.List(true);
			var promise = new Promise<BeamablePackageMeta>();

			void Callback()
			{
				if (!req.IsCompleted) return;
				EditorApplication.update -= Callback;

				if (req.Status == StatusCode.Success)
				{
					PackageInfo beamablePackage = null;
					PackageInfo serverPackage = null;
					foreach (var package in req.Result)
					{
						switch (package.name)
						{
							case BeamablePackageName:
								beamablePackage = package;
								break;
							case ServerPackageName:
								serverPackage = package;
								break;
						}
					}

					if (beamablePackage == null)
					{
						promise.CompleteError(new Exception("no beamable package found"));
						return;
					}

					if (serverPackage == null)
					{
						promise.CompleteSuccess(new BeamablePackageMeta
						{
							IsPackageAvailable = false,
							VersionNumber = beamablePackage.version
						});
					}
					else
					{
						if (beamablePackage.version != serverPackage.version)
						{
							promise.CompleteError(new Exception(
							   $"Beamable and Beamable Server need to be at the same version to work. Go to the Unity package manager and resolve the issue. server=[{serverPackage.version}] beamable=[{beamablePackage.version}]"));
							return;
						}

						promise.CompleteSuccess(new BeamablePackageMeta
						{
							IsPackageAvailable = true,
							VersionNumber = serverPackage.version
						});
					}
				}
				else if (req.Status >= StatusCode.Failure)
				{
					promise.CompleteError(new Exception(req.Error.message));
					BeamableLogger.LogError(req.Error.message);
				}

			}

			EditorApplication.update += Callback;
			return promise;
		}

		public static void ProvideServerWindow(Action windowInitializer)
		{
			ProvidePackageWindow(ServerPackageName, windowInitializer);
		}

		public static void ProvidePackageWindow(string packageName, Action windowInitializer)
		{
			_packageToWindowInitialization.Add(packageName, windowInitializer);
		}

		public static Promise<Unit> DownloadServer(BeamablePackageMeta beamableVersion)
		{
			var req = Client.Add($"{ServerPackageName}@{beamableVersion.VersionNumber}");
			var promise = new Promise<Unit>();

			void Callback()
			{
				if (!req.IsCompleted) return;

				EditorApplication.update -= Callback;

				if (req.Status == StatusCode.Success)
				{
					promise.CompleteSuccess(PromiseBase.Unit);
				}
				else if (req.Status >= StatusCode.Failure)
				{
					promise.CompleteError(new Exception(req.Error.message));
					BeamableLogger.LogError(req.Error.message);
				}

			}

			EditorApplication.update += Callback;
			return promise;
		}

		public static async Promise UpdatePackage()
		{
			IsServerUpdateRequested = true;
			await UpdateBeamablePackage();
			await CheckForPendingUpdates();
		}

		public static void OpenUrlForVersion(PackageVersion version)
		{
			var url = "https://beamable.github.io/changes/";
			if (version.IsReleaseCandidate)
			{
				url += "?preview";
			}
			else if (version.IsNightly)
			{
				url += "?ci";
			}

			url += $"#{version.Major}.{version.Minor}.{version.Patch}";
			Application.OpenURL(url);
		}

		private static Promise<Unit> UpdateBeamablePackage()
		{
			var promise = new Promise<Unit>();

			if (string.IsNullOrWhiteSpace(BeamablePackageUpdateMeta.NewestVersionNumber))
			{
				return promise;
			}

			var req = Client.Add($"{BeamablePackageName}@{BeamablePackageUpdateMeta.NewestVersionNumber}");

			void Callback()
			{
				if (!req.IsCompleted) return;

				EditorApplication.update -= Callback;

				if (req.Status == StatusCode.Success)
				{
					promise.CompleteSuccess(PromiseBase.Unit);
				}
				else if (req.Status >= StatusCode.Failure)
				{
					promise.CompleteError(new Exception(req.Error.message));
					BeamableLogger.LogError(req.Error.message);
				}

			}

			EditorApplication.update += Callback;
			return promise;
		}

		private static Promise<Unit> UpdateBeamablePackageServer()
		{
			var promise = new Promise<Unit>();

			if (string.IsNullOrWhiteSpace(BeamablePackageUpdateMeta.NewestServerVersionNumber))
			{
				return promise;
			}

			var req = Client.Add($"{ServerPackageName}@{BeamablePackageUpdateMeta.NewestServerVersionNumber}");

			void Callback()
			{
				if (!req.IsCompleted) return;

				EditorApplication.update -= Callback;
				IsServerUpdateRequested = false;
				if (req.Status == StatusCode.Success)
				{
					promise.CompleteSuccess(PromiseBase.Unit);
				}
				else if (req.Status >= StatusCode.Failure)
				{
					promise.CompleteError(new Exception(req.Error.message));
					BeamableLogger.LogError(req.Error.message);
				}

			}

			EditorApplication.update += Callback;
			return promise;
		}

		private static bool _isDownloading = false;
		public static Promise<bool> IsPackageUpdated()
		{
			var promise = new Promise<bool>();

			if (_isDownloading)
			{
				promise.CompleteSuccess(false);
				return promise;
			}

			var listReq = Client.List(false);

			void Check()
			{
				if (!listReq.IsCompleted)
				{
					EditorApplication.delayCall += Check;
					return;
				}

				var isSuccess = listReq.Status == StatusCode.Success;
				if (!isSuccess)
				{
					promise.CompleteError(new Exception($"Unable to list local packages: {listReq.Error.message}"));
					return;
				}

				var package = listReq.Result.FirstOrDefault(p => p.name.Equals(BeamablePackageName));
				if (package == null)
				{
					var serverPackage = listReq.Result.FirstOrDefault(p => p.name.Equals(ServerPackageName));
					if (serverPackage == null)
					{
						promise.CompleteError(new Exception($"Cannot find package: {serverPackage.displayName}"));
						return;
					}
					if (_isDownloading)
						return;

					EditorApplication.delayCall -= Check;
					_isDownloading = true;
					DownloadMissingPackage(serverPackage.version).Then(_ => EditorApplication.delayCall += Check);
					return;
				}

				// Quick hack to skip not production updates
				// TODO - Better way!
				if (package.version.Contains("PREVIEW"))
				{
					BeamablePackageUpdateMeta.IsInstallationIgnored = true;
					EditorPrefs.SetBool(IS_PACKAGE_UPDATE_IGNORED, true);
					promise.CompleteSuccess(true);
					return;
				}

				if (BeamablePackageUpdateMeta.CurrentVersionNumber != package.version)
				{
					if (string.IsNullOrEmpty(BeamablePackageUpdateMeta.CurrentVersionNumber))
					{
						BeamablePackageUpdateMeta.SetCurrentVersionNumber(package.version, true);
					}
					else
					{
						BeamablePackageUpdateMeta.SetCurrentVersionNumber(package.version);
					}

					if (!EditorPrefs.HasKey(IS_PACKAGE_UPDATE_IGNORED))
					{
						EditorPrefs.SetBool(IS_PACKAGE_UPDATE_IGNORED, false);
					}
				}

				var latestCompatibleVersion = package.versions.latestCompatible;
				if (BeamablePackageUpdateMeta.NewestVersionNumber != latestCompatibleVersion)
				{
					BeamablePackageUpdateMeta.SetNewestVersionNumber(latestCompatibleVersion);
					EditorPrefs.SetBool(IS_PACKAGE_UPDATE_IGNORED, false);
				}

				promise.CompleteSuccess(BeamablePackageUpdateMeta.CurrentVersionNumber == BeamablePackageUpdateMeta.NewestVersionNumber);
			}

			EditorApplication.update += Check;
			return promise;
		}

		private static Promise<bool> IsServerPackageUpdated()
		{
			var listReq = Client.List(false);
			var promise = new Promise<bool>();

			void Check()
			{
				if (!listReq.IsCompleted)
				{
					EditorApplication.delayCall += Check;
					return;
				}

				var isSuccess = listReq.Status == StatusCode.Success;
				if (!isSuccess)
				{
					promise.CompleteError(new Exception($"Unable to list local packages: {listReq.Error.message}"));
				}

				var package = listReq.Result.FirstOrDefault(p => p.name.Equals(ServerPackageName));
				if (package == null)
				{
					promise.CompleteError(new Exception($"Cannot find package: {package.displayName}"));
				}

				BeamablePackageUpdateMeta.CurrentServerVersionNumber = package.version;
				var latestCompatibleVersion = package.versions.latestCompatible;

				if (BeamablePackageUpdateMeta.NewestServerVersionNumber != latestCompatibleVersion)
				{
					BeamablePackageUpdateMeta.SetNewestServerVersionNumber(latestCompatibleVersion);
				}

				promise.CompleteSuccess(package.version == latestCompatibleVersion);
			}

			EditorApplication.update += Check;
			return promise;
		}

		private static Promise<Unit> DownloadMissingPackage(string version)
		{
			var req = Client.Add($"{BeamablePackageName}@{version}");
			var promise = new Promise<Unit>();

			void Callback()
			{
				if (!req.IsCompleted)
					return;

				EditorApplication.update -= Callback;

				if (req.Status == StatusCode.Success)
				{
					promise.CompleteSuccess(PromiseBase.Unit);
					_isDownloading = false;
				}
				else if (req.Status >= StatusCode.Failure)
				{
					promise.CompleteError(new Exception(req.Error.message));
					BeamableLogger.LogError(req.Error.message);
					_isDownloading = false;
				}
			}

			EditorApplication.update += Callback;
			return promise;
		}
	}
}
