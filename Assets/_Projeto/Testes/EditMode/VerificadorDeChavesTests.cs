using LaboratorioParalelepipedo.Nucleo.Dados;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class VerificadorDeChavesTests
    {
        // Um desafio válido; os testes trocam pedaços dele.
        private const string Base = @"{
  ""versao"": 1,
  ""desafios"": [
    {
      ""id"": ""diag-01"",
      ""estacao"": ""diagonais"",
      ""bncc"": ""EF09MA14"",
      ""enunciado"": ""Quanto mede d?"",
      ""dimensoes"": { ""a"": ""2"", ""b"": ""3"", ""c"": ""6"" },
      ""grandeza"": ""diagonalDaBase"",
      ""distratores"": [""5"", ""√5""]
    }
  ]
}";

        private static string Trocar(string de, string para)
        {
            Assert.That(Base, Does.Contain(de), "trecho de teste não existe no JSON base");
            return Base.Replace(de, para);
        }

        [Test]
        public void JsonValido_NaoTemErros()
        {
            Assert.That(VerificadorDeChaves.VerificarDesafios(Base), Is.Empty);
        }

        [Test]
        public void ChaveComErroDeDigitacao_SugereAMaisParecida()
        {
            var erros = VerificadorDeChaves.VerificarDesafios(Trocar(@"""grandeza""", @"""grandesa"""));

            Assert.That(erros, Is.EqualTo(new[]
            {
                "Desafio \"diag-01\": chave desconhecida \"grandesa\" (linha 10). Você quis dizer \"grandeza\"?",
            }));
        }

        [TestCase("enunciando", "enunciado")]
        [TestCase("distractors", "distratores")]
        [TestCase("Grandeza", "grandeza")]
        [TestCase("estação", "estacao")]
        [TestCase("tolerância", "tolerancia")]
        [TestCase("dimensões", "dimensoes")]
        public void SugestoesParaErrosComuns(string digitada, string esperada)
        {
            Assert.That(VerificadorDeChaves.MaisParecida(digitada, VerificadorDeChaves.ChavesValidasDoDesafio),
                Is.EqualTo(esperada));
        }

        [TestCase("resposta")]
        [TestCase("xyz")]
        [TestCase("q")]
        public void ChaveSemNadaParecido_ListaAsChavesValidas(string chave)
        {
            var erros = VerificadorDeChaves.VerificarDesafios(
                Trocar(@"""bncc"": ""EF09MA14"",", $@"""bncc"": ""EF09MA14"", ""{chave}"": ""x"","));

            Assert.That(erros.Count, Is.EqualTo(1));
            Assert.That(erros[0], Does.Contain($"chave desconhecida \"{chave}\"")
                .And.Contain("Chaves válidas: id, tipo, estacao, bncc, enunciado, dimensoes, grandeza, " +
                             "distratores, tolerancia, face."));
        }

        [Test]
        public void ChaveDesconhecidaNasDimensoes()
        {
            var erros = VerificadorDeChaves.VerificarDesafios(Trocar(@"""c"": ""6""", @"""h"": ""6"""));

            Assert.That(erros[0], Is.EqualTo(
                "Desafio \"diag-01\": chave desconhecida \"h\" em \"dimensoes\" (linha 9). Chaves válidas: a, b, c."));
        }

        [Test]
        public void ChaveDesconhecidaNoInicioDoArquivo()
        {
            var erros = VerificadorDeChaves.VerificarDesafios(Trocar(@"""versao""", @"""versão"""));

            Assert.That(erros[0], Does.StartWith("No início do arquivo: chave desconhecida \"versão\"")
                .And.EndWith("Você quis dizer \"versao\"?"));
        }

        [Test]
        public void DesafioSemId_UsaAPosicao()
        {
            var erros = VerificadorDeChaves.VerificarDesafios(
                Trocar(@"""id"": ""diag-01"",", @"""grandesa"": ""x"","));

            Assert.That(erros[0], Does.StartWith("Desafio na posição 1: chave desconhecida \"grandesa\""));
        }

        [Test]
        public void ChaveRepetida_DaErro()
        {
            var erros = VerificadorDeChaves.VerificarDesafios(
                Trocar(@"""bncc"": ""EF09MA14"",", @"""bncc"": ""EF09MA14"", ""bncc"": ""EF06MA17"","));

            Assert.That(erros[0], Does.Contain("chave \"bncc\" repetida"));
        }

        [Test]
        public void ReuneErrosDeVariosDesafios()
        {
            string json = Trocar(@"""grandeza""", @"""grandesa""").Replace("    }\n  ]",
                "    },\n    { \"id\": \"x\", \"enunciadoo\": \"?\", \"fase\": \"ABFE\" }\n  ]");
            Assert.That(json, Does.Contain("enunciadoo"), "o JSON de teste precisa ter fim de linha LF");

            var erros = VerificadorDeChaves.VerificarDesafios(json);

            Assert.That(erros.Count, Is.EqualTo(3));
            Assert.That(erros[1], Does.Contain("Desafio \"x\"").And.Contain("\"enunciado\"?"));
            Assert.That(erros[2], Does.Contain("\"fase\"").And.Contain("\"face\"?"));
        }

        [Test]
        public void TextosComChaveDesconhecida()
        {
            const string json = @"{ ""versao"": 1, ""textos"": [ { ""chave"": ""app.titulo"", ""testo"": ""Lab"" } ] }";

            var erros = VerificadorDeChaves.VerificarTextos(json);

            Assert.That(erros, Is.EqualTo(new[]
            {
                "Texto \"app.titulo\": chave desconhecida \"testo\" (linha 1). Você quis dizer \"texto\"?",
            }));
        }

        [TestCase("{ \"versao\": 1, }", "linha 1", "vírgula sobrando")]
        [TestCase("{ \"versao\": 1 \"desafios\": [] }", "linha 1", "falta uma vírgula")]
        [TestCase("{\n  \"versao\": 1,\n  \"desafios\": [\"a\",]\n}", "linha 3", "vírgula sobrando antes de \"]\"")]
        [TestCase("{ versao: 1 }", "coluna 3", "entre aspas")]
        [TestCase("{ \"a\": 2,5 }", "linha 1", "entre aspas, como \"2,5\"")]
        [TestCase("{ \"a\": \"sem fim }", "linha 1", "aspas de fechamento")]
        [TestCase("{ \"a\": \"quebra\nde linha\" }", "linha 1", "quebra de linha")]
        [TestCase("{ \"a\": 1 } }", "linha 1", "sobrando")]
        [TestCase("{ \"a\": sim }", "linha 1", "valor inválido")]
        [TestCase("{ \"a\": 1", "linha 1", "\"}\"")]
        public void JsonMalFormado_DaErroComLinhaColunaEMotivo(string json, string posicao, string motivo)
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => VerificadorDeChaves.VerificarDesafios(json));

            Assert.That(erro.Erros[0], Does.StartWith("o JSON está mal formado na ")
                .And.Contain(posicao).And.Contain(motivo));
        }

        [TestCase("[1, 2]")]
        [TestCase("\"texto\"")]
        public void RaizQueNaoEhObjeto_DaErro(string json)
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => VerificadorDeChaves.VerificarTextos(json));

            Assert.That(erro.Erros[0], Does.Contain("mal formado").And.Contain("objeto"));
        }

        [Test]
        public void AceitaBomEscapesENumeros()
        {
            const string json = "\uFEFF{ \"versao\": 1, \"textos\": [ { \"chave\": \"a\\u00e7\", " +
                                "\"texto\": \"x \\\"y\\\" \\n\" } ], \"z\": [-1.5e3, true, false, null] }";

            var erros = VerificadorDeChaves.VerificarTextos(json);

            Assert.That(erros.Count, Is.EqualTo(1));
            Assert.That(erros[0], Does.Contain("chave desconhecida \"z\""));
        }

        [TestCase("")]
        [TestCase(null)]
        public void JsonVazio_DaErroDeArquivoVazio(string json)
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => VerificadorDeChaves.VerificarDesafios(json));

            Assert.That(erro.Message, Does.Contain("vazio"));
        }
    }
}
