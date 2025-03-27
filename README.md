# Unity-Brotli-Json-Compression
Wanna reduce your json file size in Unity? Use Brotli Json compression!

## Features

- **Brotli Compression/Decompression ~%75 Reduced Size**
- Batch processing of folders
- File/Folder selection support
- Custom `.br` file importer

## Usage

### Editor Interface
1. Open the tool window: `Tools > BrotliJson`
2. Set your target path (default: `Assets/Resources`)
3. Choose compression level (Optimal/Fastest/Smallest)
4. Use either:
   - **Batch Operations** - Processes all files in target path
   - **Selected Files** - Works on currently selected files/folders

### Runtime Usage
```csharp
// Decompression from TextAsset
TextAsset brAsset = Resources.Load<TextAsset>("compressed-file");
string json = BrotliUtility.DecompressFromBase64(brAsset.text);
