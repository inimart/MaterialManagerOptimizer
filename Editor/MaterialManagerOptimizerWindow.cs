using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Inimart.MaterialManagerOptimizer.Editor
{

public class MaterialManagerOptimizerWindow : EditorWindow
{
    private class MaterialInfo
    {
        public Material material;
        public bool isVariant;
        public Material parentMaterial;
        public List<GameObject> usedByObjects = new List<GameObject>();
        public bool keywordsMatchParent = false;
        public string keywordDifferences = "";
    }

    private List<MaterialInfo> materials = new List<MaterialInfo>();
    private List<MaterialInfo> filteredMaterials = new List<MaterialInfo>();
    private MaterialInfo selectedMaterial;

    private Vector2 scrollPositionMain;
    private Vector2 scrollPositionObjects;
    private int sortColumn = -1;
    private bool sortAscending = true;
    private string searchFilter = "";

    private int shaderCount = 0;
    private int shaderVariantCount = 0;

    private GUIStyle headerStyle, iconStyle;
    
    // Column widths in the new order
    private float nameColumnWidth = 200f;
    private float typeColumnWidth = 70f;
    private float parentColumnWidth = 150f;
    private float switchColumnWidth = 80f;
    private float keywordsMatchColumnWidth = 60f;
    private float keywordsDiffColumnWidth = 150f;
    private float shaderColumnWidth = 150f;

    [MenuItem("Tools/Material Manager Optimizer")]
    public static void ShowWindow()
    {
        MaterialManagerOptimizerWindow window = GetWindow<MaterialManagerOptimizerWindow>("Material Manager Optimizer");
        window.minSize = new Vector2(1100, 400);
    }

    private void OnEnable()
    {
        RefreshMaterialList();
        InitializeStyles();
    }

    private void OnGUI()
    {
        InitializeStyles();
        
        EditorGUILayout.BeginHorizontal();
        DrawMaterialList();
        
        if (selectedMaterial != null)
        {
            DrawObjectsList();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    #region Style Initialization
    private void InitializeStyles()
    {
        // We only need styles for text alignment and headers now, 
        // as backgrounds are drawn manually for robustness.
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.toolbarButton) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        }
        
        if (iconStyle == null)
        {
            iconStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        }
    }
    #endregion

    #region UI Drawing
    private void DrawMaterialList()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawListHeaderControls();
        EditorGUILayout.Space(2);
        DrawTableHeaders();
        
        scrollPositionMain = EditorGUILayout.BeginScrollView(scrollPositionMain);
        if (filteredMaterials != null)
        {
            for (int i = 0; i < filteredMaterials.Count; i++)
            {
                DrawMaterialRow(filteredMaterials[i], i);
            }
        }
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawListHeaderControls()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("Refresh", GUILayout.Width(80))) RefreshMaterialList();
        EditorGUILayout.LabelField($"Materials: {materials.Count}", EditorStyles.toolbarButton, GUILayout.Width(100));
        EditorGUILayout.LabelField($"Shaders: {shaderCount}", EditorStyles.toolbarButton, GUILayout.Width(100));
        EditorGUILayout.LabelField($"Variants: {shaderVariantCount}", EditorStyles.toolbarButton, GUILayout.Width(100));
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.LabelField($"Showing: {filteredMaterials.Count}", EditorStyles.toolbarButton, GUILayout.Width(100));
        EditorGUI.BeginChangeCheck();
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField, GUILayout.MinWidth(150));
        if (EditorGUI.EndChangeCheck()) ApplyFilter();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawTableHeaders()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        // Columns in the requested order
        DrawSortableHeader("Material Name", 0, nameColumnWidth);
        DrawSortableHeader("Type", 1, typeColumnWidth);
        DrawSortableHeader("Parent", 2, parentColumnWidth);
        GUILayout.Label("Switch", headerStyle, GUILayout.Width(switchColumnWidth));
        DrawSortableHeader("Match", 3, keywordsMatchColumnWidth);
        GUILayout.Label("Keywords Diff", headerStyle, GUILayout.Width(keywordsDiffColumnWidth));
        DrawSortableHeader("Shader", 4, shaderColumnWidth);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSortableHeader(string title, int columnIndex, float width)
    {
        if (sortColumn == columnIndex) title += sortAscending ? " ▲" : " ▼";
        if (GUILayout.Button(title, headerStyle, GUILayout.Width(width))) SortByColumn(columnIndex);
    }
    
    private void DrawMaterialRow(MaterialInfo materialInfo, int index)
    {
        if (materialInfo?.material == null) return;
        
        // **FIXED**: Manually draw background rects for robust alternating colors.
        Rect rowRect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(22));

        if (Event.current.type == EventType.Repaint)
        {
            if (selectedMaterial == materialInfo)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.2f, 0.4f, 0.6f, 0.5f));
            }
            else if (index % 2 != 0)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.5f, 0.5f, 0.5f, 0.1f));
            }
        }
        
        // --- Columns in the new order ---
        
        // Material Name
        if (GUILayout.Button(materialInfo.material.name, EditorStyles.label, GUILayout.Width(nameColumnWidth))) SelectMaterial(materialInfo);
        
        // Type
        EditorGUILayout.LabelField(materialInfo.isVariant ? "Variant" : "Material", GUILayout.Width(typeColumnWidth));
        
        // Parent
        EditorGUI.BeginChangeCheck();
        Material newParent = (Material)EditorGUILayout.ObjectField(materialInfo.parentMaterial, typeof(Material), false, GUILayout.Width(parentColumnWidth));
        if (EditorGUI.EndChangeCheck() && newParent != materialInfo.material)
        {
            if (newParent != null) SetVariantParent(materialInfo, newParent);
        }

        // Switch
        if (GUILayout.Button(materialInfo.isVariant ? "> Material" : "> Variant", GUILayout.Width(switchColumnWidth))) SwitchMaterialType(materialInfo);

        // Match
        if (materialInfo.isVariant)
        {
            GUI.contentColor = materialInfo.keywordsMatchParent ? Color.green : Color.red;
            EditorGUILayout.LabelField(materialInfo.keywordsMatchParent ? "✓" : "✗", iconStyle, GUILayout.Width(keywordsMatchColumnWidth));
            GUI.contentColor = Color.white;
        } else GUILayout.Space(keywordsMatchColumnWidth);

        // Keywords Diff
        EditorGUILayout.LabelField(materialInfo.keywordDifferences, GUILayout.Width(keywordsDiffColumnWidth));

        // Shader
        EditorGUILayout.ObjectField(materialInfo.material.shader, typeof(Shader), false, GUILayout.Width(shaderColumnWidth));
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawObjectsList()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinWidth(200));
        if (selectedMaterial?.material == null)
        {
            EditorGUILayout.LabelField("No material selected.", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical(); return;
        }

        EditorGUILayout.LabelField("Objects using: " + selectedMaterial.material.name, EditorStyles.boldLabel);
        
        scrollPositionObjects = EditorGUILayout.BeginScrollView(scrollPositionObjects);
        var objects = selectedMaterial.usedByObjects.Where(o => o != null).ToList();
        for(int i = 0; i < objects.Count; i++)
        {
            // **FIXED**: Apply alternating row colors to this list as well.
            Rect rowRect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(20));
            if (Event.current.type == EventType.Repaint && i % 2 != 0)
            {
                EditorGUI.DrawRect(rowRect, new Color(0.5f, 0.5f, 0.5f, 0.1f));
            }
            
            if (GUILayout.Button(objects[i].name, EditorStyles.label))
            {
                SelectAndFocusObject(objects[i]);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region Core Logic
    public void RefreshMaterialList() { /* Unchanged from previous version */ 
        if (this == null) return;
        materials.Clear();
        var materialUsage = new Dictionary<Material, List<GameObject>>();
        var uniqueShaders = new HashSet<Shader>();
        var uniqueShaderVariants = new HashSet<string>();
        var renderers = FindObjectsOfType<Renderer>(true);
        foreach (var renderer in renderers) {
            foreach (var mat in renderer.sharedMaterials) {
                if (mat == null) continue;
                if (!materialUsage.ContainsKey(mat)) materialUsage[mat] = new List<GameObject>();
                if (!materialUsage[mat].Contains(renderer.gameObject)) materialUsage[mat].Add(renderer.gameObject);
                if (mat.shader != null) {
                    uniqueShaders.Add(mat.shader);
                    string keywords = string.Join(" ", mat.shaderKeywords.OrderBy(s => s));
                    uniqueShaderVariants.Add(mat.shader.name + keywords);
                }
            }
        }
        shaderCount = uniqueShaders.Count;
        shaderVariantCount = uniqueShaderVariants.Count;
        foreach (var kvp in materialUsage) {
            var info = new MaterialInfo { material = kvp.Key, isVariant = IsVariant(kvp.Key), usedByObjects = kvp.Value };
            if (info.isVariant) {
                info.parentMaterial = GetVariantParent(kvp.Key);
                if (info.parentMaterial != null) {
                    info.keywordDifferences = GetKeywordDifferences(info.material, info.parentMaterial);
                    info.keywordsMatchParent = string.IsNullOrEmpty(info.keywordDifferences);
                }
            }
            materials.Add(info);
        }
        ApplyFilter();
        Repaint();
    }
    
    private void ApplyFilter() { /* Unchanged */ 
        if (string.IsNullOrEmpty(searchFilter)) {
            filteredMaterials = new List<MaterialInfo>(materials);
        } else {
            string lowerFilter = searchFilter.ToLowerInvariant();
            filteredMaterials = materials.Where(m => m.material.name.ToLowerInvariant().Contains(lowerFilter)).ToList();
        }
        SortByColumn(sortColumn, false);
    }
    
    private string GetKeywordDifferences(Material mat1, Material mat2) { /* Unchanged */
        if (mat1 == null || mat2 == null) return "N/A";
        var keywords1 = new HashSet<string>(mat1.shaderKeywords);
        var keywords2 = new HashSet<string>(mat2.shaderKeywords);
        keywords1.SymmetricExceptWith(keywords2);
        return string.Join(", ", keywords1);
    }
    
    private void SwitchMaterialType(MaterialInfo materialInfo) { /* Unchanged */ 
        Material mat = materialInfo.material;
        if (mat == null) { RefreshMaterialList(); return; }
        string path = AssetDatabase.GetAssetPath(mat);
        if (string.IsNullOrEmpty(path)) { EditorUtility.DisplayDialog("Error", "Cannot switch an unsaved material.", "OK"); return; }
        try {
            if (materialInfo.isVariant) {
                mat.parent = null;
                EditorUtility.SetDirty(mat);
            } else {
                EditorUtility.DisplayDialog("Info", $"To convert '{mat.name}' to a variant, assign a 'Parent' material.", "OK");
                return;
            }
            AssetDatabase.SaveAssets();
            RefreshMaterialList();
        }
        catch (System.Exception e) { Debug.LogError($"Error switching material type for {mat.name}: {e}"); }
    }
    
    private void SetVariantParent(MaterialInfo materialInfo, Material newParent) { /* Unchanged */ 
        materialInfo.material.parent = newParent;
        EditorUtility.SetDirty(materialInfo.material);
        AssetDatabase.SaveAssets();
        RefreshMaterialList();
    }
    
    private void SortByColumn(int column, bool toggleAscending = true) {
        if (toggleAscending) {
            if (sortColumn == column) sortAscending = !sortAscending;
            else sortAscending = true;
        }
        sortColumn = column;
        if (sortColumn < 0) {
            filteredMaterials = filteredMaterials.OrderBy(m => m.material.name).ToList();
            return;
        }

        System.Func<MaterialInfo, object> keySelector;
        // Remapped cases for the new column order
        switch (column) {
            case 0: keySelector = m => m.material.name; break; // Name
            case 1: keySelector = m => m.isVariant; break; // Type
            case 2: keySelector = m => m.parentMaterial?.name ?? ""; break; // Parent
            case 3: keySelector = m => m.isVariant ? (m.keywordsMatchParent ? 2 : 1) : 0; break; // Match
            case 4: keySelector = m => m.material.shader?.name ?? ""; break; // Shader
            default: keySelector = m => m.material.name; break;
        }
        
        if (sortAscending) filteredMaterials = filteredMaterials.OrderBy(keySelector).ThenBy(m => m.material.name).ToList();
        else filteredMaterials = filteredMaterials.OrderByDescending(keySelector).ThenBy(m => m.material.name).ToList();
    }

    private bool IsVariant(Material material) => material.parent as Material != null;
    private Material GetVariantParent(Material variant) => variant.parent as Material;

    private void SelectMaterial(MaterialInfo info) { /* Unchanged */ 
        if (info?.material == null) { selectedMaterial = null; return; }
        selectedMaterial = info;
        Selection.activeObject = info.material;
    }
    
    private void SelectAndFocusObject(GameObject obj) { /* Unchanged */ 
        if (obj == null) return;
        Selection.activeGameObject = obj;
        EditorGUIUtility.PingObject(obj);
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
    }
    #endregion
}
}