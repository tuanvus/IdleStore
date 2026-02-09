using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class BatchPlistSlicer : EditorWindow
{
    public List<Texture2D> texturesToSlice = new List<Texture2D>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Batch Plist Slicer (Auto Apply)")]
    public static void ShowWindow()
    {
        BatchPlistSlicer window = GetWindow<BatchPlistSlicer>("Batch Slicer");
        window.minSize = new Vector2(450, 500);
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Cắt Texture Hàng Loạt (Auto Apply)", EditorStyles.boldLabel);

        DrawDropArea();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Xóa Hết List", GUILayout.Width(100))) texturesToSlice.Clear();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Số lượng: {texturesToSlice.Count} file");
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        DrawHeaderLine();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
        for (int i = 0; i < texturesToSlice.Count; i++) DrawItemRow(i);
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✂️ CẮT & APPLY TẤT CẢ", GUILayout.Height(40))) SliceAllTextures();
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
    }

    void DrawDropArea()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\n📂 KÉO NHIỀU FILE ẢNH VÀO ĐÂY\n(Drag & Drop Textures)", EditorStyles.helpBox);

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
                        if (draggedOpject is Texture2D tex && !texturesToSlice.Contains(tex)) texturesToSlice.Add(tex);
                    }
                }

                break;
        }
    }

    void DrawHeaderLine()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Texture (Ảnh)", GUILayout.Width(150));
        GUILayout.Label("➡", GUILayout.Width(20));
        GUILayout.Label("File Plist Tìm Thấy", GUILayout.ExpandWidth(true));
        GUILayout.Label("Xóa", GUILayout.Width(40));
        GUILayout.EndHorizontal();
    }

    void DrawItemRow(int index)
    {
        Texture2D texture = texturesToSlice[index];
        if (texture == null) return;
        GUILayout.BeginHorizontal("box");
        texturesToSlice[index] = (Texture2D)EditorGUILayout.ObjectField(texture,
            typeof(Texture2D),
            false,
            GUILayout.Width(150),
            GUILayout.Height(18));
        GUILayout.Label("➡", GUILayout.Width(20));

        string texturePath = AssetDatabase.GetAssetPath(texture);
        string foundPlistPath = TryFindPlistPath(texturePath);

        if (!string.IsNullOrEmpty(foundPlistPath))
        {
            Object plistAsset = AssetDatabase.LoadAssetAtPath<Object>(foundPlistPath);
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            EditorGUILayout.ObjectField(plistAsset, typeof(Object), false);
            GUI.backgroundColor = oldColor;
        }
        else
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = Color.red;
            GUILayout.Label("❌ Missing Plist!", style, GUILayout.ExpandWidth(true));
        }

        if (GUILayout.Button("X", GUILayout.Width(30))) texturesToSlice.RemoveAt(index);
        GUILayout.EndHorizontal();
    }

    string TryFindPlistPath(string texturePath)
    {
        string plistPath = Path.ChangeExtension(texturePath, ".plist");
        if (File.Exists(plistPath)) return plistPath;
        string txtPath = Path.ChangeExtension(texturePath, ".txt");
        if (File.Exists(txtPath)) return txtPath;
        string xmlPath = Path.ChangeExtension(texturePath, ".xml");
        if (File.Exists(xmlPath)) return xmlPath;
        return null;
    }

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

        // Refresh lại Database lần cuối để chắc chắn UI cập nhật
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Hoàn tất", $"Thành công: {success}\nThất bại: {fail}", "OK");
    }

    bool SliceOneTexture(
        Texture2D texture,
        string texturePath,
        string plistPath)
    {
        try
        {
            string plistContent = File.ReadAllText(plistPath);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return false;

            // --- BƯỚC 1: RESET SẠCH SẼ (Revert to Default) ---
            if (importer.textureType == TextureImporterType.Sprite ||
                importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureType = TextureImporterType.Default;
                importer.spritesheet = new SpriteMetaData[0];
                importer.SaveAndReimport();
            }

            // --- BƯỚC 2: CẤU HÌNH IMPORT ---
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            // Cấu hình Max Size & NPOT để tránh resize ảnh
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 8192;

            // Lấy size thực tế
            int width = 0, height = 0;
            importer.GetSourceTextureWidthAndHeight(out width, out height);

            Debug.Log($"-> Cutting '{texture.name}' (Size: {width}x{height})");

            // --- BƯỚC 3: PARSE PLIST ---
            List<SpriteMetaData> metaDataList = ParsePlist(plistContent, height);

            if (metaDataList.Count == 0)
            {
                Debug.LogError($"-> Lỗi: Không tìm thấy frame nào trong plist của '{texture.name}'");
                return false;
            }

            // Gán Sprite Sheet
            importer.spritesheet = metaDataList.ToArray();

            // --- BƯỚC 4: APPLY & SAVE ---
            // Lưu các thay đổi vào SerializedObject
            EditorUtility.SetDirty(importer);

            // Lưu và Reimport lần 1 (để ghi Meta)
            importer.SaveAndReimport();

            // BƯỚC CUỐI: FORCE UPDATE (Tương đương nút APPLY)
            // Đảm bảo Unity nhận diện ngay lập tức sự thay đổi về Sprite con
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi ngoại lệ khi xử lý '{texture.name}': {ex.Message}");
            return false;
        }
    }

    // --- LOGIC PARSE XML ---
    List<SpriteMetaData> ParsePlist(
        string xmlText,
        int textureHeight)
    {
        List<SpriteMetaData> list = new List<SpriteMetaData>();
        XmlDocument doc = new XmlDocument();

        XmlReaderSettings settings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };
        if (xmlText.Contains("<!DOCTYPE"))
        {
            int s = xmlText.IndexOf("<!DOCTYPE");
            int e = xmlText.IndexOf(">", s);
            xmlText = xmlText.Remove(s, e - s + 1);
        }

        try
        {
            using (StringReader sr = new StringReader(xmlText))
            using (XmlReader reader = XmlReader.Create(sr, settings))
            {
                doc.Load(reader);
            }
        }
        catch
        {
            return list;
        }

        XmlNode framesDict = doc.SelectSingleNode("//key[text()='frames']/following-sibling::dict[1]");
        if (framesDict == null) return list;

        foreach (XmlNode child in framesDict.ChildNodes)
        {
            if (child.Name != "key") continue;

            string spriteName = child.InnerText;
            XmlNode dataDict = child.NextSibling;

            if (dataDict == null || dataDict.Name != "dict") continue;

            string frameStr = "";
            bool rotated = false;

            frameStr = GetStringValue(dataDict, "frame");
            if (!string.IsNullOrEmpty(frameStr))
            {
                rotated = GetBoolValue(dataDict, "rotated");
            }
            else
            {
                frameStr = GetStringValue(dataDict, "textureRect");
                rotated = GetBoolValue(dataDict, "textureRotated");
            }

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

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            list.Add(new SpriteMetaData
            {
                name = Path.GetFileNameWithoutExtension(spriteName),
                rect = new Rect(x, y, w, h),
                alignment = (int)SpriteAlignment.Center
            });
        }

        return list;
    }

    string GetStringValue(
        XmlNode dict,
        string keyName)
    {
        foreach (XmlNode node in dict.ChildNodes)
        {
            if (node.Name == "key" && node.InnerText == keyName)
            {
                XmlNode next = node.NextSibling;
                return (next != null && next.Name == "string") ? next.InnerText : "";
            }
        }

        return "";
    }

    bool GetBoolValue(
        XmlNode dict,
        string keyName)
    {
        foreach (XmlNode node in dict.ChildNodes)
        {
            if (node.Name == "key" && node.InnerText == keyName)
            {
                XmlNode next = node.NextSibling;
                if (next != null)
                {
                    if (next.Name == "true") return true;
                    if (next.Name == "false") return false;
                }
            }
        }

        return false;
    }

    Rect ParseRect(string s)
    {
        s = s.Replace("{", "").Replace("}", "");
        string[] p = s.Split(',');
        return p.Length >= 4
            ? new Rect(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]))
            : new Rect(0, 0, 0, 0);
    }
}