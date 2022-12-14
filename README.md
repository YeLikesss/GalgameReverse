## 佛系放一些本人Galgame分析的成果

* <font size=5>国产Galgame请看[这边](https://github.com/YeLikesss/CNGALTools)</font>
* <font size=5>开源项目需要自己下载Visual Studio进行编译</font>
* <font size=5>程序仅在Windows 7 x64 SP1下开发并测试通过 其他系统如出现兼容性问题需自行解决</font>
* <font size=5>如部分项目不开源请谅解</font>

### 使用条例

* <font size=5>禁止用于任何商业行为</font>
* <font size=5>禁止用于任何形式机器翻译</font>
* <font size=5>禁止用于移植到任何非原生x86/x64 Windows平台上</font>
* <font size=5>禁止用于任何AI学习</font>



### 项目内容

#### 1.NekoNyan发行商

* 1.自研封包V1 (Unity)  (静态提取)

&emsp;&emsp;游戏测试

&emsp;&emsp;&emsp;《蒼の彼方のフォーリズム》

&emsp;&emsp;&emsp;《蒼の彼方のフォーリズム EXTRA1》

&emsp;&emsp;&emsp;《金色ラブリッチェ》

&emsp;&emsp;编译环境

&emsp;&emsp;&emsp;.Net 6.0



#### 2.HikariField发行商

* 1.《アオナツライン》 (Unity Official Chs Ver) (静态提取)

    &emsp;提取完毕后自行使用AssetStudio打开
    
    &emsp;编译环境 
    
    &emsp;&emsp;.Net 6.0

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

    &emsp;&emsp;MSVC 2022 x86 + .Net6.x x86 Only



#### SP1.优化补丁

* 1.《素敵な彼女の作り方》 垂直同步补丁 降低显卡占用(Steam版)