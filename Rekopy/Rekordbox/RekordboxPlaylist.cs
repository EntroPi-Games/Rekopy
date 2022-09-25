using System;
using System.Collections.Generic;
using System.Xml;

namespace Rekopy
{
	public class RekordboxPlaylist : IPlaylist
	{
		private const string NameAttributeKey = "Name";

		private readonly HashSet<RekordboxPlaylist> m_Children = new();
		private readonly List<int> m_TrackIds = new();

		public string Name => Node != null ? Node.Attributes[NameAttributeKey].Value : string.Empty;
		public IPlaylist Parent { get; }
		public PlaylistType PlaylistType { get; }
		public XmlNode Node { get; }
		public IReadOnlyCollection<IPlaylist> Children => m_Children;
		public IReadOnlyCollection<int> TrackIds => m_TrackIds;

		public RekordboxPlaylist(RekordboxPlaylist parent, XmlNode node)
		{
			Parent = parent;
			Node = node;

			const string typeAttributeKey = "Type";
			if (Int32.TryParse(Node.Attributes[typeAttributeKey].Value, out int typeValue))
			{
				PlaylistType = typeValue > 0 ? PlaylistType.Playlist : PlaylistType.Folder;
			}
			else
			{
				PlaylistType = PlaylistType.None;
			}
		}

		public void AddChild(RekordboxPlaylist playlist)
		{
			m_Children.Add(playlist);
		}

		public void AddTrackId(int trackId)
		{
			m_TrackIds.Add(trackId);
		}
	}
}