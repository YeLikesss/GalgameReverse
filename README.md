## 佛系放一些本人Galgame分析的成果

* <font size=5>国产Galgame请看[这边](https://github.com/YeLikesss/CNGALTools)</font>
* <font size=5>开源项目需要自己下载Visual Studio进行编译</font>
* <font size=5>程序仅在Windows 7 x64 SP1下开发并测试通过 其他系统如出现兼容性问题需自行解决</font>
* <font size=5>GUI仅支持100%DPI下运行  如需高DPI支持  请自己写(逃)</font>
* <font size=5>如部分项目不开源请谅解</font>

### 使用条例

* <font size=5>禁止用于任何商业行为</font>
* <font size=5>禁止用于移植到任何非原生x86/x64 Windows(包括但不限于Kirikiroid Ons Wine Exagear Windows ARM等平台上)</font>
* <font size=5>禁止用于任何AI学习</font>

### 一经发现  跑路并不再更新    (垃圾圈子机翻移植烂活就是多)

### 项目内容

#### 1.NekoNyan发行商

* 1.自研封包V1 (Unity)  (静态提取)

  &emsp;使用方法

  &emsp;已配置好发布文件  右键ExtractGUI然后发布即可使用

  &emsp;游戏测试

  &emsp;&emsp;《蒼の彼方のフォーリズム》 V10

  &emsp;&emsp;《蒼の彼方のフォーリズム EXTRA1》 V10

  &emsp;&emsp;《金色ラブリッチェ》 V10

  &emsp;&emsp;《蒼の彼方のフォーリズム EXTRA2》 V11

  &emsp;编译环境

  &emsp;&emsp;.Net 6.x

#### 2.HikariField发行商

* 1.《アオナツライン》 (Unity Official Chs Ver) (静态提取)

    &emsp;提取完毕后自行使用AssetStudio打开
    
    &emsp;编译环境 
    
    &emsp;&emsp;.Net 6.x
    
* 2.《未来ラジオと人工鳩》 (Unity Official Chs-Cht-Eng Ver)  [HikariField --- NekoNyan] (静态提取)

    &emsp;编译环境 

    &emsp;&emsp;.Net 6.x

#### 3.Wamsoft外包厂

* 1.KrkrZCxdecV2  动态解包+Hash爆破GUI(仅动态版本)

    &emsp;解包配置文件

    ```
    {
    	"enableExtract": false,
    	"enableHasher": false,
    	"enableDynamicHashDecoder": false,
    	"extractDelayTimes":15000,
    	"packageFiles": [
    		"data.xp3",
    		"evimage.xp3",
    		"fgimage.xp3",
    		"bgimage.xp3",
    		"voice.xp3",
    		"chapter1.xp3",
    		"chapter2.xp3",
    		"patch@r267.xp3",
    		"patch@r377.xp3",
    		"patch_append81.xp3",
    		"patch_append82.xp3"
    	]
    }
    enableExtract 开启动态解包
    enableHasher 开启动态Hash模块
    enableDynamicHashDecoder 启动Hash爆破GUI (动态版本)
    extractDelayTimes 解包延时
    packageFiles 你要解的封包
    
    需要使用DebugView观察Log输出, 详细内容自行看源码
    
    下列文件必须在同一路径下
    ArchiveExtractor.dll
    ArchiveExtractorLoader.exe
    ArchiveExtractor.json
    CxHashDecoder.exe
    ```

    &emsp;GUI使用

    &emsp;&emsp;自己看源码  有详细注释 (已配置好发布参数文件  直接发布即可编译单文件版本)

    &emsp;编译环境

    &emsp;&emsp;MSVC 2022 x86 + .Net 6.x x86 Only
    
* 2.PbdDecoder pbd立绘合成 (静态合成)

    &emsp;注意事项

    &emsp;&emsp;1.合成器未内置tlg解码器  需要先把tlg转成png  可以使用[GarBro](https://github.com/morkt/GARbro)进行编码转换

    &emsp;&emsp;2.自带发布编译参数  一键发布即可完成编译 

    &emsp;使用方法

    &emsp;&emsp;请看SP.2里面的图片教程

    &emsp;编译环境

    &emsp;&emsp;.Net 6.x

    &emsp;Nuget依赖

    &emsp;&emsp;K4os.Compression.LZ4

    &emsp;&emsp;System.Drawing.Common

#### 4.SyawaseWorks发行商

* 1.《ハミダシクリエイティブ》 (Offical Steam Release Ver) (破解&Dumper)

    &emsp;方案

    &emsp;&emsp;LocalFile Encryptor + Themida3.x VM + Xbundle SteamAPI
  
    &emsp;使用方法

    &emsp;&emsp;1.CFF Explorer添加导入表 并且加载顺序在hamidashi.dll之前
    
    &emsp;&emsp;2.将Steam补丁放到游戏目录下

    &emsp;&emsp;3.Dumper_Output为资源文件输出

    &emsp;编译环境

    &emsp;&emsp;MSVC 2022 x86

    &emsp;依赖

    &emsp;&emsp;Detours

* 1.《HappyLiveShowUp》 (Offical Steam Release Ver) (破解)

    &emsp;方案

    &emsp;&emsp;PackageFile Encryptor + Themida3.x VM + Xbundle SteamAPI
  
    &emsp;使用方法

    &emsp;&emsp;1.CFF Explorer添加导入表 并且加载顺序在hamidashi.dll之前
    
    &emsp;&emsp;2.将Steam补丁放到游戏目录下

    &emsp;编译环境

    &emsp;&emsp;MSVC 2022 x86

    &emsp;依赖

    &emsp;&emsp;Detours

#### SP1.优化补丁

* 1.《素敵な彼女の作り方》 垂直同步补丁 降低显卡占用(Steam版)

#### SP2.笔记

* 1.Pbd立绘合成器使用方法