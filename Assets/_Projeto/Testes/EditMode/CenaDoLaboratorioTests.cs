using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using LaboratorioParalelepipedo.XR;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>Confere a cena gerada (Assets/_Projeto/Cenas/Laboratorio.unity) contra as regras do projeto.</summary>
    public class CenaDoLaboratorioTests
    {
        private const string CaminhoDaCena = "Assets/_Projeto/Cenas/Laboratorio.unity";
        private Scene _cena;

        [OneTimeSetUp]
        public void Abrir()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(CaminhoDaCena), Is.Not.Null,
                "Cena não encontrada. Gere com Laboratório > 5. Gerar cena do laboratório.");
            _cena = EditorSceneManager.OpenScene(CaminhoDaCena, OpenSceneMode.Additive);
        }

        [OneTimeTearDown]
        public void Fechar()
        {
            if (_cena.IsValid()) EditorSceneManager.CloseScene(_cena, true);
        }

        private static void MesmaCor(Color obtida, Color esperada) =>
            Assert.That(ColorUtility.ToHtmlStringRGBA(obtida), Is.EqualTo(ColorUtility.ToHtmlStringRGBA(esperada)));

        private T[] Todos<T>() where T : Component =>
            _cena.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();

        [Test]
        public void TemNoMaximoUmaLuzEmTempoReal()
        {
            var luzes = Todos<Light>();

            Assert.That(luzes.Count(l => l.lightmapBakeType != LightmapBakeType.Baked), Is.LessThanOrEqualTo(1));
        }

        [Test]
        public void AUnicaCameraEhADoOVRCameraRig()
        {
            var cameras = Todos<Camera>().Where(c => c.enabled).ToArray();

            Assert.That(cameras.Length, Is.EqualTo(1));
            Assert.That(cameras[0].transform.root.name, Is.EqualTo("OVRCameraRig"));
        }

        [Test]
        public void NenhumPainelOuObjetoPresoACamera()
        {
            var rig = _cena.GetRootGameObjects().Single(g => g.name == "OVRCameraRig");

            Assert.That(rig.GetComponentsInChildren<TextoDoCatalogo>(true), Is.Empty, "painéis presos ao mundo, nunca à cabeça");
            Assert.That(rig.GetComponentsInChildren<VisualDaCaixa>(true), Is.Empty);
        }

        [Test]
        public void TextosUsamChavesQueExistem()
        {
            var textos = Todos<TextoDoCatalogo>();

            Assert.That(textos, Is.Not.Empty);
            foreach (var texto in textos)
            foreach (string chave in texto.Chaves)
                Assert.That(ChavesDeTexto.Todas, Does.Contain(chave), $"{texto.name}: chave \"{chave}\" não existe");
        }

        [Test]
        public void DadosLigadosAosArquivosDoApp()
        {
            var dados = Todos<DadosDoLaboratorio>().Single();

            Assert.That(dados.Catalogo.Quantidade, Is.EqualTo(ChavesDeTexto.Todas.Length));
            Assert.That(dados.Desafios, Is.Not.Empty);
        }

        [Test]
        public void CaixaTemMateriaisEFonte()
        {
            var caixa = Todos<VisualDaCaixa>().Single();
            var renderizadores = caixa.GetComponentsInChildren<MeshRenderer>(true)
                .Where(r => r.GetComponent<TMPro.TextMeshPro>() == null).ToArray();

            Assert.That(renderizadores, Is.Not.Empty);
            Assert.That(renderizadores.All(r => r.sharedMaterial != null), "material faltando na caixa");
            Assert.That(caixa.GetComponentsInChildren<TMPro.TextMeshPro>(true).All(t => t.font.name == "Nunito SDF"));
        }

        [Test]
        public void MateriaisSaoCompartilhadosEComInstancing()
        {
            var materiais = Todos<MeshRenderer>().Where(r => r.GetComponent<TMPro.TextMeshPro>() == null)
                .Select(r => r.sharedMaterial).Where(m => m != null).Distinct().ToArray();

            foreach (var material in materiais.Where(m => AssetDatabase.GetAssetPath(m).StartsWith("Assets/_Projeto")))
                Assert.That(material.enableInstancing, $"{material.name} sem GPU instancing");
            Assert.That(materiais.Where(m => !AssetDatabase.Contains(m)).Select(m => m.name), Is.Empty,
                "material criado na cena em vez de asset compartilhado");
        }

        [Test]
        public void FacesDaCaixaSaoTranslucidasEDosDoisLados()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Projeto/Materiais/FaceDaCaixa.mat");

            Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(PaletaDoLaboratorio.Face));
            Assert.That(material.GetFloat("_Surface"), Is.EqualTo(1f), "transparente");
            Assert.That(material.GetFloat("_Cull"), Is.EqualTo(0f), "sem culling: vista por dentro também");
        }

        [Test]
        public void ModoEditor_FicaForaDoBuild_ELigadoAoRig()
        {
            var modo = _cena.GetRootGameObjects().Single(g => g.name == "ModoEditor");

            Assert.That(modo.tag, Is.EqualTo("EditorOnly"), "objetos EditorOnly não vão para o APK");
            var serializado = new SerializedObject(modo.GetComponent("ModoEditor"));
            Assert.That(((Transform)serializado.FindProperty("_espacoDeRastreamento").objectReferenceValue).name,
                Is.EqualTo("TrackingSpace"));
            Assert.That(serializado.FindProperty("_alturaDosOlhos").floatValue, Is.EqualTo(1.4f));
        }

        [Test]
        public void PainelDeControles_SoMostraAsAcoesQueFuncionam()
        {
            var painel = Todos<TextoDoCatalogo>().Single(t => t.name == "Controles");
            var componentes = Todos<MonoBehaviour>();
            var acoes = AcoesDoControle.Nenhuma;
            if (componentes.Any(c => c is IAlvoDoRaio)) acoes |= AcoesDoControle.Apontar;
            if (componentes.Any(c => c is IPegavel)) acoes |= AcoesDoControle.Pegar;
            if (componentes.Any(c => c is LocomocaoDoAluno)) acoes |= AcoesDoControle.Teleporte | AcoesDoControle.Giro;

            Assert.That(painel.Chaves, Is.EqualTo(TextosDosControles.Chaves(acoes)));
            Assert.That(painel.Chaves, Is.EqualTo(new[] { ChavesDeTexto.ControlesApontar, ChavesDeTexto.ControlesPegar }),
                "estacionária: sem teleporte nem giro; gatilho para os vértices e botões, lateral para a caixa");
        }

        [Test]
        public void RaioNasDuasMaos_PresoAosControles()
        {
            var raios = Todos<RaioDoControle>();

            Assert.That(raios.Select(r => r.Controle), Is.EquivalentTo(new[] { OVRInput.Controller.LTouch, OVRInput.Controller.RTouch }));
            foreach (var raio in raios)
            {
                Assert.That(raio.transform.parent.name, Does.EndWith("ControllerAnchor"));
                var serializado = new SerializedObject(raio);
                foreach (string campo in new[] { "_linha", "_materialLivre", "_materialNoAlvo", "_modelo" })
                    Assert.That(serializado.FindProperty(campo).objectReferenceValue, Is.Not.Null, $"{raio.name}.{campo}");
                MesmaCor(((Material)serializado.FindProperty("_materialNoAlvo").objectReferenceValue).GetColor("_BaseColor"),
                    PaletaDoLaboratorio.DestaqueAzul);
            }
        }

        [Test]
        public void Estacionaria_SemTeleporteNemGiro()
        {
            Assert.That(Todos<LocomocaoDoAluno>(), Is.Empty, "o analógico não faz nada");
            Assert.That(Todos<Transform>().Where(t => t.name == "ArcoDoTeleporte" || t.name == "MarcadorDoTeleporte"), Is.Empty,
                "nem o arco nem a circunferência do destino do teleporte");
        }

        [Test]
        public void PegarACaixa_NuncaMoveOAlunoNemAMesa()
        {
            var caixa = Todos<CaixaPegavel>().Single();
            Transform rig = Todos<RecentralizacaoDoAluno>().Single().transform;
            Transform mesa = _cena.GetRootGameObjects().Single(g => g.name == "Mesa").transform;
            Vector3 rigAntes = rig.position, mesaAntes = mesa.position, caixaAntes = caixa.transform.position;
            Quaternion giroDoRigAntes = rig.rotation, giroDaMesaAntes = mesa.rotation;
            var mao = new GameObject("MaoDeTeste").transform;
            try
            {
                mao.SetPositionAndRotation(caixaAntes + new Vector3(0.1f, 0.2f, -0.3f), Quaternion.identity);
                caixa.GuardarPosicaoNaMesa();   // o Awake faz isto ao dar Play
                caixa.Pegar(mao);
                for (int i = 0; i < 60; i++)
                {
                    mao.SetPositionAndRotation(mao.position + new Vector3(0.01f, 0.005f, -0.01f), Quaternion.Euler(0f, i * 3f, i * 2f));
                    caixa.Avancar(1f / 72f);
                }
                Assert.That(Vector3.Distance(caixa.transform.position, caixaAntes), Is.GreaterThan(0.1f), "a caixa foi com a mão");
                caixa.Soltar(mao);
                for (int i = 0; i < 72; i++) caixa.Avancar(1f / 72f);

                Assert.That(rig.position, Is.EqualTo(rigAntes), "o aluno não se move");
                Assert.That(rig.rotation, Is.EqualTo(giroDoRigAntes));
                Assert.That(mesa.position, Is.EqualTo(mesaAntes), "a mesa não se move");
                Assert.That(mesa.rotation, Is.EqualTo(giroDaMesaAntes));
                Assert.That(Vector3.Distance(caixa.transform.position, caixaAntes), Is.LessThan(1e-4f), "a caixa voltou à mesa");
            }
            finally
            {
                Object.DestroyImmediate(mao.gameObject);
            }
        }

        [Test]
        public void TelaEscura_PresaACabeca_DesligadaForaDoGiroEDaRecentralizacao()
        {
            var tela = Todos<TelaEscura>().Single();
            var esfera = (Renderer)new SerializedObject(tela).FindProperty("_esfera").objectReferenceValue;

            Assert.That(esfera.transform.parent.name, Is.EqualTo("CenterEyeAnchor"));
            Assert.That(esfera.enabled, Is.False, "sem vinheta parado nem no teleporte");
            Assert.That(esfera.sharedMaterial.GetFloat("_Cull"), Is.EqualTo((float)UnityEngine.Rendering.CullMode.Front), "vista por dentro");
            Assert.That(esfera.GetComponent<Collider>(), Is.Null);
        }

        [Test]
        public void Caixa_PegavelComColisorECorpoCinematico()
        {
            var caixa = Todos<CaixaPegavel>().Single();
            var colisor = caixa.GetComponent<BoxCollider>();

            // A caixa 4 × 3 × 2 da Estação 1, a primeira que o aluno vê (x = a, y = c, z = b; 1 unidade = 10 cm).
            Assert.That(Vector3.Distance(colisor.size, new Vector3(0.4f, 0.2f, 0.3f)), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(colisor.center, new Vector3(0f, 0.1f, 0f)), Is.LessThan(1e-4f));
            Assert.That(caixa.GetComponent<Rigidbody>().isKinematic);
            Assert.That(new SerializedObject(caixa).FindProperty("_materialDaArestaDestacada").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void SoACaixaEOsBotoesTemColisor()
        {
            // O raio só para no que é interativo; sala, mesa e fundo dos painéis não bloqueiam. Os vértices ganham
            // colisor ao montar cada caixa, na hora de rodar.
            var colisores = Todos<Collider>();

            Assert.That(colisores.Count(c => c.GetComponent<CaixaPegavel>() != null), Is.EqualTo(1));
            foreach (var colisor in colisores.Where(c => c.GetComponent<CaixaPegavel>() == null))
                Assert.That(colisor.GetComponentInParent<BotaoDoPainel>(true), Is.Not.Null, colisor.name);
            Assert.That(Todos<BotaoDoPainel>().Select(b => b.name), Is.EquivalentTo(new[]
            {
                "BotaoComecar", "Opcao1", "Opcao2", "Opcao3", "Opcao4", "BotaoProximoDesafio", "BotaoRefazer",
                "BotaoIrParaAEstacao3", "Opcao1", "Opcao2", "Opcao3", "Opcao4", "BotaoIrParaAEstacao2", "BotaoRefazer",
                "Opcao1", "Opcao2", "Opcao3", "Opcao4", "BotaoAbrirOuFechar", "BotaoRefazer",
            }));
        }

        [Test]
        public void VersaoApareceNosCreditos()
        {
            var versao = Todos<TextoDaVersao>().Single();

            Assert.That(versao.Chave, Is.EqualTo(ChavesDeTexto.AppVersao));
            Assert.That(versao.transform.parent.name, Is.EqualTo("PainelTitulo"));
        }

        [Test]
        public void Diagnostico_NumPainelSeparado_LigadoARecentralizacao_ComAVersaoNosCreditos()
        {
            var painel = Todos<PainelDeDiagnostico>().Single();
            var serializado = new SerializedObject(painel);

            Assert.That(painel.transform.root.name, Is.EqualTo("PainelDiagnostico"), "painel próprio, preso ao mundo");
            Assert.That(serializado.FindProperty("_recentralizacao").objectReferenceValue,
                Is.EqualTo(Todos<RecentralizacaoDoAluno>().Single()));
            Assert.That(serializado.FindProperty("_dados").objectReferenceValue, Is.Not.Null);
            Assert.That(Todos<TextoDaVersao>().Single().transform.parent.name, Is.EqualTo("PainelTitulo"),
                "a versão continua nos créditos");
            Transform titulo = _cena.GetRootGameObjects().Single(g => g.name == "PainelTitulo").transform;
            Assert.That(Vector3.Distance(painel.transform.root.position, titulo.position), Is.GreaterThan(1.2f), "longe dos créditos");
        }

        [Test]
        public void Recentralizacao_NoRig_ComACabecaEATelaEscura()
        {
            var recentralizacao = Todos<RecentralizacaoDoAluno>().Single();
            var serializado = new SerializedObject(recentralizacao);

            Assert.That(recentralizacao.gameObject.name, Is.EqualTo("OVRCameraRig"));
            Assert.That(((Transform)serializado.FindProperty("_rig").objectReferenceValue).name, Is.EqualTo("OVRCameraRig"));
            Assert.That(((Transform)serializado.FindProperty("_cabeca").objectReferenceValue).name, Is.EqualTo("CenterEyeAnchor"));
            Assert.That(serializado.FindProperty("_telaEscura").objectReferenceValue, Is.EqualTo(Todos<TelaEscura>().Single()));
            Assert.That(serializado.FindProperty("_pontoDePartida").vector2Value, Is.EqualTo(Vector2.zero), "onde a cena foi desenhada");
        }

        [Test]
        public void DicaNasDuasMaos_DesligadaNoInicio()
        {
            var raios = Todos<RaioDoControle>();

            foreach (var raio in raios)
            {
                Assert.That(raio.Dica, Is.Not.Null, raio.name);
                Assert.That(raio.Dica.transform.parent, Is.EqualTo(raio.transform.parent), "na âncora do mesmo controle");
                var serializado = new SerializedObject(raio.Dica);
                foreach (string campo in new[] { "_dados", "_mao", "_cabeca", "_conteudo", "_texto", "_fundo" })
                    Assert.That(serializado.FindProperty(campo).objectReferenceValue, Is.Not.Null, $"{raio.name}.{campo}");
                Assert.That(((GameObject)serializado.FindProperty("_conteudo").objectReferenceValue).activeSelf, Is.False);
            }
        }

        [Test]
        public void PaineisDosLados_DeFrenteParaOAluno_SemCobrirOTitulo()
        {
            Transform instrucoes = _cena.GetRootGameObjects().Single(g => g.name == "PainelInstrucoes").transform;
            Transform estacao = _cena.GetRootGameObjects().Single(g => g.name == "PainelEstacao").transform;
            Transform titulo = _cena.GetRootGameObjects().Single(g => g.name == "PainelTitulo").transform;
            float metadeDoTitulo = titulo.Find("Fundo").localScale.x / 2;

            Assert.That(instrucoes.position.x, Is.LessThan(0f), "controles à esquerda");
            Assert.That(estacao.position.x, Is.GreaterThan(0f), "estação à direita");
            Assert.That(Todos<TextoDoCatalogo>().Single(t => t.name == "Controles").transform.parent, Is.EqualTo(instrucoes));
            foreach (Transform painel in new[] { instrucoes, estacao })
            {
                Vector3 paraOPainel = new Vector3(painel.position.x, 0f, painel.position.z).normalized;
                Assert.That(Vector3.Dot(painel.forward, paraOPainel), Is.GreaterThan(0.999f), $"{painel.name} virado para a origem");
                Assert.That(painel.position.z, Is.LessThan(titulo.position.z), painel.name);

                // Vista de cima a partir da origem: a borda de dentro do painel, projetada no plano do título, fica fora dele.
                float metade = painel.Find("Fundo").localScale.x / 2;
                Vector3 bordaDeDentro = painel.position - painel.right * metade * Mathf.Sign(painel.position.x);
                float xNoTitulo = bordaDeDentro.x * titulo.position.z / bordaDeDentro.z;
                Assert.That(Mathf.Abs(xNoTitulo), Is.GreaterThan(metadeDoTitulo), $"{painel.name} cobre o título");
            }
        }

        [Test]
        public void PainelDeControles_InteiroNaVistaInicial()
        {
            // Vista inicial: origem olhando para +z; o Quest 2 vê ~45° para cada lado.
            Transform painel = _cena.GetRootGameObjects().Single(g => g.name == "PainelInstrucoes").transform;
            float metade = painel.Find("Fundo").localScale.x / 2;
            foreach (float lado in new[] { -1f, 1f })
            {
                Vector3 borda = painel.position + painel.right * metade * lado;
                float angulo = Mathf.Atan2(Mathf.Abs(borda.x), borda.z) * Mathf.Rad2Deg;
                Assert.That(angulo, Is.LessThan(45f), $"borda a {angulo:0}°");
            }
        }

        [Test]
        public void Caixa_GiradaNaMesa_LongeDoPontoDePartida()
        {
            var caixa = Todos<CaixaPegavel>().Single();
            Bounds limites = caixa.GetComponent<BoxCollider>().bounds;
            var olhos = new Vector3(0f, 1.4f, 0f);

            Assert.That(caixa.transform.eulerAngles.y, Is.EqualTo(Editor.GeradorDaCena.GiroDaCaixaNaMesa).Within(1e-3f));
            Assert.That(caixa.transform.eulerAngles.x, Is.EqualTo(0f).Within(1e-3f), "só em torno do eixo vertical");
            Assert.That(Vector3.Distance(limites.ClosestPoint(olhos), olhos), Is.GreaterThan(0.7f),
                "o aluno nunca começa dentro ou perto demais da caixa");
            Assert.That(Vector3.Distance(limites.ClosestPoint(Vector3.zero), Vector3.zero), Is.GreaterThan(0.6f), "nem os pés");
        }

        [Test]
        public void Titulo_AcimaDosRotulosDoTopoDaCaixa()
        {
            Transform titulo = _cena.GetRootGameObjects().Single(g => g.name == "PainelTitulo").transform;
            float baseDoTitulo = titulo.position.y - titulo.Find("Fundo").localScale.y / 2;
            var rotulos = Todos<VisualDaCaixa>().Single().transform.Find(VisualDaCaixa.GrupoRotulos);

            foreach (float olhos in new[] { 1.3f, 1.5f, 1.7f })
                foreach (Transform rotulo in rotulos)
                {
                    // Topo do texto (meia letra acima do centro), visto dos olhos na origem, e a base do título.
                    Vector3 topo = rotulo.position + Vector3.up * 0.03f;
                    float anguloDoRotulo = Mathf.Atan2(topo.y - olhos, new Vector2(topo.x, topo.z).magnitude);
                    float anguloDoTitulo = Mathf.Atan2(baseDoTitulo - olhos, titulo.position.z);
                    Assert.That(anguloDoTitulo - anguloDoRotulo, Is.GreaterThan(1f * Mathf.Deg2Rad),
                        $"{rotulo.name} encosta no título com olhos a {olhos} m");
                }
        }

        [Test]
        public void TituloTemBotaoComecar_QueLevaAEstacao1_QueLevaAEstacao2()
        {
            var elementos = new SerializedObject(Todos<EstacaoDosElementosNaCena>().Single());
            var comecar = (BotaoDoPainel)elementos.FindProperty("_comecar").objectReferenceValue;
            Assert.That(comecar.name, Is.EqualTo("BotaoComecar"));
            Assert.That(comecar.transform.parent.name, Is.EqualTo("PainelTitulo"));
            Assert.That(((GameObject)elementos.FindProperty("_painelDaEstacao").objectReferenceValue).activeSelf, Is.False);
            Assert.That(elementos.FindProperty("_dados").objectReferenceValue, Is.Not.Null);
            Assert.That(elementos.FindProperty("_desenho").objectReferenceValue, Is.EqualTo(Todos<DesenhoDaFace>().Single()));
            Assert.That(elementos.FindProperty("_opcoes").arraySize, Is.EqualTo(4));
            Assert.That(elementos.FindProperty("_proximaEstacao").objectReferenceValue,
                Is.EqualTo(Todos<EstacaoDasDiagonaisNaCena>().Single()));

            var diagonais = new SerializedObject(Todos<EstacaoDasDiagonaisNaCena>().Single());
            Assert.That(diagonais.FindProperty("_comecar").objectReferenceValue, Is.Null, "a Estação 2 começa pela 1");
            Assert.That(((GameObject)diagonais.FindProperty("_painelDaEstacao").objectReferenceValue).activeSelf, Is.False);
            Assert.That(diagonais.FindProperty("_desenho").objectReferenceValue, Is.EqualTo(Todos<DesenhoDasDiagonais>().Single()));
            Assert.That(diagonais.FindProperty("_opcoes").arraySize, Is.EqualTo(4));

            var vibracao = new SerializedObject(Todos<VibracaoDoAcerto>().Single());
            Assert.That(vibracao.FindProperty("_estacao").objectReferenceValue, Is.Not.Null);
            Assert.That(vibracao.FindProperty("_estacaoDosElementos").objectReferenceValue, Is.Not.Null);
            Assert.That(Todos<VibracaoDoAcerto>().Single().gameObject.name, Is.EqualTo("OVRCameraRig"));
        }

        [Test]
        public void LaranjaCheioSoParaAvancar_RefazerEAbrirFecharSaoSecundarios()
        {
            // Laranja nas opções quer dizer "resposta errada": só os botões de avançar são laranja.
            Color Cor(BotaoDoPainel botao) => botao.transform.Find("Fundo").GetComponent<Renderer>().sharedMaterial.color;
            bool Igual(Color x, Color y) => Mathf.Abs(x.r - y.r) + Mathf.Abs(x.g - y.g) + Mathf.Abs(x.b - y.b) < 0.01f;
            foreach (BotaoDoPainel botao in Todos<BotaoDoPainel>())
            {
                bool avancar = botao.name == "BotaoComecar" || botao.name.StartsWith("BotaoIrParaAEstacao");
                bool secundario = botao.name == "BotaoRefazer" || botao.name == "BotaoAbrirOuFechar" ||
                    botao.name == "BotaoProximoDesafio";
                Assert.That(Igual(Cor(botao), PaletaDoLaboratorio.BotaoPrincipal), Is.EqualTo(avancar), botao.name);
                if (!secundario) continue;
                Assert.That(Igual(Cor(botao), Color.white), Is.True, botao.name);
                Transform contorno = botao.transform.Find("Contorno");
                Assert.That(contorno, Is.Not.Null, botao.name);
                Assert.That(Igual(contorno.GetComponent<Renderer>().sharedMaterial.color, PaletaDoLaboratorio.Texto), Is.True);
                Assert.That(contorno.GetComponent<Collider>(), Is.Null);
            }
        }

        [Test]
        public void PaineisDasTresEstacoes_NoMesmoLugar()
        {
            Transform um = Todos<Transform>().Single(t => t.name == "PainelEstacaoDosElementos");
            foreach (string nome in new[] { "PainelEstacao", "PainelEstacaoDasAreas" })
            {
                Transform outro = Todos<Transform>().Single(t => t.name == nome);
                Assert.That(Vector3.Distance(um.position, outro.position), Is.LessThan(1e-4f), nome);
                Assert.That(Quaternion.Angle(um.rotation, outro.rotation), Is.LessThan(0.01f), nome);
            }
        }

        [Test]
        public void DiagnosticoEscondido_AbertoPeloAtalhoDosAnalogicos()
        {
            var painel = Todos<PainelDeDiagnostico>().Single();
            var conteudo = (GameObject)new SerializedObject(painel).FindProperty("_conteudo").objectReferenceValue;

            Assert.That(conteudo.activeSelf, Is.False, "só o professor abre");
            Assert.That(Todos<AtalhoDoDiagnostico>().Single().gameObject.name, Is.EqualTo("OVRCameraRig"));
        }

        [Test]
        public void RotulosDaCaixa_AparecemAtravesDasFaces_ECobertosPelaTelaEscura()
        {
            var visual = Todos<VisualDaCaixa>().Single();
            Material rotulo = visual.MaterialDoRotulo;
            Material face = (Material)new SerializedObject(visual).FindProperty("_materialDaFace").objectReferenceValue;
            Material escura = ((Renderer)new SerializedObject(Todos<TelaEscura>().Single()).FindProperty("_esfera")
                .objectReferenceValue).sharedMaterial;

            Assert.That(rotulo, Is.Not.Null);
            Assert.That(rotulo.shader.name, Does.Contain("Overlay"));
            Assert.That(rotulo.renderQueue, Is.GreaterThan(face.renderQueue), "as faces não desbotam os rótulos");
            Assert.That(escura.renderQueue, Is.GreaterThan(rotulo.renderQueue), "na recentralização, tudo some");
            foreach (var texto in visual.GetComponentsInChildren<TMPro.TextMeshPro>(true))
                Assert.That(texto.fontSharedMaterial, Is.EqualTo(rotulo), texto.name);
        }

        [Test]
        public void CenaEstaNoBuild()
        {
            var cenas = EditorBuildSettings.scenes.Where(c => c.enabled).Select(c => c.path).ToArray();

            Assert.That(cenas, Is.EqualTo(new[] { CaminhoDaCena }));
        }
    }
}
