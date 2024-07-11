using System;
using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class AllInOneShader : BaseShaderGUI
    {
        readonly string[] propertiesNames = { "OutlineSwitch", "OutlineWidth", "FlickerSwitch", "FlickerFrequency", "FlickerColor", "_DissolutionSwitch", "DissolutionMap", "FresnelSwitch", "FresnelColor", "FresnelPower", "HSISwitch", "Brightness", "Saturation", "Contrast", "FogSwitch" };
        bool m_IsShowCustomOptions = true;
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            m_IsShowCustomOptions = EditorGUILayout.Foldout(m_IsShowCustomOptions, "Custom Options", EditorStyles.foldoutHeader);
            if (m_IsShowCustomOptions)
            {
                foreach (var property in properties)
                {
                    foreach (var name in propertiesNames)
                    {
                        if (property.name.Contains(name))
                        {
                            var guiContent = new GUIContent(property.displayName);
                            materialEditor.ShaderProperty(property, guiContent);
                        }
                    }
                }
            }
            base.OnGUI(materialEditor, properties);
        }
        AllInOneGUI.AllInOneProperties shadingModelProperties;
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            shadingModelProperties = new AllInOneGUI.AllInOneProperties(properties);
        }
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, AllInOneGUI.SetMaterialKeywords);
        }
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            EditorGUIUtility.labelWidth = 0f;

            base.DrawSurfaceOptions(material);
        }
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            AllInOneGUI.Inputs(shadingModelProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            AllInOneGUI.Advanced(shadingModelProperties);
            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);
        }
    }
}