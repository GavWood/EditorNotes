[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

# EditorNotes

Creates a notes tab for use inside the Editor

- [How to use](#how-to-use)
- [Install](#install)
  - [via Git URL](#via-git-url)
  - Copy Editor scripts to your Unity project. 
- [Configuration](#configuration)

## How to use

Using the BaaWolf menu at the top of your Unity Editor window select Notes. This creates a floating notes window.

Notes are not sent over the Internet. They are stored in Editor preferences so your work is saved locally and private.

Use this how you like. However, it is not intended as a replacement for proper documentation. Best used for a quick *todo* or thoughts.

This comes with no warranty or promise that notes cannot be lost.

## Install

Package should now appear in package manager.

### via Git URL

Open `Packages/manifest.json` with your favorite text editor. Add following line to the dependencies block:
```json
{
  "dependencies": {
    "com.baawolf.EditorNotes": "https://github.com/baawolf/EditorNotes.git"
  }
}
```

## License

MIT License

Copyright © 2024 BaaWolf
