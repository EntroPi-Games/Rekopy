using System.Xml;

namespace Rekopy
{
	public class RekordboxCollectionWriter
	{
		private readonly XmlDocument m_XmlDocument;
		private readonly XmlNode m_PlaylistRootNode;

		public RekordboxCollectionWriter(RekordboxXmlDocument rekordboxCollectionReader, IPlaylist rootPlaylist)
		{
			m_XmlDocument = new XmlDocument();

			XmlDocument readerDocument = rekordboxCollectionReader.XmlDocument;

			XmlNode headerNode = m_XmlDocument.ImportNode(readerDocument.FirstChild, false);
			m_XmlDocument.AppendChild(headerNode);

			XmlNode djPlaylistsNode = m_XmlDocument.ImportNode(readerDocument.LastChild, false);
			m_XmlDocument.AppendChild(djPlaylistsNode);

			foreach (XmlNode readerChildNode in readerDocument.LastChild.ChildNodes)
			{
				XmlNode importedChildNode = m_XmlDocument.ImportNode(readerChildNode, false);
				djPlaylistsNode.AppendChild(importedChildNode);
			}

			m_PlaylistRootNode = djPlaylistsNode.SelectSingleNode("PLAYLISTS");

			AddPlaylist(rootPlaylist, m_PlaylistRootNode);
		}

		public void WriteToFile(string filename)
		{
			m_XmlDocument.Save(filename);
		}

		private void AddPlaylist(IPlaylist playlist, XmlNode parentNode)
		{
			XmlNode importedPlaylistNode = m_XmlDocument.ImportNode(playlist.Node, playlist.PlaylistType == PlaylistType.Playlist);
			parentNode.AppendChild(importedPlaylistNode);

			int includedChildCount = 0;

			foreach (IPlaylist child in playlist.Children)
			{
				if (child.IncludeInExport())
				{
					AddPlaylist(child, importedPlaylistNode);
					++includedChildCount;
				}
			}

			if (playlist.PlaylistType == PlaylistType.Folder)
			{
				importedPlaylistNode.Attributes["Count"].Value = includedChildCount.ToString();
			}
		}
	}
}