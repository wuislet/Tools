﻿/************************** 
 * 文件名:AutoSetTextureUISprite.cs; 
 * 文件描述:导入图片资源到Unity时，自动修改为UI 2D Sprite，自动设置打包tag 为文件夹名字; 
 * 创建日期:2015/05/04; 
 * Author:陈鹏; 
 ***************************/

using UnityEngine;
using System.Collections;
using UnityEditor;

public class AutoSetTextureUISprite : AssetPostprocessor
{

    void OnPreprocessTexture()
    {

        //自动设置类型;  
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.textureType = TextureImporterType.Sprite;

        //自动设置打包tag;  
        string dirName = System.IO.Path.GetDirectoryName(assetPath);
        Debug.Log("Import ---  " + dirName);
        string folderStr = System.IO.Path.GetFileName(dirName);
        Debug.Log("Set Packing Tag ---  " + folderStr);

        textureImporter.spritePackingTag = folderStr;
    }
}