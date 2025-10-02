using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class CommandPalette : Form
    {
        private TextBox _searchBox;
        private ListBox _resultsListBox;
        private Settings _settings;
        private List<SearchResult> _allResults = new List<SearchResult>();
        
        public AppShortcut? SelectedAppShortcut { get; private set; }
        public ClipboardShortcut? SelectedClipboardShortcut { get; private set; }

        public CommandPalette(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadAllItems();
        }

        private void InitializeComponent()
        {
            this.Text = "QuickLauncher - Command Palette";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.KeyPreview = true;

            // Search box
            _searchBox = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(570, 30),
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _searchBox.TextChanged += SearchBox_TextChanged;
            _searchBox.KeyDown += SearchBox_KeyDown;
            this.Controls.Add(_searchBox);

            // Results list
            _resultsListBox = new ListBox
            {
                Location = new Point(10, 50),
                Size = new Size(570, 330),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BackColor = Color.FromArgb(37, 37, 38),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 40
            };
            _resultsListBox.DrawItem += ResultsListBox_DrawItem;
            _resultsListBox.DoubleClick += ResultsListBox_DoubleClick;
            _resultsListBox.KeyDown += ResultsListBox_KeyDown;
            this.Controls.Add(_resultsListBox);

            this.KeyDown += CommandPalette_KeyDown;
        }

        private void LoadAllItems()
        {
            _allResults.Clear();

            // Add app shortcuts
            foreach (var shortcut in _settings.Shortcuts.Where(s => s.IsEnabled))
            {
                string appsDisplay = shortcut.Apps.Count == 1
                    ? System.IO.Path.GetFileNameWithoutExtension(shortcut.Apps[0].ExePath)
                    : $"{shortcut.Apps.Count} applications";

                _allResults.Add(new SearchResult
                {
                    Type = SearchResultType.AppShortcut,
                    Name = shortcut.Name,
                    Description = appsDisplay,
                    Category = shortcut.Category,
                    Hotkey = shortcut.KeyDisplayName,
                    AppShortcut = shortcut
                });
            }

            // Add clipboard shortcuts
            foreach (var clipboardShortcut in _settings.ClipboardShortcuts.Where(c => c.IsEnabled))
            {
                string preview = clipboardShortcut.Content.Length > 50
                    ? clipboardShortcut.Content.Substring(0, 50).Replace("\r\n", " ").Replace("\n", " ") + "..."
                    : clipboardShortcut.Content.Replace("\r\n", " ").Replace("\n", " ");

                _allResults.Add(new SearchResult
                {
                    Type = SearchResultType.ClipboardShortcut,
                    Name = clipboardShortcut.Name,
                    Description = preview,
                    Category = clipboardShortcut.Category,
                    Hotkey = clipboardShortcut.KeyDisplayName,
                    ClipboardShortcut = clipboardShortcut
                });
            }

            RefreshResults("");
        }

        private void SearchBox_TextChanged(object? sender, EventArgs e)
        {
            RefreshResults(_searchBox.Text);
        }

        private void RefreshResults(string searchText)
        {
            _resultsListBox.Items.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all results grouped by category
                var grouped = _allResults.GroupBy(r => r.Category).OrderBy(g => g.Key);
                foreach (var group in grouped)
                {
                    foreach (var result in group)
                    {
                        _resultsListBox.Items.Add(result);
                    }
                }
            }
            else
            {
                // Fuzzy search
                var matches = FuzzySearch(searchText, _allResults);
                foreach (var match in matches.Take(20))
                {
                    _resultsListBox.Items.Add(match);
                }
            }

            if (_resultsListBox.Items.Count > 0)
            {
                _resultsListBox.SelectedIndex = 0;
            }
        }

        private List<SearchResult> FuzzySearch(string searchText, List<SearchResult> items)
        {
            searchText = searchText.ToLower();
            var scored = new List<(SearchResult result, int score)>();

            foreach (var item in items)
            {
                int score = CalculateFuzzyScore(searchText, item);
                if (score > 0)
                {
                    scored.Add((item, score));
                }
            }

            return scored.OrderByDescending(x => x.score).Select(x => x.result).ToList();
        }

        private int CalculateFuzzyScore(string query, SearchResult item)
        {
            int score = 0;
            string name = item.Name.ToLower();
            string description = item.Description.ToLower();
            string category = item.Category.ToLower();

            // Exact match bonus
            if (name.Contains(query))
                score += 100;
            
            if (name.StartsWith(query))
                score += 50;

            // Description match
            if (description.Contains(query))
                score += 30;

            // Category match
            if (category.Contains(query))
                score += 20;

            // Character-by-character fuzzy matching
            int nameIndex = 0;
            int queryIndex = 0;
            bool consecutive = false;
            
            while (nameIndex < name.Length && queryIndex < query.Length)
            {
                if (name[nameIndex] == query[queryIndex])
                {
                    score += consecutive ? 5 : 1;
                    consecutive = true;
                    queryIndex++;
                }
                else
                {
                    consecutive = false;
                }
                nameIndex++;
            }

            // If all characters matched
            if (queryIndex == query.Length)
                score += 10;

            return score;
        }

        private void ResultsListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            var result = (SearchResult)_resultsListBox.Items[e.Index];
            
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(0, 122, 204) : Color.FromArgb(37, 37, 38);
            Color fgColor = Color.White;
            Color descColor = Color.FromArgb(180, 180, 180);

            e.Graphics.FillRectangle(new SolidBrush(bgColor), e.Bounds);

            // Icon/Type indicator
            string typeIcon = result.Type == SearchResultType.AppShortcut ? "🚀" : "📋";
            e.Graphics.DrawString(typeIcon, new Font("Segoe UI", 12F), 
                new SolidBrush(fgColor), e.Bounds.Left + 5, e.Bounds.Top + 10);

            // Name
            e.Graphics.DrawString(result.Name, new Font("Segoe UI", 10F, FontStyle.Bold), 
                new SolidBrush(fgColor), e.Bounds.Left + 35, e.Bounds.Top + 5);

            // Description
            e.Graphics.DrawString(result.Description, new Font("Segoe UI", 8F), 
                new SolidBrush(descColor), e.Bounds.Left + 35, e.Bounds.Top + 22);

            // Hotkey on the right
            if (!string.IsNullOrWhiteSpace(result.Hotkey))
            {
                SizeF hotkeySize = e.Graphics.MeasureString(result.Hotkey, new Font("Segoe UI", 8F));
                e.Graphics.DrawString(result.Hotkey, new Font("Segoe UI", 8F), 
                    new SolidBrush(descColor), e.Bounds.Right - hotkeySize.Width - 10, e.Bounds.Top + 12);
            }

            e.DrawFocusRectangle();
        }

        private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (_resultsListBox.Items.Count > 0)
                {
                    int newIndex = Math.Min(_resultsListBox.SelectedIndex + 1, _resultsListBox.Items.Count - 1);
                    _resultsListBox.SelectedIndex = newIndex;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_resultsListBox.Items.Count > 0)
                {
                    int newIndex = Math.Max(_resultsListBox.SelectedIndex - 1, 0);
                    _resultsListBox.SelectedIndex = newIndex;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelectedItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ResultsListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ExecuteSelectedItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ResultsListBox_DoubleClick(object? sender, EventArgs e)
        {
            ExecuteSelectedItem();
        }

        private void CommandPalette_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void ExecuteSelectedItem()
        {
            if (_resultsListBox.SelectedIndex < 0)
                return;

            var result = (SearchResult)_resultsListBox.SelectedItem;
            
            if (result.Type == SearchResultType.AppShortcut)
            {
                SelectedAppShortcut = result.AppShortcut;
                this.DialogResult = DialogResult.OK;
            }
            else if (result.Type == SearchResultType.ClipboardShortcut)
            {
                SelectedClipboardShortcut = result.ClipboardShortcut;
                this.DialogResult = DialogResult.OK;
            }

            this.Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _searchBox.Focus();
        }
    }

    public enum SearchResultType
    {
        AppShortcut,
        ClipboardShortcut
    }

    public class SearchResult
    {
        public SearchResultType Type { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string Hotkey { get; set; } = "";
        public AppShortcut? AppShortcut { get; set; }
        public ClipboardShortcut? ClipboardShortcut { get; set; }
    }
}
