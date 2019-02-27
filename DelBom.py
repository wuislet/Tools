# -*- coding:utf8 -*-
import os, codecs
print(os.getcwd())

for root, dirs, files in os.walk(os.getcwd()):
    print(" ======================= > ")
    for name in files:
        if(not name.endswith(".lua")):
            continue
            
        fullPath = os.path.join(root, name)
        print(fullPath)
        
        
        fp = open(fullPath, 'rb')
        content = fp.read()
        
        # u = s.decode("utf-8-sig")
        # s = u.encode("utf-8")
        fp.close()
        
        if ((content[0] == codecs.BOM_UTF8[0]) and (content[1] == codecs.BOM_UTF8[1]) and (content[2] == codecs.BOM_UTF8[2])):
            
            print(">>>>>>>>>>>>>>>>>>>>>%s<<<<<<<<<<<<<<<<<<<<<<" % fullPath)
            content = content[3:]
            # print(content)
            
            fp = open(fullPath, "wb")
            fp.write(content)
            fp.close()