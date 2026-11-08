using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class TextosDosControlesTests
    {
        [Test]
        public void SoAsAcoesDisponiveis_NaOrdemFixa()
        {
            var chaves = TextosDosControles.Chaves(AcoesDoControle.Giro | AcoesDoControle.Pegar | AcoesDoControle.Teleporte);

            Assert.That(chaves, Is.EqualTo(new[]
                { ChavesDeTexto.ControlesPegar, ChavesDeTexto.ControlesTeleporte, ChavesDeTexto.ControlesGiro }));
        }

        [Test]
        public void TodasENenhuma()
        {
            Assert.That(TextosDosControles.Chaves(AcoesDoControle.Apontar | AcoesDoControle.Pegar |
                                                  AcoesDoControle.Teleporte | AcoesDoControle.Giro).Length, Is.EqualTo(4));
            Assert.That(TextosDosControles.Chaves(AcoesDoControle.Nenhuma), Is.Empty);
        }
    }
}
