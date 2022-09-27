using Eto.Forms;
using Eto.Drawing;
using System.IO;
using System.Linq;
using System;
using System.Xml;

namespace Rekopy
{
	public partial class MainForm : Form
	{
		private const int NameColumnIndex = 0;
		private const int ExportColumnIndex = 1;
		private const int PlaylistDataColumnIndex = 2;

		private readonly TreeGridView m_TreeGridView;
		private RekordboxXmlDocument m_RekordboxXmlDocument;

		public MainForm()
		{
			Title = "Rekopy";
			MinimumSize = new Size(1024, 512);

			m_TreeGridView = new TreeGridView();

			m_TreeGridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell(NameColumnIndex),
				AutoSize = true,
				MinWidth = 256
			});

			m_TreeGridView.Columns.Add(new GridColumn
			{
				HeaderText = "Export",
				DataCell = new CheckBoxCell(ExportColumnIndex),
				Editable = false,
				MinWidth = 48,
				MaxWidth = 48
			});

			m_TreeGridView.CellClick += OnCellClicked;
			m_TreeGridView.CellDoubleClick += OnCellDoubleClicked;
			Content = m_TreeGridView;

			Command openCollectionCommand = new() { MenuText = "Open Rekordbox XML collection", ToolBarText = "Open Collection" };
			openCollectionCommand.Executed += (sender, e) => ShowOpenFileDialog();

