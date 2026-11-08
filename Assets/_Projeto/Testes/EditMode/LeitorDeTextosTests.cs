using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo.Dados;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class LeitorDeTextosTests
    {
        private static ArquivoDeTextosDto Arquivo(int versao, params (string chave, string texto)[] itens)
        {
            var textos = new TextoDto[itens.Length];
            for (int i = 0; i < itens.Length; i++)
                textos[i] = new TextoDto { chave = itens[i].chave, texto = itens[i].texto };
            return new ArquivoDeTextosDto { versao = versao, textos = textos };
        }

        [Test]
        public void ArquivoValido_DaUmCatalogoComOsTextos()
        {
            var catalogo = LeitorDeTextos.Ler(Arquivo(1,
                ("app.titulo", "Laboratório do Paralelepípedo"),
                ("estacao.areas.fechar", "Fechar a caixa")));

            Assert.That(catalogo.Quantidade, Is.EqualTo(2));
            Assert.That(catalogo.Obter("estacao.areas.fechar"), Is.EqualTo("Fechar a caixa"));
            Assert.That(catalogo.Contem("app.titulo"), Is.True);
        }

        [Test]
        public void ChaveQueFalta_ApareceEntreColchetes()
        {
            var catalogo = LeitorDeTextos.Ler(Arquivo(1, ("app.titulo", "Lab")));

            Assert.That(catalogo.Obter("nao.existe"), Is.EqualTo("[nao.existe]"));
            Assert.That(catalogo.TentarObter("nao.existe", out _), Is.False);
        }

        [Test]
        public void ArquivoNulo_DaErroDeArquivoVazio()
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => LeitorDeTextos.Ler(null));

            Assert.That(erro.Message, Does.Contain("textos.json").And.Contain("vazio"));
        }

        [TestCase(0)]
        [TestCase(2)]
        public void VersaoDiferenteDe1_DaErro(int versao)
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => LeitorDeTextos.Ler(Arquivo(versao, ("a", "b"))));

            Assert.That(erro.Erros[0], Does.Contain("\"versao\" deve ser 1"));
        }

        [Test]
        public void SemTextos_DaErro()
        {
            Assert.Throws<DadosInvalidosException>(() => LeitorDeTextos.Ler(Arquivo(1)));
            Assert.Throws<DadosInvalidosException>(() =>
                LeitorDeTextos.Ler(new ArquivoDeTextosDto { versao = 1, textos = null }));
        }

        [Test]
        public void ReuneTodosOsErrosDeUmaVez()
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => LeitorDeTextos.Ler(Arquivo(1,
                ("app.titulo", "Lab"),
                ("", "sem chave"),
                ("geral.acerto", "  "),
                ("app.titulo", "de novo"))));

            Assert.That(erro.Erros, Is.EqualTo(new List<string>
            {
                "texto na posição 2: falta a \"chave\".",
                "chave \"geral.acerto\": o \"texto\" está vazio.",
                "chave \"app.titulo\" repetida (posições 1 e 4).",
            }));
            Assert.That(erro.Message, Does.StartWith("textos.json tem 3 erros:"));
        }

        [Test]
        public void ItemNulo_ContaComoSemChave()
        {
            var arquivo = new ArquivoDeTextosDto { versao = 1, textos = new TextoDto[] { null } };

            var erro = Assert.Throws<DadosInvalidosException>(() => LeitorDeTextos.Ler(arquivo));

            Assert.That(erro.Erros[0], Does.Contain("posição 1"));
        }

        [Test]
        public void ChavesDeTexto_NaoSeRepetem()
        {
            Assert.That(ChavesDeTexto.Todas, Is.Unique);
        }
    }
}
