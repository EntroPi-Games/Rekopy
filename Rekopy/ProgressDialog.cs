using System;
using System.ComponentModel;
using Eto.Forms;

namespace Rekopy
{
	public class ProgressData
	{
		public const float MaxProgress = 1f;
		public float Progress { get; set; } = 0;

		public float RemainingProgress => Math.Max(0, MaxProgress - Progress);
		public bool IsProgressComplete => Progress >= MaxProgress;

		public void CompleteProgress()
		{
			Progress = MaxProgress;
		}
	}

	public class ProgressDialog : Dialog
	{
		public ProgressData Data { get; }

		private readonly ProgressBar m_ProgressBar;
		private readonly UITimer m_Timer;

		public ProgressDialog(int width)
		{
			Data = new ProgressData();

			m_ProgressBar = new ProgressBar { MaxValue = width, Width = width };

			m_Timer = new UITimer(OnUiTimerElapsed);
			m_Timer.Start();

			Closing += OnClosing;
			Closed += OnClosed;

			Title = "Exporting playlists";
			Content = m_ProgressBar;
		}

		private void OnUiTimerElapsed(object sender, EventArgs e)
		{
			m_ProgressBar.Value = (int)Math.Round(Data.Progress * m_ProgressBar.MaxValue);

			if (Data.IsProgressComplete)
			{
				Close();
			}
		}

		private void OnClosing(object sender, CancelEventArgs eventArgs)
		{
			// Prevent users from closing the progress dialog before it is complete.
			eventArgs.Cancel = Data.IsProgressComplete == false;
		}

		private void OnClosed(object sender, EventArgs eventArgs)
		{
			m_Timer.Stop();
			m_Timer.Dispose();
		}
	}
}