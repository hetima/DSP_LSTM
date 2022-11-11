# LSTM - Logistics Station Traffic Manager

Mod for Dyson Sphere Program. Needs BepInEx.

## Recent Changes

### v0.8.1
- Changed the way station info looks and added config `stationInfoIconSize`


## About

Lists the supply and demand balance of the Logistics Station by item and by planet.

- Show overall status for each item
- Show the status of all stations on specific planet
- Show station storage ratio and total actual amounts information in the statistics panel
- Improve transport behavior

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


Compatibility with other mods has not yet been checked.

Main Window

![screen shot](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen.jpg)

Statistics Panel

![screen shot2](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen2.jpg)

Icon Info

![screenshot3](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen3.jpg)

## One-time Demand
A one-time remote transport can be activated.  
Enabling One-time Demand in the configuration adds functionality to the right-click menu in the LSTM window and to the remote status toggle button in the station window. It is executed from the requesting station.

![screenshot4](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen4.jpg)

When One-time Demand is executed, transportation is performed from the nearest station available for supply to the target station. If it is farther than the maximum supply range, it will be excluded, but if Ignore Supply Range is turned on, it will be included.The quantity transported is the available capacity on the demand side or the maximum loading capacity of the carrier.For example, if you run this function with capacity set to 100, then 100 supplies will be delivered. The receiver can execute this function in any demand/supply/storage state.

![screenshot5](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen5.jpg)

![screenshot6](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen6.jpg)

## Traffic Log
Displays transport logs from game startup (up to 10,000 entries). To use it, you must turn it on in the config window and restart the game.

The log window can be opened from the "LOG" button in the main window or from the menu that appears by right-clicking. Shortcuts can also be set, but they are not present in the settings window and must be edited directly in the config file.  
Right-clicking on a row brings up a panel where you can refine filters and view them in the starmap.

![screenshot7](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen7.jpg)

## TrafficLogic
Change transport behavior. Can be turned on and off individually. Default is all off. It must be turned on in the configuration to be used.  

__Incompatible__ with some mods (StationRangeLimiter, etc.). TrafficLogic will not work if you use these mods.

### Smart Transport `TLSmartTransport` (experimental)
Priority will be given to nearby stations as much as possible. For now, only remote transports is supported.  
This is a rough implementation, so it may not work as expected. If this is enabled, `TLDCBalance` will be forced disabled because of some incompatibilities. Only this setting can be used with some of the above mods, but using with IntelligentTransport may have unexpected results.

### Consider Opposite Range `TLConsiderOppositeRange`
The partner's maximum transport distance will now be calculated too, and the transport will not be executed unless both maximum transport distances are exceeded. Applies to all transport, local and remote.  
As an exception, remote transports with a maximum demand capacity of less than 1,000 and the __Space Warper__ will be transported ignoring this settings.  

### Remote Cluster `TLRemoteCluster`
Groups stations together and separates them from other groups or unconfigured stations.  
You can include the string `[C:name]` in the station name to belong to the `name` cluster, where __C: is UPPER CASE__.  
Once a cluster has been established, stations can only be transported by stations that belong to the same cluster.  
The cluster name `any` has a special meaning. A cluster with this name can interact with all other clusters and unconfigured stations. Demanding from `any` cluster is likely to upset the balance of other clusters, so it is better to keep it supply only.  
As an exception, the __Space Warper__ will be transported ignoring the cluster settings.  

For now, the only way to set it up is to edit the name directly (You can edit the name in the station window by clicking on it. This is a default game feature). Please wait for additional features such as a settings UI.

### Local Cluster `TLLocalCluster`
This is the same function as Remote Cluster. Applies to local transport. The configuration is almost the same as Remote Cluster, with `[c:name]` in the station name, where __c: is lower case__.

Remote Cluster and Local Cluster can be configured simultaneously.  
Example: [C:r01][c:l01]station#1

