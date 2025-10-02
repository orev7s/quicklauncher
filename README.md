# QuickLauncher

A lightweight Windows application that allows you to launch single or multiple applications using global keyboard shortcuts.

![QuickLauncher](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-6.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

### 🎯 Core Features

#### App Shortcuts
✨ **Global Keyboard Shortcuts** - Launch applications from anywhere with customizable hotkeys  
🚀 **Multiple Apps Per Shortcut** - Launch several applications at once with a single keypress  
🎯 **Custom Naming & Categories** - Organize shortcuts like "Development Apps", "Games", etc.  
🎨 **Application Icons** - Visual identification with app icons in the shortcut list  
⚙️ **Command-Line Arguments** - Pass custom arguments to each application  
🛡️ **Run as Administrator** - Launch apps with elevated privileges when needed  
⏱️ **Flexible Launch Delays** - Fixed delay, wait for process, window, or network  
⚡ **Parallel/Sequential Launch** - Launch all apps at once or one by one  

#### Paste Shortcuts
📋 **Instant Text Pasting** - Paste predefined text snippets with a keyboard shortcut  
✍️ **Multi-line Support** - Store and paste prompts, emails, code snippets, recipes, and more  
🔄 **Snippet Cycling** - Multiple variations that cycle on each activation  
📝 **Template Variables** - Dynamic content with date, time, username, clipboard, GUID, and more  
⚡ **Quick Access** - Skip clipboard managers and paste directly with your custom hotkey  

### 🎪 Advanced Features

#### Smart Launching
🧠 **Conditional Logic** - Only launch if conditions are met (network, battery, AC power)  
🎯 **Launch Behaviors** - Launch if not running, bring to front, or always launch new instance  
🔍 **Process Detection** - Check if apps are running before launching  
⚡ **Smart Window Management** - Automatically bring existing windows to front  

#### Window Positioning & Multi-Monitor
🖥️ **Multi-Monitor Support** - Position windows on specific monitors  
📐 **Precise Positioning** - Set exact X, Y coordinates and window size  
📏 **Window States** - Maximize, minimize, fullscreen, or normal  
💾 **Remember Layouts** - Automatically save and restore window positions  

#### Profiles & Context Switching
🎭 **Multiple Profiles** - Create different profiles for Work, Gaming, Personal, etc.  
🔄 **Auto-Switch Profiles** - Automatically activate profiles based on context  
⏰ **Time-Based Switching** - Activate profiles at specific times or days  
🖥️ **Monitor-Based** - Switch when specific monitors are connected  
💻 **App-Based** - Activate profiles when certain apps are running  
🌐 **Network-Based** - Switch based on network availability  
🔋 **Battery-Based** - Activate profiles based on battery level or AC power  
⚡ **Priority System** - Higher priority profiles override lower ones  

#### Search & Command Palette
🔍 **Fuzzy Search** - Search all shortcuts by name, description, or category  
⌨️ **Keyboard-Driven** - No mouse needed, navigate with arrow keys  
🎯 **Quick Launch** - Find and execute shortcuts without remembering hotkeys  
📊 **Grouped Results** - Results organized by category  
⚡ **Default Hotkey** - Ctrl+Shift+Space (customizable)  

#### Groups & Organization
📁 **Shortcut Groups** - Organize related shortcuts together  
🔄 **Bulk Enable/Disable** - Toggle entire groups on/off  
🏷️ **Categories** - Categorize shortcuts and groups for better organization  
🎯 **Profile Integration** - Link groups to profiles for context-based activation  

### 🎨 UI & Customization

🌙 **Dark Mode** - Easy on the eyes with modern dark theme  
🔔 **Toast Notifications** - Visual feedback for actions with elegant notifications  
🎨 **Customizable Interface** - Resize windows, customize columns  
📊 **Modern Design** - Clean, flat design that's easy to navigate  

### 🔧 Technical Features

📊 **Comprehensive Logging** - Debug and track all actions  
📁 **Auto Log Cleanup** - Keeps last 7 days automatically  
🔍 **Hotkey Conflict Detection** - Smart warnings for duplicate hotkeys  
📤 **Import/Export** - Share or backup your complete configuration  
💾 **Persistent Settings** - Everything saved automatically  
🔔 **System Tray Integration** - Runs quietly in the background  
⚡ **Lightweight & Fast** - Minimal resource usage, async operations  
🪟 **Start with Windows** - Optional startup with Windows boot

## Screenshots

<img width="693" height="551" alt="{DD83BFA3-E8FE-4839-A8CA-D0DDE777E678}" src="https://github.com/user-attachments/assets/f56b08bc-878a-4019-a659-6d653714bbc5" />


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

### Creating an App Shortcut

1. Click **"Add Shortcut"** in the main window
2. Enter a **name** for your shortcut (e.g., "Development Apps", "Games")
3. Click **"Add App..."** to select one or more `.exe` files
4. For each app, configure:
   - **Arguments**: Optional command-line parameters (e.g., `--fullscreen`)
   - **Run as Administrator**: Check if the app needs elevated privileges
5. Set **Launch Delay** (optional): Milliseconds to wait between launching each app
6. Click in the **Shortcut** field and press your desired key combination (e.g., `Ctrl+Alt+D`)
7. Click **OK** to save

### Creating a Paste Shortcut

1. Switch to the **"Paste Shortcuts"** tab
2. Click **"Add Paste Shortcut"**
3. Enter a **name** for your shortcut (e.g., "Email Signature", "Code Template")
4. Type or paste the **content** you want to paste (supports multiple lines)
5. Click in the **Shortcut** field and press your desired key combination (e.g., `Ctrl+Alt+E`)
6. Click **OK** to save

### Example Use Cases

#### App Shortcuts
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

#### Paste Shortcuts
**Email Signature**
- Name: "Email Signature"
- Content: Your full email signature with contact info
- Hotkey: `Ctrl+Alt+S`

**Code Snippet**
- Name: "React Component"
- Content: Your frequently used React component template
- Hotkey: `Ctrl+Alt+R`

**Customer Response**
- Name: "Thank You Reply"
- Content: Professional thank you message template
- Hotkey: `Ctrl+Alt+T`

**Recipe Instructions**
- Name: "Favorite Recipe"
- Content: Step-by-step cooking instructions
- Hotkey: `Ctrl+Alt+F`

### Managing Shortcuts

#### App Shortcuts Tab
- **Edit**: Select a shortcut and click "Edit" or double-click it
  - Click "Edit..." button next to an app to modify its arguments or admin settings
- **Remove**: Select a shortcut and click "Remove"
- **Launch**: Press the assigned hotkey from anywhere in Windows
- **Import**: Load shortcuts from a JSON file
- **Export**: Save your shortcuts to share or backup

#### Paste Shortcuts Tab
- **Edit**: Select a paste shortcut and click "Edit" or double-click it
- **Remove**: Select a paste shortcut and click "Remove"
- **Paste**: Press the assigned hotkey from anywhere in Windows to instantly paste the text

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

## 📖 Template Variables Reference

Use these variables in your paste shortcuts for dynamic content:

### Date & Time
- `{{date}}` - Current date (yyyy-MM-dd)
- `{{date:FORMAT}}` - Custom date format (e.g., `{{date:MM/dd/yyyy}}`)
- `{{time}}` - Current time (HH:mm:ss)
- `{{datetime}}` - Date and time
- `{{year}}`, `{{month}}`, `{{day}}` - Individual components
- `{{dayofweek}}` - Day name (Monday, Tuesday, etc.)

### System Information
- `{{username}}` - Current Windows username
- `{{computername}}` - Computer name
- `{{userdomain}}` - User domain
- `{{env:VAR_NAME}}` - Environment variable (e.g., `{{env:PATH}}`)

### Dynamic Content
- `{{clipboard}}` - Current clipboard content
- `{{guid}}` - New GUID
- `{{guid:n}}` - GUID without hyphens
- `{{input:Prompt text}}` - Prompt user for input
- `{{random:MIN:MAX}}` - Random number in range

### Examples

**Email Signature:**
```
Best regards,
{{username}}
{{computername}} | {{date}}
```

**Meeting Notes:**
```
Meeting Notes - {{date}}
Time: {{time}}
Attendees: {{input:Enter attendees}}

Agenda:
```

**Code Comment:**
```
// Created by {{username}} on {{date}}
// TODO: {{input:What needs to be done?}}
```

## 🎯 Use Cases

### Developer Workflow
- Launch IDE, browser, terminal, and database tool with one hotkey
- Position each app on specific monitors
- Insert code snippets with template variables
- Auto-switch to "Dev" profile during work hours

### Content Creator
- Launch OBS, editing software, and asset managers
- Cycle through different video descriptions
- Use templates for video metadata
- Profile-based switching for streaming vs editing

### Remote Work
- Launch meeting apps, communication tools, and task managers
- Paste meeting notes templates
- Auto-switch profiles based on time zones
- Conditional launches based on network

### Gaming
- Launch Discord, game launchers, and streaming software
- Paste team strategies or build orders
- Profile activation when gaming apps are detected
- Bring game to front if already running

## 🐛 Troubleshooting

### Hotkey Not Working
- Check if another app is using the same hotkey
- View logs at `%AppData%\QuickLauncher\logs\`
- Try a different key combination
- Restart QuickLauncher

### App Won't Launch
- Verify the exe path is correct
- Check launch conditions are met (network, battery, etc.)
- Review logs for error details
- Ensure you have permissions to run the app

### Profile Not Switching
- Check trigger conditions in profile settings
- Verify profile priority (higher priority wins)
- Enable Debug logging to see evaluation details
- Make sure auto-switching is enabled

### Window Positioning Issues
- Increase the wait time for window to appear
- Some apps don't allow programmatic positioning
- Check that monitor index is correct
- Review window manager logs

### Template Variables Not Working
- Enable "Use Template Variables" checkbox
- Check variable syntax: `{{variable}}`
- Some variables require user input
- Review logs for template processing errors

## 📁 File Locations

- **Settings**: `%AppData%\QuickLauncher\settings.json`
- **Logs**: `%AppData%\QuickLauncher\logs\`
- **Logs kept for**: 7 days (auto-cleanup)

## 🎮 Pro Tips

1. **Start Simple** - Create a few essential shortcuts first
2. **Use Profiles** - Set up work and personal profiles for automatic switching
3. **Command Palette** - When you forget a hotkey, use Ctrl+Shift+Space to search
4. **Smart Launch** - Use "Bring to Front" for apps you keep open all day
5. **Window Layouts** - Perfect for multi-monitor developer setups
6. **Template Power** - Use variables for dynamic content in snippets
7. **Groups** - Organize related shortcuts together for easier management
8. **Logs** - Enable Debug logging when troubleshooting issues

## 🚀 Roadmap

### ✅ Completed
- [x] Global keyboard shortcuts
- [x] Multiple apps per shortcut
- [x] Command-line arguments
- [x] Run as administrator
- [x] Import/Export configuration
- [x] Hotkey conflict detection
- [x] Launch delays
- [x] Paste shortcuts
- [x] Dark mode theme
- [x] Profiles and context switching
- [x] Application window positioning
- [x] Smart launching with conditions
- [x] Template variables
- [x] Command palette / fuzzy search
- [x] Toast notifications
- [x] Comprehensive logging
- [x] Shortcut groups
- [x] Multi-monitor support
- [x] Process detection
- [x] Parallel/sequential launching
- [x] Snippet cycling

### 🔮 Future Ideas
- [ ] Cloud sync for settings
- [ ] Macro recording and playback
- [ ] Script execution support (PowerShell, Python)
- [ ] Web URL shortcuts
- [ ] Statistics dashboard
- [ ] Scheduled backup
- [ ] Plugin/extension system
- [ ] Rich text formatting in paste shortcuts
- [ ] Custom hotkey for profile switching
- [ ] Global search across all text snippets

---

**Made with ❤️ for productivity enthusiasts**
