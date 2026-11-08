using System.IO;
using LaboratorioParalelepipedo.Nucleo.Dados;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace LaboratorioParalelepipedo.Editor
{
    /// <summary>
    /// Gera o atlas estático da fonte "Nunito SDF" com os caracteres dos JSON de dados mais os extras
    /// (regra em <see cref="CaracteresDoAtlas"/>, a mesma do Tools/verificar_glifos.py) e a deixa como
    /// fonte padrão do TextMeshPro. Estático: nada muda no asset ao dar Play, e o .ttf não vai para o APK.
    /// Rode de novo quando um texto novo usar um caractere fora do atlas (o teste EditMode avisa).
    /// Batchmode: -executeMethod LaboratorioParalelepipedo.Editor.AtlasDaFonte.Regenerar -quit
    /// </summary>
    public static class AtlasDaFonte
    {
        public const string CaminhoDaFonte = "Assets/_Projeto/Fontes/Nunito-SemiBold.ttf";
        public const string CaminhoDaFonteTmp = "Assets/_Projeto/Fontes/Nunito SDF.asset";
        public const string CaminhoDosExtras = "Assets/_Projeto/Fontes/caracteres-extras.txt";
        public const string CaminhoDosTextos = "Assets/_Projeto/Dados/textos.json";
        public const string CaminhoDosDesafios = "Assets/_Projeto/Dados/desafios.json";

        private const int TamanhoDeAmostra = 72;
        private const int Margem = 7;
        private const int LarguraDoAtlas = 1024;
        private const int AlturaDoAtlas = 1024;

        /// <summary>Caracteres que o atlas deve ter, lidos dos arquivos do projeto.</summary>
        public static string CaracteresEsperados() =>
            CaracteresDoAtlas.Coletar(Ler(CaminhoDosExtras), Ler(CaminhoDosTextos), Ler(CaminhoDosDesafios));

        [MenuItem("Laboratório/Regenerar atlas da fonte")]
        public static void Regenerar()
        {
            var ttf = AssetDatabase.LoadAssetAtPath<Font>(CaminhoDaFonte);
            if (ttf == null)
            {
                Falhar($"Fonte não encontrada em '{CaminhoDaFonte}'.");
                return;
            }

            string caracteres = CaracteresEsperados();
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CaminhoDaFonteTmp);
            if (fonte == null)
            {
                fonte = TMP_FontAsset.CreateFontAsset(ttf, TamanhoDeAmostra, Margem, GlyphRenderMode.SDFAA,
                    LarguraDoAtlas, AlturaDoAtlas, AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: false);
                fonte.name = Path.GetFileNameWithoutExtension(CaminhoDaFonteTmp);
                AssetDatabase.CreateAsset(fonte, CaminhoDaFonteTmp);
                AssetDatabase.AddObjectToAsset(fonte.material, fonte);
                AssetDatabase.AddObjectToAsset(fonte.atlasTexture, fonte);
            }
            else
            {
                // Mantém o mesmo asset (e o GUID usado pela cena): volta a dinâmico, ajusta o tamanho e limpa.
                fonte.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                var serializado = new SerializedObject(fonte);
                serializado.FindProperty("m_AtlasWidth").intValue = LarguraDoAtlas;
                serializado.FindProperty("m_AtlasHeight").intValue = AlturaDoAtlas;
                serializado.FindProperty("m_AtlasPadding").intValue = Margem;
                serializado.ApplyModifiedPropertiesWithoutUndo();
                fonte.isMultiAtlasTexturesEnabled = false;
                if (FontEngine.LoadFontFace(ttf, TamanhoDeAmostra) == FontEngineError.Success)
                    fonte.faceInfo = FontEngine.GetFaceInfo();
                fonte.material.SetFloat(ShaderUtilities.ID_GradientScale, Margem + 1);
                fonte.material.SetFloat(ShaderUtilities.ID_TextureWidth, LarguraDoAtlas);
                fonte.material.SetFloat(ShaderUtilities.ID_TextureHeight, AlturaDoAtlas);
            }

            fonte.ClearFontAssetData(setAtlasSizeToZero: true);
            fonte.TryAddCharacters(caracteres, out string faltando);
            if (!string.IsNullOrEmpty(faltando))
            {
                Falhar($"Não couberam no atlas ou não existem na Nunito: {faltando}");
                return;
            }

            fonte.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(fonte.atlasTexture);
            EditorUtility.SetDirty(fonte.material);
            EditorUtility.SetDirty(fonte);
            DefinirComoPadrao(fonte);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Fonte] Atlas estático de '{CaminhoDaFonteTmp}' com {fonte.characterTable.Count} caracteres " +
                      $"({LarguraDoAtlas}×{AlturaDoAtlas}); fonte padrão do TextMeshPro.");
        }

        /// <summary>Fonte padrão nas TMP Settings: textos sem fonte definida usam a Nunito, nunca a LiberationSans.</summary>
        private static void DefinirComoPadrao(TMP_FontAsset fonte)
        {
            var configuracoes = AssetDatabase.LoadAssetAtPath<TMP_Settings>(ConfiguracaoDoProjeto.ConfiguracoesDoTmp);
            if (configuracoes == null)
            {
                Falhar($"TMP Settings não encontrado em '{ConfiguracaoDoProjeto.ConfiguracoesDoTmp}'.");
                return;
            }

            var serializado = new SerializedObject(configuracoes);
            serializado.FindProperty("m_defaultFontAsset").objectReferenceValue = fonte;
            serializado.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(configuracoes);
        }

        private static string Ler(string caminho)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);
            return asset != null ? asset.text : throw new FileNotFoundException($"'{caminho}' não encontrado.");
        }

        private static void Falhar(string mensagem)
        {
            Debug.LogError($"[Fonte] {mensagem}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
