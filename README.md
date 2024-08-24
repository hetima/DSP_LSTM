# LSTM - Logistics Station Traffic Manager

Mod for Dyson Sphere Program. Needs BepInEx.

## Recent Changes

### v0.9.1
- Fix scrollbar on log window

### v0.9.0
- Remove `TrafficLogic` and `One-time Demand` for 0.10.30.22292
- Fix button on starmap


## About

Lists the supply and demand balance of the Logistics Station by item and by planet.

- Show overall status for each item
- Show the status of all stations on specific planet
- Show station storage ratio and total actual amounts information in the statistics panel

- Display the target planet in the starmap (Universe Exploration lv4 is required)
- Display navigation of the target station location
- Open station window directly (right-click locate button)(only current local planet)
- Display station contents and empty slot count as icon
- Display traffic logs

How to open a window
- from keyboard shortcut (default is LCtrl+T)
- from button added on station window
- from button added on planet detail panel on the starmap
- from button added on production list on the statistics panel (default is off)

When opening with the keyboard shortcut, if the item information is found under the mouse pointer, it is used as a item filter (e.g. Inventory, Storage, Replicator, Statistics Panel and much places where item icon is displayed).


Main Window

![screen shot](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen.jpg)

Statistics Panel

![screen shot2](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen2.jpg)

Icon Info

![screenshot3](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen3.jpg)


## Traffic Log
Displays transport logs from game startup (up to 10,000 entries). To use it, you must turn it on in the config window and restart the game.

The log window can be opened from the "LOG" button in the main window or from the menu that appears by right-clicking. Shortcuts can also be set, but they are not present in the settings window and must be edited directly in the config file.  
Right-clicking on a row brings up a panel where you can refine filters and view them in the starmap.

![screenshot7](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen7.jpg)


## Configuration

LSTM has some settings depend on BepInEx (file name is `com.hetima.dsp.LSTM.cfg`). Most settings can be edited in the config window (from gear icon at the top of the main window).

|Key|Type|Default|Description|
|---|---|---|---|
|mainWindowHotkey|shortcut|LCtrl+T|Hotkey to open/close LSTM window|
|showMaterialPicker|bool|true|Add Material Picker for quick item switching to LSTM window|
|indicatesWarperSign|bool|false|true: show sign on the list if station has warper|
|reactClosePanelKeyE|bool|true|true: close window when close panel key(E) is pressed|
|actAsStandardPanel|bool|true|true: close with other panels by esc key. false: one more esc needed|
|showStationInfo|bool|false|Show station contents and empty slot count as icon. Also affected by in-game building icon display *setting|
|showStationInfoOnlyInPlanetView|bool|false|showStationInfo is only displayed in planet view|
|showStatInStatisticsWindow|bool|true|Add station stat to statistics panel|
|showButtonInStationWindow|bool|true|Add open LSTM button to Station Window|
|showButtonInStatisticsWindow|bool|false|Add open LSTM button to statistics panel|
|showButtonInStarmap|bool|true|Add open LSTM button to detail panel on starmap|
|~~setConstructionPointToGround~~|bool|false|true: set the construction point of stations to ground level instead of top of the tower (currently disabled)|
|enableNaviToEverywhere|bool|false|double-click (left and/or right click) on Planet View to display navigation to anywhere|
|suppressOpenInventory|bool|false|Suppress open inventory when opening station window|
|enableTrafficLog|bool|false|Enable traffic log window (needs restart game)|
|hideStoragedSlot|bool|true|hide storaged slot in list view|


Hidden settings  
The following settings cannot be edited from config window. You need to edit the file directly.

|Key|Type|Default|Description|
|---|---|---|---|
|switchDisplayModeHotkey|shortcut|Tab|Hotkey to switch between planet name and station name on LSTM window|
|dropSorterKeyEracesNavi|bool|false|clear navi line when "Remove Copied Sorter Previews" shortcut is pressed|
|logWindowHotkey|shortcut|none|Hotkey to open/close Traffic Log window|
|trafficLogDisplayMax|int|2000|Maximum rows that can be displayed in Traffic Log window. min=100 max=9999|
|stationInfoIconSize|float|10.0|Station Info icon size. min=5.0 max=15.0. default is 10.0f|



## 説明

輸送ステーションの状況をアイテムごとや惑星ごとに一覧表示して需給バランスを確認できます。

- アイテムごとに全体の状況を表示
- 惑星内の全ステーションの状況を表示
- ステーションの貯蔵率と実数を統計パネルに表示

- 対象の惑星を星間ビューで表示(宇宙探査レベル4が必要)
- 対象のステーションの場所をナビ表示
- ステーションウィンドウを開く(ナビボタンを右クリック)(現在の惑星のみ対象)
- ステーションの内容をアイコンサインで表示
- 輸送ログの表示

ウィンドウの開き方
- キーボードショートカット(デフォルトは LCtrl+T)
- ステーションウィンドウに追加されるボタンから
- 星図の惑星情報パネルに追加されるボタンから
- 統計パネルに追加されるボタンから(デフォルトはオフ)

