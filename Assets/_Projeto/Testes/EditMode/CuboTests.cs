using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class CuboTests
    {
        private const double Eps = 1e-9;

        [Test]
        public void CuboDeAresta2_TemDiagonais2Raiz2E2Raiz3_AreaTotal24_Volume8()
        {
            var cubo = Paralelepipedo.Cubo(2);

            Assert.That(cubo.DiagonalDaBase, Is.EqualTo(2 * Math.Sqrt(2)).Within(Eps));
            Assert.That(cubo.Diagonal, Is.EqualTo(2 * Math.Sqrt(3)).Within(Eps));
            Assert.That(cubo.AreaTotal, Is.EqualTo(24).Within(Eps));
            Assert.That(cubo.Volume, Is.EqualTo(8).Within(Eps));
        }

        [Test]
        public void Cubo_TemAsTresDimensoesIguaisEEhCubo()
        {
            var cubo = Paralelepipedo.Cubo(2);

            Assert.That(new[] { cubo.A, cubo.B, cubo.C }, Is.EqualTo(new[] { 2.0, 2.0, 2.0 }));
            Assert.That(cubo.EhCubo, Is.True);
        }

        [Test]
        public void ParalelepipedoComTresDimensoesIguais_EhCubo()
        {
            Assert.That(new Paralelepipedo(3, 3, 3).EhCubo, Is.True);
            Assert.That(new Paralelepipedo(0.1 * 3, 0.3, 0.3).EhCubo, Is.True, "arredondamento de 0,1 · 3");
        }

        [TestCase(4, 3, 2)]
        [TestCase(2, 2, 3)]
        [TestCase(2, 3, 3)]
        public void ParalelepipedoComDimensoesDiferentes_NaoEhCubo(double a, double b, double c)
        {
            Assert.That(new Paralelepipedo(a, b, c).EhCubo, Is.False);
        }

        [TestCase(0)]
        [TestCase(-2)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void CuboComArestaInvalida_EhRecusado(double aresta)
        {
            var erro = Assert.Throws<ArgumentOutOfRangeException>(() => Paralelepipedo.Cubo(aresta));
            Assert.That(erro.ParamName, Is.EqualTo("aresta"));
        }

        [Test]
        public void TodasAsFacesDoCuboTemAMesmaArea()
        {
            var cubo = Paralelepipedo.Cubo(1.5);

            foreach (var face in cubo.Faces)
                Assert.That(face.Area, Is.EqualTo(2.25).Within(Eps), face.Nome);
        }
    }
}
