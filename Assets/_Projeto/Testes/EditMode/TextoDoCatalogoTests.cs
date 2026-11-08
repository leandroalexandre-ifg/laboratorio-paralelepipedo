using LaboratorioParalelepipedo.Apresentacao;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace LaboratorioParalelepipedo.Testes
{
    public class TextoDoCatalogoTests
    {
        private const string Textos =
            "{ \"versao\": 1, \"textos\": [ { \"chave\": \"app.titulo\", \"texto\": \"Laboratório\" }, " +
            "{ \"chave\": \"app.creditos\", \"texto\": \"NumbERS\" } ] }";

        private const string Desafios =
            "{ \"versao\": 1, \"desafios\": [ { \"id\": \"d1\", \"estacao\": \"volume\", \"bncc\": \"EF09MA19\", " +
            "\"enunciado\": \"?\", \"dimensoes\": { \"a\": \"2\", \"b\": \"3\", \"c\": \"4\" }, " +
            "\"grandeza\": \"volume\", \"distratores\": [\"9\"] } ] }";

        private GameObject _objeto;
        private TextAsset _textos;
        private TextAsset _desafios;

        [SetUp]
        public void Criar()
        {
            _objeto = new GameObject("Teste");
            _textos = new TextAsset(Textos) { name = "textos" };
            _desafios = new TextAsset(Desafios) { name = "desafios" };
        }

        [TearDown]
        public void Destruir()
        {
            Object.DestroyImmediate(_objeto);
            Object.DestroyImmediate(_textos);
            Object.DestroyImmediate(_desafios);
        }

        private TextMeshPro Mostrar(DadosDoLaboratorio dados, params string[] chaves)
        {
            var texto = _objeto.AddComponent<TextMeshPro>();
            var componente = _objeto.AddComponent<TextoDoCatalogo>();
            componente.Configurar(dados, texto, chaves);
            componente.Atualizar();
            return texto;
        }

        [Test]
        public void MostraOsTextosDasChaves_UmPorLinha()
        {
            var dados = _objeto.AddComponent<DadosDoLaboratorio>();
            dados.Configurar(_textos, _desafios);

            var texto = Mostrar(dados, "app.titulo", "app.creditos");

            Assert.That(texto.text, Is.EqualTo("Laboratório\nNumbERS"));
        }

        [Test]
        public void ChaveQueFalta_ApareceEntreColchetes()
        {
            var dados = _objeto.AddComponent<DadosDoLaboratorio>();
            dados.Configurar(_textos, _desafios);

            Assert.That(Mostrar(dados, "nao.existe").text, Is.EqualTo("[nao.existe]"));
        }

        [Test]
        public void DadosCarregaOsDesafios()
        {
            var dados = _objeto.AddComponent<DadosDoLaboratorio>();
            dados.Configurar(_textos, _desafios);

            Assert.That(dados.Desafios.Count, Is.EqualTo(1));
            Assert.That(dados.Desafios[0].Id, Is.EqualTo("d1"));
        }

        [Test]
        public void ArquivoComErro_RegistraNoLog_ESegueComDadosVazios()
        {
            var quebrado = new TextAsset("{ \"versao\": 1, \"testos\": [] }") { name = "textos" };
            try
            {
                var dados = _objeto.AddComponent<DadosDoLaboratorio>();
                dados.Configurar(quebrado, _desafios);

                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("chave desconhecida \"testos\""));
                Assert.That(Mostrar(dados, "app.titulo").text, Is.EqualTo("[app.titulo]"));
                Assert.That(dados.Desafios.Count, Is.EqualTo(1), "o outro arquivo continua valendo");
            }
            finally
            {
                Object.DestroyImmediate(quebrado);
            }
        }

        [Test]
        public void SemDadosLigados_RegistraErro()
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("sem DadosDoLaboratorio"));

            Mostrar(null, "app.titulo");
        }
    }
}
