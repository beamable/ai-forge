using System.IO;
using System.Threading.Tasks;

namespace Beamable.Server.Editor.Uploader
{
	/// <summary>
	/// One chunk of an upload.
	/// </summary>
	public class FileChunk
	{
		public readonly bool IsLast;
		public readonly long Start;
		public long End => Start + Length - 1;
		public readonly long Length;
		public readonly Stream Stream;
		public readonly long FullLength;

		// TODO: How can we make this more efficient? ~ACM 2019-12-18
		private FileChunk(Stream stream, long position, long length, bool isLast, long fullLength)
		{
			Stream = stream;
			Start = position;
			Length = length;
			IsLast = isLast;
			FullLength = fullLength;
		}

		/// <summary>
		/// Create a FileChunk from a parent stream, using the stream's position
		/// and the supplied length as range parameters.
		/// </summary>
		/// <param name="parent">Stream to build from.</param>
		/// <param name="length">Maximum chunk length.</param>
		/// <returns>Resulting chunk with range information.</returns>
		public static async Task<FileChunk> FromParent(Stream parent, long length)
		{
			if (length >= parent.Length && parent.Position == 0)
			{
				return WholeStream(parent);
			}
			var start = parent.Position;
			var isLast = false;
			var chunkLength = length;
			if (parent.Position + chunkLength > parent.Length)
			{
				chunkLength = parent.Length - parent.Position;
				isLast = true;
			}
			var buffer = new byte[chunkLength];
			await parent.ReadAsync(buffer, 0, (int)chunkLength);
			var stream = new MemoryStream(buffer);
			return new FileChunk(stream, start, chunkLength, isLast, parent.Length);
		}

		/// <summary>
		/// Make a chunk that encompasses the entirety of the supplied stream.
		/// </summary>
		/// <param name="parent">Stream to build from.</param>
		/// <returns>Chunk with range information.</returns>
		private static FileChunk WholeStream(Stream parent)
		{
			return new FileChunk(parent, 0, parent.Length, true, parent.Length);
		}
	}
}
