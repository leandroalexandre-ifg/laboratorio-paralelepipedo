using LaboratorioParalelepipedo.Apresentacao;
using NUnit.Framework;
using UnityEngine;

namespace LaboratorioParalelepipedo.Testes
{
    public class PaletaDoLaboratorioTests
    {
        private static string Hex(Color cor) => "#" + ColorUtility.ToHtmlStringRGB(cor);

        [Test]
        public void CoresDaSalaEDaCaixa_SaoAsDosMockups()
        {
            Assert.That(Hex(PaletaDoLaboratorio.Parede), Is.EqualTo("#DCE6EC"));
            Assert.That(Hex(PaletaDoLaboratorio.Piso), Is.EqualTo("#B8C7D1"));
            Assert.That(Hex(PaletaDoLaboratorio.TampoDaMesa), Is.EqualTo("#5E6E7C"));
            Assert.That(Hex(PaletaDoLaboratorio.PeDaMesa), Is.EqualTo("#43515E"));
            Assert.That(Hex(PaletaDoLaboratorio.Aresta), Is.EqualTo("#134B6E"));
            Assert.That(Hex(PaletaDoLaboratorio.Face), Is.EqualTo("#9FD3F2"));
            Assert.That(Hex(PaletaDoLaboratorio.Texto), Is.EqualTo("#1B2A3A"));
        }

        [Test]
        public void FacesTranslucidas_TemOpacidadeDe28PorCento()
        {
            Assert.That(PaletaDoLaboratorio.Face.a, Is.EqualTo(0.28f).Within(1e-6));
        }

        [Test]
        public void CoresOpacas_TemAlfaUm()
        {
            Assert.That(PaletaDoLaboratorio.Parede.a, Is.EqualTo(1f));
            Assert.That(PaletaDoLaboratorio.Aresta.a, Is.EqualTo(1f));
        }

        [TestCase(0x000000, 0f, 0f, 0f)]
        [TestCase(0xFF8000, 1f, 128 / 255f, 0f)]
        public void Hex_SeparaOsCanais(int rgb, float r, float g, float b)
        {
            var cor = PaletaDoLaboratorio.Hex(rgb);

            Assert.That(cor.r, Is.EqualTo(r).Within(1e-6));
            Assert.That(cor.g, Is.EqualTo(g).Within(1e-6));
            Assert.That(cor.b, Is.EqualTo(b).Within(1e-6));
        }
    }
}
