using System.Linq;
using Eto.Forms;

namespace Rekopy
{
	public class PlaylistView
	{
		private const int NameColumnIndex = 0;
		private const int ExportColumnIndex = 1;
		private const int PlaylistDataColumnIndex = 2;

		private readonly TreeGridView m_TreeGridView;

		public PlaylistView(Panel parentPanel, IPlaylist rootPlaylist)
		{
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

			SetRootPlaylist(rootPlaylist);

			parentPanel.Content = m_TreeGridView;
		}

		private void OnCellClicked(object sender, GridCellMouseEventArgs eventArgs)
		{
			if (eventArgs.GridColumn != null && eventArgs.GridColumn.DataCell is CheckBoxCell)
			{
				TreeGridItem item = eventArgs.Item as TreeGridItem;
				IPlaylist playlist = (IPlaylist)item.GetValue(PlaylistDataColumnIndex);

				playlist.SetIsSelected(!playlist.IsSelected);

				UpdateView();
			}
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

		private void SetRootPlaylist(IPlaylist rootPlaylist)
		{
			TreeGridItem playlistRootItem = new("Playlists", false);
			playlistRootItem.SetValue(PlaylistDataColumnIndex, rootPlaylist);

			TreeGridItemCollection itemCollection = new();
			itemCollection.Add(playlistRootItem);

			foreach (IPlaylist playlist in rootPlaylist.Children)
			{
				AddPlaylistAndChildrenToTreeItem(playlist, playlistRootItem);
			}

			m_TreeGridView.DataStore = itemCollection;
		}

		private void UpdateView()
		{
			TreeGridItemCollection itemCollection = (TreeGridItemCollection)m_TreeGridView.DataStore;
			foreach (TreeGridItem item in itemCollection.Cast<TreeGridItem>())
			{
				UpdateTreeGridItemRecursively(item);
			}

			m_TreeGridView.ReloadData();
		}

		private static void UpdateTreeGridItemRecursively(TreeGridItem item)
		{
			IPlaylist playlist = (IPlaylist)item.GetValue(PlaylistDataColumnIndex);
			item.SetValue(ExportColumnIndex, playlist.IsSelected);

			foreach (TreeGridItem child in item.Children.Cast<TreeGridItem>())
			{
				UpdateTreeGridItemRecursively(child);
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
	}
}