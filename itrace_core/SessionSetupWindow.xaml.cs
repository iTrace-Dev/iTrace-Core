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
                string path = folderDialogue.SelectedPath;
                if (path.Length > 23)
                {
                    string[] folders = path.Split('\\');

                    string path_begin = folders[0] + "\\...\\";
                    string path_end = folders[folders.Length - 1];

                    int i = folders.Length - 2;
                    while ((path_begin + folders[i] + "\\" + path_end).Length <= 23)
                    {
                        path_end = folders[i] + "\\" + path_end;
                    }
                    path = path_begin + path_end;
                }
                DataOutputDir.Text = path;
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
