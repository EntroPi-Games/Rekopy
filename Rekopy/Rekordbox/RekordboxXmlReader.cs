using System;
using System.IO;
using System.Xml;
using Eto.Forms;

namespace Rekopy
{
	public static class RekordboxXmlReader
	{
		public static bool IsFileRekordboxCollection(FileInfo xmlFile)
		{
			const string firstElementName = "DJ_PLAYLISTS";
			bool isRekordboxCollection = false;

			try
			{
				using (XmlReader reader = XmlReader.Create(xmlFile.FullName))
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
				MessageBox.Show($"Failed to read XML file ({xmlFile.FullName}): {exception}", MessageBoxType.Error);
			}

			return isRekordboxCollection;
		}
	}
}