using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Rekopy
{
	public static class RekordboxCollectionWriter
	{
		public static async Task WriteToFileAsync(string filePath, RekordboxCollectionReader rekordboxCollectionReader, IPlaylist rootPlaylist, ProgressData progressData)
		{
			await Task.Run(() => WriteToFile(filePath, rekordboxCollectionReader, rootPlaylist, progressData));

			progressData.CompleteProgress();
		}

		private static void WriteToFile(string filePath, RekordboxCollectionReader rekordboxCollectionReader, IPlaylist rootPlaylist, ProgressData progressData)
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

			XmlNode playlistRootNode = djPlaylistsNode.SelectSingleNode(RekordboxXmlNodes.Playlists);
			HashSet<int> trackIds = new();

			ImportPlaylistXmlNode(xmlDocument, rootPlaylist, playlistRootNode, trackIds);

			progressData.Progress += 0.05f;

			XmlNode collectionRootNode = djPlaylistsNode.SelectSingleNode(RekordboxXmlNodes.Collection);

			IEnumerable<int> sortedTrackIds = trackIds.OrderBy(id => id);
			ImportTrackIdNodes(xmlDocument, rekordboxCollectionReader, collectionRootNode, sortedTrackIds);

			progressData.Progress += 0.05f;

			int trackCount = trackIds.Count;
			collectionRootNode.Attributes[RekordboxXmlAttributes.Entries].Value = trackCount.ToString();

			string sourceDirectory = Path.GetDirectoryName(rekordboxCollectionReader.SourceFilePath);
			string targetDirectory = Path.GetDirectoryName(filePath);
			CopyTrackFilesAndUpdateNodePaths(collectionRootNode, sourceDirectory, targetDirectory, progressData);

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
				importedPlaylistNode.Attributes[RekordboxXmlAttributes.Count].Value = includedChildCount.ToString();
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

		private static void CopyTrackFilesAndUpdateNodePaths(XmlNode collectionRootNode, string sourceDirectory, string targetDirectory, ProgressData progressData)
		{
			const string collectionDirectoryName = "Collection";

			string collectionTargetDirectory = Path.Combine(targetDirectory, collectionDirectoryName);
			Directory.CreateDirectory(collectionTargetDirectory);

			XmlNodeList trackNodes = collectionRootNode.SelectNodes(RekordboxXmlNodes.Track);

			float progressIncrement = progressData.RemainingProgress / trackNodes.Count;

			for (int i = 0; i < trackNodes.Count; ++i)
			{
				XmlNode trackNode = trackNodes[i];

				if (TryGetTrackFilePath(trackNode, sourceDirectory, out string sourceFilePath) && File.Exists(sourceFilePath))
				{
					if (Int32.TryParse(trackNode.Attributes[RekordboxXmlAttributes.TrackId].Value, out int trackId))
					{
						string filename = Path.GetFileName(sourceFilePath);
						string trackPrefix = $"{trackId} - ";
						if (filename.StartsWith(trackPrefix) == false)
						{
							filename = $"{trackPrefix}{filename}";
						}

						string targetFilePath = Path.Combine(collectionTargetDirectory, filename);

						if (File.Exists(targetFilePath) == false)
						{
							File.Copy(sourceFilePath, targetFilePath);
						}

						string relativeFilePath = $"{collectionDirectoryName}/{filename}";
						SetTrackFilePath(trackNode, relativeFilePath);
					}
				}

				progressData.Progress += progressIncrement;
			}
		}

		private static bool TryGetTrackFilePath(XmlNode trackNode, string sourceDirectory, out string trackFilePath)
		{
			trackFilePath = trackNode.Attributes[RekordboxXmlAttributes.Location].Value;
			if (trackFilePath != null)
			{
				const string absolutePathPrefix = "file://localhost/";
				if (trackFilePath.StartsWith(absolutePathPrefix))
				{
					trackFilePath = trackFilePath.Substring(absolutePathPrefix.Length);
					trackFilePath = Uri.UnescapeDataString(trackFilePath);
				}
				else
				{
					trackFilePath = Path.Combine(sourceDirectory, Uri.UnescapeDataString(trackFilePath));
				}

				return true;
			}

			return false;
		}

		private static void SetTrackFilePath(XmlNode trackNode, string trackFilePath)
		{
			if (Uri.IsWellFormedUriString(trackFilePath, UriKind.Relative) == false)
			{
				trackFilePath = Uri.EscapeUriString(trackFilePath);
			}

			trackNode.Attributes[RekordboxXmlAttributes.Location].Value = trackFilePath;
		}
	}
}