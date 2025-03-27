#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

public class BrotliCompressorEditor : EditorWindow
{
    private static string jsonFolderPath = "Assets/Resources";
    private static bool deleteOriginalFiles = false;
    private static CompressionLevel compressionLevel = CompressionLevel.Optimal;

    [MenuItem("Tools/BrotliJson")]
    public static void ShowWindow() => GetWindow<BrotliCompressorEditor>("Brotli Compressor").Show();

    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 200;
        GUILayout.Label("Brotli JSON Compression/Decompression", EditorStyles.boldLabel);
        jsonFolderPath = EditorGUILayout.TextField("Target Path", jsonFolderPath);
        deleteOriginalFiles = EditorGUILayout.Toggle("Delete Originals?", deleteOriginalFiles);
        compressionLevel = (CompressionLevel)EditorGUILayout.EnumPopup("Compression Level", compressionLevel);

        DrawOperationButtons("Batch Operations", 
            ("Compress JSONs", () => ProcessAllFiles("*.json", ProcessCompression)),
            ("Decompress BRs", () => ProcessAllFiles("*.br", ProcessDecompression)));

        DrawOperationButtons("Selected Files",
            ("Compress Selected", ProcessSelectedCompression),
            ("Decompress Selected", ProcessSelectedDecompression));
    }

    private void DrawOperationButtons(string label, params (string name, System.Action action)[] buttons)
    {
        GUILayout.Space(10);
        GUILayout.Label(label, EditorStyles.boldLabel);
        foreach (var btn in buttons)
        {
            if (GUILayout.Button(btn.name))
                btn.action();
        }
    }

    private static void ProcessAllFiles(string pattern, System.Action<string> processor)
    {
        if (!Directory.Exists(jsonFolderPath))
        {
            Debug.LogError($"Directory not found: {jsonFolderPath}");
            return;
        }

        var files = Directory.GetFiles(jsonFolderPath, pattern, SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            Debug.LogWarning($"No {pattern} files found");
            return;
        }

        try
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Processing", 
                    Path.GetFileName(files[i]), (float)i / files.Length))
                {
                    break;
                }
                processor(files[i]);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    private static void ProcessCompression(string jsonPath)
    {
        try
        {
            var json = File.ReadAllText(jsonPath);
            var compressed = BrotliUtility.Compress(json, compressionLevel);
            
            var brPath = Path.ChangeExtension(jsonPath, ".br");
            File.WriteAllBytes(brPath, compressed);
            
            if (deleteOriginalFiles) SafeDelete(jsonPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to compress {jsonPath}: {e.Message}");
        }
    }

    private static void ProcessDecompression(string brPath)
    {
        try
        {
            var compressed = File.ReadAllBytes(brPath);
            var json = BrotliUtility.Decompress(compressed);
            
            var jsonPath = Path.ChangeExtension(brPath, ".json");
            File.WriteAllText(jsonPath, json);
            
            if (deleteOriginalFiles) SafeDelete(brPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to decompress {brPath}: {e.Message}");
        }
    }

    private static void ProcessSelectedCompression() => ProcessSelection("*.json", ProcessCompression);
    private static void ProcessSelectedDecompression() => ProcessSelection("*.br", ProcessDecompression);

    private static void ProcessSelection(string pattern, System.Action<string> processor)
    {
        string targetExtension = pattern.Replace("*.", "").ToLower();
        int processed = 0;

        foreach (var obj in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
        {
            string path = AssetDatabase.GetAssetPath(obj);
        
            if (AssetDatabase.IsValidFolder(path))
            {
                // Process all matching files in directory
                foreach (var file in Directory.GetFiles(path, pattern, SearchOption.AllDirectories))
                {
                    processor(file);
                    processed++;
                }
            }
            else if (File.Exists(path))
            {
                // Check file extension match
                string fileExtension = Path.GetExtension(path).Replace(".", "").ToLower();
                if (fileExtension == targetExtension)
                {
                    processor(path);
                    processed++;
                }
            }
        }

        Debug.Log($"Processed {processed} files");
        EditorApplication.delayCall += () => AssetDatabase.Refresh();
    }

    private static void SafeDelete(string path)
    {
        try
        {
            if (!File.Exists(path)) return;

            // Delete main file
            File.Delete(path);
            Debug.Log($"Deleted original: {path}");

            // Delete meta file
            string metaPath = path + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
                Debug.Log($"Cleaned up meta file: {metaPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete {path}: {e.Message}");
        }
    }
}
#endif