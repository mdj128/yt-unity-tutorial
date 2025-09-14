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
        if (urpLitShader == null)
        {n            Debug.LogError("Could not find the 'Universal Render Pipeline/Lit' shader. Make sure URP is installed correctly.");
            return;
        }

        foreach (string materialPath in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null && material.shader.name != "Universal Render Pipeline/Lit")
            {
                // Store the old textures
                Texture mainTex = material.GetTexture("_MainTex");
                Texture bumpMap = material.GetTexture("_BumpMap");
                Color color = material.GetColor("_Color");


                // Change the shader
                material.shader = urpLitShader;
                Debug.Log($"Converted shader for material: {material.name}");

                // Re-assign the textures to the new shader properties
                if (mainTex != null)
                {
                    material.SetTexture("_BaseMap", mainTex);
                }
                if (bumpMap != null)
                {
                    material.SetTexture("_BumpMap", bumpMap);
                }
                material.SetColor("_BaseColor", color);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished converting materials to URP shaders and re-assigning textures.");
    }
}