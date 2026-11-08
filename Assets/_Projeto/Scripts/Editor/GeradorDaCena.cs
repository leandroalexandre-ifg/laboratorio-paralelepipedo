using System.IO;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LaboratorioParalelepipedo.Editor
{
    /// <summary>
    /// Gera a cena base do laboratório: sala, mesa, caixa (arestas, faces translúcidas, vértices A–H), painéis que
    /// leem o textos.json, uma luz direcional e o OVRCameraRig. Recria a cena do zero a cada execução.
    /// Batchmode: -executeMethod LaboratorioParalelepipedo.Editor.GeradorDaCena.Gerar -quit
    /// </summary>
    public static class GeradorDaCena
    {
        public const string CaminhoDaCena = "Assets/_Projeto/Cenas/Laboratorio.unity";
        private const string PastaDosMateriais = "Assets/_Projeto/Materiais";
        private const string CaminhoDoRig = "Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab";

        private const string ShaderLit = "Universal Render Pipeline/Simple Lit";
        private const string ShaderUnlit = "Universal Render Pipeline/Unlit";

        // Medidas da sala, em metros. O aluno começa na origem, olhando para +z.
        private const float LadoDaSala = 8f;
        private const float AlturaDaParede = 3f;
        private const float EspessuraDaParede = 0.1f;
        private static readonly Vector3 CentroDaMesa = new Vector3(0f, 0f, 1.0f);
        private static readonly Vector3 TamanhoDoTampo = new Vector3(1.2f, 0.04f, 0.8f);
        private const float AlturaDoTampo = 0.76f;
        private const float LadoDoPe = 0.05f;

        /// <summary>Metros por unidade de medida da caixa: a caixa 4 × 3 × 2 vira 40 cm × 30 cm × 20 cm.</summary>
        public const float MetrosPorUnidade = 0.1f;

        // Altura das letras (TextMeshPro 3D: tamanho 1 ≈ 10 cm).
        private const float TamanhoDoTitulo = 0.8f;
        private const float TamanhoDoCredito = 0.4f;
        private const float TamanhoDoCorpo = 0.5f;

        [MenuItem("Laboratório/5. Gerar cena do laboratório")]
        public static void Gerar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var rig = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoRig);
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ConfiguracaoDoProjeto.CaminhoDaFonteTmp);
            var textos = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosTextos);
            var desafios = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosDesafios);
            if (rig == null || fonte == null || textos == null || desafios == null)
            {
                Debug.LogError("[Cena] Falta o OVRCameraRig, a fonte Nunito SDF (passo 4) ou os JSON de dados.");
                return;
            }

            CriarPasta(PastaDosMateriais);
            CriarPasta(Path.GetDirectoryName(CaminhoDaCena));

            // Cena vazia: a única câmera é a do OVRCameraRig. Os assets são carregados de novo depois do NewScene,
            // que descarrega o que nenhuma cena usa.
            Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ConfiguracaoDoProjeto.CaminhoDaFonteTmp);
            textos = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosTextos);
            desafios = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosDesafios);
            rig = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoRig);

            ConfigurarAmbiente();
            CriarRig(rig);
            CriarSala();
            CriarMesa();
            CriarCaixa(fonte);

            var dados = new GameObject("DadosDoLaboratorio").AddComponent<DadosDoLaboratorio>();
            dados.Configurar(textos, desafios);
            CriarPainel("PainelTitulo", new Vector3(0f, 1.75f, 2.4f), new Vector2(1.5f, 0.42f), dados, fonte,
                (ChavesDeTexto.AppTitulo, TamanhoDoTitulo, 0.06f), (ChavesDeTexto.AppCreditos, TamanhoDoCredito, -0.11f));
            CriarPainel("PainelControles", new Vector3(-1.25f, 1.45f, 1.9f), new Vector2(1.1f, 0.62f), dados, fonte,
                (null, TamanhoDoCorpo, 0f));

            EditorSceneManager.SaveScene(cena, CaminhoDaCena);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CaminhoDaCena, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Cena] Cena gerada em '{CaminhoDaCena}'.");
        }

        private static void ConfigurarAmbiente()
        {
            // Sem skybox e com luz ambiente de cor única: barato no Quest 2.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.52f, 0.54f);
            RenderSettings.fog = false;

            // A única luz em tempo real. Sem sombras.
            var luz = new GameObject("LuzDirecional").AddComponent<Light>();
            luz.type = LightType.Directional;
            luz.lightmapBakeType = LightmapBakeType.Realtime;
            luz.intensity = 0.6f;
            luz.shadows = LightShadows.None;
            luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CriarRig(GameObject prefab)
        {
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            rig.name = "OVRCameraRig";
            rig.transform.position = Vector3.zero;
            var gerenciador = rig.GetComponent<OVRManager>();
            if (gerenciador != null)
            {
                gerenciador.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
                EditorUtility.SetDirty(gerenciador);
            }

            Transform olho = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
            if (olho != null && olho.TryGetComponent(out Camera camera))
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = PaletaDoLaboratorio.Parede;
                camera.farClipPlane = 30f;
                EditorUtility.SetDirty(camera);
            }
        }

        private static void CriarSala()
        {
            var sala = new GameObject("Sala").transform;
            Material piso = MaterialOpaco("Piso", ShaderLit, PaletaDoLaboratorio.Piso);
            Material parede = MaterialOpaco("Parede", ShaderLit, PaletaDoLaboratorio.Parede);

            Bloco(sala, "Piso", new Vector3(0f, -0.05f, 0f), new Vector3(LadoDaSala, 0.1f, LadoDaSala), piso);
            float meio = LadoDaSala / 2;
            float y = AlturaDaParede / 2;
            Bloco(sala, "ParedeDoFundo", new Vector3(0f, y, meio), new Vector3(LadoDaSala, AlturaDaParede, EspessuraDaParede), parede);
            Bloco(sala, "ParedeDeTras", new Vector3(0f, y, -meio), new Vector3(LadoDaSala, AlturaDaParede, EspessuraDaParede), parede);
            Bloco(sala, "ParedeEsquerda", new Vector3(-meio, y, 0f), new Vector3(EspessuraDaParede, AlturaDaParede, LadoDaSala), parede);
            Bloco(sala, "ParedeDireita", new Vector3(meio, y, 0f), new Vector3(EspessuraDaParede, AlturaDaParede, LadoDaSala), parede);
        }

        private static void CriarMesa()
        {
            var mesa = new GameObject("Mesa").transform;
            mesa.position = CentroDaMesa;
            Material tampo = MaterialOpaco("TampoDaMesa", ShaderLit, PaletaDoLaboratorio.TampoDaMesa);
            Material pe = MaterialOpaco("PeDaMesa", ShaderLit, PaletaDoLaboratorio.PeDaMesa);

            Bloco(mesa, "Tampo", new Vector3(0f, AlturaDoTampo - TamanhoDoTampo.y / 2, 0f), TamanhoDoTampo, tampo);
            float alturaDoPe = AlturaDoTampo - TamanhoDoTampo.y;
            float x = TamanhoDoTampo.x / 2 - LadoDoPe;
            float z = TamanhoDoTampo.z / 2 - LadoDoPe;
            var tamanhoDoPe = new Vector3(LadoDoPe, alturaDoPe, LadoDoPe);
            for (int i = 0; i < 4; i++)
            {
                float sx = i % 2 == 0 ? -1 : 1;
                float sz = i < 2 ? -1 : 1;
                Bloco(mesa, $"Pe{i + 1}", new Vector3(sx * x, alturaDoPe / 2, sz * z), tamanhoDoPe, pe);
            }
        }

        private static void CriarCaixa(TMP_FontAsset fonte)
        {
            Material aresta = MaterialOpaco("Aresta", ShaderUnlit, PaletaDoLaboratorio.Aresta);
            Material vertice = MaterialOpaco("Vertice", ShaderUnlit, PaletaDoLaboratorio.Aresta);
            Material face = MaterialTranslucido("FaceDaCaixa", PaletaDoLaboratorio.Face);

            var caixa = new GameObject("Caixa").AddComponent<VisualDaCaixa>();
            caixa.transform.position = CentroDaMesa + Vector3.up * AlturaDoTampo;
            caixa.Configurar(aresta, face, vertice, fonte, MetrosPorUnidade);
            caixa.Construir(new Paralelepipedo(4, 3, 2));
        }

        /// <summary>Painel preso ao mundo, virado para o aluno. Sem chave no texto: mostra os controles.</summary>
        private static void CriarPainel(string nome, Vector3 posicao, Vector2 tamanho, DadosDoLaboratorio dados,
            TMP_FontAsset fonte, params (string chave, float tamanhoDaLetra, float y)[] linhas)
        {
            var painel = new GameObject(nome).transform;
            painel.position = posicao;
            Vector3 paraFrente = posicao - new Vector3(0f, posicao.y, 0f);
            painel.rotation = Quaternion.LookRotation(paraFrente.normalized, Vector3.up);

            Bloco(painel, "Fundo", new Vector3(0f, 0f, 0.01f), new Vector3(tamanho.x, tamanho.y, 0.01f),
                MaterialOpaco("FundoDoPainel", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel));

            float margem = 0.06f;
            foreach (var (chave, tamanhoDaLetra, y) in linhas)
            {
                var objeto = new GameObject(chave ?? "Controles", typeof(RectTransform));
                objeto.transform.SetParent(painel, false);
                objeto.transform.localPosition = new Vector3(0f, y, 0f);
                var texto = objeto.AddComponent<TextMeshPro>();
                texto.font = fonte;
                texto.fontSize = tamanhoDaLetra;
                texto.color = PaletaDoLaboratorio.Texto;
                texto.alignment = chave == null ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
                texto.textWrappingMode = TextWrappingModes.Normal;
                texto.lineSpacing = chave == null ? 25f : 0f;
                texto.rectTransform.sizeDelta = new Vector2(tamanho.x - 2 * margem,
                    chave == null ? tamanho.y - 2 * margem : tamanhoDaLetra * 0.15f);
                texto.text = chave ?? string.Empty;

                var textoDoCatalogo = objeto.AddComponent<TextoDoCatalogo>();
                if (chave != null) textoDoCatalogo.Configurar(dados, texto, chave);
                else
                    textoDoCatalogo.Configurar(dados, texto, ChavesDeTexto.ControlesApontar, ChavesDeTexto.ControlesPegar,
                        ChavesDeTexto.ControlesTeleporte, ChavesDeTexto.ControlesGiro);
                textoDoCatalogo.Atualizar();
            }
        }

        private static Transform Bloco(Transform pai, string nome, Vector3 posicaoLocal, Vector3 tamanho, Material material)
        {
            var bloco = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bloco.name = nome;
            Object.DestroyImmediate(bloco.GetComponent<Collider>());
            bloco.transform.SetParent(pai, false);
            bloco.transform.localPosition = posicaoLocal;
            bloco.transform.localScale = tamanho;
            var renderizador = bloco.GetComponent<MeshRenderer>();
            renderizador.sharedMaterial = material;
            renderizador.shadowCastingMode = ShadowCastingMode.Off;
            renderizador.receiveShadows = false;
            renderizador.lightProbeUsage = LightProbeUsage.Off;
            renderizador.reflectionProbeUsage = ReflectionProbeUsage.Off;
            // Estático: pronto para a iluminação baked.
            GameObjectUtility.SetStaticEditorFlags(bloco, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);
            return bloco.transform;
        }

        private static Material MaterialOpaco(string nome, string shader, Color cor)
        {
            Material material = CarregarOuCriarMaterial(nome, shader);
            material.SetColor("_BaseColor", cor);
            if (shader == ShaderLit)
            {
                // Fosco: sem brilho especular, que estourava o piso e a mesa.
                material.SetFloat("_SpecularHighlights", 0f);
                material.SetColor("_SpecColor", Color.black);
                material.SetFloat("_Smoothness", 0f);
                material.DisableKeyword("_SPECULAR_COLOR");
                material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Material transparente, visto dos dois lados (as faces da caixa aparecem por dentro também).</summary>
        private static Material MaterialTranslucido(string nome, Color cor)
        {
            Material material = CarregarOuCriarMaterial(nome, ShaderUnlit);
            material.SetColor("_BaseColor", cor);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CarregarOuCriarMaterial(string nome, string nomeDoShader)
        {
            Shader shader = Shader.Find(nomeDoShader) ??
                            throw new System.InvalidOperationException($"Shader '{nomeDoShader}' não encontrado.");
            string caminho = $"{PastaDosMateriais}/{nome}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, caminho);
            }

            material.shader = shader;
            material.enableInstancing = true;
            return material;
        }

        private static void CriarPasta(string pasta)
        {
            pasta = pasta.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(pasta)) return;
            CriarPasta(Path.GetDirectoryName(pasta));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(pasta).Replace('\\', '/'), Path.GetFileName(pasta));
        }
    }
}
