# DSPModSave

# mod存档

源码：
[Gitee](https://gitee.com/crecheng/DSPMod/tree/main/DSPModSave)
[GitHub](https://github.com/crecheng/DSPMod/tree/main/DSPModSave)

提供一个在原存档外的存储mod数据的mod存档

如果你是开发者，想要使用

首先，需要应用dll，然后 using crecheng.DSPModSave;

然后在你的插件中基础IModCanSave接口

列如

class MyPlugin : BaseUnityPlugin,IModCanSave

并且实现接口的3个方法

保存数据时调用
void Export(BinaryWriter w);

读取数据时调用
void Import(BinaryReader r);

当加载另一个存档但是没有mod存档时调用
void IntoOtherSave();

Provide a mod archive that stores mod data outside the original archive

If you are a developer and want to use

First, you need to apply the dll, and then using crecheng.DSPModSave;

Then base the IModCanSave interface in your plugin

sourse code：
[Gitee](https://gitee.com/crecheng/DSPMod/tree/main/DSPModSave)
[GitHub](https://github.com/crecheng/DSPMod/tree/main/DSPModSave)

example:

class MyPlugin: BaseUnityPlugin,IModCanSave

And 3 methods to implement the interface

Called when saving data
void Export(BinaryWriter w);

Called when reading data
void Import(BinaryReader r);

Called when another archive is loaded but there is no mod archive
void IntoOtherSave();


### Installation

1. Install BepInEx
3. Then drag DSPModSave.dll into steamapps/common/Dyson Sphere Program/BepInEx/plugins


### 安装

1. 先安装 BepInEx框架
3. 将DSPModSave.dll拖到 steamapps/common/Dyson Sphere Program/BepInEx/plugins文件夹内
