using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class BatchPlistSlicer : EditorWindow
{
    // Danh sách Texture cần xử lý
    public List<Texture2D> texturesToSlice = new List<Texture2D>();
    
    // Biến lưu vị trí thanh cuộn
    private Vector2 scrollPos;

    [MenuItem("Tools/Batch Plist Slicer (Pro)")]
    public static void ShowWindow()
    {
        BatchPlistSlicer window = GetWindow<BatchPlistSlicer>("Batch Slicer Pro");
        window.minSize = new Vector2(400, 500);
    }

    void OnGUI()
    {
        // --- 1. HEADER & DROP ZONE ---
        GUILayout.Space(10);
        GUILayout.Label("Cắt Texture Hàng Loạt", EditorStyles.boldLabel);
        
        // Vẽ vùng nhận kéo thả file
        DrawDropArea();

        GUILayout.Space(10);
        
        // Toolbar nút bấm
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Xóa Hết List", GUILayout.Width(100)))
        {
            texturesToSlice.Clear();
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Số lượng: {texturesToSlice.Count} file");
        GUILayout.EndHorizontal();

        // --- 2. SCROLL VIEW DANH SÁCH ---
        GUILayout.Space(5);
        DrawHeaderLine(); // Vẽ tiêu đề cột

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
        
        for (int i = 0; i < texturesToSlice.Count; i++)
        {
            DrawItemRow(i);
        }

        EditorGUILayout.EndScrollView();

        // --- 3. FOOTER BUTTON ---
        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✂️ CẮT TẤT CẢ (SLICE ALL)", GUILayout.Height(40)))
        {
            SliceAllTextures();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
    }

    // Vẽ vùng để người dùng kéo file vào
    void DrawDropArea()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\n📂 KÉO NHIỀU FILE ẢNH VÀO ĐÂY\n(Drag & Drop Textures Here)", EditorStyles.helpBox);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition)) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedOpject in DragAndDrop.objectReferences)
                    {
                        if (draggedOpject is Texture2D tex && !texturesToSlice.Contains(tex))
                        {
                            texturesToSlice.Add(tex);
                        }
                    }
                }
                break;
        }
    }

    // Vẽ tiêu đề bảng
    void DrawHeaderLine()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Texture (Ảnh)", GUILayout.Width(150));
        GUILayout.Label("➡", GUILayout.Width(20));
        GUILayout.Label("File Plist Tìm Thấy (Preview)", GUILayout.ExpandWidth(true));
        GUILayout.Label("Xóa", GUILayout.Width(40));
        GUILayout.EndHorizontal();
    }

    // Vẽ từng dòng trong danh sách
    void DrawItemRow(int index)
    {
        Texture2D texture = texturesToSlice[index];
        if (texture == null) return;

        GUILayout.BeginHorizontal("box");

        // Cột 1: Texture Object (Ảnh gốc)
        texturesToSlice[index] = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(150), GUILayout.Height(18));

        // Cột 2: Mũi tên
        GUILayout.Label("➡", GUILayout.Width(20));

        // Cột 3: Trạng thái Plist (HIỂN THỊ DẠNG OBJECT)
        string texturePath = AssetDatabase.GetAssetPath(texture);
        string foundPlistPath = TryFindPlistPath(texturePath);
        
        if (!string.IsNullOrEmpty(foundPlistPath))
        {
            // Tìm thấy -> Load file đó lên thành DefaultAsset để hiển thị vào ô ObjectField
            // LoadAssetAtPath giúp Unity coi file text/plist/xml như một Asset
            Object plistAsset = AssetDatabase.LoadAssetAtPath<Object>(foundPlistPath);
            
            // Vẽ ô ObjectField màu xanh (hoặc bình thường)
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green; // Highlight màu xanh để báo hiệu OK
            
            // Cho phép bấm vào để Ping file trong Project
            EditorGUILayout.ObjectField(plistAsset, typeof(Object), false);
            
            GUI.backgroundColor = oldColor; // Trả lại màu cũ
        }
        else
        {
            // Không tìm thấy -> Hiện thông báo đỏ
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = Color.red;
            GUILayout.Label("❌ Missing Plist!", style, GUILayout.ExpandWidth(true));
        }

        // Cột 4: Nút Xóa
        if (GUILayout.Button("X", GUILayout.Width(30)))
        {
            texturesToSlice.RemoveAt(index);
        }

        GUILayout.EndHorizontal();
    }

    // Logic tìm file plist (dùng chung cho cả GUI và xử lý)
    string TryFindPlistPath(string texturePath)
    {
        string plistPath = Path.ChangeExtension(texturePath, ".plist");
        if (File.Exists(plistPath)) return plistPath;

        string txtPath = Path.ChangeExtension(texturePath, ".txt");
        if (File.Exists(txtPath)) return txtPath;

        string xmlPath = Path.ChangeExtension(texturePath, ".xml");
        if (File.Exists(xmlPath)) return xmlPath;

        return null; // Không tìm thấy
    }

    // --- LOGIC XỬ LÝ CẮT ---
    void SliceAllTextures()
    {
        int success = 0;
        int fail = 0;

        foreach (var tex in texturesToSlice)
        {
            if (tex == null) continue;
            string texPath = AssetDatabase.GetAssetPath(tex);
            string plistPath = TryFindPlistPath(texPath);
            Debug.Log($"Processing '{tex.name}'...");

            if (string.IsNullOrEmpty(plistPath))
            {
                Debug.LogError($"Bỏ qua '{tex.name}': Không tìm thấy plist.");
                fail++;
                continue;
            }

            if (SliceOneTexture(tex, texPath, plistPath)) success++;
            else fail++;
        }

        EditorUtility.DisplayDialog("Kết quả", $"Thành công: {success}\nThất bại: {fail}", "OK");
    }

    bool SliceOneTexture(Texture2D texture, string texturePath, string plistPath)
    {
        try
        {
            string plistContent = File.ReadAllText(plistPath);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return false;

            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            List<SpriteMetaData> metaDataList = ParsePlist(plistContent, texture.height);
            
            if (metaDataList.Count == 0) return false;

            importer.spritesheet = metaDataList.ToArray();
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    // --- PARSE XML (Giữ nguyên logic cũ) ---
    List<SpriteMetaData> ParsePlist(string xmlText, int textureHeight)
    {
        List<SpriteMetaData> list = new List<SpriteMetaData>();
        XmlDocument doc = new XmlDocument();
        try { doc.LoadXml(xmlText); }
        catch (XmlException) 
        {
            int s = xmlText.IndexOf("<!DOCTYPE");
            if (s != -1) {
                int e = xmlText.IndexOf(">", s);
                xmlText = xmlText.Remove(s, e - s + 1);
                doc.LoadXml(xmlText);
            }
        }

        XmlNode framesDict = doc.SelectSingleNode("plist/dict/key[text()='frames']");
        if (framesDict == null) return list;
        framesDict = framesDict.NextSibling;

        foreach (XmlNode keyNode in framesDict.SelectNodes("key"))
        {
            string spriteName = keyNode.InnerText;
            XmlNode data = keyNode.NextSibling;
            
            string frameStr = GetDictValue(data, "frame");
            bool rotated = IsDictKeyTrue(data, "rotated");

            if (string.IsNullOrEmpty(frameStr)) continue;

            Rect rect = ParseRect(frameStr);
            float x = rect.x;
            float y = textureHeight - rect.y - rect.height;
            float w = rotated ? rect.height : rect.width;
            float h = rotated ? rect.width : rect.height;

            if (rotated) {
                 y = textureHeight - rect.y - rect.width;
                 w = rect.height; h = rect.width;
            }
            if (x < 0) x = 0; if (y < 0) y = 0;

            list.Add(new SpriteMetaData {
                name = Path.GetFileNameWithoutExtension(spriteName),
                rect = new Rect(x, y, w, h),
                alignment = (int)SpriteAlignment.Center
            });
        }
        return list;
    }

    string GetDictValue(XmlNode dict, string keyName) {
        XmlNode key = dict.SelectSingleNode($"key[text()='{keyName}']");
        return key != null ? key.NextSibling.InnerText : "";
    }
    bool IsDictKeyTrue(XmlNode dict, string keyName) {
        XmlNode key = dict.SelectSingleNode($"key[text()='{keyName}']");
        return key != null ? key.NextSibling.Name == "true" : false;
    }
    Rect ParseRect(string s) {
        s = s.Replace("{", "").Replace("}", "");
        string[] p = s.Split(',');
        return p.Length >= 4 ? new Rect(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])) : new Rect(0,0,0,0);
    }
}