### Remote Distance/Capacity Balance `TLDCBalance`
Increase and decrease the maximum transport distance according to the storage ratio.
In addition to turning the function itself on and off, you also need to set how much you want to change the distance. The value is a number between 1.0 and 100.0.

- `TLDCSupplyMultiplier`: multiply the distance by x while the supply storage is 70%-100%.
- `TLDCDemandMultiplier`: multiply the distance by x while the demand storage is 0%-30%.
- `TLDCSupplyDenominator`: divide the distance by x while the supply storage is 0%-30%.

When the value is set to 1, the distance does not change. There is no setting to reduce the demand distance. It does not affect storage with a maximum capacity of less than 2,000.

### Remote Demand Delay `TLRemoteDemandDelay`
Delays the triggering of remote demand.
This applies to slots with a maximum storage capacity is **5,000** or more.
Remote demand will not be executed until the total stock (actual stock + the amount in transit) falls below **98%**.
This subtle deviation solves a situation where there is no room for a local demand to occur.

### Local Demand Delay `TLLocalDemandDelay`
Delays the triggering of local demand.
Same as `TLRemoteDemandDelay` but threshold is **2,500** / **99%**.


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
|setConstructionPointToGround|bool|false|true: set the construction point of stations to ground level instead of top of the tower|
|enableNaviToEverywhere|bool|false|double-click (left and/or right click) on Planet View to display navigation to anywhere|
|enableOneTimeDemand|bool|false|enable One-Time Demand|
|oneTimeDemandIgnoreSupplyRange|bool|false|One-Time Demand ignores supply range|
|suppressOpenInventory|bool|false|Suppress open inventory when opening station window|
|enableTrafficLog|bool|false|Enable traffic log window (needs restart game)|

TrafficLogic settings

|Key|Type|Default|Description|
|---|---|---|---|
|TLSmartTransport|bool|false|enable TrafficLogic:Smart Transport|
|TLConsiderOppositeRange|bool|false|enable TrafficLogic:Consider Opposite Range|
|TLRemoteCluster|bool|false|enable TrafficLogic:Remote Cluster|
|TLLocalCluster|bool|false|enable TrafficLogic:Local Cluster|
|TLDCBalance|bool|false|enable TrafficLogic:Remote Distance/Capacity Balance|
|TLDCSupplyMultiplier|float|1.0|Multiplier for Remote Supply Distance/Capacity Balance (1-100)|
|TLDCDemandMultiplier|float|1.0|Multiplier for Remote Demand Distance/Capacity Balance (1-100)|
|TLDCSupplyDenominator|float|1.0|Denominator for Remote Supply Distance/Capacity Balance (1-100)|
|TLRemoteDemandDelay|bool|false|enable TrafficLogic:Remote Demand Delay|
|TLLocalDemandDelay|bool|false|enable TrafficLogic:Local Demand Delay|

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
- 輸送ロジックの改良

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

## One-time Demand
一回限りの輸送を発動させることができます。  
設定でOne-time Demandを有効にすると、LSTMウィンドウの右クリックメニューとステーションウィンドウのリモート状態切り替えボタンに機能が追加されます。要求する側のステーションから実行します。

![screenshot4](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen4.jpg)

One-time Demandを実行するといちばん近い供給可能なステーションから対象のステーションへ輸送が行われます。最大輸送距離より遠い場合は除外されますが、 Ignore Supply Range をオンにしておくと、それを無視して輸送実行します。輸送される量は要求側の空き容量、もしくは輸送船の最大積載量です。例えばキャパシティを100にして実行すると100個の物資が運ばれてきます。受け取る側は demand/supply/storage どの状態でもこの機能を実行することができます。

![screenshot5](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen5.jpg)

