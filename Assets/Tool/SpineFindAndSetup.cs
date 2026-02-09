
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class SpineFileInfo
{
    public string Name;
    public string PathFileAtlas;
    public string PathFileSkel;
    public string PathFileImage;
}

public enum SaveLocation
{
    InsideAssets,
    OutsideAssets
}

public enum FileOperation
{
    Copy,
    Move
}

public class SpineFindAndSetup : MonoBehaviour
{
    [SerializeField] private Object _folderTxtSpine;
    [SerializeField] private Object _folderImageSpine;

    [SerializeField] private SerializedDictionary<string, SpineFileInfo> _allSpineFiles;
    [SerializeField] private FileOperation _operationMode = FileOperation.Copy;
    private const string RootFolder = "Assets/SkeletonAsset";
    [SerializeField] private SaveLocation _saveLocation = SaveLocation.InsideAssets;

    [SerializeField, FolderPath(AbsolutePath = true)]
    private string _outsideFolder;
    

    private static string ProjectRoot
    {
        get { return Path.GetDirectoryName(Application.dataPath); }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(_outsideFolder))
        {
            _outsideFolder = Path.Combine(ProjectRoot, "SpineOutput");
        }
#endif
    }

    [Button]
    public void Setup()
    {
        FindAllFileSpine();
        CreateFolderRoot();

        foreach (var kvp in _allSpineFiles)
        {
            var info = kvp.Value;
            string subFolder = CreateFolderSub(info.Name);

            // Copy từng file vào subFolder
            if (!string.IsNullOrEmpty(info.PathFileSkel))
                MoveFile(info.PathFileSkel, subFolder);

            if (!string.IsNullOrEmpty(info.PathFileAtlas))
                MoveFile(info.PathFileAtlas, subFolder);

            if (!string.IsNullOrEmpty(info.PathFileImage))
                MoveFile(info.PathFileImage, subFolder);
        }

        AssetDatabase.Refresh();
        Debug.Log("✅ Copy xong hết vào SkeletonAsset!");
    }

    public void FindAllFileSpine()
    {
#if UNITY_EDITOR
        _allSpineFiles.Clear();

        if (_folderTxtSpine == null || _folderImageSpine == null)
        {
            Debug.LogWarning("Chưa gán folder TxtSpine hoặc ImageSpine!");
            return;
        }

        string folderTxtPath = AssetDatabase.GetAssetPath(_folderTxtSpine);
        string folderImagePath = AssetDatabase.GetAssetPath(_folderImageSpine);

        if (!AssetDatabase.IsValidFolder(folderTxtPath) || !AssetDatabase.IsValidFolder(folderImagePath))
        {
            Debug.LogError("Object được gán không phải là folder!");
            return;
        }

        // Quét file .skel.bytes
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folderTxtPath });

        foreach (string guid in guids)
        {
            string skelPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!skelPath.EndsWith(".skel.bytes")) continue;

            string fileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(skelPath));

            // 🔹 Tìm atlas
            string atlasPath = null;
            string dirPath = Path.GetDirectoryName(skelPath);
            if (!string.IsNullOrEmpty(dirPath))
            {
                string fullDirPath = Path.Combine(Directory.GetCurrentDirectory(), dirPath);
                if (Directory.Exists(fullDirPath))
                {
                    foreach (var file in Directory.GetFiles(fullDirPath))
                    {
                        string candidateName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                        if (candidateName == fileName &&
                            (file.EndsWith(".atlas") || file.EndsWith(".atlas.txt") || file.EndsWith(".atlas.json")))
                        {
                            atlasPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                            break;
                        }
                    }
                }
            }

            // 🔹 Tìm image
            string[] imageGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderImagePath });
            string imagePath = null;
            foreach (string imgGuid in imageGuids)
            {
                string candidate = AssetDatabase.GUIDToAssetPath(imgGuid);
                string candidateName = Path.GetFileNameWithoutExtension(candidate);

                // so sánh chính xác với fileName
                if (candidateName == fileName &&
                    (candidate.EndsWith(".png") || candidate.EndsWith(".jpg")))
                {
                    imagePath = candidate;
                    break;
                }
            }

            var info = new SpineFileInfo()
            {
                Name = fileName,
                PathFileSkel = skelPath,
                PathFileAtlas = atlasPath,
                PathFileImage = imagePath
            };

            _allSpineFiles[fileName] = info;
        }

        Debug.Log($"Tìm thấy {_allSpineFiles.Count} Spine files!");
#endif
    }

    public void CreateFolderRoot()
    {
#if UNITY_EDITOR
        if (!AssetDatabase.IsValidFolder(RootFolder))
        {
            AssetDatabase.CreateFolder("Assets", "SkeletonAsset");
        }
#endif
    }

    public string CreateFolderSub(string subFolderName)
    {
#if UNITY_EDITOR
        string subFolderPath = $"{RootFolder}/{subFolderName}";
        if (!AssetDatabase.IsValidFolder(subFolderPath))
        {
            AssetDatabase.CreateFolder(RootFolder, subFolderName);
        }

        return subFolderPath;
#else
        return null;
#endif
    }

    public void MoveFile(
        string from,
        string toFolder)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(toFolder)) return;

        string fileName = Path.GetFileName(from);
        string projectRoot = Path.GetDirectoryName(Application.dataPath); // ✅ gốc project

        if (_saveLocation == SaveLocation.InsideAssets)
        {
            // ---- Lưu vào trong Assets ----
            string toPath = $"{toFolder}/{fileName}";
            string absFrom = Path.Combine(projectRoot, from); // từ Assets/...
            string absTo = Path.Combine(projectRoot, toPath); // tới Assets/...

            if (_operationMode == FileOperation.Copy)
            {
                if (!File.Exists(absTo))
                {
                    File.Copy(absFrom, absTo, overwrite: true);
                    Debug.Log($"📂 Copied {from} ➝ {toPath}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ File {toPath} đã tồn tại, bỏ qua copy.");
                }
            }
            else if (_operationMode == FileOperation.Move)
            {
                if (File.Exists(absTo))
                {
                    Debug.LogWarning($"⚠️ File {toPath} đã tồn tại, bỏ qua move.");
                    return;
                }

                File.Move(absFrom, absTo);
                Debug.Log($"📂 Moved {from} ➝ {toPath}");
            }
        }
        else
        {
            // ---- Lưu ra ngoài Assets ----
            string absFrom = Path.Combine(projectRoot, from); // từ Assets/...
            string absTo = Path.Combine(toFolder, fileName); // toFolder đã là absolute

            if (_operationMode == FileOperation.Copy)
            {
                if (!File.Exists(absTo))
                {
                    File.Copy(absFrom, absTo, overwrite: true);
                    Debug.Log($"📂 Copied {from} ➝ {absTo}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ File {absTo} đã tồn tại, bỏ qua copy.");
                }
            }
            else if (_operationMode == FileOperation.Move)
            {
                if (File.Exists(absTo))
                {
                    Debug.LogWarning($"⚠️ File {absTo} đã tồn tại, bỏ qua move.");
                    return;
                }

                File.Move(absFrom, absTo);
                Debug.Log($"📂 Moved {from} ➝ {absTo}");
            }
        }
#endif
    }
}
#endif
