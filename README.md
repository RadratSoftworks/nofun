<h1><img src="https://github.com/RadratSoftworks/nofun/assets/25717050/821e82c9-81bd-4b4c-bde5-40869701a8d4" alt="Nofun" style="max-height: 3rem;" /> Nofun</h1>

Nofun contains a Mophun emulator, written in C# and currently run under Unity environment.

Currently, the emulator still is not mature for many 3D games, and does not support encrypted/compressed.

## Download

It's recommended to download the emulator through the **Releases** section on the project's Github page.

## Screenshots

| The DaVinci Code         |  Sushi Fighter           |
:-------------------------:|:-------------------------:
![The DaVinci Code - PC](https://github.com/RadratSoftworks/nofun/assets/25717050/d881873b-2c12-4b77-91b0-161b1c4c0598) | ![Sushi Fighter - PC](https://github.com/RadratSoftworks/nofun/assets/25717050/e7ca4f63-4611-4833-a1d9-7edfa4b27e8f)

| Honey Cave 2             |  Rally Pro Contest       |
:-------------------------:|:-------------------------:
![Screenshot_2023-05-24-04-17-14-662_com Radrat nofun](https://github.com/RadratSoftworks/nofun/assets/25717050/65c0b87e-0c15-4e59-ae1e-8afde21f4d20) | ![Screenshot_2023-05-24-04-18-09-280_com Radrat nofun](https://github.com/RadratSoftworks/nofun/assets/25717050/c5b8fb07-605b-40b8-939d-47e6b3a6c4f1)

## Controls

- W,A,S,D/arrow keys/DPad: movement
- Gamepad A/Enter/Right mouse: Fire1
- Gamepad B/Space: Fire2
- Gamepad Select/Esc/Three bars button on screen: Back

## Game configuration

When launching a game for the first time, a configuration screen is opened.

To access and edit the configuration of a running game again, click/touch on the Cog/Gear/Settings button on the screen.

**Note**: for Sony Ericcsion game:
- You may need to select a specific SE phone model in order to run a game (T300/T6x0), else the game will throw the "Terminal not found" error (the game checks for running phone model)
- In addition, you should select System version 1.30 to run Sony Ericssion phone games.

## Portablity

The core code in Scripts folder has also been prepared and designed to allow other backends like SDL2 to integrate in.

## Attributions

Thanks Mr. JaGoTu for providing decompression algorithm.

Thanks Mr. 1upus for helping with games' encryption.

Thanks for the effort of Kahvibreak server for preserving needed resources.

## License

Copyright 2023 Radrat Softworks.

The code is licensed under Apache License 2.0. Visit the [LICENSE](LICENSE) file for more information.
