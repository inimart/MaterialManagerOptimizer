using UnityEditor;

namespace Inimart.MaterialManagerOptimizer.Editor
{

/// <summary>
/// This class listens for changes in project assets.
/// If a material is modified, imported, or deleted, it triggers a refresh
/// on the MaterialManagerOptimizerWindow if it's open.
/// </summary>
public class MaterialManagerOptimizerAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool materialChanged = false;

        // Check both imported/modified and deleted assets for any .mat file
        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                materialChanged = true;
                break;
            }
        }

        if (!materialChanged)
        {
            foreach (string path in deletedAssets)
            {
                if (path.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
                {
                    materialChanged = true;
                    break;
                }
            }
        }
        
        // Also check moved assets
        if (!materialChanged)
        {
            foreach (string path in movedAssets)
            {
                if (path.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
                {
                    materialChanged = true;
                    break;
                }
            }
        }


        if (materialChanged)
        {
            // Use a delayed call to ensure the editor has finished its own processing
            // before we refresh our window's data.
            EditorApplication.delayCall += () =>
            {
                var window = EditorWindow.GetWindow<MaterialManagerOptimizerWindow>(false, null, false);
                if (window != null)
                {
                    window.RefreshMaterialList();
                }
            };
        }
    }
}
}