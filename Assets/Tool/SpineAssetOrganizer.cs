using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SpineOrganizerWindow : EditorWindow
{
    // Biến để lưu folder kéo thả vào
    private DefaultAsset sourceFolder;
    private DefaultAsset destinationFolder;

    // Tạo menu để mở cửa sổ
    [MenuItem("Tools/Spine Organizer Window")]
    public static void ShowWindow()
    {
        GetWindow<SpineOrganizerWindow>("Spine Organizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cài đặt Folder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Ô kéo thả Folder Nguồn
        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Folder Nguồn", "Folder cha chứa file hoặc subfolder cần xử lý"), 
            sourceFolder, 
            typeof(DefaultAsset), 
            false
        );

        // Ô kéo thả Folder Đích
        destinationFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Folder Đích (Gom nhóm)", "Nơi tạo folder con khi dùng chức năng Gom Nhóm. Để trống = Folder Nguồn"), 
            destinationFolder, 
            typeof(DefaultAsset), 
            false
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // --- CHỨC NĂNG 1: GOM FILE ---
        GUILayout.Label("Chức năng 1: Gom file vào Folder", EditorStyles.boldLabel);
        if (GUILayout.Button("Gom nhóm file Spine (Auto Group)", GUILayout.Height(35)))
        {
            if (CheckInput()) OrganizeSpineFiles();
        }

        EditorGUILayout.Space();

        // --- CHỨC NĂNG 2: ĐỔI TÊN ---
        GUILayout.Label("Chức năng 2: Sửa lỗi Import", EditorStyles.boldLabel);
        if (GUILayout.Button("Đổi đuôi .atlas -> .atlas.txt (Quét toàn bộ Subfolder)", GUILayout.Height(35)))
        {
            if (CheckInput()) RenameAtlasFilesRecursive();
        }
    }

    // Hàm kiểm tra đầu vào
    private bool CheckInput()
    {
        if (sourceFolder == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Bạn chưa chọn Folder Nguồn!", "OK");
            return false;
        }
        return true;
    }

    // ---------------------------------------------------------
    // CHỨC NĂNG: ĐỔI TÊN RECURSIVE
    // ---------------------------------------------------------
    private void RenameAtlasFilesRecursive()
    {
        string rootPath = AssetDatabase.GetAssetPath(sourceFolder);
        
        // Lấy tất cả file .atlas trong folder nguồn VÀ TẤT CẢ SUBFOLDER
        // SearchOption.AllDirectories là chìa khóa để quét đệ quy
        string[] atlasFiles = Directory.GetFiles(rootPath, "*.atlas", SearchOption.AllDirectories);

        if (atlasFiles.Length == 0)
        {
            Debug.Log("Không tìm thấy file .atlas nào cần đổi tên.");
            return;
        }

        int count = 0;
        AssetDatabase.StartAssetEditing(); // Tối ưu hiệu năng

        foreach (string sysPath in atlasFiles)
        {
            // Chuyển đường dẫn hệ thống thành đường dẫn Unity (Assets/...)
            string unityPath = sysPath.Replace("\\", "/");
            
            // AssetDatabase.RenameAsset yêu cầu đường dẫn cũ và TÊN MỚI (chỉ tên, không kèm đường dẫn)
            string fileName = Path.GetFileName(unityPath); // vd: hero.atlas
            string newName = fileName + ".txt";            // vd: hero.atlas.txt

            string error = AssetDatabase.RenameAsset(unityPath, newName);

            if (string.IsNullOrEmpty(error))
            {
                count++;
            }
            else
            {
                Debug.LogError($"Lỗi đổi tên file {fileName}: {error}");
            }
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Hoàn tất", $"Đã đổi đuôi {count} file từ .atlas sang .atlas.txt", "OK");
    }

    // ---------------------------------------------------------
    // CHỨC NĂNG: GOM NHÓM (CODE CŨ)
    // ---------------------------------------------------------
    private void OrganizeSpineFiles()
    {
        if (destinationFolder == null) destinationFolder = sourceFolder;

        string sourcePath = AssetDatabase.GetAssetPath(sourceFolder);
        string destPath = AssetDatabase.GetAssetPath(destinationFolder);

        // Lưu ý: Chức năng gom nhóm này chỉ quét ở thư mục gốc được chọn (TopDirectoryOnly)
        // để tránh làm loạn cấu trúc các folder đã quy hoạch.
        string[] allFilePaths = Directory.GetFiles(sourcePath);

        Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();
        HashSet<string> targetExtensions = new HashSet<string> { ".atlas", ".json", ".png", ".txt" };

        foreach (string rawFilePath in allFilePaths)
        {
            string filePath = rawFilePath.Replace("\\", "/");
            if (filePath.EndsWith(".meta")) continue;

            string extension = Path.GetExtension(filePath).ToLower();
            
            // Xử lý logic tên file: Nếu là .atlas.txt thì bỏ .txt đi để lấy tên gốc
            string fileNameWithoutExt;
            if (filePath.EndsWith(".atlas.txt"))
            {
                fileNameWithoutExt = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filePath));
            }
            else
            {
                fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            }

            if (targetExtensions.Contains(extension) || filePath.EndsWith(".atlas.txt"))
            {
                if (!fileGroups.ContainsKey(fileNameWithoutExt))
                {
                    fileGroups[fileNameWithoutExt] = new List<string>();
                }
                fileGroups[fileNameWithoutExt].Add(filePath);
            }
        }

        int groupCount = 0;
        AssetDatabase.StartAssetEditing();

        foreach (var group in fileGroups)
        {
            string baseName = group.Key;
            List<string> files = group.Value;

            if (files.Count > 0)
            {
                string targetSubFolderPath = destPath + "/" + baseName;
                if (!AssetDatabase.IsValidFolder(targetSubFolderPath))
                {
                    string destGuid = AssetDatabase.AssetPathToGUID(destPath);
                    AssetDatabase.CreateFolder(destPath, baseName);
                }

                foreach (string currentFilePath in files)
                {
                    string fileName = Path.GetFileName(currentFilePath);
                    string destinationFilePath = targetSubFolderPath + "/" + fileName;

                    if (currentFilePath == destinationFilePath) continue;

                    AssetDatabase.MoveAsset(currentFilePath, destinationFilePath);
                }
                groupCount++;
            }
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Hoàn tất", $"Đã gom {groupCount} nhóm file!", "OK");
    }
}

