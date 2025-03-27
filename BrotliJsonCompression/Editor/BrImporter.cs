using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;

[ScriptedImporter(1, "br")]
public class BrImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(ctx.assetPath);
            string base64 = System.Convert.ToBase64String(fileData);
            TextAsset textAsset = new TextAsset(base64);
            textAsset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            
            ctx.AddObjectToAsset("main", textAsset);
            ctx.SetMainObject(textAsset);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to import {ctx.assetPath}: {e.Message}");
            ctx.LogImportError($"Import failed: {e.Message}");
        }
    }
}