using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CsbScanner : MonoBehaviour
{
    [Header("Kéo file .csb vào đây")]
    // Dùng DefaultAsset để Unity nhận diện được file lạ (.csb)
    public UnityEngine.Object targetFile;

    // Nút bấm trên menu chuột phải của Component
    [ContextMenu("Quét File CSB")]
    public void ScanFile()
    {
        if (targetFile == null)
        {
            Debug.LogError("❌ Chưa kéo file vào ô Target File!");
            return;
        }

#if UNITY_EDITOR
        // 1. Lấy đường dẫn tương đối (Assets/...)
        string assetPath = AssetDatabase.GetAssetPath(targetFile);
        
        // 2. Chuyển thành đường dẫn tuyệt đối trên ổ cứng (C:/Project/Assets/...)
        // Cách này an toàn nhất để File.ReadAllBytes đọc được
        string fullPath = Path.GetFullPath(assetPath);

        Debug.Log($"📂 Đang đọc file: <color=yellow>{assetPath}</color>");

        try
        {
            if (File.Exists(fullPath))
            {
                // Đọc bytes
                byte[] bytes = File.ReadAllBytes(fullPath);
                
                // Chuyển sang string (để Regex quét)
                // Lưu ý: File binary sẽ có nhiều ký tự rác, nhưng Regex sẽ lọc ra cái cần thiết
                string rawContent = Encoding.ASCII.GetString(bytes);

                // Regex Pattern: Tìm các chuỗi ký tự kết thúc bằng đuôi file resource
                // Thêm IgnoreCase để bắt được cả .PNG, .JPG viết hoa
                string pattern = @"[\w\-\/]+\.(png|jpg|jpeg|plist|mp3|wav|json|fnt|ttf)";
                
                var matches = Regex.Matches(rawContent, pattern, RegexOptions.IgnoreCase);

                if (matches.Count > 0)
                {
                    Debug.Log($"✅ Tìm thấy <color=green>{matches.Count}</color> resources:");
                    foreach (Match match in matches)
                    {
                        // In ra Console
                        Debug.Log($"- {match.Value}");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Không tìm thấy đường dẫn resource nào (hoặc file đã bị mã hóa/nén chặt).");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Lỗi khi đọc file: {ex.Message}");
        }
#else
        Debug.LogError("Script này chỉ dùng trong Unity Editor để soi file thôi nhé!");
#endif
    }
}
