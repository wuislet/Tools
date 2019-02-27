---
Author: brechtos  
FORM:   http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/
---

1. ConditionalHidePropertyDrawer放在Editor, ConditionalHideAttribute放在Script.
1. 代码中使用[ConditionalHide("bool变量名", true)]设置被控制的变量