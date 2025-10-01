# QuickLauncher

A lightweight Windows application that allows you to launch single or multiple applications using global keyboard shortcuts.

![QuickLauncher](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

✨ **Global Keyboard Shortcuts** - Launch applications from anywhere with customizable hotkeys  
🚀 **Multiple Apps Per Shortcut** - Launch several applications at once with a single keypress  
🎯 **Custom Naming** - Name your shortcuts like "Development Apps", "Games", etc.  
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
4. Click in the **Shortcut** field and press your desired key combination (e.g., `Ctrl+Alt+D`)
5. Click **OK** to save

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
- **Remove**: Select a shortcut and click "Remove"
- **Launch**: Press the assigned hotkey from anywhere in Windows

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

- [ ] Add application icons to the shortcut list
- [ ] Support for launching apps with command-line arguments
- [ ] Import/Export shortcuts configuration
- [ ] Hotkey conflict detection
- [ ] Delay between launching multiple apps
- [ ] Run applications as administrator option

---

**Made with ❤️ for productivity enthusiasts**
