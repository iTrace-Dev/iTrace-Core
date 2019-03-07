using System;
using System.Windows;

namespace iTrace_Core
{
    /// <summary>
    /// Interaction logic for SessionSetupWindow.xaml
    /// </summary>
    public partial class SessionSetupWindow : Window
    {
        public SessionSetupWindow()
        {
            SessionManager sessionData = SessionManager.GetInstance();
            InitializeComponent();
            TaskName.Text = sessionData.TaskName;
            ResearcherName.Text = sessionData.ResearcherName;
            ParticipantID.Text = sessionData.ParticipantID;
            DataOutputDir.Text = sessionData.DataRootDir;
        }

        private void DirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialogue = new System.Windows.Forms.FolderBrowserDialog();
            folderDialogue.SelectedPath = SessionManager.GetInstance().DataRootDir;

            System.Windows.Forms.DialogResult result = folderDialogue.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialogue.SelectedPath))
            {
                DataOutputDir.Text = folderDialogue.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.GetInstance().SetupSession(TaskName.Text, ResearcherName.Text, ParticipantID.Text, DataOutputDir.Text);
            Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TaskName.Text = "";
            ResearcherName.Text = "";
            ParticipantID.Text = "";
            DataOutputDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SessionManager.GetInstance().SetupSession(TaskName.Text, ResearcherName.Text, ParticipantID.Text, DataOutputDir.Text);
        }
    }
}
