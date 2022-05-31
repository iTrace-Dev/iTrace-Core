/********************************************************************************************************************************************************
* @file FILESessionSetupWindow.xaml.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

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
