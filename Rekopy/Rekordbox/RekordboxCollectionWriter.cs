using System.Xml;

namespace Rekopy
{
	public static class RekordboxCollectionWriter
	{
		public static void WriteToFile(string filePath, RekordboxXmlDocument rekordboxCollectionReader, IPlaylist rootPlaylist)
		{
			XmlDocument xmlDocument = new();
			XmlDocument readerDocument = rekordboxCollectionReader.XmlDocument;

			XmlNode headerNode = xmlDocument.ImportNode(readerDocument.FirstChild, false);
			xmlDocument.AppendChild(headerNode);

			XmlNode djPlaylistsNode = xmlDocument.ImportNode(readerDocument.LastChild, false);
			xmlDocument.AppendChild(djPlaylistsNode);

			foreach (XmlNode readerChildNode in readerDocument.LastChild.ChildNodes)
			{
				XmlNode importedChildNode = xmlDocument.ImportNode(readerChildNode, false);
				djPlaylistsNode.AppendChild(importedChildNode);
			}

			XmlNode playlistRootNode = djPlaylistsNode.SelectSingleNode("PLAYLISTS");

			ImportPlaylistXmlNode(xmlDocument, rootPlaylist, playlistRootNode);

			xmlDocument.Save(filePath);
		}

		private static void ImportPlaylistXmlNode(XmlDocument xmlDocument, IPlaylist playlist, XmlNode parentNode)
		{
			XmlNode importedPlaylistNode = xmlDocument.ImportNode(playlist.Node, playlist.PlaylistType == PlaylistType.Playlist);
			parentNode.AppendChild(importedPlaylistNode);

			int includedChildCount = 0;

			foreach (IPlaylist child in playlist.Children)
			{
				if (child.IncludeInExport())
				{
					ImportPlaylistXmlNode(xmlDocument, child, importedPlaylistNode);
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