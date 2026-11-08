using System.IO;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using LaboratorioParalelepipedo.XR;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LaboratorioParalelepipedo.Editor
{
    /// <summary>
    /// Gera a cena base do laboratório: sala, mesa, caixa pegável (arestas, faces translúcidas, vértices A–H),
    /// painéis que leem o textos.json, uma luz direcional e o OVRCameraRig com os raios dos dois controles, o
    /// teleporte e o giro por passos. Recria a cena do zero a cada execução.
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

        // Altura das letras (TextMeshPro 3D: tamanho 1 ≈ 10 cm).
        private const float TamanhoDoTitulo = 0.8f;
        private const float TamanhoDoCredito = 0.4f;
        private const float TamanhoDoCorpo = 0.5f;

        [MenuItem("Laboratório/5. Gerar cena do laboratório")]
        public static void Gerar()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var rig = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoRig);
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AtlasDaFonte.CaminhoDaFonteTmp);
            var textos = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosTextos);
            var desafios = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosDesafios);
            if (rig == null || fonte == null || textos == null || desafios == null)
            {
                Debug.LogError("[Cena] Falta o OVRCameraRig, a fonte Nunito SDF (Regenerar atlas da fonte) ou os JSON de dados.");
                return;
            }

            CriarPasta(PastaDosMateriais);
            CriarPasta(Path.GetDirectoryName(CaminhoDaCena));

            // Cena vazia: a única câmera é a do OVRCameraRig. Os assets são carregados de novo depois do NewScene,
            // que descarrega o que nenhuma cena usa.
            Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AtlasDaFonte.CaminhoDaFonteTmp);
            textos = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosTextos);
            desafios = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfiguracaoDoProjeto.CaminhoDosDesafios);
            rig = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoRig);

            ConfigurarAmbiente();
            GameObject instanciaDoRig = CriarRig(rig);
            CriarModoEditor(instanciaDoRig);
            CriarSala();
            CriarMesa();
            CaixaPegavel caixa = CriarCaixa(fonte);
            CriarInteracao(instanciaDoRig, caixa);

            var dados = new GameObject("DadosDoLaboratorio").AddComponent<DadosDoLaboratorio>();
            dados.Configurar(textos, desafios);
            CriarPainel("PainelTitulo", new Vector3(0f, 1.75f, 2.4f), new Vector2(1.5f, 0.42f), dados, fonte,
                (ChavesDeTexto.AppTitulo, TamanhoDoTitulo, 0.06f), (ChavesDeTexto.AppCreditos, TamanhoDoCredito, -0.11f));
            CriarPainel("PainelControles", new Vector3(-1.25f, 1.45f, 1.9f), new Vector2(1.1f, 0.62f), dados, fonte,
                (null, TamanhoDoCorpo, 0f));
            Debug.Log($"[Cena] Painel de controles com: {AcoesDisponiveis()}.");

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

        private static GameObject CriarRig(GameObject prefab)
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

            return rig;
        }

        /// <summary>
        /// Olhar com o mouse e clique como gatilho, só no Editor sem óculos. Tag EditorOnly: o Unity tira o objeto
        /// do build, e o código do ModoEditor também só existe no Editor.
        /// </summary>
        private static void CriarModoEditor(GameObject rig)
        {
            var objeto = new GameObject("ModoEditor") { tag = "EditorOnly" };
            var modo = objeto.AddComponent<ModoEditor>();
            modo.Configurar(rig.transform.Find("TrackingSpace"),
                rig.transform.Find("TrackingSpace/CenterEyeAnchor").GetComponent<Camera>());
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

        private static CaixaPegavel CriarCaixa(TMP_FontAsset fonte)
        {
            Material aresta = MaterialOpaco("Aresta", ShaderUnlit, PaletaDoLaboratorio.Aresta);
            Material vertice = MaterialOpaco("Vertice", ShaderUnlit, PaletaDoLaboratorio.Aresta);
            Material face = MaterialTranslucido("FaceDaCaixa", PaletaDoLaboratorio.Face);
            Material destacada = MaterialOpaco("ArestaDestacada", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul);

            var objeto = new GameObject("Caixa");
            objeto.transform.position = CentroDaMesa + Vector3.up * AlturaDoTampo;
            var visual = objeto.AddComponent<VisualDaCaixa>();
            visual.Configurar(aresta, face, vertice, fonte);
            visual.Construir(new Paralelepipedo(4, 3, 2));

            // Corpo cinemático: o colisor se move com a caixa na mão sem a física recalcular a cena estática.
            var corpo = objeto.AddComponent<Rigidbody>();
            corpo.isKinematic = true;
            corpo.useGravity = false;
            var pegavel = objeto.AddComponent<CaixaPegavel>();
            pegavel.Configurar(destacada);
            pegavel.AjustarColisor();
            return pegavel;
        }

        /// <summary>
        /// Raios dos dois controles (com um modelo simples de cada um), teleporte (arco e marcador no chão) e giro
        /// por passos (esfera escura presa à cabeça, desligada fora do giro).
        /// </summary>
        private static void CriarInteracao(GameObject rig, CaixaPegavel caixa)
        {
            Material livre = MaterialOpaco("Raio", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel);
            Material noAlvo = MaterialOpaco("RaioNoAlvo", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul);
            Material controle = MaterialOpaco("Controle", ShaderUnlit, PaletaDoLaboratorio.PeDaMesa);
            Material valido = MaterialOpaco("Teleporte", ShaderUnlit, PaletaDoLaboratorio.Teleporte);
            Material invalido = MaterialOpaco("TeleporteInvalido", ShaderUnlit, PaletaDoLaboratorio.DestaqueLaranja);
            Material escuro = MaterialDoEscurecimento();

            RaioDoControle esquerdo = CriarRaio(rig.transform, "TrackingSpace/LeftHandAnchor/LeftControllerAnchor",
                OVRInput.Controller.LTouch, livre, noAlvo, controle);
            RaioDoControle direito = CriarRaio(rig.transform, "TrackingSpace/RightHandAnchor/RightControllerAnchor",
                OVRInput.Controller.RTouch, livre, noAlvo, controle);

            var arco = new GameObject("ArcoDoTeleporte").AddComponent<LineRenderer>();
            ConfigurarLinha(arco, 0.012f, valido);
            arco.positionCount = 0;
            arco.enabled = false;
            Transform marcador = Primitiva(PrimitiveType.Cylinder, arco.transform, "MarcadorDoTeleporte", Vector3.zero,
                new Vector3(0.3f, 0.003f, 0.3f), valido);
            marcador.GetComponent<MeshRenderer>().enabled = false;

            Transform cabeca = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
            Transform esfera = Primitiva(PrimitiveType.Sphere, cabeca, "EscurecimentoDoGiro", Vector3.zero,
                Vector3.one * 0.3f, escuro);
            esfera.GetComponent<MeshRenderer>().enabled = false;

            var locomocao = rig.AddComponent<LocomocaoDoAluno>();
            Definir(locomocao,
                ("_rig", rig.transform), ("_cabeca", cabeca), ("_raioEsquerdo", esquerdo), ("_raioDireito", direito),
                ("_arco", arco), ("_marcador", marcador.GetComponent<MeshRenderer>()),
                ("_materialValido", valido), ("_materialInvalido", invalido),
                ("_escurecimento", esfera.GetComponent<MeshRenderer>()), ("_caixa", caixa));
            var serializado = new SerializedObject(locomocao);
            float ladoPorDentro = LadoDaSala - EspessuraDaParede;
            serializado.FindProperty("_salaPorDentro").vector2Value = new Vector2(ladoPorDentro, ladoPorDentro);
            serializado.FindProperty("_centroDaMesa").vector3Value = CentroDaMesa;
            serializado.FindProperty("_tamanhoDaMesa").vector2Value = new Vector2(TamanhoDoTampo.x, TamanhoDoTampo.z);
            serializado.FindProperty("_alturaDaMesa").floatValue = AlturaDoTampo;
            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RaioDoControle CriarRaio(Transform rig, string caminho, OVRInput.Controller controle,
            Material livre, Material noAlvo, Material materialDoControle)
        {
            Transform ancora = rig.Find(caminho) ??
                               throw new System.InvalidOperationException($"Âncora '{caminho}' não encontrada no OVRCameraRig.");

            // Modelo simples: um bastão inclinado como o punho do Touch, atrás da ponta do raio.
            Transform modelo = Primitiva(PrimitiveType.Capsule, ancora, "ModeloDoControle", new Vector3(0f, -0.02f, -0.03f),
                new Vector3(0.035f, 0.045f, 0.035f), materialDoControle);
            modelo.localRotation = Quaternion.Euler(60f, 0f, 0f);

            var objeto = new GameObject($"Raio{(controle == OVRInput.Controller.LTouch ? "Esquerdo" : "Direito")}");
            objeto.transform.SetParent(ancora, false);
            var linha = objeto.AddComponent<LineRenderer>();
            ConfigurarLinha(linha, 0.004f, livre);
            linha.positionCount = 2;
            var raio = objeto.AddComponent<RaioDoControle>();
            raio.Configurar(controle, linha, livre, noAlvo, modelo.gameObject);
            return raio;
        }

        private static void ConfigurarLinha(LineRenderer linha, float largura, Material material)
        {
            linha.sharedMaterial = material;
            linha.widthMultiplier = largura;
            linha.useWorldSpace = true;
            linha.numCapVertices = 2;
            linha.shadowCastingMode = ShadowCastingMode.Off;
            linha.receiveShadows = false;
            linha.lightProbeUsage = LightProbeUsage.Off;
            linha.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        /// <summary>Ações que funcionam nesta cena: o painel de controles só mostra estas.</summary>
        internal static AcoesDoControle AcoesDisponiveis()
        {
            var acoes = AcoesDoControle.Nenhuma;
            foreach (var componente in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (componente is IAlvoDoRaio) acoes |= AcoesDoControle.Apontar;
                if (componente is IPegavel) acoes |= AcoesDoControle.Pegar;
                if (componente is LocomocaoDoAluno) acoes |= AcoesDoControle.Teleporte | AcoesDoControle.Giro;
            }

            return acoes;
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
                else textoDoCatalogo.Configurar(dados, texto, TextosDosControles.Chaves(AcoesDisponiveis()));
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

        /// <summary>Peça de malha embutida, sem colisor e sem sombras (objetos que se movem: nada de estático).</summary>
        private static Transform Primitiva(PrimitiveType tipo, Transform pai, string nome, Vector3 posicaoLocal,
            Vector3 escala, Material material)
        {
            var objeto = GameObject.CreatePrimitive(tipo);
            objeto.name = nome;
            Object.DestroyImmediate(objeto.GetComponent<Collider>());
            objeto.transform.SetParent(pai, false);
            objeto.transform.localPosition = posicaoLocal;
            objeto.transform.localScale = escala;
            var renderizador = objeto.GetComponent<MeshRenderer>();
            renderizador.sharedMaterial = material;
            renderizador.shadowCastingMode = ShadowCastingMode.Off;
            renderizador.receiveShadows = false;
            renderizador.lightProbeUsage = LightProbeUsage.Off;
            renderizador.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return objeto.transform;
        }

        private static void Definir(Object componente, params (string campo, Object valor)[] valores)
        {
            var serializado = new SerializedObject(componente);
            foreach (var (campo, valor) in valores)
            {
                SerializedProperty propriedade = serializado.FindProperty(campo) ??
                                                 throw new System.InvalidOperationException($"Campo '{campo}' não existe em {componente.GetType().Name}.");
                propriedade.objectReferenceValue = valor;
            }

            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Esfera preta vista por dentro (culling da frente), desenhada por cima de tudo o que está a mais de 15 cm.
        /// A opacidade vem do LocomocaoDoAluno, por MaterialPropertyBlock.
        /// </summary>
        private static Material MaterialDoEscurecimento()
        {
            Material material = MaterialTranslucido("EscurecimentoDoGiro", new Color(0f, 0f, 0f, 0f));
            material.SetFloat("_Cull", (float)CullMode.Front);
            material.renderQueue = (int)RenderQueue.Overlay;
            EditorUtility.SetDirty(material);
            return material;
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
