[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

# EditorNotes

Allows you to quickly open a Documentation folder and enables a notes tab for use inside the Editor that lets you load, edit and save text files.

- [How to use](#how-to-use)
- [Install](#install)
  - [via Git URL](#via-git-url)
  - Copy Editor scripts to your Unity project. 
- [Configuration](#configuration)

## How to use

EditorNotes provides shortcuts for creating a standardized "Documentation" folder in your Unity project. It also lets you quickly open the documentation folder, the project root, or the parent directory.

![image](https://github.com/user-attachments/assets/8547ef44-ee41-4fd5-be35-af5fcd4d9140)

Its built-in editor window allows you to edit and manage notes directly inside Unity. You can quickly jot down important project information, as shown here with placeholder text from Lorem Ipsum. You can save these into your newly created documentation folder and keep safe with source control.

![image](https://github.com/user-attachments/assets/08d91e55-fbc3-4c27-b9af-39a66e6792f3)

## Install

Package should now appear in package manager.

### via Git URL

Open `Packages/manifest.json` with your favorite text editor. Add following line to the dependencies block:
```json
{
  "dependencies": {
    "com.baawolf.EditorNotes": "https://github.com/gavwood/EditorNotes.git"
  }
}
```

### Via Add Package via Package Manager

Open Package Manager from Unity's Window menu.

Choose the option: Add package from git URL, and add (https://github.com/gavwood/EditorNotes.git)

## License

MIT License

Copyright © 2024 BaaWolf
