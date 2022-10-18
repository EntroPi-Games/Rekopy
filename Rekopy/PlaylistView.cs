using System.Linq;
using Eto.Forms;

namespace Rekopy
{
	public class PlaylistView
	{
		private const int NameColumnIndex = 0;
		private const int ExportColumnIndex = 1;
		private const int PlaylistDataColumnIndex = 2;

		public TreeGridView TreeGridView { get; }

		public PlaylistView(IPlaylist rootPlaylist)
		{
			TreeGridView = new TreeGridView();

			TreeGridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell(NameColumnIndex),
				AutoSize = true,
				MinWidth = 256
			});

			TreeGridView.Columns.Add(new GridColumn
			{
				HeaderText = "Export",
				DataCell = new CheckBoxCell(ExportColumnIndex),
				Editable = false,
				MinWidth = 48,
				MaxWidth = 48
			});

			TreeGridView.CellClick += OnCellClicked;
			TreeGridView.CellDoubleClick += OnCellDoubleClicked;

			SetRootPlaylist(rootPlaylist);
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
				TreeGridView.ReloadData();
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

			TreeGridView.DataStore = itemCollection;
		}

		private void UpdateView()
		{
			TreeGridItemCollection itemCollection = (TreeGridItemCollection)TreeGridView.DataStore;
			foreach (TreeGridItem item in itemCollection.Cast<TreeGridItem>())
			{
				UpdateTreeGridItemRecursively(item);
			}

			TreeGridView.ReloadData();
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