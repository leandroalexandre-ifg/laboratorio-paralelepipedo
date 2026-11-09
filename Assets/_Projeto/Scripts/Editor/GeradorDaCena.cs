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

        /// <summary>
        /// Giro da caixa na mesa, em graus, em torno do eixo vertical (o fundo vai para a direita): da origem, o aluno
        /// vê a frente e a face da esquerda e os 8 vértices separados. 20° separa por ao menos 6,8 cm os vértices de
        /// todas as caixas da estação de diagonais (2×3×6, 3×4×12, 2×2×2); com 30°, A cobriria C na 2×3×6 (a diagonal
        /// AC da base, de 33,7°, ficaria quase na linha do olhar). A convenção dos vértices não muda.
        /// </summary>
        public const float GiroDaCaixaNaMesa = 20f;

        /// <summary>Distância e direção (graus, + à direita) dos painéis laterais, a partir da origem do aluno.</summary>
        private const float DistanciaDaEstacao = 1.25f, DirecaoDaEstacao = 38f;
        private const float DistanciaDosControles = 1.45f, DirecaoDosControles = -31f;
        private static readonly Vector3 TamanhoDoTampo = new Vector3(1.2f, 0.04f, 0.8f);
        private const float AlturaDoTampo = 0.76f;
        private const float LadoDoPe = 0.05f;

        // Altura das letras (TextMeshPro 3D: tamanho 1 ≈ 10 cm).
        private const float TamanhoDoTitulo = 0.8f;
        private const float TamanhoDoCredito = 0.4f;
        private const float TamanhoDoCorpo = 0.5f;
        private const float TamanhoDaDica = 0.2f;

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
            var dados = new GameObject("DadosDoLaboratorio").AddComponent<DadosDoLaboratorio>();
            dados.Configurar(textos, desafios);
            CaixaPegavel caixa = CriarCaixa(fonte, PrimeiraCaixaDaEstacao(desafios));
            RecentralizacaoDoAluno recentralizacao = CriarInteracao(instanciaDoRig, dados, fonte);

            // Título com "Começar" ao fundo, acima da caixa; estação à direita da mesa; controles à esquerda.
            // Alto o bastante para não encostar nos rótulos B e C (topo da caixa) com olhos de 1,3 m a 1,7 m.
            Transform titulo = CriarPainel("PainelTitulo", new Vector3(0f, 2.1f, 2.3f), new Vector2(1.4f, 0.72f), dados, fonte,
                (ChavesDeTexto.AppTitulo, TamanhoDoTitulo, 0.2f), (ChavesDeTexto.AppCreditos, TamanhoDoCredito, 0.05f));
            CriarVersao(titulo, dados, fonte, new Vector3(0f, -0.04f, 0f), 1.28f);
            BotaoDoPainel comecar = CriarBotao(titulo, "BotaoComecar", new Vector3(0f, -0.2f, 0f), new Vector2(0.5f, 0.14f),
                dados, fonte, ChavesDeTexto.AberturaComecar, principal: true);
            // "Começar" leva à Estação 1; o fechamento dela leva à Estação 2 (mesmo lugar do painel).
            EstacaoDasDiagonaisNaCena estacao = CriarEstacaoDasDiagonais(dados, fonte, caixa, null);
            EstacaoDosElementosNaCena elementos = CriarEstacaoDosElementos(dados, fonte, caixa, comecar, estacao);
            instanciaDoRig.AddComponent<VibracaoDoAcerto>().Configurar(estacao, elementos);

            CriarPainelDeInstrucoes(dados, fonte);
            Debug.Log($"[Cena] Painel de controles com: {AcoesDisponiveis()}.");
            CriarPainelDeDiagnostico(dados, fonte, recentralizacao, instanciaDoRig, estacao, elementos);

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

        /// <summary>Caixa do primeiro desafio da Estação 1 (4 × 3 × 2 no desafios.json atual), a que o aluno vê ao entrar.</summary>
        private static Paralelepipedo PrimeiraCaixaDaEstacao(TextAsset desafios)
        {
            var daEstacao = Nucleo.Estacoes.EstacaoDosElementos.DesafiosDaEstacao(CarregadorDeDados.CarregarDesafios(desafios));
            return daEstacao.Count > 0 ? daEstacao[0].Caixa : new Paralelepipedo(4, 3, 2);
        }

        private static CaixaPegavel CriarCaixa(TMP_FontAsset fonte, Paralelepipedo caixa)
        {
            Material aresta = MaterialOpaco("Aresta", ShaderUnlit, PaletaDoLaboratorio.Aresta);
            Material vertice = MaterialOpaco("Vertice", ShaderUnlit, PaletaDoLaboratorio.Aresta);
            Material face = MaterialTranslucido("FaceDaCaixa", PaletaDoLaboratorio.Face);
            Material destacada = MaterialOpaco("ArestaDestacada", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul);

            var objeto = new GameObject("Caixa");
            // 2 mm acima do tampo: a face de baixo, translúcida, brigava com o tampo pela profundidade (z-fighting).
            objeto.transform.SetPositionAndRotation(CentroDaMesa + Vector3.up * (AlturaDoTampo + 0.002f),
                Quaternion.Euler(0f, GiroDaCaixaNaMesa, 0f));   // a CaixaPegavel guarda esta orientação para a volta
            var visual = objeto.AddComponent<VisualDaCaixa>();
            visual.Configurar(aresta, face, vertice, fonte, MaterialDoRotuloSobreposto(fonte));
            visual.Construir(caixa);

            // Corpo cinemático: o colisor se move com a caixa na mão sem a física recalcular a cena estática.
            var corpo = objeto.AddComponent<Rigidbody>();
            corpo.isKinematic = true;
            corpo.useGravity = false;
            var pegavel = objeto.AddComponent<CaixaPegavel>();
            pegavel.Configurar(destacada);
            pegavel.AjustarColisor();

            var desenho = objeto.AddComponent<DesenhoDasDiagonais>();
            desenho.Configurar(visual,
                MaterialOpaco("DiagonalDaBase", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul),
                MaterialOpaco("DiagonalDaCaixa", ShaderUnlit, PaletaDoLaboratorio.DestaqueLaranja),
                MaterialTranslucido("TrianguloDaBase", ComOpacidade(PaletaDoLaboratorio.DestaqueAzul, 0.35f)),
                MaterialTranslucido("TrianguloLevantado", ComOpacidade(PaletaDoLaboratorio.DestaqueLaranja, 0.3f)),
                MaterialTranslucido("BaseDestacada", ComOpacidade(PaletaDoLaboratorio.BasesEscuro, 0.3f)),
                MaterialOpaco("MarcaDeAnguloReto", ShaderUnlit, PaletaDoLaboratorio.Texto), fonte);

            // Estação 1: arestas da face em montagem e a face pronta com a cor do par dela.
            objeto.AddComponent<DesenhoDaFace>().Configurar(visual,
                MaterialOpaco("ArestaDaFace", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul),
                MaterialTranslucido("FaceMontadaBases", ComOpacidade(PaletaDoLaboratorio.BasesClaro, OpacidadeDaFaceMontada)),
                MaterialTranslucido("FaceMontadaFrenteEFundo", ComOpacidade(PaletaDoLaboratorio.FrenteEFundoClaro, OpacidadeDaFaceMontada)),
                MaterialTranslucido("FaceMontadaEsquerdaEDireita",
                    ComOpacidade(PaletaDoLaboratorio.EsquerdaEDireitaClaro, OpacidadeDaFaceMontada)), fonte);
            return pegavel;
        }

        /// <summary>
        /// Cópia do material da Nunito SDF com o shader Overlay do TextMeshPro: os rótulos da caixa (vértices, a, b, c,
        /// d, D) aparecem mesmo atrás das arestas e faces. Fila 3010: depois das faces e triângulos translúcidos (3000),
        /// que não os desbotam, e antes da tela escura (3050), que continua cobrindo os rótulos na recentralização.
        /// </summary>
        private const int FilaDosRotulos = (int)RenderQueue.Transparent + 10;
        private const int DeslocamentoDaTelaEscura = 50;

        private static Material MaterialDoRotuloSobreposto(TMP_FontAsset fonte)
        {
            Material material = CarregarOuCriarMaterial("RotuloSobreposto", "TextMeshPro/Mobile/Distance Field Overlay");
            material.CopyPropertiesFromMaterial(fonte.material);
            material.shader = Shader.Find("TextMeshPro/Mobile/Distance Field Overlay");
            material.renderQueue = FilaDosRotulos;
            EditorUtility.SetDirty(material);
            return material;
        }

        private const float OpacidadeDaFaceMontada = 0.85f;

        private static Color ComOpacidade(Color cor, float opacidade) => new Color(cor.r, cor.g, cor.b, opacidade);

        private static readonly Vector2 TamanhoDoPainelDaEstacao = new Vector2(0.9f, 0.86f);
        private const float MargemDaEstacao = 0.04f;
        private static float LarguraUtilDaEstacao => TamanhoDoPainelDaEstacao.x - 2 * MargemDaEstacao;
        private static float TopoDaEstacao => TamanhoDoPainelDaEstacao.y / 2 - MargemDaEstacao;

        /// <summary>
        /// Painel de uma estação, perto (1,25 m), à direita da caixa e virado para o aluno, escondido até a estação
        /// começar: fundo, título pequeno no canto e, logo abaixo, a instrução em destaque (grande, negrito, azul).
        /// As Estações 1 e 2 usam o mesmo lugar (só uma fica à vista).
        /// </summary>
        private static Transform PainelDaEstacao(string nome, DadosDoLaboratorio dados, TMP_FontAsset fonte, string chaveDoTitulo,
            out Transform fundo, out TextMeshPro instrucao, float alturaDaInstrucao)
        {
            Vector2 tamanho = TamanhoDoPainelDaEstacao;
            var painel = new GameObject(nome).transform;
            painel.SetPositionAndRotation(NaFrenteDoAluno(DistanciaDaEstacao, DirecaoDaEstacao, 1.3f),
                Quaternion.Euler(0f, DirecaoDaEstacao, 0f));
            Bloco(painel, "Fundo", new Vector3(0f, 0f, 0.01f), new Vector3(tamanho.x, tamanho.y, 0.01f),
                MaterialOpaco("FundoDoPainel", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel));
            fundo = painel.Find("Fundo");

            float largura = LarguraUtilDaEstacao, topo = TopoDaEstacao;
            TextMeshPro tituloDaEstacao = TextoDaEstacao(painel, fonte, "TituloDaEstacao", topo, 0.05f, 0f, largura, 0.28f,
                PaletaDoLaboratorio.Texto, TextAlignmentOptions.TopLeft, 0.2f);
            var doCatalogo = tituloDaEstacao.gameObject.AddComponent<TextoDoCatalogo>();
            doCatalogo.Configurar(dados, tituloDaEstacao, chaveDoTitulo);
            doCatalogo.Atualizar();
            instrucao = TextoDaEstacao(painel, fonte, "Instrucao", topo - 0.055f, alturaDaInstrucao, 0f, largura, 0.6f,
                PaletaDoLaboratorio.DestaqueAzul, TextAlignmentOptions.TopLeft, 0.3f);
            instrucao.fontStyle = FontStyles.Bold;
            return painel;
        }

        private static TextMeshPro TextoDaEstacao(Transform painel, TMP_FontAsset fonte, string nome, float y, float altura,
            float x, float larguraDoTexto, float letra, Color cor, TextAlignmentOptions alinhamento, float letraMinima)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            objeto.transform.SetParent(painel, false);
            objeto.transform.localPosition = new Vector3(x, y - altura / 2, 0f);
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.font = fonte;
            texto.fontSize = letra;
            texto.color = cor;
            texto.alignment = alinhamento;
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.richText = true;
            texto.enableAutoSizing = true;   // reduz a letra só se o texto não couber (nunca corta)
            texto.fontSizeMin = letraMinima;
            texto.fontSizeMax = letra;
            texto.rectTransform.sizeDelta = new Vector2(larguraDoTexto, altura);
            texto.text = string.Empty;
            return texto;
        }

        /// <summary>Botões da pergunta, 2 × 2, escondidos.</summary>
        private static BotaoDoPainel[] OpcoesDaEstacao(Transform painel, DadosDoLaboratorio dados, TMP_FontAsset fonte)
        {
            float largura = LarguraUtilDaEstacao;
            var opcoes = new BotaoDoPainel[4];
            for (int i = 0; i < opcoes.Length; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * largura / 4;
                float y = i < 2 ? -0.21f : -0.335f;
                opcoes[i] = CriarBotao(painel, $"Opcao{i + 1}", new Vector3(x, y, 0f), TamanhoDoBotaoDaEstacao, dados, fonte,
                    null, principal: false);
                opcoes[i].gameObject.SetActive(false);
            }
            return opcoes;
        }

        private static Vector2 TamanhoDoBotaoDaEstacao => new Vector2(LarguraUtilDaEstacao / 2 - 0.02f, 0.11f);

        /// <summary>
        /// Estação 1: no painel da estação (o mesmo lugar da Estação 2), de cima para baixo: título pequeno, instrução
        /// em destaque (o enunciado, a dica ou o acerto), objetivo e número do desafio, vértices tocados em letra
        /// grande e os botões da pergunta (2 × 2) ou do fechamento. Escondido até "Começar".
        /// </summary>
        private static EstacaoDosElementosNaCena CriarEstacaoDosElementos(DadosDoLaboratorio dados, TMP_FontAsset fonte,
            CaixaPegavel caixa, BotaoDoPainel comecar, EstacaoDasDiagonaisNaCena proxima)
        {
            Transform painel = PainelDaEstacao("PainelEstacaoDosElementos", dados, fonte, ChavesDeTexto.ElementosTitulo,
                out Transform fundo, out TextMeshPro instrucao, 0.17f);
            float largura = LarguraUtilDaEstacao, topo = TopoDaEstacao;
            TextMeshPro texto = TextoDaEstacao(painel, fonte, "TextoDaEstacao", topo - 0.235f, 0.1f, 0f, largura, 0.3f,
                PaletaDoLaboratorio.Texto, TextAlignmentOptions.TopLeft, 0.22f);
            TextMeshPro tocados = TextoDaEstacao(painel, fonte, "Tocados", topo - 0.345f, 0.11f, 0f, largura, 0.6f,
                PaletaDoLaboratorio.DestaqueAzul, TextAlignmentOptions.TopLeft, 0.4f);
            tocados.fontStyle = FontStyles.Bold;
            tocados.textWrappingMode = TextWrappingModes.NoWrap;

            BotaoDoPainel[] opcoes = OpcoesDaEstacao(painel, dados, fonte);
            BotaoDoPainel irParaAProxima = CriarBotao(painel, "BotaoIrParaAEstacao2", new Vector3(-largura / 4, -0.27f, 0f),
                TamanhoDoBotaoDaEstacao, dados, fonte, ChavesDeTexto.ElementosProximaEstacao, principal: true);
            BotaoDoPainel refazer = CriarBotao(painel, "BotaoRefazer", new Vector3(largura / 4, -0.27f, 0f),
                TamanhoDoBotaoDaEstacao, dados, fonte, ChavesDeTexto.ElementosRefazer, principal: false);
            irParaAProxima.gameObject.SetActive(false);
            refazer.gameObject.SetActive(false);
            painel.gameObject.SetActive(false);

            var estacao = new GameObject("EstacaoDosElementos").AddComponent<EstacaoDosElementosNaCena>();
            estacao.Configurar(dados, caixa.GetComponent<VisualDaCaixa>(), caixa, caixa.GetComponent<DesenhoDaFace>(), proxima,
                comecar, painel.gameObject, instrucao, texto, tocados, opcoes, irParaAProxima, refazer,
                AssetDatabase.LoadAssetAtPath<Material>($"{PastaDosMateriais}/Vertice.mat"),
                MaterialOpaco("VerticeApontado", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul),
                MaterialOpaco("VerticeErrado", ShaderUnlit, PaletaDoLaboratorio.DestaqueLaranja));
            estacao.ConfigurarFundo(fundo, TamanhoDoPainelDaEstacao.y / 2, baseSoTexto: topo - 0.465f - MargemDaEstacao,
                baseComBotoes: -TamanhoDoPainelDaEstacao.y / 2);
            return estacao;
        }

        /// <summary>
        /// Estação 2: painel à direita da caixa, perto e virado para o aluno. De cima para baixo: título pequeno,
        /// instrução em destaque, objetivo e medidas, contas de d e de D em duas colunas grandes e os botões da
        /// pergunta (2 × 2) ou do fechamento. Escondido até "Começar".
        /// </summary>
        private static EstacaoDasDiagonaisNaCena CriarEstacaoDasDiagonais(DadosDoLaboratorio dados, TMP_FontAsset fonte,
            CaixaPegavel caixa, BotaoDoPainel comecar)
        {
            Transform painel = PainelDaEstacao("PainelEstacao", dados, fonte, ChavesDeTexto.DiagonaisTitulo, out Transform fundo,
                out TextMeshPro instrucao, 0.15f);
            Vector2 tamanho = TamanhoDoPainelDaEstacao;
            float largura = LarguraUtilDaEstacao, topo = TopoDaEstacao;
            const float margem = MargemDaEstacao;
            TextMeshPro TextoDaEstacao(string nome, float y, float altura, float x, float larguraDoTexto, float letra,
                Color cor, TextAlignmentOptions alinhamento, float letraMinima) =>
                GeradorDaCena.TextoDaEstacao(painel, fonte, nome, y, altura, x, larguraDoTexto, letra, cor, alinhamento, letraMinima);

            TextMeshPro texto = TextoDaEstacao("TextoDaEstacao", topo - 0.21f, 0.13f, 0f, largura, 0.3f,
                PaletaDoLaboratorio.Texto, TextAlignmentOptions.TopLeft, 0.22f);
            // As contas, o conteúdo principal: d à esquerda (azul), D à direita (laranja), letra grande.
            float coluna = largura / 2 - 0.01f;
            TextMeshPro contasDaBase = TextoDaEstacao("ContasDaBase", topo - 0.345f, 0.19f, -largura / 4, coluna, 0.46f,
                PaletaDoLaboratorio.DestaqueAzul, TextAlignmentOptions.TopLeft, 0.32f);
            TextMeshPro contasDaDiagonal = TextoDaEstacao("ContasDaDiagonal", topo - 0.345f, 0.19f, largura / 4, coluna, 0.46f,
                PaletaDoLaboratorio.DestaqueLaranja, TextAlignmentOptions.TopLeft, 0.32f);
            // As duas colunas com a mesma letra (sem auto-ajuste): a conta mais longa só passa um pouco da coluna.
            foreach (TextMeshPro conta in new[] { contasDaBase, contasDaDiagonal })
            {
                conta.enableAutoSizing = false;
                conta.fontSize = 0.46f;
                conta.textWrappingMode = TextWrappingModes.NoWrap;
                conta.overflowMode = TextOverflowModes.Overflow;
            }

            BotaoDoPainel[] opcoes = OpcoesDaEstacao(painel, dados, fonte);
            Vector2 tamanhoDaOpcao = TamanhoDoBotaoDaEstacao;

            BotaoDoPainel proximo = CriarBotao(painel, "BotaoProximoDesafio", new Vector3(-largura / 4, -0.27f, 0f),
                tamanhoDaOpcao, dados, fonte, ChavesDeTexto.DiagonaisProximoDesafio, principal: true);
            BotaoDoPainel refazer = CriarBotao(painel, "BotaoRefazer", new Vector3(largura / 4, -0.27f, 0f),
                tamanhoDaOpcao, dados, fonte, ChavesDeTexto.DiagonaisRefazer, principal: false);
            proximo.gameObject.SetActive(false);
            refazer.gameObject.SetActive(false);
            painel.gameObject.SetActive(false);
            float topoDoFundo = tamanho.y / 2;

            var estacao = new GameObject("EstacaoDasDiagonais").AddComponent<EstacaoDasDiagonaisNaCena>();
            estacao.Configurar(dados, caixa.GetComponent<VisualDaCaixa>(), caixa, caixa.GetComponent<DesenhoDasDiagonais>(),
                comecar, painel.gameObject, instrucao, texto, contasDaBase, contasDaDiagonal, opcoes, proximo, refazer,
                AssetDatabase.LoadAssetAtPath<Material>($"{PastaDosMateriais}/Vertice.mat"),
                MaterialOpaco("VerticeApontado", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul),
                MaterialOpaco("VerticeErrado", ShaderUnlit, PaletaDoLaboratorio.DestaqueLaranja));
            estacao.ConfigurarFundo(fundo, topoDoFundo, baseSoTexto: topo - 0.34f - margem,
                baseComContas: topo - 0.535f - margem, baseComBotoes: -tamanho.y / 2);
            return estacao;
        }

        /// <summary>
        /// Botão de painel: fundo com colisor (o raio acerta) e texto na frente. Com <paramref name="chave"/>, o texto
        /// vem do textos.json; sem ela, quem usa o botão escreve. O principal é laranja, com texto branco.
        /// </summary>
        private static BotaoDoPainel CriarBotao(Transform painel, string nome, Vector3 posicao, Vector2 tamanho,
            DadosDoLaboratorio dados, TMP_FontAsset fonte, string chave, bool principal)
        {
            var botao = new GameObject(nome).transform;
            botao.SetParent(painel, false);
            botao.localPosition = posicao;

            Material normal = principal
                ? MaterialOpaco("BotaoPrincipal", ShaderUnlit, PaletaDoLaboratorio.BotaoPrincipal)
                : MaterialOpaco("BotaoNormal", ShaderUnlit, PaletaDoLaboratorio.Parede);
            var fundo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fundo.name = "Fundo";
            fundo.transform.SetParent(botao, false);
            fundo.transform.localPosition = new Vector3(0f, 0f, -0.004f);
            fundo.transform.localScale = new Vector3(tamanho.x, tamanho.y, 0.012f);
            var renderizador = fundo.GetComponent<MeshRenderer>();
            renderizador.sharedMaterial = normal;
            renderizador.shadowCastingMode = ShadowCastingMode.Off;
            renderizador.receiveShadows = false;
            renderizador.lightProbeUsage = LightProbeUsage.Off;
            renderizador.reflectionProbeUsage = ReflectionProbeUsage.Off;

            var objeto = new GameObject("Texto", typeof(RectTransform));
            objeto.transform.SetParent(botao, false);
            objeto.transform.localPosition = new Vector3(0f, 0f, -0.012f);
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.font = fonte;
            texto.fontSize = 0.5f;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            texto.enableAutoSizing = true;   // reduz só se não couber no botão
            texto.fontSizeMin = 0.3f;
            texto.fontSizeMax = 0.5f;
            texto.rectTransform.sizeDelta = tamanho - new Vector2(0.04f, 0.02f);
            texto.text = chave ?? string.Empty;
            if (chave != null)
            {
                var doCatalogo = objeto.AddComponent<TextoDoCatalogo>();
                doCatalogo.Configurar(dados, texto, chave);
                doCatalogo.Atualizar();
            }

            var componente = botao.gameObject.AddComponent<BotaoDoPainel>();
            componente.Configurar(renderizador, texto, normal,
                MaterialOpaco("BotaoApontado", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul),
                MaterialOpaco("BotaoCerto", ShaderUnlit, PaletaDoLaboratorio.Teleporte),
                MaterialOpaco("BotaoErrado", ShaderUnlit, PaletaDoLaboratorio.DestaqueLaranja),
                principal ? Color.white : PaletaDoLaboratorio.Texto);
            return componente;
        }

        /// <summary>
        /// Experiência estacionária: raios dos dois controles (com um modelo simples e uma dica acima de cada um) e
        /// recentralização com a esfera escura presa à cabeça. Sem teleporte nem giro por passos: o analógico não faz
        /// nada (o código da locomoção continua no projeto, fora da cena).
        /// </summary>
        private static RecentralizacaoDoAluno CriarInteracao(GameObject rig, DadosDoLaboratorio dados, TMP_FontAsset fonte)
        {
            Material livre = MaterialOpaco("Raio", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel);
            Material noAlvo = MaterialOpaco("RaioNoAlvo", ShaderUnlit, PaletaDoLaboratorio.DestaqueAzul);
            Material controle = MaterialOpaco("Controle", ShaderUnlit, PaletaDoLaboratorio.PeDaMesa);
            Material escuro = MaterialDoEscurecimento();

            RaioDoControle esquerdo = CriarRaio(rig.transform, "TrackingSpace/LeftHandAnchor/LeftControllerAnchor",
                OVRInput.Controller.LTouch, livre, noAlvo, controle);
            RaioDoControle direito = CriarRaio(rig.transform, "TrackingSpace/RightHandAnchor/RightControllerAnchor",
                OVRInput.Controller.RTouch, livre, noAlvo, controle);

            Transform cabeca = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
            Material fundoDaDica = MaterialOpaco("FundoDoPainel", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel);
            esquerdo.ConfigurarDica(CriarDica(esquerdo.transform.parent, cabeca, dados, fonte, fundoDaDica));
            direito.ConfigurarDica(CriarDica(direito.transform.parent, cabeca, dados, fonte, fundoDaDica));

            Transform esfera = Primitiva(PrimitiveType.Sphere, cabeca, "TelaEscura", Vector3.zero,
                Vector3.one * 0.3f, escuro);
            esfera.GetComponent<MeshRenderer>().enabled = false;
            var telaEscura = esfera.gameObject.AddComponent<TelaEscura>();
            telaEscura.Configurar(esfera.GetComponent<MeshRenderer>());

            var recentralizacao = rig.AddComponent<RecentralizacaoDoAluno>();
            Definir(recentralizacao, ("_rig", rig.transform), ("_cabeca", cabeca), ("_telaEscura", telaEscura));
            return recentralizacao;
        }

        /// <summary>
        /// Diagnóstico para o professor, num painel próprio à direita do aluno, longe dos créditos: versão,
        /// recentralização (quando rodou), controles e erros. Escondido; os dois analógicos afundados por 3 s mostram.
        /// </summary>
        private static void CriarPainelDeDiagnostico(DadosDoLaboratorio dados, TMP_FontAsset fonte,
            RecentralizacaoDoAluno recentralizacao, GameObject rig, EstacaoDasDiagonaisNaCena estacao,
            EstacaoDosElementosNaCena elementos)
        {
            var tamanho = new Vector2(1.3f, 1.05f);
            var painel = new GameObject("PainelDiagnostico").transform;
            painel.position = new Vector3(1.75f, 1.4f, 0.5f);
            Vector3 paraFrente = painel.position - new Vector3(0f, painel.position.y, 0f);
            painel.rotation = Quaternion.LookRotation(paraFrente.normalized, Vector3.up);
            var conteudo = new GameObject("Conteudo").transform;
            conteudo.SetParent(painel, false);
            Bloco(conteudo, "Fundo", new Vector3(0f, 0f, 0.01f), new Vector3(tamanho.x, tamanho.y, 0.01f),
                MaterialOpaco("FundoDoPainel", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel));

            var objeto = new GameObject("Diagnostico", typeof(RectTransform));
            objeto.transform.SetParent(conteudo, false);
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.font = fonte;
            texto.fontSize = 0.3f;
            texto.color = PaletaDoLaboratorio.Texto;
            texto.alignment = TextAlignmentOptions.TopLeft;
            texto.richText = false;
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.overflowMode = TextOverflowModes.Truncate;
            texto.rectTransform.sizeDelta = tamanho - new Vector2(0.1f, 0.1f);
            texto.text = "Diagnóstico";
            var diagnostico = painel.gameObject.AddComponent<PainelDeDiagnostico>();
            diagnostico.Configurar(texto, dados, recentralizacao, conteudo.gameObject,
                rig.transform.Find("TrackingSpace/CenterEyeAnchor"), estacao, elementos);
            conteudo.gameObject.SetActive(false);   // só o atalho do professor mostra
            rig.AddComponent<AtalhoDoDiagnostico>().Configurar(diagnostico);
        }

        /// <summary>
        /// Dica acima de um controle ("Segure o botão lateral para pegar", "Solte para ir até lá", "Aqui não dá"):
        /// texto curto sobre um fundo claro, desligada até haver o que dizer.
        /// </summary>
        private static DicaDaMao CriarDica(Transform ancora, Transform cabeca, DadosDoLaboratorio dados, TMP_FontAsset fonte,
            Material fundo)
        {
            var dica = new GameObject("Dica").transform;
            dica.SetParent(ancora, false);
            var conteudo = new GameObject("Conteudo").transform;
            conteudo.SetParent(dica, false);

            var objeto = new GameObject("Texto", typeof(RectTransform));
            objeto.transform.SetParent(conteudo, false);
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.font = fonte;
            texto.fontSize = TamanhoDaDica;
            texto.color = PaletaDoLaboratorio.Texto;
            texto.alignment = TextAlignmentOptions.Center;
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
            texto.text = string.Empty;

            // Atrás do texto (a frente da dica aponta para longe do aluno).
            Transform placa = Primitiva(PrimitiveType.Cube, conteudo, "Fundo", new Vector3(0f, 0f, 0.003f),
                new Vector3(0.26f, 0.06f, 0.002f), fundo);
            conteudo.gameObject.SetActive(false);

            var componente = dica.gameObject.AddComponent<DicaDaMao>();
            componente.Configurar(dados, ancora, cabeca, conteudo.gameObject, texto, placa);
            return componente;
        }

        /// <summary>
        /// Painel "Como usar os controles", à esquerda da mesa e virado para o aluno: um título e uma linha por
        /// botão, só com as ações que funcionam nesta cena (<see cref="AcoesDisponiveis"/>).
        /// </summary>
        private static void CriarPainelDeInstrucoes(DadosDoLaboratorio dados, TMP_FontAsset fonte)
        {
            // Estreito e perto do centro: inteiro na vista inicial, sem cobrir o título nem a caixa.
            var tamanho = new Vector2(0.7f, 0.6f);
            var painel = new GameObject("PainelInstrucoes").transform;
            painel.SetPositionAndRotation(NaFrenteDoAluno(DistanciaDosControles, DirecaoDosControles, 1.35f),
                Quaternion.Euler(0f, DirecaoDosControles, 0f));   // virado para o aluno
            Bloco(painel, "Fundo", new Vector3(0f, 0f, 0.01f), new Vector3(tamanho.x, tamanho.y, 0.01f),
                MaterialOpaco("FundoDoPainel", ShaderUnlit, PaletaDoLaboratorio.FundoDoPainel));

            float margem = 0.06f;
            TextMeshPro titulo = TextoDoPainel(painel, ChavesDeTexto.ControlesTitulo, fonte, TamanhoDoCorpo,
                new Vector3(0f, tamanho.y / 2 - margem - 0.04f, 0f), new Vector2(tamanho.x - 2 * margem, 0.09f),
                TextAlignmentOptions.Center);
            titulo.fontStyle = FontStyles.Bold;
            titulo.GetComponent<TextoDoCatalogo>().Configurar(dados, titulo, ChavesDeTexto.ControlesTitulo);
            titulo.GetComponent<TextoDoCatalogo>().Atualizar();

            float alturaDoCorpo = tamanho.y - 2 * margem - 0.12f;
            TextMeshPro corpo = TextoDoPainel(painel, "Controles", fonte, TamanhoDoCorpo * 0.7f,
                new Vector3(0f, -tamanho.y / 2 + margem + alturaDoCorpo / 2, 0f),
                new Vector2(tamanho.x - 2 * margem, alturaDoCorpo), TextAlignmentOptions.TopLeft);
            corpo.paragraphSpacing = 12f;
            corpo.enableAutoSizing = true;   // nunca corta: reduz a letra se o texto do JSON crescer
            corpo.fontSizeMin = TamanhoDoCorpo * 0.5f;
            corpo.fontSizeMax = TamanhoDoCorpo * 0.7f;
            var controles = corpo.GetComponent<TextoDoCatalogo>();
            controles.Configurar(dados, corpo, TextosDosControles.Chaves(AcoesDisponiveis()));
            controles.Atualizar();
        }

        /// <summary>Ponto a <paramref name="distancia"/> m da origem, na direção dada (graus, + à direita), na altura y.</summary>
        private static Vector3 NaFrenteDoAluno(float distancia, float direcao, float y)
        {
            float rad = direcao * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad) * distancia, y, Mathf.Cos(rad) * distancia);
        }

        private static TextMeshPro TextoDoPainel(Transform painel, string nome, TMP_FontAsset fonte, float tamanhoDaLetra,
            Vector3 posicao, Vector2 tamanho, TextAlignmentOptions alinhamento)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            objeto.transform.SetParent(painel, false);
            objeto.transform.localPosition = posicao;
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.font = fonte;
            texto.fontSize = tamanhoDaLetra;
            texto.color = PaletaDoLaboratorio.Texto;
            texto.alignment = alinhamento;
            texto.textWrappingMode = TextWrappingModes.Normal;
            texto.rectTransform.sizeDelta = tamanho;
            objeto.AddComponent<TextoDoCatalogo>();
            return texto;
        }

        /// <summary>Versão do app (Application.version) logo abaixo dos créditos: mostra qual build está no óculos.</summary>
        private static void CriarVersao(Transform painel, DadosDoLaboratorio dados, TMP_FontAsset fonte, Vector3 posicao, float largura)
        {
            var objeto = new GameObject(ChavesDeTexto.AppVersao, typeof(RectTransform));
            objeto.transform.SetParent(painel, false);
            objeto.transform.localPosition = posicao;
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.font = fonte;
            texto.fontSize = 0.3f;
            texto.color = PaletaDoLaboratorio.Texto;
            texto.alignment = TextAlignmentOptions.Center;
            texto.rectTransform.sizeDelta = new Vector2(largura, 0.05f);
            var versao = objeto.AddComponent<TextoDaVersao>();
            versao.Configurar(dados, texto, ChavesDeTexto.AppVersao);
            texto.text = $"Versão {PlayerSettings.bundleVersion}";
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

        /// <summary>Painel preso ao mundo, virado para o aluno, com uma linha de texto por chave.</summary>
        private static Transform CriarPainel(string nome, Vector3 posicao, Vector2 tamanho, DadosDoLaboratorio dados,
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
                var objeto = new GameObject(chave, typeof(RectTransform));
                objeto.transform.SetParent(painel, false);
                objeto.transform.localPosition = new Vector3(0f, y, 0f);
                var texto = objeto.AddComponent<TextMeshPro>();
                texto.font = fonte;
                texto.fontSize = tamanhoDaLetra;
                texto.color = PaletaDoLaboratorio.Texto;
                texto.alignment = TextAlignmentOptions.Center;
                texto.textWrappingMode = TextWrappingModes.Normal;
                texto.rectTransform.sizeDelta = new Vector2(tamanho.x - 2 * margem, tamanhoDaLetra * 0.15f);
                texto.text = chave;

                var textoDoCatalogo = objeto.AddComponent<TextoDoCatalogo>();
                textoDoCatalogo.Configurar(dados, texto, chave);
                textoDoCatalogo.Atualizar();
            }

            return painel;
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
        /// Esfera preta vista por dentro (culling da frente), na fila Transparent (o URP a recalcula pelo tipo de
        /// superfície ao importar). Centrada na câmera, é a última dos transparentes e cobre tudo o que está a mais de
        /// 15 cm. A opacidade vem da TelaEscura (giro e recentralização), por MaterialPropertyBlock.
        /// </summary>
        private static Material MaterialDoEscurecimento()
        {
            Material material = MaterialTranslucido("EscurecimentoDoGiro", new Color(0f, 0f, 0f, 0f));
            material.SetFloat("_Cull", (float)CullMode.Front);
            // _QueueOffset é do URP: a fila fica em 3050 (o URP recalcula a fila a partir dele, então ela não volta a 3000).
            material.SetFloat("_QueueOffset", DeslocamentoDaTelaEscura);
            material.renderQueue = (int)RenderQueue.Transparent + DeslocamentoDaTelaEscura;
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
