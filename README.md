﻿# RPELineParentUnbinder

- 这是一个简易的用于解绑RPE谱面父子线的工具，使用C#编写。
- 本工具当前存在问题：位置可能镜像偏移，十分抱歉，我已经尽力了。
- 父子线嵌套越多，内存占用越大，处理时间越长，且谱面文件也可能超过100MB。
- 谱面在转换后 __完全不可逆向修改被转换的子线__ 