キーボードショートカットで開くときに、マウスポインタ上にアイテム情報を見つけたらそのアイテムでフィルタ表示します（インベントリ、ストレージ、合成機、統計パネル、その他のアイコンが表示されている場所）



## Release Notes

### v0.9.1
- Fix scrollbar on log window

<details>
<summary>Previous Changelog</summary>

### v0.9.0
- Remove `TrafficLogic` and `One-time Demand` for 0.10.30.22292
- Fix button on starmap

### v0.8.8
- Fix font size for game version 0.10.29.22015


### v0.8.7
- PLS can show custom names on the Local list
- Local Cluster that was not functioning was explicitly disabled


### v0.8.6
- Fix for game version 0.10.28.21150


### v0.8.5
- Support Dark Fog Update(0.10.28.20779)
- `setConstructionPointToGround` is disabled
- Possibility of Cluster system does not function properly


### v0.8.4
- Proliferators ignore Cluster and Opposite Range

### v0.8.3
- Changed Remote Demand Delay from 98% to 96%
- Changed Local Demand Delay from 99% to 98%

### v0.8.2
- Added option "Hide Storaged Slot" (default is on)

### v0.8.1
- Changed the way station info looks and added config `stationInfoIconSize`

### v0.8.0
- Added Traffic Log Window (default is off)

### v0.7.2
- Added option "Suppress Open Inventory Window (when opening station window)" to config window (default is off)

### v0.7.1
- Minor bug fix

### v0.7.0
- Added One-time Demand

### v0.6.7
- Support game version 0.9.27.14546 (it doesn't work on prior to this version)


### v0.6.6
- Storaged count is displayed in blue if keep locked on sandbox mode for game version 0.9.26

### v0.6.5
- Added ability that double-click (left and/or right click) on Planet View to display navigation to anywhere (on/off in ConfigWindow or `enableNaviToEverywhere`. default is off) (this feature is basically unrelated to this mod, but I introduced it because it's easy to incorporate)

### v0.6.4
- Filtering by star system with no item selected will display all remote items.
- Added option show station info icon only in planet view (ConfigWindow or `showStationInfoOnlyInPlanetView`)
- Improved scroll bar appearance

### v0.6.3
- Fixed slider values not displaying properly on ConfigWindow

### v0.6.2
- Added most settings to ConfigWindow

### v0.6.1
- The default values for `setConstructionPointToGround` and `showStationInfo` have been changed to `false`

### v0.6.0
- Added ConfigWindow to change some settings (not yet fulfilled) (open from the button at the top of main window)
- Added the ability to display station contents and empty slot count as icon (config `showStationInfo` or ConfigWindow) (also affected by in-game building icon display setting)
- Fix incompatible with DSP Drone Clearing Mod

### v0.5.2
- Added TrafficLogic setting `TLLocalDemandDelay` default is false
- Added setting `setConstructionPointToGround` for faster construction. default is true
- Supports indicator display of sprayed with Proliferator

### v0.5.1
- Minor bug fix

### v0.5.0
- Added station storage ratio and total actual amounts information to statistics panel (config `showStatInStatisticsWindow` default is true)
- Added ability to filter by star system to main window

### v0.4.0
- Added Material Picker for quick item switching (add config `showMaterialPicker` for customize this)
- Probably fixed the problem of last slot not showing up when other mods that increase slots are installed

### v0.3.6
- Right-click locate button to open the station window (only current local planet).
- add config `reactClosePanelKeyE` close window when close panel key(E) is pressed. default is true.
- Press Tab to switch between planet name and station name (add config `switchDisplayModeHotkey` for customize this)

### v0.3.5
- Added setting `indicatesWarperSign`
- `TLRemoteDemandDelay` will now be applied to all remote demands

### v0.3.4
- Support for capacity upgrade in game version 0.9.24

### v0.3.3
- Added TrafficLogic setting `TLSmartTransport`
- `TLConsiderOppositeRange` ignores when the capacity of demand is small
- Fix `TLDCBalance` wrong demand range

### v0.3.2
- Rebuild for 0.8.23.9808
- Added TrafficLogic setting `TLRemoteDemandDelay`

### v0.3.1
- Added TrafficLogic setting `TLDCBalance`
- TLConsiderOppositeRange ignores Space Warper
- Local list will be sorted by items

### v0.3.0
- Added key feature that called "TrafficLogic"
- Added TrafficLogic setting `TLConsiderOppositeRange`, `TLRemoteCluster` and `TLLocalCluster`
- Added setting `dropSorterKeyEracesNavi`

### v0.2.3
- Rebuild for 0.8.22.9331
- Added setting `actAsStandardPanel` (I forgot to list it)

### v0.2.2

- Fix local station name was not displayed
- Changed the internal design of the button display (a bit more stable when off but needs restart)

### v0.2.1

- To display planet in the starmap, "Space Exploration" Lv4 is required

### v0.2.0

- Added keyboard shortcut for open/close window
- Each Orbital Collector in same gas giants was combined into a row
- Improve performance

### v0.1.0

- Initial Release for 0.8.21.8562

</details>

