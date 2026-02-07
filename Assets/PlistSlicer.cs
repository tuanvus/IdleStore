using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class PlistSlicer : EditorWindow
{
    public Texture2D sourceTexture;
    
    // Đổi từ TextAsset sang DefaultAsset để nhận mọi loại file
    public DefaultAsset plistFile; 

    [MenuItem("Tools/Plist Slicer")]
    public static void ShowWindow()
    {
        GetWindow<PlistSlicer>("Plist Slicer");
    }

    void OnGUI()
    {
        GUILayout.Label("Cắt Texture từ Plist (Hỗ trợ .plist gốc)", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Texture Gốc", sourceTexture, typeof(Texture2D), false);
        
        // Cho phép kéo file .plist vào đây
        plistFile = (DefaultAsset)EditorGUILayout.ObjectField("File Plist", plistFile, typeof(DefaultAsset), false);

        if (GUILayout.Button("Cắt Ảnh (Slice)"))
        {
            if (sourceTexture != null && plistFile != null)
            {
                SliceTexture();
            }
            else
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng chọn đủ Texture và File Plist!", "OK");
            }
        }
    }

    void SliceTexture()
    {
        // Lấy đường dẫn file plist thật
        string plistPath = AssetDatabase.GetAssetPath(plistFile);
        string texturePath = AssetDatabase.GetAssetPath(sourceTexture);

        // Đọc nội dung file text từ ổ đĩa (vì Unity không tự load nội dung .plist)
        string plistContent = File.ReadAllText(plistPath);

        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError("Không tìm thấy Texture Importer.");
            return;
        }

        importer.isReadable = true;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        
        // Parse Plist từ nội dung vừa đọc
        List<SpriteMetaData> metaDataList = ParsePlist(plistContent, sourceTexture.height);

        importer.spritesheet = metaDataList.ToArray();
        
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        Debug.Log($"✅ Đã cắt xong {metaDataList.Count} sprites từ file {Path.GetFileName(plistPath)}!");
    }

    List<SpriteMetaData> ParsePlist(string xmlText, int textureHeight)
    {
        List<SpriteMetaData> list = new List<SpriteMetaData>();
        XmlDocument doc = new XmlDocument();
        
        // Một số file plist có DOCTYPE chặn việc load XML offline, ta có thể bỏ qua nó
        // bằng cách dùng XmlReaderSettings hoặc đơn giản là try-catch
        try 
        {
            doc.LoadXml(xmlText);
        }
        catch (XmlException)
        {
            // Hack fix: Xóa dòng DOCTYPE nếu gây lỗi
            int startIndex = xmlText.IndexOf("<!DOCTYPE");
            if (startIndex != -1)
            {
                int endIndex = xmlText.IndexOf(">", startIndex);
                xmlText = xmlText.Remove(startIndex, endIndex - startIndex + 1);
                doc.LoadXml(xmlText);
            }
        }

        XmlNode framesDict = doc.SelectSingleNode("plist/dict/key[text()='frames']");
        if (framesDict == null)
        {
            Debug.LogError("Không tìm thấy key 'frames' trong plist. Kiểm tra lại format.");
            return list;
        }
        
        framesDict = framesDict.NextSibling; // Lấy node <dict> chứa các frame

        XmlNodeList keys = framesDict.SelectNodes("key");
        
        foreach (XmlNode keyNode in keys)
        {
            string spriteName = keyNode.InnerText;
            XmlNode spriteDataDict = keyNode.NextSibling;

            string frameStr = GetDictValue(spriteDataDict, "frame");
            bool rotated = IsDictKeyTrue(spriteDataDict, "rotated");

            if (string.IsNullOrEmpty(frameStr)) continue;

            Rect rect = ParseRect(frameStr);

            float x = rect.x;
            float y = textureHeight - rect.y - rect.height; 

            float w = rotated ? rect.height : rect.width;
            float h = rotated ? rect.width : rect.height;

            if (rotated)
            {
                 y = textureHeight - rect.y - rect.width; 
                 w = rect.height; 
                 h = rect.width;  
            }
            else 
            {
                 w = rect.width;
                 h = rect.height;
            }

            // Đảm bảo rect không bị âm hoặc lòi ra ngoài texture
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            SpriteMetaData meta = new SpriteMetaData
            {
                name = Path.GetFileNameWithoutExtension(spriteName), // Bỏ đuôi .png trong tên sprite
                rect = new Rect(x, y, w, h),
                alignment = (int)SpriteAlignment.Center
            };
            
            list.Add(meta);
        }

        return list;
    }

    string GetDictValue(XmlNode dict, string keyName)
    {
        XmlNode key = dict.SelectSingleNode($"key[text()='{keyName}']");
        if (key != null) return key.NextSibling.InnerText;
        return "";
    }

    bool IsDictKeyTrue(XmlNode dict, string keyName)
    {
        XmlNode key = dict.SelectSingleNode($"key[text()='{keyName}']");
        if (key != null) return key.NextSibling.Name == "true";
        return false;
    }

    Rect ParseRect(string s)
    {
        s = s.Replace("{", "").Replace("}", "");
        string[] parts = s.Split(',');
        
        if (parts.Length >= 4)
        {
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float w = float.Parse(parts[2]);
            float h = float.Parse(parts[3]);
            return new Rect(x, y, w, h);
        }
        return new Rect(0,0,0,0);
    }
}
