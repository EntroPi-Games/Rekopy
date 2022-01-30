using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;

namespace Rekopy
{
	public partial class MainForm : Form
	{
		private TreeGridView m_TreeGridView;
		private RekordboxXmlDocument m_RekordboxXmlDocument;

		public MainForm()
		{
			Title = "Rekopy";
			MinimumSize = new Size(1024, 512);

			m_TreeGridView = new TreeGridView();

			m_TreeGridView.Columns.Add(new GridColumn
			{
				DataCell = new TextBoxCell(0),
				AutoSize = true,
				MinWidth = 256
			});

			m_TreeGridView.Columns.Add(new GridColumn
			{
				HeaderText = "Export",
				DataCell = new CheckBoxCell(1),
				Editable = false,
				MinWidth = 48,
				MaxWidth = 48
			});

			Content = m_TreeGridView;

			var openCollectionCommand = new Command { MenuText = "Open Rekordbox XML collection" };
			openCollectionCommand.Executed += (sender, e) => ShowOpenFileDialog();

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
					new SubMenuItem { Text = "&File", Items = { openCollectionCommand } },
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
			//ToolBar = new ToolBar { Items = { clickMe } };
		}

		private void ShowOpenFileDialog()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				CheckFileExists = true
			};

			FileFilter xmlFileFilter = new FileFilter("XML", new string[] { ".xml" });
			openFileDialog.Filters.Add(xmlFileFilter);

			if (openFileDialog.ShowDialog(this) == DialogResult.Ok)
			{
				OpenRekordboxCollectionFile(openFileDialog.FileName);
			}
		}

		private void OpenRekordboxCollectionFile(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (RekordboxXmlDocument.IsFileRekordboxCollection(fileInfo) == true)
			{
				m_RekordboxXmlDocument = new RekordboxXmlDocument(fileInfo, m_TreeGridView);
			}
		}
	}
}
