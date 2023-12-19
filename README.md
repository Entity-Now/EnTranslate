# EnTranslate一款简单的划词翻译插件

## 简介
1. 支持划词翻译（鼠标悬浮到单词上方将自动翻译）
2. 支持播放单词发音
3. 支持调用百度翻译接口翻译注释文档 （暂未实现）
4. 强大的单词拆分能力: 支持驼峰, 下划线形式等各种单词拆分
5. 丰富的本地词库: 包含 340 万+离线单词, 支持各种生僻单词

EnTranslate内部实现方法借鉴于vscode插件[Code Translate](https://github.com/w88975/code-translate-vscode) ，感谢w88975作者将其开源。

## 下载
[vs 官方下载](https://marketplace.visualstudio.com/items?itemName=Entity-Now.Translate);

## 预览

- 鼠标悬停翻译、自动拆分单词组合
![黑色主题](https://cdn.jsdelivr.net/gh/Entity-Now/EnTranslate/docs/black.png)
![白色主题](https://cdn.jsdelivr.net/gh/Entity-Now/EnTranslate/docs/white.png)
![设置界面](https://cdn.jsdelivr.net/gh/Entity-Now/EnTranslate/docs/settings.png)

## 支持
> 如果觉得插件好用，麻烦帮忙点个star。
> 如果你发现任何bug，你可以提交issue，我看到会尽快修复。


## 计划
    [x] 支持在线翻译
    [x] 将中文文本替换为英文（用于变量取名）
    [x] 翻译文档注释

## 更新

1. 2023.7.16 
    - 修复光标悬浮在特殊字符上面会导致vs崩溃的问题。
    - 其他bug

2. 2023.9.26
   - 更新到v2版本，使用新的api（更稳定）
   - 支持切换主题，修复不同主题下看不清文字的问题
   - 支持播放单词发音
   - xxxx

3. 2023.9.27
    - 修复配置不生效的问题
    - 修改配置存放路径
    - 添加在词库中找不到单词的提示
    - 修复一些问题

4. 2023.10.11
    - 修改分词代码
    - ...

5. 2023.10.30
    - 支持使用微软Edge语言接口播放单词
    - 修复一些问题
    - ...
