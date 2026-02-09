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
        // Hiển thị cửa sổ Editor
        GetWindow<SpineOrganizerWindow>("Spine Organizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cài đặt Folder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Ô kéo thả Folder Nguồn (Nơi chứa các file lộn xộn)
        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Folder Nguồn (Source)", "Kéo folder chứa các file .png, .json, .atlas vào đây"),
            sourceFolder,
            typeof(DefaultAsset),
            false
        );

        // Ô kéo thả Folder Đích (Nơi sẽ tạo các folder con). 
        // Nếu để trống, code sẽ mặc định dùng luôn Folder Nguồn.
        destinationFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Folder Đích (Dest)",
                "Kéo folder bạn muốn chứa các folder con. Nếu để trống sẽ dùng Folder Nguồn"),
            destinationFolder,
            typeof(DefaultAsset),
            false
        );

        EditorGUILayout.Space();

        // Nút bấm xử lý
        if (GUILayout.Button("Bắt đầu Gom File", GUILayout.Height(40)))
        {
            if (sourceFolder == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Bạn chưa chọn Folder Nguồn!", "OK");
                return;
            }

            // Nếu không chọn đích, mặc định đích = nguồn
            if (destinationFolder == null) destinationFolder = sourceFolder;

            OrganizeSpineFiles();
        }
    }

    private void OrganizeSpineFiles()
    {
        // Lấy đường dẫn Assets/... của folder
        string sourcePath = AssetDatabase.GetAssetPath(sourceFolder);
        string destPath = AssetDatabase.GetAssetPath(destinationFolder);

        // Kiểm tra xem có đúng là folder không (tránh trường hợp kéo nhầm file thường)
        if (!AssetDatabase.IsValidFolder(sourcePath) || !AssetDatabase.IsValidFolder(destPath))
        {
            Debug.LogError("Input không phải là Folder hợp lệ!");
            return;
        }

        // Lấy tất cả file trong thư mục nguồn
        // Directory.GetFiles trả về đường dẫn hệ thống hoặc relative tùy môi trường, ta cần chuẩn hóa
        string[] allFilePaths = Directory.GetFiles(sourcePath);

        Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();
        HashSet<string> targetExtensions = new HashSet<string> { ".atlas", ".json", ".png", ".txt" };

        foreach (string rawFilePath in allFilePaths)
        {
            // Chuẩn hóa đường dẫn thành dấu / để tránh lỗi AssetDatabase
            string filePath = rawFilePath.Replace("\\", "/");

            if (filePath.EndsWith(".meta")) continue;

            string extension = Path.GetExtension(filePath).ToLower();
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // Logic lọc file Spine
            if (targetExtensions.Contains(extension))
            {
                if (!fileGroups.ContainsKey(fileNameWithoutExt))
                {
                    fileGroups[fileNameWithoutExt] = new List<string>();
                }

                fileGroups[fileNameWithoutExt].Add(filePath);
            }
        }

        int groupCount = 0;
        AssetDatabase.StartAssetEditing(); // Bắt đầu batch process

        foreach (var group in fileGroups)
        {
            string baseName = group.Key;
            List<string> files = group.Value;

            // Chỉ xử lý nếu có file trong nhóm
            if (files.Count > 0)
            {
                // Đường dẫn thư mục con mới tại Folder Đích
                string targetSubFolderPath = destPath + "/" + baseName;

                // Tạo folder nếu chưa có
                if (!AssetDatabase.IsValidFolder(targetSubFolderPath))
                {
                    AssetDatabase.CreateFolder(destPath, baseName);
                }

                // Di chuyển file
                foreach (string currentFilePath in files)
                {
                    string fileName = Path.GetFileName(currentFilePath);
                    string destinationFilePath = targetSubFolderPath + "/" + fileName;

                    // Kiểm tra nếu file đã ở đúng chỗ thì bỏ qua
                    if (currentFilePath == destinationFilePath) continue;

                    string error = AssetDatabase.MoveAsset(currentFilePath, destinationFilePath);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"Lỗi di chuyển {fileName}: {error}");
                    }
                }

                groupCount++;
            }
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh(); // Cập nhật lại UI Unity

        EditorUtility.DisplayDialog("Hoàn tất", $"Đã xử lý xong {groupCount} nhóm file!", "OK");
    }
}