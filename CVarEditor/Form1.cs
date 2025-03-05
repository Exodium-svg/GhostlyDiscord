using Common.Utils;
using System.Diagnostics;
using System.Windows.Forms;

namespace CVarEditor
{
    // All this code is shit but I just don't care
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cVarGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            cVarGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            cVarGrid.Columns.Add("key", "name");
            cVarGrid.Columns.Add("type", "type");
            cVarGrid.Columns.Add("data", "data");

            cVarGrid.ReadOnly = false;       // Allow editing
            cVarGrid.AllowUserToAddRows = true;  // Allow adding new rows
            cVarGrid.AllowUserToDeleteRows = true; // Allow deleting rows
            cVarGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2; // Edit on key press
        }

        private void OpenFilePathWindow(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select CVar file",
                Multiselect = false,
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filepath = dialog.FileName;

                ConsoleVariables cVars = new ConsoleVariables(filepath);

                cVarGrid.Rows.Clear();

                foreach (var kvp in cVars.CVars)
                {
                    ConsoleVariable variable = kvp.Value;

                    string value = variable.Type switch
                    {
                        ConsoleVariableType.String => (string)variable.Value,
                        ConsoleVariableType.Int => variable.Value.ToString(),
                        ConsoleVariableType.Float => variable.Value.ToString(),
                        ConsoleVariableType.Int64 => variable.Value.ToString(),
                        ConsoleVariableType.UInt64 => variable.Value.ToString(),
                        _ => throw new NotSupportedException($"Variable type {(ushort)variable.Type} is not supported.")
                    };

                    cVarGrid.Rows.Add(variable.Name, variable.Type, variable.Value);
                }
            }
        }

        private void SaveFilePathWindow(object sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Save CVar File",
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string filepath = dialog.FileName;

            ConsoleVariables cVars = new ConsoleVariables();

            foreach (DataGridViewRow row in cVarGrid.Rows)
            {
                if (row.IsNewRow) continue;

                string name = row.Cells["key"].Value?.ToString() ?? "";
                string typeStr = row.Cells["type"].Value?.ToString() ?? "";
                string valueStr = row.Cells["data"].Value?.ToString() ?? "";

                if (Enum.TryParse(typeStr, out ConsoleVariableType type))
                {
                    object value = type switch
                    {
                        ConsoleVariableType.String => valueStr,
                        ConsoleVariableType.Int => int.TryParse(valueStr, out int intValue) ? intValue : 0,
                        ConsoleVariableType.Float => float.TryParse(valueStr, out float floatValue) ? floatValue : 0f,
                        ConsoleVariableType.Int64 => long.TryParse(valueStr, out long longValue) ? longValue : 0L,
                        ConsoleVariableType.UInt64 => ulong.TryParse(valueStr, out ulong ulongValue) ? ulongValue : 0UL,
                        _ => throw new NotSupportedException($"Variable type {(ushort)type} is not supported.")
                    };

                    cVars.SetCVar(name, type, value);
                }
            }

            using FileStream fs = new FileStream(filepath, FileMode.Truncate, FileAccess.Write);
            cVars.SaveVariables(fs);
            fs.Close();

            MessageBox.Show("CVar file saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}