![screenshot6](https://raw.githubusercontent.com/hetima/DSP_LSTM/main/screen6.jpg)

## Traffic Log
ゲーム起動時からの輸送ログを表示します（最大1万件）。設定ウィンドウでオンにしてゲームを再起動する必要があります。

メインウィンドウの「LOG」ボタンや右クリックして出てくるメニューからログウィンドウを開くことができます。ショートカットも設定できますが、設定ウィンドウには存在しないので直接ファイルを編集してください。
ログを右クリックするとフィルタを絞り込んだり惑星ビューで表示したりできるパネルが表示されます。

## TrafficLogic
輸送の挙動を変更します。個別に設定でオンオフできます。デフォルトはすべてオフです。使用するには設定ウィンドウでオンにする必要があります。  

一部のmod(StationRangeLimiter 等)とは互換性がありません。これらのmodを使用している場合 TrafficLogic は機能しません。  

### Smart Transport `TLSmartTransport` (experimental)
なるべく近くのステーションを優先して輸送します。いまのところリモートのみ対応です。  
雑な実装なので期待通りには動かないかもしれません。これをオンにすると `TLDCBalance` は若干相性が悪いため強制的にオフになります。この設定のみ上記の一部modと併用可能ですが、IntelligentTransport との併用は予期せぬ結果になるかもしれません。

### Consider Opposite Range `TLConsiderOppositeRange`
相手の最大輸送距離も考慮されるようになり、双方の最大輸送距離を越えていないと輸送が実行されなくなります。ローカル/リモートすべてのステーションに適用されます。  
例外としてリモート demand の最大貯蔵量が1,000未満の場合と、 __空間歪曲器__ はこの設定を無視して輸送されます。  

### Remote Cluster `TLRemoteCluster`
ステーションをグループ化し、他のグループや未設定のステーションと切り離します。   
ステーション名に `[C:name]` という文字列を含めることで `name` のクラスターに属します。 __C:は大文字__ です。  
クラスターが設定されたステーションは同じクラスターに属するステーションとしか輸送できなくなります。  
`any` というクラスター名は特別な意味を持ちます。この名前のクラスターは他のすべてのクラスター、および未設定のステーションとやりとりできます。 `any` クラスターで要求を実行すると他クラスターのバランスが崩れる可能性が高いので、供給専用にした方が良いでしょう。  
例外として __空間歪曲器__ はクラスター設定を無視して輸送されます。  

今のところ直接名前を編集するしか設定方法がありません（ステーションウィンドウの名前をクリックすることで編集できます。これはデフォルトの機能です）。設定UIなどの機能追加をする予定なのでお待ち下さい。

### Local Cluster `TLLocalCluster`
Remote Cluster と同じ機能です。ローカル輸送に適用されます。設定方法は Remote Cluster とほぼ同じで、ステーション名に `[c:name]` と記述します。 __c:は小文字__ です。

Remote Cluster と Local Cluster は同時に設定できます。  
例：[C:r01][c:l01]station#1

### Remote Distance/Capacity Balance `TLDCBalance`
貯蔵量に応じて最大輸送距離を増減させます。機能自体のオンオフに加えて、どのくらい距離を変更するか設定する必要があります。値は1.0～100.0の数値です。

- `TLDCSupplyMultiplier`: supply の貯蔵量が 70%-100% の間は距離をx倍に
- `TLDCDemandMultiplier`: demand の貯蔵量が 0%-30% の間は距離をx倍に
- `TLDCSupplyDenominator`: supply の貯蔵量が 0%-30% の間は距離を1/x倍に

値を1にすると距離は変化しません。demand の距離を縮める設定はありません。最大貯蔵量が2,000未満のストレージには影響しません。

### Remote Demand Delay `TLRemoteDemandDelay`
リモート輸入の発動を遅らせます。最大貯蔵量 **5,000** 以上のスロットが対象で、総在庫(実在庫+輸送中の数)が **98%** を下回るまでリモート輸入を実行しません。これによりローカル側の demand やベルト搬入が発生する隙がない状態を回避できます。

### Local Demand Delay `TLLocalDemandDelay`
ローカル輸入の発動を遅らせます。`TLRemoteDemandDelay` と同じような機能ですが、発動条件は最大貯蔵量 **2,500** 以上 / 貯蔵率 **99%** です


## Release Notes

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

