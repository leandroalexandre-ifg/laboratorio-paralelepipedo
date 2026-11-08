using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ParalelepipedoAreasEVolumeTests
    {
        private const double Eps = 1e-9;

        [Test]
        public void Caixa4x3x2_TemAreaTotal52()
        {
            var caixa = new Paralelepipedo(4, 3, 2);

            Assert.That(caixa.AreaDaBase, Is.EqualTo(12).Within(Eps));
            Assert.That(caixa.AreaLateral, Is.EqualTo(28).Within(Eps));
            Assert.That(caixa.AreaTotal, Is.EqualTo(52).Within(Eps));
        }

        [Test]
        public void Caixa4x3x3_TemVolume36()
        {
            var caixa = new Paralelepipedo(4, 3, 3);

            Assert.That(caixa.Volume, Is.EqualTo(36).Within(Eps));
        }

        [Test]
        public void AreaTotal_EhASomaDasAreasDasSeisFaces()
        {
            var caixa = new Paralelepipedo(2.5, 1.2, Math.Sqrt(2));

            double soma = 0;
            foreach (var face in caixa.Faces) soma += face.Area;

            Assert.That(caixa.AreaTotal, Is.EqualTo(soma).Within(Eps));
        }

        [Test]
        public void AreaLateral_EhASomaDasFacesQueNaoSaoBases()
        {
            var caixa = new Paralelepipedo(4, 3, 2);

            double soma = 0;
            foreach (var face in caixa.Faces)
                if (face.Par != ParDeFaces.Bases) soma += face.Area;

            Assert.That(caixa.AreaLateral, Is.EqualTo(soma).Within(Eps));
        }

        [TestCase(4, 3, 2)]
        [TestCase(2.5, 1.2, 1.4142135623730951)]
        public void CumpreAsRelacoesDeTodoPrismaReto(double a, double b, double c)
        {
            IPrisma prisma = new Paralelepipedo(a, b, c);

            Assert.That(prisma.Altura, Is.EqualTo(c));
            Assert.That(prisma.AreaLateral, Is.EqualTo(prisma.PerimetroDaBase * prisma.Altura).Within(Eps));
            Assert.That(prisma.AreaTotal, Is.EqualTo(2 * prisma.AreaDaBase + prisma.AreaLateral).Within(Eps));
            Assert.That(prisma.Volume, Is.EqualTo(prisma.AreaDaBase * prisma.Altura).Within(Eps));
        }

        [Test]
        public void DimensoesNaoInteiras_UsamAMesmaFormula()
        {
            var caixa = new Paralelepipedo(2.5, 1.2, Math.Sqrt(2));

            Assert.That(caixa.Volume, Is.EqualTo(3 * Math.Sqrt(2)).Within(Eps));
        }
    }
}