			Command exportPlaylistsCommand = new() { MenuText = "Export Selected playlist to new collection", ToolBarText = "Export Selected Playlists" };
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
					// File submenu
					new SubMenuItem { Text = "&File", Items = { openCollectionCommand, exportPlaylistsCommand } },
					// new SubMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
					// new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
				},
				ApplicationItems =
				{
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar			
			ToolBar = new ToolBar { Items = { openCollectionCommand, exportPlaylistsCommand } };
		}

		private void OnCellClicked(object sender, GridCellMouseEventArgs eventArgs)
		{
			if (eventArgs.GridColumn != null && eventArgs.GridColumn.DataCell is CheckBoxCell)
			{
				TreeGridItem item = eventArgs.Item as TreeGridItem;
				bool originalValue = (bool)item.Values[1];
				bool value = !originalValue;

				SetItemAndChildrenValuesRecursively(item, value);

				if (value == false)
				{
					SetParentValuesRecursively(item, value);
				}

				m_TreeGridView.ReloadData();
			}
		}

		private static void SetItemAndChildrenValuesRecursively(TreeGridItem item, bool value)
		{
			SetItemValue(item, value);

			foreach (TreeGridItem child in item.Children.Cast<TreeGridItem>())
			{
				SetItemAndChildrenValuesRecursively(child, value);
			}
		}

		private static void SetParentValuesRecursively(TreeGridItem item, bool value)
		{
			if (item.Parent != null && item.Parent is TreeGridItem parentItem)
			{
				SetItemValue(parentItem, value);
				SetParentValuesRecursively(parentItem, value);
			}
		}

		private static void SetItemValue(TreeGridItem item, bool value)
		{
			item.Values[1] = value;
		}

		private void OnCellDoubleClicked(object sender, GridCellMouseEventArgs eventArgs)
		{
			if (eventArgs.GridColumn != null && eventArgs.GridColumn.DataCell is TextBoxCell)
			{
				TreeGridItem item = eventArgs.Item as TreeGridItem;
				item.Expanded = !item.Expanded;
				m_TreeGridView.ReloadData();
			}
		}

		private void ShowOpenFileDialog()
		{
			OpenFileDialog openFileDialog = new()
			{
				CheckFileExists = true
			};

			FileFilter xmlFileFilter = new("XML", new string[] { ".xml" });
			openFileDialog.Filters.Add(xmlFileFilter);

			if (openFileDialog.ShowDialog(this) == DialogResult.Ok)
			{
				OpenRekordboxCollectionFile(openFileDialog.FileName);
			}
		}

		private void OpenRekordboxCollectionFile(string filePath)
		{
			FileInfo fileInfo = new(filePath);
			if (RekordboxXmlDocument.IsFileRekordboxCollection(fileInfo) == true)
			{
				m_RekordboxXmlDocument = new RekordboxXmlDocument(fileInfo);

				TreeGridItem playlistRootItem = new("Playlists", false);
				playlistRootItem.SetValue(PlaylistDataColumnIndex, m_RekordboxXmlDocument.RootPlaylist);

				TreeGridItemCollection itemCollection = new();
				itemCollection.Add(playlistRootItem);

				foreach (IPlaylist playlist in m_RekordboxXmlDocument.RootPlaylist.Children)
				{
					AddPlaylistAndChildrenToTreeItem(playlist, playlistRootItem);
				}

				m_TreeGridView.DataStore = itemCollection;
			}
		}

		private static void AddPlaylistAndChildrenToTreeItem(IPlaylist playlist, TreeGridItem parentItem)
		{
			TreeGridItem item = new(playlist.Name, false);
			item.SetValue(PlaylistDataColumnIndex, playlist);

			parentItem.Children.Add(item);

			foreach (IPlaylist nestedPlaylist in playlist.Children)
			{
				AddPlaylistAndChildrenToTreeItem(nestedPlaylist, item);
			}
		}

		private void ExportSelectedPlaylists()
		{
			TreeGridItemCollection itemCollection = (TreeGridItemCollection)m_TreeGridView.DataStore;
			if (itemCollection != null && itemCollection.Count > 0)
			{
				TreeGridItem rootPlaylistItem = (TreeGridItem)itemCollection[0];
				if (ShouldItemPlaylistBeExported(rootPlaylistItem))
				{
					SaveFileDialog saveFileDialog = new();

					FileFilter xmlFileFilter = new("XML", new string[] { ".xml" });
					saveFileDialog.Filters.Add(xmlFileFilter);

					if (saveFileDialog.ShowDialog(this) == DialogResult.Ok)
					{
						RekordboxCollectionWriter writer = new(m_RekordboxXmlDocument);

						ExportPlaylistItem(writer, null, rootPlaylistItem);

						writer.WriteToFile(saveFileDialog.FileName);
					}
				}
				else
				{
					MessageBox.Show("No playlists selected for export.", "Nothing selected", MessageBoxType.Information);
				}
			}
			else
			{
				MessageBox.Show("Please open a rekordbox collection before exporting.", "Can't export", MessageBoxType.Information);
			}
		}

		private static bool ShouldItemPlaylistBeExported(TreeGridItem item)
		{
			bool shouldExport = false;

			if (item.GetValue(PlaylistDataColumnIndex) is IPlaylist playlist)
			{
				if (playlist.PlaylistType == PlaylistType.Playlist)
				{
					shouldExport = GetExportValue(item);
				}
				else if (playlist.PlaylistType == PlaylistType.Folder)
				{
					foreach (TreeGridItem childItem in item.Children.Cast<TreeGridItem>())
					{
						if (ShouldItemPlaylistBeExported(childItem))
						{
							shouldExport = true;
							break;
						}
					}
				}
			}

			return shouldExport;
		}

		private static void ExportPlaylistItem(RekordboxCollectionWriter writer, XmlNode parent, TreeGridItem item)
		{
			if (item.GetValue(PlaylistDataColumnIndex) is IPlaylist playlist)
			{
				XmlNode playlistNode = writer.AddPlaylist(playlist, parent);

				foreach (TreeGridItem childItem in item.Children.Cast<TreeGridItem>())
				{
					if (ShouldItemPlaylistBeExported(childItem))
					{
						ExportPlaylistItem(writer, playlistNode, childItem);
					}
				}
			}
		}

		private static bool GetExportValue(TreeGridItem item)
		{
			return (bool)item.GetValue(ExportColumnIndex);
		}
	}
}
