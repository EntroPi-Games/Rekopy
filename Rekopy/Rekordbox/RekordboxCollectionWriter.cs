using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Rekopy
{
	public static class RekordboxCollectionWriter
	{
		public static void WriteToFile(string filePath, RekordboxCollectionReader rekordboxCollectionReader, IPlaylist rootPlaylist)
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
			HashSet<int> trackIds = new();

			ImportPlaylistXmlNode(xmlDocument, rootPlaylist, playlistRootNode, trackIds);

			XmlNode collectionRootNode = djPlaylistsNode.SelectSingleNode("COLLECTION");

			IEnumerable<int> sortedTrackIds = trackIds.OrderBy(id => id);
			ImportTrackIdNodes(xmlDocument, rekordboxCollectionReader, collectionRootNode, sortedTrackIds);

			int trackCount = trackIds.Count;
			collectionRootNode.Attributes["Entries"].Value = trackCount.ToString();

			xmlDocument.Save(filePath);
		}

		private static void ImportPlaylistXmlNode(XmlDocument xmlDocument, IPlaylist playlist, XmlNode parentNode, HashSet<int> trackIds)
		{
			XmlNode importedPlaylistNode = xmlDocument.ImportNode(playlist.Node, playlist.PlaylistType == PlaylistType.Playlist);
			parentNode.AppendChild(importedPlaylistNode);

			int includedChildCount = 0;

			foreach (IPlaylist child in playlist.Children)
			{
				if (child.IncludeInExport())
				{
					ImportPlaylistXmlNode(xmlDocument, child, importedPlaylistNode, trackIds);
					++includedChildCount;
				}
			}

			if (playlist.PlaylistType == PlaylistType.Playlist)
			{
				foreach (int trackId in playlist.TrackIds)
				{
					if (trackIds.Contains(trackId) == false)
					{
						trackIds.Add(trackId);
					}
				}
			}
			else if (playlist.PlaylistType == PlaylistType.Folder)
			{
				importedPlaylistNode.Attributes["Count"].Value = includedChildCount.ToString();
			}
		}

		private static void ImportTrackIdNodes(XmlDocument xmlDocument, RekordboxCollectionReader rekordboxCollectionReader, XmlNode collectionRootNode, IEnumerable<int> trackIds)
		{
			foreach (int trackId in trackIds)
			{
				if (rekordboxCollectionReader.TrackNodes.TryGetValue(trackId, out XmlNode trackNode))
				{
					XmlNode importedTrackNode = xmlDocument.ImportNode(trackNode, true);
					collectionRootNode.AppendChild(importedTrackNode);
				}
			}
		}
	}
}