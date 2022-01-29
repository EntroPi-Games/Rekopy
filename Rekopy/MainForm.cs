using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;

namespace Rekopy
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			Title = "Rekopy";
			MinimumSize = new Size(1024, 512);

			Content = new StackLayout
			{
				Padding = 10
			};

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
			if (RekordboxXmlReader.IsFileRekordboxCollection(fileInfo) == true)
			{
				var content = Content as StackLayout;
				content.Items.Add(filePath);
			}
		}
	}
}
