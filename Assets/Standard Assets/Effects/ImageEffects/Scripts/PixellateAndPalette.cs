using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu("Image Effects/Rendering/Pixellate")]
    public class PixellateAndPalette : MonoBehaviour
    {
        public int m_ResolutionX;
        public int m_ResolutionY;

        public Texture m_PaletteTexture;
        private Texture previousTexture;

        public Shader m_PixellateShader;
        private Material m_PixellateMaterial;

        private static Material CreateMaterial(Shader shader){
            if (!shader){
                return null;
            }

            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;
            return m;
        }

        private static void DestroyMaterial(Material mat){
            if(mat){
                DestroyImmediate (mat);
                mat = null;
            }
        }

        void OnDisable(){
            DestroyMaterial(m_PixellateMaterial);
        }

        void Start(){
            CreateMaterials();
        }

        private void CreateMaterials(){
            if(!m_PixellateMaterial && m_PixellateShader.isSupported){
                m_PixellateMaterial = CreateMaterial(m_PixellateShader);
                m_PixellateMaterial.renderQueue = 5000;
            }
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination){
            CreateMaterials();

            if(m_PaletteTexture != previousTexture && m_PaletteTexture != null){
                m_PixellateMaterial.SetFloat("_PaletteColorCount", (float)(m_PaletteTexture.width));
                m_PixellateMaterial.SetTexture("_Palette", m_PaletteTexture);

                previousTexture = m_PaletteTexture;
            }

            m_PixellateMaterial.SetFloat("_ResolutionX", (float)(m_ResolutionX));
            m_PixellateMaterial.SetFloat("_ResolutionY", (float)(m_ResolutionY));

            Graphics.Blit(source, destination, m_PixellateMaterial, 0);
        }
    }
}
