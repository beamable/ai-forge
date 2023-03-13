using Beamable.Editor;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.DockerCommands;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace Beamable.Server.Editor.Uploader
{
	/// <summary>
	/// Container uploader sub-component of Beamable.
	/// </summary>
	public class ContainerUploadHarness
	{
		public event Action<float, long, long> onProgress;

		/// <summary>
		/// Log a message to the progress panel.
		/// </summary>
		public void Log(string message)
		{
			// TODO add back in a progress system
			//         ProgressPanel.LogMessage(message);
		}

		/// <summary>
		/// Receive a progress report and display it.
		/// </summary>
		public void ReportUploadProgress(string name, long amount, long total)
		{
			var progress = total == 0 ? 1 : (float)amount / total;
			MicroservicesDataModel.Instance.AddLogMessage(name, new LogMessage
			{
				Level = LogLevel.INFO,
				Timestamp = LogMessage.GetTimeDisplay(DateTime.Now),
				Message = $"Uploading Service. service=[{name}] amount=[{amount}] total=[{total}]"
			});
			onProgress?.Invoke(progress, amount, total);
		}

		public async Task<ImageDetails> GetImageId(MicroserviceDescriptor descriptor)
		{
			var command = new GetImageDetailsCommand(descriptor);
			var imageId = await command.StartAsync();

			return imageId;
		}

		public async Task SaveImage(MicroserviceDescriptor descriptor, string outputPath, string imageId = null)
		{
			if (imageId == null)
			{
				imageId = (await GetImageId(descriptor)).imageId;
			}

			var saveImageCommand = new SaveImageCommand(descriptor, imageId, outputPath);

			await saveImageCommand.StartAsync();
		}


		/// <summary>
		/// Upload the specified container to the private Docker registry.
		/// </summary>
		public async Task UploadContainer(MicroserviceDescriptor descriptor, CancellationToken token, Action onSuccess, Action onFailure, string imageId = null)
		{

			// TODO: Either check disk space prior to extraction, or offer a streaming-only solution? ~ACM 2019-12-18
			var filename = FileUtil.GetUniqueTempPathInProject() + ".tar";
			var folder = FileUtil.GetUniqueTempPathInProject();

			try
			{
				if (imageId == null)
				{
					imageId = (await GetImageId(descriptor)).imageId;
				}
				await SaveImage(descriptor, filename, imageId);
				using (var file = File.OpenRead(filename))
				{
					var tar = TarArchive.CreateInputTarArchive(file, Encoding.Default);
					tar.ExtractContents(folder);
				}

				var beamable = BeamEditorContext.Default;
				await beamable.InitializePromise;

				var uploader = new ContainerUploader(beamable, this, descriptor, imageId);
				await uploader.Upload(folder, token);

				onSuccess?.Invoke();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				onFailure?.Invoke();
				throw ex;
			}
			finally
			{
				Directory.Delete(folder, true);
				File.Delete(filename);
			}
		}
	}
}
