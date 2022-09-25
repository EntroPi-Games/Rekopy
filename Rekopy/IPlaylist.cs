using System.Collections.Generic;

namespace Rekopy
{
	public enum PlaylistType { None, Folder, Playlist };

	public interface IPlaylist
	{
		public string Name { get; }
		public IPlaylist Parent { get; }
		public PlaylistType PlaylistType { get; }
		public IReadOnlyCollection<IPlaylist> Children { get; }
		public IReadOnlyCollection<int> TrackIds { get; }
	}
}