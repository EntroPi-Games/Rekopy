using System;
using System.Collections.Generic;
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

		private readonly RekordboxPlaylist m_RootPlaylist;
		private readonly Dictionary<int, XmlNode> m_TrackNodes = new();

		public IPlaylist RootPlaylist => m_RootPlaylist;

		public RekordboxXmlDocument(FileInfo xmlFileInfo)
		{
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

			m_RootPlaylist = new RekordboxPlaylist(null, m_PlaylistRootNode);

			XmlNodeList playlistNodeList = m_PlaylistRootNode.SelectNodes("NODE");
			foreach (XmlNode node in playlistNodeList)
			{
				AddChildrenToPlaylist(node, m_RootPlaylist);
			}
		}

		private void AddChildrenToPlaylist(XmlNode node, RekordboxPlaylist parent)
		{
			RekordboxPlaylist playlist = new(parent, node);
			parent.AddChild(playlist);

			if (node.HasChildNodes == true)
			{
				XmlNodeList childNodeList = node.SelectNodes("NODE");
				foreach (XmlNode childNode in childNodeList)
				{
					AddChildrenToPlaylist(childNode, playlist);
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