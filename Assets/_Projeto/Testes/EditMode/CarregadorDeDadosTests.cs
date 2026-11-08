using System;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using NUnit.Framework;
using UnityEngine;

namespace LaboratorioParalelepipedo.Testes
{
    public class CarregadorDeDadosTests
    {
        private const string Textos = @"{
  ""versao"": 1,
  ""textos"": [
    { ""chave"": ""estacao.areas.fechar"", ""texto"": ""Fechar a caixa"" },
    { ""chave"": ""estacao.volume.formula"", ""texto"": ""V = a · b · c"" }
  ]
}";

        private const string Desafios = @"{
  ""versao"": 1,
  ""desafios"": [
    {
      ""id"": ""diag-01"", ""estacao"": ""diagonais"", ""bncc"": ""EF09MA14"",
      ""enunciado"": ""Quanto mede a diagonal da base?"",
      ""dimensoes"": { ""a"": ""2"", ""b"": ""3"", ""c"": ""6"" },
      ""grandeza"": ""diagonalDaBase"", ""distratores"": [""5"", ""√5"", ""13""]
    },
    {
      ""id"": ""elem-01"", ""tipo"": ""montarFace"", ""estacao"": ""elementos"", ""bncc"": ""EF06MA17"",
      ""enunciado"": ""Monte a face ABFE."",
      ""dimensoes"": { ""a"": ""4"", ""b"": ""3"", ""c"": ""2"" },
      ""face"": ""ABFE""
    }
  ]
}";

        [Test]
        public void CarregaTextosDoJson()
        {
            var catalogo = CarregadorDeDados.CarregarTextos(Textos);

            Assert.That(catalogo.Obter("estacao.areas.fechar"), Is.EqualTo("Fechar a caixa"));
            Assert.That(catalogo.Obter("estacao.volume.formula"), Is.EqualTo("V = a · b · c"));
        }

        [Test]
        public void CarregaDesafiosDosDoisTipos()
        {
            var desafios = CarregadorDeDados.CarregarDesafios(Desafios);

            Assert.That(desafios.Count, Is.EqualTo(2));
            var diagonal = (DesafioDeResposta)desafios[0];
            Assert.That(diagonal.RespostaCorreta, Is.EqualTo(Math.Sqrt(13)).Within(1e-9));
            Assert.That(diagonal.Distratores[1].Texto, Is.EqualTo("√5"));
            Assert.That(((DesafioDeMontarFace)desafios[1]).FaceAlvo.Nome, Is.EqualTo("ABFE"));
        }

        [Test]
        public void NumerosSemAspas_TambemSaoLidos()
        {
            // O JsonUtility entrega 2.5 como "2.500000"; o interpretador lê do mesmo jeito.
            string json = Desafios
                .Replace(@"""a"": ""2"", ""b"": ""3""", @"""a"": 2.5, ""b"": 3")
                .Replace(@"""grandeza"": ""diagonalDaBase""", @"""grandeza"": ""diagonalDaBase"", ""tolerancia"": 0.05");

            var desafio = (DesafioDeResposta)CarregadorDeDados.CarregarDesafios(json)[0];

            Assert.That(desafio.Comprimento.Valor, Is.EqualTo(2.5));
            Assert.That(desafio.Comprimento.Texto, Is.EqualTo("2,5"));
            Assert.That(desafio.Tolerancia, Is.EqualTo(0.05).Within(1e-12));
        }

        [Test]
        public void ToleranciaNulaNoJson_ContaComoAusente()
        {
            string json = Desafios.Replace(@"""grandeza"": ""diagonalDaBase""",
                @"""grandeza"": ""diagonalDaBase"", ""tolerancia"": null");

            Assert.That(((DesafioDeResposta)CarregadorDeDados.CarregarDesafios(json)[0]).Tolerancia, Is.EqualTo(0.01));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void JsonVazio_DaErroDeArquivoVazio(string json)
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => CarregadorDeDados.CarregarDesafios(json));

            Assert.That(erro.Message, Does.Contain("desafios.json").And.Contain("vazio"));
        }

        [TestCase("{ invalido")]
        [TestCase("[1, 2]")]
        [TestCase("{\"versao\": 1, \"textos\": [ { \"chave\": \"a\" \"texto\": \"b\" } ] }")]
        public void JsonMalFormado_DaErroClaro(string json)
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => CarregadorDeDados.CarregarTextos(json));

            Assert.That(erro.Message, Does.Contain("textos.json").And.Contain("mal formado"));
        }

        [Test]
        public void ErrosDoNucleo_ChegamComONomeDoArquivo()
        {
            string json = Desafios.Replace(@"""face"": ""ABFE""", @"""face"": ""ABCG""");

            var erro = Assert.Throws<DadosInvalidosException>(() =>
                CarregadorDeDados.CarregarDesafios(json, "meus-desafios.json"));

            Assert.That(erro.Message, Does.StartWith("meus-desafios.json tem 1 erro:")
                .And.Contain("não existe na caixa"));
        }

        [Test]
        public void ChaveComErroDeDigitacao_ApareceAntesDosErrosDeConteudo()
        {
            string json = Desafios.Replace(@"""grandeza"": ""diagonalDaBase""", @"""grandesa"": ""diagonalDaBase""");

            var erro = Assert.Throws<DadosInvalidosException>(() => CarregadorDeDados.CarregarDesafios(json));

            Assert.That(erro.Erros.Count, Is.EqualTo(2));
            Assert.That(erro.Erros[0], Does.Contain("chave desconhecida \"grandesa\"")
                .And.Contain("Você quis dizer \"grandeza\"?"));
            Assert.That(erro.Erros[1], Does.Contain("falta a \"grandeza\""));
        }

        [Test]
        public void JsonMalFormado_ApontaLinhaEColuna()
        {
            string json = Desafios.Replace(@"""bncc"": ""EF09MA14"",", @"""bncc"": ""EF09MA14""");

            var erro = Assert.Throws<DadosInvalidosException>(() => CarregadorDeDados.CarregarDesafios(json));

            Assert.That(erro.Erros.Count, Is.EqualTo(1));
            Assert.That(erro.Erros[0], Does.Contain("mal formado na linha 6").And.Contain("falta uma vírgula"));
        }

        [Test]
        public void TextAsset_UsaONomeDoAsset()
        {
            var asset = new TextAsset(Textos) { name = "textos-teste" };
            try
            {
                Assert.That(CarregadorDeDados.CarregarTextos(asset).Quantidade, Is.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void TextAssetNaoLigado_DaErroClaro()
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => CarregadorDeDados.CarregarDesafios((TextAsset)null));

            Assert.That(erro.Message, Does.Contain("não foi ligado"));
        }
    }
}
