# QuickLauncher

A lightweight Windows application that allows you to launch single or multiple applications using global keyboard shortcuts.

![QuickLauncher](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

✨ **Global Keyboard Shortcuts** - Launch applications from anywhere with customizable hotkeys  
🚀 **Multiple Apps Per Shortcut** - Launch several applications at once with a single keypress  
🎯 **Custom Naming** - Name your shortcuts like "Development Apps", "Games", etc.  
🎨 **Application Icons** - Visual identification with app icons in the shortcut list  
⚙️ **Command-Line Arguments** - Pass custom arguments to each application  
🛡️ **Run as Administrator** - Launch apps with elevated privileges when needed  
⏱️ **Launch Delay** - Configure delay between launching multiple apps  
🔍 **Hotkey Conflict Detection** - Warns when hotkeys conflict with existing shortcuts  
📤 **Import/Export** - Share or backup your shortcuts configuration  
💾 **Persistent Settings** - Your shortcuts are automatically saved and loaded  
🔔 **System Tray Integration** - Runs quietly in the background  
⚡ **Lightweight** - Minimal resource usage  
🪟 **Start with Windows** - Optional startup with Windows boot

## Screenshots

### Main Interface
The main window displays all your configured shortcuts with their assigned hotkeys.

### Add/Edit Shortcuts
Easily add multiple applications to a single shortcut and assign a global hotkey.

## Installation

### Requirements
- Windows 7 or later
- .NET 6.0 Runtime or later

### Download & Run
1. Download the latest release from the [Releases](https://github.com/orev7s/quicklauncher/releases) page
2. Extract the archive
3. Run `QuickLauncher.exe`

### Build from Source
```bash
# Clone the repository
git clone https://github.com/orev7s/quicklauncher.git
cd quicklauncher

# Build the project
dotnet build -c Release

# Run the application
dotnet run -c Release
```

## Usage

### Creating a Shortcut

1. Click **"Add Shortcut"** in the main window
2. Enter a **name** for your shortcut (e.g., "Development Apps", "Games")
3. Click **"Add App..."** to select one or more `.exe` files
4. For each app, configure:
   - **Arguments**: Optional command-line parameters (e.g., `--fullscreen`)
   - **Run as Administrator**: Check if the app needs elevated privileges
5. Set **Launch Delay** (optional): Milliseconds to wait between launching each app
6. Click in the **Shortcut** field and press your desired key combination (e.g., `Ctrl+Alt+D`)
7. Click **OK** to save

### Example Use Cases

**Development Environment**
- Name: "Dev Apps"
- Apps: Visual Studio Code, Chrome, Postman, Docker Desktop
- Hotkey: `Ctrl+Alt+D`

**Gaming Setup**
- Name: "Gaming"
- Apps: Discord, Steam, Spotify
- Hotkey: `Ctrl+Alt+G`

**Work Mode**
- Name: "Work"
- Apps: Outlook, Teams, Excel, Chrome
- Hotkey: `Ctrl+Alt+W`

### Managing Shortcuts

- **Edit**: Select a shortcut and click "Edit" or double-click it
  - Click "Edit..." button next to an app to modify its arguments or admin settings
- **Remove**: Select a shortcut and click "Remove"
- **Launch**: Press the assigned hotkey from anywhere in Windows
- **Import**: Load shortcuts from a JSON file
- **Export**: Save your shortcuts to share or backup

### Settings

- **Start with Windows**: Check the box to automatically start QuickLauncher when Windows boots
- **System Tray**: The application minimizes to the system tray when closed. Right-click the tray icon to access quick options.

## Keyboard Shortcuts

QuickLauncher supports the following modifier keys:
- `Ctrl` (Control)
- `Alt` (Alternate)
- `Shift`

You must use **at least one modifier key** combined with a regular key (e.g., `Ctrl+Alt+A`).

### Hotkey Examples
- `Ctrl+Alt+D` - Launch development apps
- `Ctrl+Shift+G` - Launch games
- `Alt+Shift+W` - Launch work applications
- `Ctrl+Alt+1` - Quick launch set 1

## Advanced Features

### Command-Line Arguments

Launch applications with specific parameters:
- Chrome with a specific profile: `--profile-directory="Profile 1"`
- Games in fullscreen: `--fullscreen` or `-windowed`
- Custom config files: `/config=myconfig.ini`

### Run as Administrator

Some applications require elevated privileges. Enable "Run as Administrator" for:
- System utilities
- Applications that modify system settings
- Development tools requiring elevated access

**Note**: You'll see a UAC prompt each time these apps are launched.

### Launch Delay

When launching multiple apps, add a delay to prevent:
- System resource overload
- Apps competing for initialization
- Database connection conflicts

Recommended delays:
- Light apps (browsers, editors): 100-500ms
- Heavy apps (IDEs, databases): 1000-2000ms

### Import/Export Shortcuts

**Export**: Save your shortcuts configuration to share with team members or backup
1. Click "Export..."
2. Choose location and filename
3. Share the JSON file

**Import**: Load shortcuts from a file
1. Click "Import..."
2. Select the JSON file
3. Confirm to replace current shortcuts

### Hotkey Conflict Detection

QuickLauncher automatically detects when you try to use a hotkey that's already assigned to another shortcut. You'll be warned and can choose to:
- Use a different hotkey
- Override the existing shortcut (the old one will stop working)

## Configuration

Settings are automatically saved to:
```
%APPDATA%\QuickLauncher\settings.json
```

The configuration file contains:
- Application shortcuts and their hotkeys
- Startup preferences
- Window settings

## Technical Details

- **Framework**: .NET 6.0 (Windows Forms)
- **Language**: C#
- **Dependencies**: 
  - Newtonsoft.Json (13.0.3)
- **Architecture**: Single-process application with global hotkey registration via Windows API

## Troubleshooting

### Hotkey doesn't work
- Ensure the hotkey isn't already in use by another application
- Try using a different key combination
- Restart QuickLauncher after making changes

### Application won't start
- Verify .NET 6.0 Runtime is installed
- Check if the application is blocked by antivirus software
- Run as administrator if necessary

### Apps aren't launching
- Verify all application paths exist
- Check that you have permission to execute the applications
- Review the error message displayed

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by the need for efficient workflow automation
- Built with Windows Forms for maximum compatibility

## Support

If you encounter any issues or have questions:
- Open an [issue](https://github.com/orev7s/quicklauncher/issues) on GitHub
- Check existing issues for solutions

## Roadmap

- [x] Add application icons to the shortcut list
- [x] Support for launching apps with command-line arguments
- [x] Import/Export shortcuts configuration
- [x] Hotkey conflict detection
- [x] Delay between launching multiple apps
- [x] Run applications as administrator option
- [ ] Dark mode theme
- [ ] Profiles for different work contexts
- [ ] Application window positioning
- [ ] Startup delay for individual apps
- [ ] Hotkey recording/playback

---

**Made with ❤️ for productivity enthusiasts**
