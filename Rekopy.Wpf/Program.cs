using System;
using Eto.Forms;

namespace Rekopy.Wpf
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(Eto.Platforms.Wpf).Run(new MainForm());
		}
	}
}
