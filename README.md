# Verification of Picture MD5

A WPF application for verifying MD5 hashes of image files in a directory.

## Features

- **Window State Persistence**: Automatically saves and restores window position, size, and state
- **Directory Selection**: Browse and select directories containing image files
- **Image Processing**: Loads all JPG and JPEG files from the selected directory
- **MD5 Calculation**: Computes and displays MD5 hash for each image file
- **Image Preview**: Displays selected image from the list
- **Settings Persistence**: Saves the last selected directory between sessions

## Usage

1. **Select Directory**: 
   - Enter a directory path in the textbox or click "Browse..." to select a folder
   - The last selected directory is automatically restored on startup

2. **Load Images**:
   - Click "Load Images" to scan the selected directory for JPG and JPEG files
   - The application will calculate the MD5 hash for each image found

3. **View Results**:
   - The DataGrid displays all found images with their full paths and MD5 hashes
   - Click on any row to preview the corresponding image in the right panel

4. **Window Management**:
   - Window position, size, and state are automatically saved when closing the application
   - These settings are restored when the application is launched again

## Technical Details

- **Framework**: .NET Framework 4.8
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Settings Storage**: `appsettings.json` in the application directory
- **Supported Formats**: JPG, JPEG
- **Hash Algorithm**: MD5 using `System.Security.Cryptography.MD5`

## File Structure

```
VerificationPictureMD5/
├── App.xaml              - Application entry point and settings management
├── App.xaml.cs           - Application logic and settings persistence
├── MainWindow.xaml       - Main window UI definition
├── MainWindow.xaml.cs    - Main window logic and image processing
├── VerificationPictureMD5.csproj - Project configuration
└── README.md             - This documentation file
```

## Requirements

- Windows operating system
- .NET Framework 4.8 or later
- Visual Studio 2019 or later (for development)

## Installation

1. Clone or download the project
2. Open `VerificationPictureMD5.csproj` in Visual Studio
3. Build and run the application

No additional dependencies are required beyond the .NET Framework 4.8.
