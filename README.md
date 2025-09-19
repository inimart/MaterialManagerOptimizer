# Material Manager Optimizer

A powerful Unity Editor tool for analyzing and optimizing materials to maximize batching efficiency and improve runtime performance.

## 📋 Overview

Material Manager Optimizer is a Unity Editor extension that helps developers identify and resolve material batching bottlenecks in their projects. It provides a comprehensive view of all materials in your scene, facilitates the creation of material variants, and helps you leverage Unity's SRP Batcher to significantly reduce draw calls.

## 🎯 Features

- **Material Analysis**: Comprehensive overview of all materials and their usage in the scene
- **Variant Management**: Easy creation and management of material variants
- **Shader Keyword Tracking**: Identifies keyword differences between materials and their parents
- **SRP Batcher Optimization**: Helps maximize batching efficiency
- **Real-time Updates**: Automatically refreshes when materials are modified
- **Object Usage Tracking**: Shows which GameObjects use each material

## 🚀 Installation

### Via Package Manager (Local)
1. Download or clone this repository
2. Open Unity Package Manager (Window → Package Manager)
3. Click the **+** button → **Add package from disk...**
4. Navigate to and select the `package.json` file

### Via Git URL
1. Open Unity Package Manager
2. Click the **+** button → **Add package from git URL...**
3. Enter: `https://github.com/yourusername/material-manager-optimizer.git`

### As Embedded Package
Move the `MaterialManagerOptimizer` folder into your project's `Packages/` directory

## 📖 Understanding Batching

### What is Batching?

Batching is a crucial optimization technique where Unity groups multiple objects together and renders them in a single draw call instead of individual ones. This dramatically reduces CPU overhead and improves performance.

### Why Keep Batches Low?

Each draw call requires communication between the CPU and GPU, which is expensive. High batch counts can cause:
- **CPU Bottlenecks**: Processing overhead for each draw call
- **Reduced Frame Rate**: More time spent on rendering setup
- **Poor Mobile Performance**: Mobile GPUs are particularly sensitive to draw call counts
- **Increased Battery Consumption**: More CPU/GPU cycles mean higher power usage

### Unity's SRP Batcher

The Scriptable Render Pipeline (SRP) Batcher is Unity's modern batching system that:
- **Persistent GPU Data**: Keeps material properties on the GPU between frames
- **Reduced State Changes**: Minimizes shader and material property changes
- **Shader Variant Optimization**: Groups objects using the same shader variant
- **Per-Object Data**: Uses efficient per-object buffers for transforms and properties

#### SRP Batcher Requirements
For materials to be SRP Batcher compatible:
1. Objects must use the same shader variant
2. Materials can have different properties
3. Shaders must be SRP Batcher compatible (most URP/HDRP shaders are)

### Leveraging Material Variants

Material Variants are key to maximizing SRP Batching efficiency:

#### Benefits of Material Variants
- **Shared Shader**: Variants use the parent material's shader
- **Property Inheritance**: Only override specific properties
- **Reduced Memory**: Variants are lighter than full materials
- **Better Batching**: Objects with variant materials can batch together if they share the same parent

#### Best Practices
1. **Create a Base Material**: Set up your shader and common properties
2. **Use Variants for Variations**: Create variants for different colors, textures, or values
3. **Keep Keywords Consistent**: Ensure variants match parent shader keywords for optimal batching
4. **Minimize Unique Shaders**: Use fewer shader variations across your project

## 🔧 How It Works

### MaterialManagerOptimizerWindow

The main editor window that provides the user interface for material analysis and management.

#### Key Components:
- **Material List**: Displays all materials found in the scene with sortable columns
- **Variant Detection**: Automatically identifies material variants and their parents
- **Keyword Analysis**: Compares shader keywords between variants and parents
- **Object Usage Panel**: Shows which GameObjects use the selected material
- **Search & Filter**: Quickly find specific materials
- **Type Conversion**: Convert between standard materials and variants

#### Column Information:
- **Material Name**: The asset name of the material
- **Type**: Whether it's a standard Material or a Variant
- **Parent**: The parent material (for variants)
- **Switch**: Button to convert between material types
- **Match**: Indicates if variant keywords match the parent (✓/✗)
- **Keywords Diff**: Shows keyword differences from parent
- **Shader**: The shader used by the material

### MaterialManagerOptimizerAssetPostprocessor

An asset postprocessor that monitors material changes in your project.

#### Functionality:
- **Automatic Detection**: Listens for material imports, modifications, and deletions
- **Window Refresh**: Triggers automatic refresh of the Material Manager window
- **Real-time Updates**: Ensures the material list stays current
- **Performance Optimized**: Uses delayed calls to batch refresh operations

#### Monitored Events:
- Material creation
- Material modification
- Material deletion
- Material movement/renaming

## 💡 Usage Tips

1. **Open the Tool**: Navigate to `Tools → Material Manager Optimizer`

2. **Analyze Your Materials**:
   - Look for materials using the same shader
   - Identify materials that could be variants
   - Check the shader variant count

3. **Create Variants**:
   - Select materials that are variations of a base material
   - Assign the parent material
   - Ensure keywords match for optimal batching

4. **Monitor Performance**:
   - Keep track of total shader count
   - Minimize unique shader variants
   - Use the Match column to identify keyword mismatches

5. **Optimize Iteratively**:
   - Convert similar materials to variants
   - Consolidate shaders where possible
   - Test performance improvements in Play mode

## 📊 Performance Impact

By properly organizing materials and utilizing variants, you can expect:
- **30-70% reduction in draw calls** (typical scenarios)
- **Improved frame rates**, especially on mobile
- **Reduced memory usage** from material consolidation
- **Better CPU utilization** with fewer state changes

## 🔍 Troubleshooting

### Materials Not Batching
- Check if materials are using SRP Batcher compatible shaders
- Verify shader keywords match between variants and parents
- Ensure objects are not using different shader variants

### Window Not Updating
- Click the Refresh button manually
- Check if MaterialManagerOptimizerAssetPostprocessor is active
- Reimport the package if automatic updates stop working

## 📝 Requirements

- Unity 2020.3 or higher
- Universal Render Pipeline (URP) or High Definition Render Pipeline (HDRP) recommended
- Works with Built-in Render Pipeline with limited batching benefits

## 📄 License

[Add your license information here]

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests.

## 📧 Support

For questions, issues, or suggestions, please [open an issue](https://github.com/yourusername/material-manager-optimizer/issues) on GitHub.

---

Made with ❤️ for the Unity community by Inimart