using System.Collections.Generic;
using System.Xml;

namespace Rekopy
{
	public enum PlaylistType { None, Folder, Playlist };

	public interface IPlaylist
	{
		public string Name { get; }
		public IPlaylist Parent { get; }
		public PlaylistType PlaylistType { get; }
		public XmlNode Node { get; }
		public IReadOnlyCollection<IPlaylist> Children { get; }
		public IReadOnlyCollection<int> TrackIds { get; }
		public bool IsSelected { get; }

		public void SetIsSelected(bool isSelected, bool includeChildren = true);
		public bool IncludeInExport();
	}
}