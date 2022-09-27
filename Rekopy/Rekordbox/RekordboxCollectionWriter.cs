using System.Xml;

namespace Rekopy
{
	public class RekordboxCollectionWriter
	{
		private readonly XmlDocument m_XmlDocument;
		private readonly XmlNode m_PlaylistRootNode;

		public RekordboxCollectionWriter(RekordboxXmlDocument rekordboxCollectionReader)
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
		}

		public XmlNode AddPlaylist(IPlaylist playlist, XmlNode parent)
		{
			XmlNode importedPlaylistNode = m_XmlDocument.ImportNode(playlist.Node, playlist.PlaylistType == PlaylistType.Playlist);

			if (parent != null)
			{
				parent.AppendChild(importedPlaylistNode);
			}
			else
			{
				m_PlaylistRootNode.AppendChild(importedPlaylistNode);
			}

			return importedPlaylistNode;
		}

		public void WriteToFile(string filename)
		{
			m_XmlDocument.Save(filename);
		}
	}
}