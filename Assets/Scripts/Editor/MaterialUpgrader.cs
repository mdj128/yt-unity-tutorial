
using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialUpgrader : EditorWindow
{
    [MenuItem("Assets/Upgrade Tree9 Materials to URP")]
    public static void UpgradeMaterials()
    {
        string assetPath = "Assets/Tree9";
        string[] materialPaths = Directory.GetFiles(Application.dataPath + assetPath.Substring("Assets".Length), "*.mat", SearchOption.AllDirectories);

        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLitShader == null)
        {
            Debug.LogError("Could not find the 'Universal Render Pipeline/Lit' shader. Make sure URP is installed correctly.");
            return;
        }

        foreach (string materialPath in materialPaths)
        {
            string relativePath = "Assets" + materialPath.Substring(Application.dataPath.Length);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(relativePath);

            if (material != null && material.shader.name != urpLitShader.name)
            {
                Debug.Log("Upgrading material: " + material.name);

                Texture mainTex = material.GetTexture("_MainTex");
                Texture bumpMap = material.GetTexture("_BumpMap");

                material.shader = urpLitShader;

                material.SetTexture("_BaseMap", mainTex);
                material.SetTexture("_BumpMap", bumpMap);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Material upgrade complete!");
    }
}
