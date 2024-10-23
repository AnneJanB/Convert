using System;
using System.Windows.Forms;

namespace FormatConverter
{
    public partial class ScopeSelectionForm : Form
    {
        public ScopeSelectionForm()
        {
            InitializeComponent();
        }

        public enum Scope
        {
            EntireSolution,
            CurrentProject,
            AllOpenDocuments
        }

        public Scope SelectedScope { get; private set; }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (entireSolutionRadioButton.Checked)
            {
                SelectedScope = Scope.EntireSolution;
            }
            else if (currentProjectRadioButton.Checked)
            {
                SelectedScope = Scope.CurrentProject;
            }
            else if (allOpenDocumentsRadioButton.Checked)
            {
                SelectedScope = Scope.AllOpenDocuments;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
