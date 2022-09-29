using Eto.Forms;
using Eto.Drawing;
using System.IO;

namespace Rekopy
{
	public partial class MainForm : Form
	{
		private RekordboxCollectionReader m_RekordboxCollectionReader;

		public MainForm()
		{
			Title = "Rekopy";
			MinimumSize = new Size(1024, 512);

			Command openCollectionCommand = new() { MenuText = "Open Rekordbox XML collection", ToolBarText = "Open Collection" };
			openCollectionCommand.Executed += (sender, e) => OpenRekordboxCollection();

			Command exportPlaylistsCommand = new() { MenuText = "Export selected playlists", ToolBarText = "Export Selected" };
			exportPlaylistsCommand.Executed += (sender, e) => ExportSelectedPlaylists();

			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					new SubMenuItem { Text = "&File", Items = { openCollectionCommand, exportPlaylistsCommand } }
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			ToolBar = new ToolBar { Items = { openCollectionCommand, exportPlaylistsCommand } };
		}

		private void OpenRekordboxCollection()
		{
			OpenFileDialog openFileDialog = new()
			{
				CheckFileExists = true
			};

			FileFilter xmlFileFilter = new("XML", new string[] { ".xml" });
			openFileDialog.Filters.Add(xmlFileFilter);

			if (openFileDialog.ShowDialog(this) == DialogResult.Ok)
			{
				FileInfo fileInfo = new(openFileDialog.FileName);
				if (RekordboxCollectionReader.IsFileRekordboxCollection(fileInfo) == true)
				{
					m_RekordboxCollectionReader = new RekordboxCollectionReader(fileInfo);
					PlaylistView view = new(this, m_RekordboxCollectionReader.RootPlaylist);
				}
			}
		}

		private void ExportSelectedPlaylists()
		{
			if (m_RekordboxCollectionReader != null && m_RekordboxCollectionReader.RootPlaylist != null)
			{
				if (m_RekordboxCollectionReader.RootPlaylist.IncludeInExport())
				{
					SaveFileDialog saveFileDialog = new();

					FileFilter xmlFileFilter = new("XML", new string[] { ".xml" });
					saveFileDialog.Filters.Add(xmlFileFilter);

					if (saveFileDialog.ShowDialog(this) == DialogResult.Ok)
					{
						RekordboxCollectionWriter.WriteToFile(saveFileDialog.FileName, m_RekordboxCollectionReader, m_RekordboxCollectionReader.RootPlaylist);
					}
				}
				else
				{
					MessageBox.Show("No playlists selected for export.", "Nothing selected", MessageBoxType.Information);
				}
			}
			else
			{
				MessageBox.Show("Please open a Rekordbox collection before exporting.", "No collection opened", MessageBoxType.Information);
			}
		}
	}
}
