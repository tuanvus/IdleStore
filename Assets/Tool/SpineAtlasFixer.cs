using UnityEngine;
using UnityEditor;
using System.IO;

public class SpineAtlasFixer : EditorWindow
{
    private DefaultAsset targetFolder;

    [MenuItem("Tools/Spine Atlas Fixer (Force Extension)")]
    public static void ShowWindow()
    {
        GetWindow<SpineAtlasFixer>("Atlas Fixer v3");
    }

    private void OnGUI()
    {
        GUILayout.Label("Công cụ Sửa Đuôi File Spine", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Sử dụng System.IO để CƯỠNG ÉP đổi đuôi file.\nSửa lỗi: .atlas.txt.atlas -> .atlas.txt\nĐổi tên: .atlas -> .atlas.txt", MessageType.Warning);
        
        EditorGUILayout.Space();

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            new GUIContent("Chọn Folder gốc", "Kéo folder cha vào đây"), 
            targetFolder, 
            typeof(DefaultAsset), 
            false
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Quét và Sửa Đuôi File", GUILayout.Height(40)))
        {
            if (targetFolder == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng kéo Folder cần xử lý vào!", "OK");
                return;
            }

            FixAtlasExtensionsForce();
        }
    }

    private void FixAtlasExtensionsForce()
    {
        string rootPath = AssetDatabase.GetAssetPath(targetFolder);
        
        // Chuyển path Unity (Assets/...) thành path hệ thống (C:/Project/Assets/...)
        // Để dùng được System.IO
        string fullRootPath = Path.GetFullPath(rootPath);

        if (!Directory.Exists(fullRootPath))
        {
            Debug.LogError("Folder không tồn tại trên ổ cứng!");
            return;
        }

        // Lấy tất cả file trong thư mục (bao gồm subfolder)
        string[] allFiles = Directory.GetFiles(fullRootPath, "*", SearchOption.AllDirectories);

        int countFixed = 0;
        int countRenamed = 0;

        foreach (string filePath in allFiles)
        {
            if (filePath.EndsWith(".meta")) continue;

            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            string newFilePath = "";

            // TRƯỜNG HỢP 1: Sửa lỗi file bị thừa đuôi (321start.atlas.txt.atlas -> 321start.atlas.txt)
            if (fileName.EndsWith(".atlas.txt.atlas"))
            {
                // Cắt bỏ ".atlas" ở cuối (6 ký tự)
                string newName = fileName.Substring(0, fileName.Length - 6);
                newFilePath = Path.Combine(directory, newName);
                
                Debug.Log($"[Fix Lỗi] Đổi: {fileName} -> {newName}");
                countFixed++;
            }
            // TRƯỜNG HỢP 2: File .atlas chuẩn (321start.atlas -> 321start.atlas.txt)
            else if (fileName.EndsWith(".atlas") && !fileName.EndsWith(".txt.atlas"))
            {
                string newName = fileName + ".txt";
                newFilePath = Path.Combine(directory, newName);
                
                Debug.Log($"[Đổi Tên] Đổi: {fileName} -> {newName}");
                countRenamed++;
            }

            // Thực hiện đổi tên vật lý
            if (!string.IsNullOrEmpty(newFilePath))
            {
                try
                {
                    // 1. Đổi tên file chính
                    File.Move(filePath, newFilePath);

                    // 2. Xử lý file .meta (nếu có) để giữ references
                    string metaSource = filePath + ".meta";
                    string metaDest = newFilePath + ".meta";
                    
                    if (File.Exists(metaSource))
                    {
                        // Nếu file meta đích đã tồn tại (do lỗi cũ) thì xóa đi trước
                        if (File.Exists(metaDest)) File.Delete(metaDest);
                        
                        File.Move(metaSource, metaDest);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi khi xử lý file {fileName}: {ex.Message}");
                }
            }
        }

        // Quan trọng: Bắt Unity quét lại file hệ thống để nhận diện sự thay đổi
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Hoàn tất", 
            $"Đã xử lý xong!\n- Sửa lỗi đuôi: {countFixed}\n- Thêm đuôi .txt: {countRenamed}", 
            "OK");
    }
}
