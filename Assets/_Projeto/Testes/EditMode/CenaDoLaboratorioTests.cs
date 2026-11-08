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
            Assert.That(painel.Chaves, Is.EqualTo(new[]
                { ChavesDeTexto.ControlesPegar, ChavesDeTexto.ControlesTeleporte, ChavesDeTexto.ControlesGiro }),
                "por enquanto não há alvo para o gatilho: o 'apontar e escolher' chega com as estações");
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
        public void Locomocao_NoRig_ComASalaEAMesaDaCena()
        {
            var locomocao = Todos<LocomocaoDoAluno>().Single();
            var serializado = new SerializedObject(locomocao);

            Assert.That(locomocao.gameObject.name, Is.EqualTo("OVRCameraRig"));
            Assert.That(((Transform)serializado.FindProperty("_rig").objectReferenceValue).name, Is.EqualTo("OVRCameraRig"));
            Assert.That(((Transform)serializado.FindProperty("_cabeca").objectReferenceValue).name, Is.EqualTo("CenterEyeAnchor"));
            foreach (string campo in new[] { "_raioEsquerdo", "_raioDireito", "_arco", "_marcador", "_materialValido",
                         "_materialInvalido", "_escurecimento", "_caixa" })
                Assert.That(serializado.FindProperty(campo).objectReferenceValue, Is.Not.Null, campo);
            MesmaCor(((Material)serializado.FindProperty("_materialValido").objectReferenceValue).GetColor("_BaseColor"),
                PaletaDoLaboratorio.Teleporte);

            var regras = locomocao.Regras;
            Assert.That(regras.Avaliar(0, 1), Is.EqualTo(MotivoDoTeleporte.NaMesa));
            Assert.That(regras.Avaliar(0, 3.7), Is.EqualTo(MotivoDoTeleporte.PertoDaParede));
            Assert.That(regras.Avaliar(0, 0), Is.EqualTo(MotivoDoTeleporte.Nenhum), "onde o aluno começa vale");
        }

        [Test]
        public void Escurecimento_PresoACabeca_DesligadoForaDoGiro()
        {
            var locomocao = Todos<LocomocaoDoAluno>().Single();
            var esfera = (Renderer)new SerializedObject(locomocao).FindProperty("_escurecimento").objectReferenceValue;

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

            // A caixa 4 × 3 × 2 a 10 cm por unidade (EscalaDaCaixa): 40 × 20 × 30 cm (x, altura, z).
            Assert.That(Vector3.Distance(colisor.size, new Vector3(0.4f, 0.2f, 0.3f)), Is.LessThan(1e-4f));
            Assert.That(Vector3.Distance(colisor.center, new Vector3(0f, 0.1f, 0f)), Is.LessThan(1e-4f));
            Assert.That(caixa.GetComponent<Rigidbody>().isKinematic);
            Assert.That(new SerializedObject(caixa).FindProperty("_materialDaArestaDestacada").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void SoACaixaTemColisor()
        {
            // O raio só para no que é interativo; sala, mesa, painéis, marcador e modelos não bloqueiam.
            var colisores = Todos<Collider>();

            Assert.That(colisores.Select(c => c.name), Is.EqualTo(new[] { "Caixa" }));
        }

        [Test]
        public void CenaEstaNoBuild()
        {
            var cenas = EditorBuildSettings.scenes.Where(c => c.enabled).Select(c => c.path).ToArray();

            Assert.That(cenas, Is.EqualTo(new[] { CaminhoDaCena }));
        }
    }
}
