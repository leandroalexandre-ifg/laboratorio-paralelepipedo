using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class MovimentoDoRigTests
    {
        private const double Tolerancia = 1e-9;

        private static void Igual(Vetor3 obtido, Vetor3 esperado)
        {
            Assert.That(obtido.X, Is.EqualTo(esperado.X).Within(Tolerancia), "x");
            Assert.That(obtido.Y, Is.EqualTo(esperado.Y).Within(Tolerancia), "y");
            Assert.That(obtido.Z, Is.EqualTo(esperado.Z).Within(Tolerancia), "z");
        }

        [Test]
        public void GiroDe45Graus()
        {
            Assert.That(MovimentoDoRig.AnguloDoGiroEmGraus, Is.EqualTo(45));
        }

        [Test]
        public void Teleporte_LevaACabecaAoDestino_ManteAAlturaDoRig()
        {
            var rig = new Vetor3(1, 0, 2);
            var cabeca = new Vetor3(1.3, 1.5, 1.8);   // a cabeça não fica no centro do rig
            var destino = new Vetor3(-2, 0, 0.5);

            Vetor3 novoRig = MovimentoDoRig.Teleportar(rig, cabeca, destino);
            Vetor3 novaCabeca = novoRig + (cabeca - rig);

            Igual(new Vetor3(novaCabeca.X, 0, novaCabeca.Z), new Vetor3(-2, 0, 0.5));
            Assert.That(novoRig.Y, Is.EqualTo(0));
        }

        [Test]
        public void Giro_DeixaACabecaNoLugar()
        {
            var rig = new Vetor3(0, 0, 0);
            var cabeca = new Vetor3(0.2, 1.5, 0.1);

            Vetor3 novoRig = MovimentoDoRig.GirarEmTornoDaCabeca(rig, cabeca, 45);

            // A cabeça está em (0,2; 0,1) do rig; depois do giro de 45° o rig gira junto, e a cabeça volta ao lugar.
            double r = System.Math.Sqrt(0.5);
            var deslocamentoGirado = new Vetor3(0.2 * r + 0.1 * r, 0, -0.2 * r + 0.1 * r);
            Igual(novoRig + deslocamentoGirado, new Vetor3(0.2, 0, 0.1));
        }

        [Test]
        public void Giro_ParaADireita_EmSentidoHorarioVistoDeCima()
        {
            // Rig 1 m à frente da cabeça (+z); girar 90° à direita leva o rig para +x.
            Vetor3 novoRig = MovimentoDoRig.GirarEmTornoDaCabeca(new Vetor3(0, 0, 1), new Vetor3(0, 1.5, 0), 90);

            Igual(novoRig, new Vetor3(1, 0, 0));
        }

        [Test]
        public void OitoPassosDaoUmaVolta()
        {
            var rig = new Vetor3(0.4, 0, -0.3);
            var cabeca = new Vetor3(1, 1.6, 1);
            Vetor3 atual = rig;
            double angulo = 0;
            for (int i = 0; i < 8; i++)
            {
                atual = MovimentoDoRig.GirarEmTornoDaCabeca(atual, cabeca, MovimentoDoRig.AnguloDoGiroEmGraus);
                angulo = MovimentoDoRig.NormalizarAngulo(angulo + MovimentoDoRig.AnguloDoGiroEmGraus);
            }

            Igual(atual, rig);
            Assert.That(angulo, Is.EqualTo(0).Within(Tolerancia));
        }

        [TestCase(-45, 315)]
        [TestCase(360, 0)]
        [TestCase(405, 45)]
        public void NormalizarAngulo(double graus, double esperado)
        {
            Assert.That(MovimentoDoRig.NormalizarAngulo(graus), Is.EqualTo(esperado).Within(Tolerancia));
        }
    }
}
