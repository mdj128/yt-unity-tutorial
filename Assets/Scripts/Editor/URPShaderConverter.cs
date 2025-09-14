using UnityEngine;
using UnityEditor;
using System.IO;

public class URPShaderConverter : EditorWindow
{
    [MenuItem("Tools/Fix Polygon Tree Shaders")]
    public static void ConvertShaders()
    {
        string assetPath = "Assets/polygonTrees";
        string[] materialPaths = Directory.GetFiles(assetPath, "*.mat", SearchOption.AllDirectories);

        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader urpVertexColorUnlit = Shader.Find("Universal Render Pipeline/Unlit Vertex Color");
        if (urpLitShader == null)
        {
            Debug.LogError("Could not find the 'Universal Render Pipeline/Lit' shader. Make sure URP is installed correctly.");
            return;
        }
        if (urpVertexColorUnlit == null)
        {
            Debug.LogWarning("Could not find 'Universal Render Pipeline/Unlit Vertex Color' shader. Materials without textures may still appear gray if they rely on vertex colors.");
        }

        foreach (string materialPath in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                continue;
            }

            // Store the old textures and colors BEFORE switching shader
            Texture mainTex = material.GetTexture("_MainTex");
            Texture bumpMap = material.GetTexture("_BumpMap");
            Color color = Color.white;
            if (material.HasProperty("_Color"))
            {
                color = material.GetColor("_Color");
            }

            // Decide which shader to use: if there's an albedo texture, prefer URP Lit; otherwise, use a vertex-color shader (if available)
            bool hasMainTex = mainTex != null;
            if (hasMainTex)
            {
                material.shader = urpLitShader;
                Debug.Log($"Converted shader for material: {material.name}");

                // Re-assign the textures to the new shader properties
                material.SetTexture("_BaseMap", mainTex);
                if (bumpMap != null) material.SetTexture("_BumpMap", bumpMap);
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            }
            else if (urpVertexColorUnlit != null)
            {
                material.shader = urpVertexColorUnlit;
                // Ensure base color matches original tint (if any)
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                Debug.Log($"Assigned Vertex Color shader to material without textures: {material.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished converting materials to URP shaders and re-assigning textures.");
    }
}
