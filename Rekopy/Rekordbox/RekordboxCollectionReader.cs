using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Eto.Forms;

namespace Rekopy
{
	public class RekordboxCollectionReader
	{
		private readonly XmlNode m_CollectionRootNode;
		private readonly XmlNode m_PlaylistRootNode;

		private readonly RekordboxPlaylist m_RootPlaylist;
		private readonly Dictionary<int, XmlNode> m_TrackNodes = new();

		public XmlDocument XmlDocument { get; }
		public IReadOnlyDictionary<int, XmlNode> TrackNodes => m_TrackNodes;
		public IPlaylist RootPlaylist => m_RootPlaylist;

		public RekordboxCollectionReader(FileInfo xmlFileInfo)
		{
			XmlDocument = new();
			XmlDocument.Load(xmlFileInfo.FullName);

			const string collectionRootNodePath = RekordboxXmlNodes.Collection;
			m_CollectionRootNode = XmlDocument.DocumentElement.SelectSingleNode(collectionRootNodePath);

			XmlNodeList trackNodes = m_CollectionRootNode.SelectNodes(RekordboxXmlNodes.Track);
			foreach (XmlNode trackNode in trackNodes)
			{
				if (Int32.TryParse(trackNode.Attributes[RekordboxXmlAttributes.TrackId].Value, out int trackId))
				{
					m_TrackNodes.Add(trackId, trackNode);
				}
			}

			const string playlistRootNodePath = $"{RekordboxXmlNodes.Playlists}/{RekordboxXmlNodes.Node}";
			m_PlaylistRootNode = XmlDocument.DocumentElement.SelectSingleNode(playlistRootNodePath);

			m_RootPlaylist = new RekordboxPlaylist(null, m_PlaylistRootNode);

			XmlNodeList playlistNodeList = m_PlaylistRootNode.SelectNodes(RekordboxXmlNodes.Node);
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
				XmlNodeList childNodeList = node.SelectNodes(RekordboxXmlNodes.Node);
				foreach (XmlNode childNode in childNodeList)
				{
					AddChildrenToPlaylist(childNode, playlist);
				}
			}
		}

		public static bool IsFileRekordboxCollection(FileInfo xmlFileInfo)
		{
			const string firstElementName = RekordboxXmlNodes.DjPlaylists;
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