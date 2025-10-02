using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuickLauncher
{
    public static class TemplateProcessor
    {
        public static string ProcessTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                return template;

            string result = template;

            // Date/Time variables
            result = Regex.Replace(result, @"\{\{date\}\}", DateTime.Now.ToString("yyyy-MM-dd"), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{date:([^}]+)\}\}", m => DateTime.Now.ToString(m.Groups[1].Value), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{time\}\}", DateTime.Now.ToString("HH:mm:ss"), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{time:([^}]+)\}\}", m => DateTime.Now.ToString(m.Groups[1].Value), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{datetime\}\}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{datetime:([^}]+)\}\}", m => DateTime.Now.ToString(m.Groups[1].Value), RegexOptions.IgnoreCase);
            
            // Day of week
            result = Regex.Replace(result, @"\{\{dayofweek\}\}", DateTime.Now.DayOfWeek.ToString(), RegexOptions.IgnoreCase);
            
            // Year, Month, Day
            result = Regex.Replace(result, @"\{\{year\}\}", DateTime.Now.Year.ToString(), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{month\}\}", DateTime.Now.Month.ToString(), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{day\}\}", DateTime.Now.Day.ToString(), RegexOptions.IgnoreCase);
            
            // User information
            result = Regex.Replace(result, @"\{\{username\}\}", Environment.UserName, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{computername\}\}", Environment.MachineName, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{userdomain\}\}", Environment.UserDomainName, RegexOptions.IgnoreCase);
            
            // Clipboard content
            if (Regex.IsMatch(result, @"\{\{clipboard\}\}", RegexOptions.IgnoreCase))
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        result = Regex.Replace(result, @"\{\{clipboard\}\}", Clipboard.GetText(), RegexOptions.IgnoreCase);
                    }
                }
                catch
                {
                    // If clipboard access fails, leave the variable as-is
                }
            }
            
            // GUID
            result = Regex.Replace(result, @"\{\{guid\}\}", Guid.NewGuid().ToString(), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{guid:n\}\}", Guid.NewGuid().ToString("N"), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\{\{guid:d\}\}", Guid.NewGuid().ToString("D"), RegexOptions.IgnoreCase);
            
            // Environment variables
            result = Regex.Replace(result, @"\{\{env:([^}]+)\}\}", m =>
            {
                string envVar = m.Groups[1].Value;
                return Environment.GetEnvironmentVariable(envVar) ?? m.Value;
            }, RegexOptions.IgnoreCase);
            
            // Input prompt (blocking)
            result = Regex.Replace(result, @"\{\{input:([^}]+)\}\}", m =>
            {
                string prompt = m.Groups[1].Value;
                return PromptForInput(prompt);
            }, RegexOptions.IgnoreCase);
            
            // Random number
            result = Regex.Replace(result, @"\{\{random:(\d+):(\d+)\}\}", m =>
            {
                if (int.TryParse(m.Groups[1].Value, out int min) && 
                    int.TryParse(m.Groups[2].Value, out int max))
                {
                    Random random = new Random();
                    return random.Next(min, max + 1).ToString();
                }
                return m.Value;
            }, RegexOptions.IgnoreCase);

            return result;
        }

        private static string PromptForInput(string prompt)
        {
            using (var inputDialog = new Form())
            {
                inputDialog.Text = "Input Required";
                inputDialog.Size = new System.Drawing.Size(400, 150);
                inputDialog.StartPosition = FormStartPosition.CenterScreen;
                inputDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputDialog.MaximizeBox = false;
                inputDialog.MinimizeBox = false;

                Label label = new Label
                {
                    Text = prompt,
                    Location = new System.Drawing.Point(10, 10),
                    Size = new System.Drawing.Size(370, 30),
                    AutoSize = false
                };
                inputDialog.Controls.Add(label);

                TextBox textBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 45),
                    Size = new System.Drawing.Size(370, 20)
                };
                inputDialog.Controls.Add(textBox);

                Button okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(225, 80),
                    Size = new System.Drawing.Size(75, 25)
                };
                inputDialog.Controls.Add(okButton);
                inputDialog.AcceptButton = okButton;

                Button cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(305, 80),
                    Size = new System.Drawing.Size(75, 25)
                };
                inputDialog.Controls.Add(cancelButton);
                inputDialog.CancelButton = cancelButton;

                return inputDialog.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
    }
}
