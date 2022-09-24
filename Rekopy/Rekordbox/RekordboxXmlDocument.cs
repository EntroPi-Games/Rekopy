using System;
using System.IO;
using System.Xml;
using Eto.Forms;

namespace Rekopy
{
	public class RekordboxXmlDocument
	{
		private const string PlaylistRootNodePath = "PLAYLISTS/NODE";

		private readonly XmlDocument m_XmlDocument;

		private XmlNode PlaylistRootNode => m_XmlDocument.DocumentElement.SelectSingleNode(PlaylistRootNodePath);

		public RekordboxXmlDocument(FileInfo xmlFileInfo, TreeGridView treeGridView)
		{
			m_XmlDocument = new();
			m_XmlDocument.Load(xmlFileInfo.FullName);

			TreeGridItemCollection itemCollection = new TreeGridItemCollection();

			TreeGridItem playlistRootItem = new("Playlists", false);
			itemCollection.Add(playlistRootItem);

			XmlNodeList playlistNodeList = PlaylistRootNode.SelectNodes("NODE");
			foreach (XmlNode node in playlistNodeList)
			{
				AddNodeAndChildrenToTreeItem(node, playlistRootItem);
			}

			treeGridView.DataStore = itemCollection;
		}

		private bool TryGetPlaylistNode(IReadOnlyCollection<string> playlistNames, out XmlNode playlistNode)
		{
			playlistNode = PlaylistRootNode;
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