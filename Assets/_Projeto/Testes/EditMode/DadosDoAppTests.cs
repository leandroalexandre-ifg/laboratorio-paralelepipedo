using System;
using System.Collections.Generic;
using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>Confere os arquivos de dados de verdade do app (Assets/_Projeto/Dados).</summary>
    public class DadosDoAppTests
    {
        private const string PastaDeDados = "Assets/_Projeto/Dados/";

        private static TextAsset Carregar(string arquivo)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(PastaDeDados + arquivo);
            Assert.That(asset, Is.Not.Null, $"{PastaDeDados}{arquivo} não foi importado como TextAsset");
            return asset;
        }

        private static IReadOnlyList<Desafio> Desafios() => CarregadorDeDados.CarregarDesafios(Carregar("desafios.json"));

        private static T Buscar<T>(string id) where T : Desafio => (T)Desafios().Single(d => d.Id == id);

        [Test]
        public void TextosJson_CarregaSemErros()
        {
            Assert.DoesNotThrow(() => CarregadorDeDados.CarregarTextos(Carregar("textos.json")));
        }

        [Test]
        public void TextosJson_TemTodasAsChavesDoApp()
        {
            var catalogo = CarregadorDeDados.CarregarTextos(Carregar("textos.json"));

            var faltando = ChavesDeTexto.Todas.Where(c => !catalogo.Contem(c)).ToList();
            Assert.That(faltando, Is.Empty, "chaves que faltam em textos.json");
        }

        [Test]
        public void TextosJson_NaoTemChavesDesconhecidas()
        {
            var catalogo = CarregadorDeDados.CarregarTextos(Carregar("textos.json"));

            var sobrando = catalogo.Chaves.Where(c => !ChavesDeTexto.Todas.Contains(c)).ToList();
            Assert.That(sobrando, Is.Empty, "chaves em textos.json que o app não usa (erro de digitação?)");
        }

        [Test]
        public void TextosJson_TrazOsCreditos()
        {
            var catalogo = CarregadorDeDados.CarregarTextos(Carregar("textos.json"));

            Assert.That(catalogo.Obter(ChavesDeTexto.AppCreditos),
                Is.EqualTo("Desenvolvido por NumbERS · IFG – Câmpus Inhumas"));
        }

        [Test]
        public void TextosComMarcadores_FormatamSemErro()
        {
            var catalogo = CarregadorDeDados.CarregarTextos(Carregar("textos.json"));

            foreach (string chave in catalogo.Chaves)
            {
                string texto = catalogo.Obter(chave);
                Assert.DoesNotThrow(() => string.Format(texto, "x", "y"), chave);
            }
        }

        [Test]
        public void DesafiosJson_CarregaSemErros()
        {
            Assert.DoesNotThrow(() => Desafios());
        }

        [TestCase(Estacao.Elementos)]
        [TestCase(Estacao.Diagonais)]
        [TestCase(Estacao.Areas)]
        [TestCase(Estacao.Volume)]
        public void CadaEstacao_TemPeloMenosDoisDesafios(Estacao estacao)
        {
            Assert.That(Desafios().Count(d => d.Estacao == estacao), Is.GreaterThanOrEqualTo(2));
        }

        [TestCase(Estacao.Elementos, "EF06MA17")]
        [TestCase(Estacao.Diagonais, "EF09MA14")]
        [TestCase(Estacao.Volume, "EF09MA19")]
        public void CodigosDaBncc_PorEstacao(Estacao estacao, string codigo)
        {
            foreach (var desafio in Desafios().Where(d => d.Estacao == estacao))
                Assert.That(desafio.CodigoBncc, Is.EqualTo(codigo), desafio.Id);
        }

        [Test]
        public void EstacaoElementos_TemOsDoisTipos()
        {
            var elementos = Desafios().Where(d => d.Estacao == Estacao.Elementos).ToList();

            Assert.That(elementos.OfType<DesafioDeMontarFace>(), Is.Not.Empty);
            Assert.That(elementos.OfType<DesafioDeResposta>().Where(d => d.Grandeza.EhContagem()), Is.Not.Empty);
        }

        [Test]
        public void ExemplosDosMockups_TemAsRespostasEsperadas()
        {
            Assert.That(Buscar<DesafioDeResposta>("diag-01").RespostaCorreta, Is.EqualTo(Math.Sqrt(13)).Within(1e-9));
            Assert.That(Buscar<DesafioDeResposta>("diag-02").RespostaCorreta, Is.EqualTo(7).Within(1e-9));
            Assert.That(Buscar<DesafioDeResposta>("area-01").RespostaCorreta, Is.EqualTo(52).Within(1e-9));
            Assert.That(Buscar<DesafioDeResposta>("vol-01").RespostaCorreta, Is.EqualTo(36).Within(1e-9));
            Assert.That(Buscar<DesafioDeMontarFace>("elem-01").FaceAlvo.Nome, Is.EqualTo("ABFE"));
        }

        [Test]
        public void HaUmDesafioComMedidaIrracionalQuePedeARessalva()
        {
            var comRaiz = Desafios().Where(d =>
                d.Comprimento.Texto.Contains("√") || d.Largura.Texto.Contains("√") || d.Altura.Texto.Contains("√"))
                .ToList();

            Assert.That(comRaiz, Is.Not.Empty);
            var volume = comRaiz.Single(d => d.Estacao == Estacao.Volume);
            Assert.That(new ContagemDeCubinhos(volume.Caixa).PrecisaDaRessalva, Is.True);
        }

        [Test]
        public void DesafiosDeVolume_CabemNoLimiteDeCubinhos()
        {
            foreach (var desafio in Desafios().Where(d => d.Estacao == Estacao.Volume))
                Assert.That(new ContagemDeCubinhos(desafio.Caixa).ExcedeLimite, Is.False, desafio.Id);
        }
    }
}
