using System;
using System.IO;
using System.Xml;
using Eto.Forms;

namespace Rekopy
{
	public class RekordboxXmlDocument
	{
		private readonly XmlDocument m_XmlDocument;

		public RekordboxXmlDocument(FileInfo xmlFileInfo, TreeGridView treeGridView)
		{
			m_XmlDocument = new XmlDocument();
			m_XmlDocument.Load(xmlFileInfo.FullName);

			XmlNode rootNode = m_XmlDocument.DocumentElement;
			XmlNodeList nodeList = rootNode.SelectNodes("PLAYLISTS/NODE/NODE");

			TreeGridItemCollection itemCollection = new TreeGridItemCollection();

			TreeGridItem playlistRootItem = new TreeGridItem("Playlists", false);
			itemCollection.Add(playlistRootItem);
			foreach (XmlNode node in nodeList)
			{
				AddNodeAndChildrenToTreeItem(node, playlistRootItem);
			}

			treeGridView.DataStore = itemCollection;
		}

		private void AddNodeAndChildrenToTreeItem(XmlNode node, TreeGridItem parentItem)
		{
			TreeGridItem item = new TreeGridItem(node.Attributes["Name"].Value, false);
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