using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class DicasDosControlesTests
    {
        [Test]
        public void SemNadaAcontecendo_SemDica()
        {
            Assert.That(DicasDosControles.Chave(false, false, false, false), Is.Null);
        }

        [Test]
        public void RaioNaCaixa_DicaDePegar()
        {
            Assert.That(DicasDosControles.Chave(false, false, true, false), Is.EqualTo(ChavesDeTexto.DicaPegar));
        }

        [Test]
        public void SegurandoACaixa_SemDicaDePegar()
        {
            Assert.That(DicasDosControles.Chave(false, false, true, true), Is.Null);
        }

        [TestCase(true, ChavesDeTexto.DicaTeleporteValido)]
        [TestCase(false, ChavesDeTexto.DicaTeleporteInvalido)]
        public void MirandoOTeleporte_DizSeDa(bool valido, string esperada)
        {
            Assert.That(DicasDosControles.Chave(true, valido, false, false), Is.EqualTo(esperada));
            Assert.That(DicasDosControles.Chave(true, valido, true, false), Is.EqualTo(esperada), "o teleporte vem antes");
        }

        [Test]
        public void RaioNumVertice_DicaDoGatilho_AntesDaDeDePegar()
        {
            Assert.That(DicasDosControles.Chave(false, false, true, false, true), Is.EqualTo(ChavesDeTexto.DicaEscolher));
            Assert.That(DicasDosControles.Chave(true, true, false, false, true), Is.EqualTo(ChavesDeTexto.DicaTeleporteValido));
        }
    }
}
