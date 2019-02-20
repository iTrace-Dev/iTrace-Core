using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            StudyName.Text = sessionData.StudyName;
            ResearcherName.Text = sessionData.ResearcherName;
            ParticipantID.Text = sessionData.ParticipantID;
            DataOutputDir.Text = sessionData.DataRootDir;
        }

        private void DirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialogue = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialogue.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialogue.SelectedPath))
            {
                DataOutputDir.Text = folderDialogue.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.GetInstance().SetupSession(StudyName.Text, ResearcherName.Text, ParticipantID.Text, DataOutputDir.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            StudyName.Text = "";
            ResearcherName.Text = "";
            ParticipantID.Text = "";
            DataOutputDir.Text = "";

            SessionManager.GetInstance().SetupSession(StudyName.Text, ResearcherName.Text, ParticipantID.Text, DataOutputDir.Text);
        }
    }
}
