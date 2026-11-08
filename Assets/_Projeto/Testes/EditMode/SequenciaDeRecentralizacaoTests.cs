using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class SequenciaDeRecentralizacaoTests
    {
        private const double Quadro = 1.0 / 64;   // binário exato, perto de 72 fps
        private static readonly Vetor3 Cabeca = new Vetor3(0.3, 1.5, -0.2);

        // Avança até a sequência mandar repor ou até o tempo acabar; devolve quantos quadros levou (-1: não repôs).
        private static int AteRepor(SequenciaDeRecentralizacao s, bool pronta, double segundos)
        {
            for (int i = 1; i <= segundos / Quadro; i++)
                if (s.Avancar(pronta, Cabeca, 0, Quadro)) return i;
            return -1;
        }

        [Test]
        public void Espera_SoConfirmaDepoisDeMeioSegundoParada()
        {
            var espera = new EsperaPelaPoseEstavel();
            espera.Comecar();
            int quadros = 0;
            while (!espera.Avancar(true, Cabeca, 0, Quadro)) quadros++;
            Assert.That((quadros + 1) * Quadro, Is.EqualTo(EsperaPelaPoseEstavel.SegundosParada + Quadro).Within(1e-9));
            Assert.That(espera.Esperando, Is.False);
            Assert.That(espera.Avancar(true, Cabeca, 0, Quadro), Is.False, "uma vez só por pedido");
        }

        [Test]
        public void Espera_GiroRapidoRecomecaAContagem()
        {
            var espera = new EsperaPelaPoseEstavel();
            espera.Comecar();
            for (int i = 0; i < 20; i++) espera.Avancar(true, Cabeca, 0, Quadro);
            // O espaço de rastreamento girou 90° num quadro.
            Assert.That(espera.Avancar(true, Cabeca, 90, Quadro), Is.False);
            int quadros = 1;
            while (!espera.Avancar(true, Cabeca, 90, Quadro)) quadros++;
            Assert.That(quadros * Quadro, Is.GreaterThanOrEqualTo(EsperaPelaPoseEstavel.SegundosParada));
        }

        [Test]
        public void Espera_GiroLentoDaCabecaContaComoParada()
        {
            var espera = new EsperaPelaPoseEstavel();
            espera.Comecar();
            double olhar = 0;
            bool estavel = false;
            for (int i = 0; i < 64 && !estavel; i++)
            {
                olhar += 30 * Quadro;   // 30 °/s, bem abaixo do limite
                estavel = espera.Avancar(true, Cabeca, olhar, Quadro);
            }
            Assert.That(estavel, Is.True);
        }

        [Test]
        public void Espera_SemPoseNaoConfirma()
        {
            var espera = new EsperaPelaPoseEstavel();
            espera.Comecar();
            for (int i = 0; i < 200; i++) Assert.That(espera.Avancar(false, Cabeca, 0, Quadro), Is.False);
            Assert.That(espera.Esperando, Is.True);
        }

        [Test]
        public void Espera_SemComecarNaoFazNada()
        {
            Assert.That(new EsperaPelaPoseEstavel().Avancar(true, Cabeca, 0, 10), Is.False);
        }

        [Test]
        public void Inicio_ComecaEscuro_RepoeNoEscuro_EClareia()
        {
            var s = new SequenciaDeRecentralizacao();
            s.ComecarNoEscuro();
            Assert.That(s.Opacidade, Is.EqualTo(1));
            Assert.That(s.Fase, Is.EqualTo(FaseDaRecentralizacao.Escura));

            Assert.That(AteRepor(s, true, 2), Is.GreaterThan(0));
            Assert.That(s.Opacidade, Is.EqualTo(1), "repõe com a tela toda escura");
            Assert.That(s.Fase, Is.EqualTo(FaseDaRecentralizacao.Clareando));

            for (int i = 0; i < 20; i++) s.Avancar(true, Cabeca, 0, Quadro);
            Assert.That(s.Opacidade, Is.EqualTo(0));
            Assert.That(s.Ativa, Is.False);
        }

        [Test]
        public void Pedido_EscureceAntesDeRepor()
        {
            var s = new SequenciaDeRecentralizacao();
            s.Pedir();
            Assert.That(s.Fase, Is.EqualTo(FaseDaRecentralizacao.Escurecendo));
            double anterior = 0;
            while (s.Fase == FaseDaRecentralizacao.Escurecendo)
            {
                Assert.That(s.Avancar(true, Cabeca, 0, Quadro), Is.False, "nada se move à vista");
                Assert.That(s.Opacidade, Is.GreaterThan(anterior));
                anterior = s.Opacidade;
            }
            Assert.That(s.Opacidade, Is.EqualTo(1));
            Assert.That(AteRepor(s, true, 2), Is.GreaterThan(0));
        }

        [Test]
        public void SemPose_ClareiaDepoisDoEscuroMaximo_SemRepor_ERefazQuandoAPoseVolta()
        {
            var s = new SequenciaDeRecentralizacao();
            s.ComecarNoEscuro();
            Assert.That(AteRepor(s, false, SequenciaDeRecentralizacao.EscuroMaximo + 0.1), Is.EqualTo(-1));
            Assert.That(s.Fase, Is.EqualTo(FaseDaRecentralizacao.Clareando), "não prende o aluno no escuro");

            for (int i = 0; i < 20; i++) s.Avancar(false, Cabeca, 0, Quadro);
            Assert.That(s.Ativa, Is.False);

            s.Avancar(true, Cabeca, 0, Quadro);
            Assert.That(s.Fase, Is.EqualTo(FaseDaRecentralizacao.Escurecendo), "a pose voltou: refaz");
        }

        [Test]
        public void PoseQueNaoParaDeMexer_RepoeAssimMesmoNoEscuroMaximo()
        {
            var s = new SequenciaDeRecentralizacao();
            s.ComecarNoEscuro();
            double olhar = 0;
            bool repos = false;
            int quadros = 0;
            while (!repos && quadros < 1000)
            {
                olhar += 400 * Quadro;   // gira rápido o tempo todo
                repos = s.Avancar(true, Cabeca, MovimentoDoRig.NormalizarAngulo(olhar), Quadro);
                quadros++;
            }
            Assert.That(repos, Is.True);
            Assert.That(quadros * Quadro, Is.EqualTo(SequenciaDeRecentralizacao.EscuroMaximo).Within(Quadro));
        }

        [Test]
        public void PedidoClareando_VoltaAEscurecerDaOpacidadeAtual()
        {
            var s = new SequenciaDeRecentralizacao();
            s.ComecarNoEscuro();
            AteRepor(s, true, 2);
            s.Avancar(true, Cabeca, 0, Quadro * 2);
            double atual = s.Opacidade;
            Assert.That(atual, Is.LessThan(1).And.GreaterThan(0));

            s.Pedir();
            s.Avancar(true, Cabeca, 0, 0);
            Assert.That(s.Fase, Is.EqualTo(FaseDaRecentralizacao.Escurecendo));
            Assert.That(s.Opacidade, Is.EqualTo(atual).Within(1e-9), "sem salto para o claro");
        }

        [Test]
        public void PedidoNoEscuro_RecomecaAEspera()
        {
            var s = new SequenciaDeRecentralizacao();
            s.ComecarNoEscuro();
            for (int i = 0; i < 20; i++) Assert.That(s.Avancar(true, Cabeca, 0, Quadro), Is.False);
            s.Pedir();
            int quadros = AteRepor(s, true, 2);
            Assert.That(quadros * Quadro, Is.GreaterThanOrEqualTo(EsperaPelaPoseEstavel.SegundosParada));
        }
    }
}
