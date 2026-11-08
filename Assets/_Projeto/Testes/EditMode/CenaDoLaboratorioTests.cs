using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo.Dados;
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
        public void CenaEstaNoBuild()
        {
            var cenas = EditorBuildSettings.scenes.Where(c => c.enabled).Select(c => c.path).ToArray();

            Assert.That(cenas, Is.EqualTo(new[] { CaminhoDaCena }));
        }
    }
}
