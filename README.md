# WarmTofuMod
This is a mod for Initial Drift Online. The goal is to fix various issues and add some quality of life improvements and even new features

# List of changes
- Added suspension spring force and damper customization. Check garage for options
- Replaced Lobby button with Teleport button that can take you Downhill or Uphill. And also Dowhnill 2 on Irohazaka and Uphill 2 on Haruna
- Huge performance optimizations
- Added realtime tofu delivery timer
- Added a feature to invite players to battle race on every map (except Driftsekai) in both Uphill and Downhill

Irohazaka has additional Uphill and Downhill from the other side of the mountain making a total of 4 directions!

You can also select which one of you leads or chases or you both start in parallel

And also you can enable or disable nitro and collision between both players for the duration of the race

It works only with players who have WarmTofuMod 1.6.0 or newer installed

Press Tab to open player list and click on player button to open race settings. Only players who have the mod will have a button

- Added 3 new tofu delivery routes
 1. Irohazaka: Downhill from the other side of the mountain
 2. Irohazaka: Uphill from the other side of the mountain
 3. Haruna: Uphill from the very end of the map to the beggining and not the mid point

Check feature screenshots to see the locations

- Added map selection to game launcher
- Transmission mode no longer resets when you restart the game
- Camera mode no longer resets when you change maps/restart the game
- Fixed front suspension not saving during tuning
- Fixed broken physics when cars are respawned
- Removed "wait" when respawning
- All Haruna (Akina) map respawns in air e.t.c. fixed
- Rotate the car 180 degrees and place it on an oncoming lane if respawned during uphill tofu delivery on Haruna
- Fixed 3D leaderboard on Akagi downhill
- Fixed Akagi menu leaderboard not working sometimes
- Optimized 3D leaderboard updating
- Fixed player name labels sometimes not rotating properly
- Changed the size of player name labels above cars so they don't block the view that much
- Removed rival distance and arrow indicators during race for the same reason
- Fixed finish not existing when racing on Akagi
- Fixed E button not working when inviting to race battle on Usui and Akagi
- Fixed leaderboards sometimes having empty player name
- Fixed leaderboards hard to scroll
- Submit new tofu time upon delivery and not when you start the game
  
<details>
  <summary>Performance screenshot comparisons</summary>
  Before
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/0b9eee6e-5726-4617-a347-8b8939899f3c">
  After
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/5709990f-edb9-4e56-b9fa-0d59df03a4d9">
</details>
<details>
  <summary>More feature screenshots</summary>
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/82434723-7ca5-400f-af51-5c220b583cc8">
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/4f8555b6-f5cf-4df9-a432-51342c7dc907">
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/9f8f3da9-d24d-4d72-ac25-2496f5e8710d">
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/b39ee05a-3e64-4211-b6c9-b9494cf3e965">
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/cfa45063-5f92-4c9a-b3e5-a9105e0f492e">
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/a08cb924-0757-486d-b76a-dfe66a02c722">
  <img src="https://github.com/Kert/WarmTofuMod/assets/5028587/da36db87-a013-44ab-aa1d-bc564ea43921">
</details>

# How to install
First you need to prepare the game for using mods
## BepInEx 5 Install
Grab an x64 zip with [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21)

Extract the contents into the game folder

Run the game once and close it.

Then Open ```%game folder%/BepInEx/config/BepInEx.cfg``` and change entry point type under ```[Preloader.Entrypoint]``` to ```MonoBehaviour``` like this:
```
[Preloader.Entrypoint]
Type = MonoBehaviour
```

Read [BepInEx 5 Installing](https://docs.bepinex.dev/articles/user_guide/installation/index.html) for more info if you have problems

Then grab ```MMHOOK.zip``` from [here](https://github.com/Kert/WarmTofuMod/releases/tag/MMHOOK) and extract archive into your game folder

It's pre-compiled libraries that were made using [MonoMod HookGen](https://github.com/MonoMod/MonoMod/blob/master/README-RuntimeDetour.md) to make modding easier

## WarmTofuMod Install (or any other mod)
Take mod's *.dll file and put it into ```%game folder%/BepInEx/plugins/```

Compiled WarmTofuMod *.dll files are [here](https://github.com/Kert/WarmTofuMod/releases)

# Compiling mod yourself
Copy original game's ```Assembly-CSharp.dll``` from ```%game folder%\Initial Drift_Data\Managed``` to ```%game folder%/BepInEx/utils/``` and rename it to ```PUBLIC-Assembly-CSharp.dll```

You need a compiler that can target **netstandard2.0**

After that you just run **build.bat**

The project expects that your game folder is ```C:\Program Files (x86)\Steam\steamapps\common\Initial Drift Online\```

You should change all instances of that in **.csproj** and **build.bat** if yours is different 

# Установка
Для начала необходимо подготовить игру к использованию модов
## Установка и настройка BepInEx 5
- Скачать zip-архив [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21) версии x64
- Распаковать архив в папку игры (Найти папку игры можно через Steam: ПКМ на игре в списке->Управление->Посмотреть локальные файлы)
- Запустить игру и закрыть, чтобы появились файлы настроек BepInEx 5
- Открыть любым текстовым редактором (например блокнотом) файл ```BepInEx.cfg``` из папки ```%папка с игрой%/BepInEx/config/```
- Найти в файле текст ```[Preloader.Entrypoint]``` и на следующей строке добавить ```Type = MonoBehaviour```. Должно выглядеть так:
```
[Preloader.Entrypoint]
Type = MonoBehaviour
```
- Скачать ```MMHOOK.zip``` [отсюда](https://github.com/Kert/WarmTofuMod/releases/tag/MMHOOK) и распаковать в папку игры

## Установка мода WarmTofuMod
- Скачать файл мода WarmTofuMod *.dll [тут](https://github.com/Kert/WarmTofuMod/releases)
- Скопировать файл *.dll в папку ```%папка игры%/BepInEx/plugins/```
