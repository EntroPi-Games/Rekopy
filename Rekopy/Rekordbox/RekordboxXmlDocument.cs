using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Eto.Forms;

namespace Rekopy
{
	public class RekordboxXmlDocument
	{
		private readonly XmlDocument m_XmlDocument;
		private readonly XmlNode m_CollectionRootNode;
		private readonly XmlNode m_PlaylistRootNode;
		private readonly Dictionary<int, XmlNode> m_TrackNodes = new();

		public RekordboxXmlDocument(FileInfo xmlFileInfo, TreeGridView treeGridView)
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();

			m_XmlDocument = new();
			m_XmlDocument.Load(xmlFileInfo.FullName);

			const string collectionRootNodePath = "COLLECTION";
			m_CollectionRootNode = m_XmlDocument.DocumentElement.SelectSingleNode(collectionRootNodePath);

			XmlNodeList trackNodes = m_CollectionRootNode.SelectNodes("TRACK");
			foreach (XmlNode trackNode in trackNodes)
			{
				if (Int32.TryParse(trackNode.Attributes["TrackID"].Value, out int trackId))
				{
					m_TrackNodes.Add(trackId, trackNode);
				}
			}

			const string playlistRootNodePath = "PLAYLISTS/NODE";
			m_PlaylistRootNode = m_XmlDocument.DocumentElement.SelectSingleNode(playlistRootNodePath);

			TreeGridItemCollection itemCollection = new();

			TreeGridItem playlistRootItem = new("Playlists", false);
			itemCollection.Add(playlistRootItem);

			XmlNodeList playlistNodeList = m_PlaylistRootNode.SelectNodes("NODE");
			foreach (XmlNode node in playlistNodeList)
			{
				AddNodeAndChildrenToTreeItem(node, playlistRootItem);
			}

			treeGridView.DataStore = itemCollection;

			stopwatch.Stop();
			Console.WriteLine($"Loading rekordbox playlist took {stopwatch.Elapsed} seconds");
		}

		private bool TryGetPlaylistNode(IReadOnlyCollection<string> playlistNames, out XmlNode playlistNode)
		{
			playlistNode = m_PlaylistRootNode;
			bool succeeded = true;

			foreach (string playlistName in playlistNames)
			{
				playlistNode = playlistNode.SelectSingleNode($"NODE[@Name='{playlistName}']");
				if (playlistNode == null)
				{
					succeeded = false;
					break;
				}
			}

			return succeeded;
		}
		private void AddNodeAndChildrenToTreeItem(XmlNode node, TreeGridItem parentItem)
		{
			TreeGridItem item = new(node.Attributes["Name"].Value, false);
			parentItem.Children.Add(item);

			if (node.HasChildNodes == true)
			{
				XmlNodeList childNodeList = node.SelectNodes("NODE");
				foreach (XmlNode childNode in childNodeList)
				{
					AddNodeAndChildrenToTreeItem(childNode, item);
				}
			}
		}

		public static bool IsFileRekordboxCollection(FileInfo xmlFileInfo)
		{
			const string firstElementName = "DJ_PLAYLISTS";
			bool isRekordboxCollection = false;

			try
			{
				using (XmlReader reader = XmlReader.Create(xmlFileInfo.FullName))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							isRekordboxCollection = reader.Name == firstElementName;
							break;
						}
					}
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show($"Failed to read XML file ({xmlFileInfo.FullName}): {exception}", MessageBoxType.Error);
			}

			return isRekordboxCollection;
		}
	}
}