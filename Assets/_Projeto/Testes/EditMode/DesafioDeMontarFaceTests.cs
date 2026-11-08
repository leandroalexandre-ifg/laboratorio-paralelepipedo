using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class DesafioDeMontarFaceTests
    {
        private static DesafioDeMontarFace Criar(string face, string id = "elem-01") =>
            new DesafioDeMontarFace(id, Estacao.Elementos, "EF06MA17", "Monte a face pedida.", 4, 3, 2, face);

        private static string[] Toques(string letras)
        {
            var nomes = new string[letras.Length];
            for (int i = 0; i < letras.Length; i++) nomes[i] = letras[i].ToString();
            return nomes;
        }

        [TestCase("ABFE")]
        [TestCase("FEAB")]
        [TestCase("EFBA")]
        [TestCase("abfe")]
        [TestCase(" A B F E ")]
        [TestCase("A-B-F-E")]
        public void FaceEmQualquerOrdemCiclica_IdentificaAFaceDaConvencao(string face)
        {
            var desafio = Criar(face);

            Assert.That(desafio.Tipo, Is.EqualTo(TipoDeDesafio.MontarFace));
            Assert.That(desafio.FaceAlvo.Nome, Is.EqualTo("ABFE"));
            Assert.That(desafio.AceitaQualquerFace, Is.False);
        }

        [TestCase("qualquer")]
        [TestCase("Qualquer")]
        [TestCase(" QUALQUER ")]
        public void Qualquer_AceitaTodasAsFaces(string face)
        {
            var desafio = Criar(face);

            Assert.That(desafio.AceitaQualquerFace, Is.True);
            Assert.That(desafio.FaceAlvo, Is.Null);
        }

        [TestCase("AFBE")]
        [TestCase("ABCG")]
        [TestCase("ABF")]
        [TestCase("XYZW")]
        [TestCase("frente")]
        public void FaceInexistente_DaErroClaro(string face)
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() => Criar(face));

            Assert.That(erro.Message, Does.Contain("elem-01").And.Contain("não existe na caixa"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void SemFace_DaErroClaro(string face)
        {
            var erro = Assert.Throws<DesafioInvalidoException>(() => Criar(face));

            Assert.That(erro.Message, Does.Contain("falta a face"));
        }

        [TestCase("ABFE")]
        [TestCase("BAEF")]
        [TestCase("ABFEA")]
        public void MontarAFacePedida_EhCerto(string toques)
        {
            var resultado = AvaliadorDeResposta.Avaliar(Criar("ABFE"), Toques(toques));

            Assert.That(resultado.Correta, Is.True);
            Assert.That(resultado.FaceMontada.Nome, Is.EqualTo("ABFE"));
        }

        [Test]
        public void MontarOutraFace_EhErradoMasInformaAFaceMontada()
        {
            var resultado = AvaliadorDeResposta.Avaliar(Criar("ABFE"), Toques("ABCD"));

            Assert.That(resultado.Correta, Is.False);
            Assert.That(resultado.FormouAlgumaFace, Is.True);
            Assert.That(resultado.FaceMontada.Nome, Is.EqualTo("ABCD"));
        }

        [TestCase("AFBE")]
        [TestCase("ABF")]
        [TestCase("")]
        public void ToquesQueNaoFormamFace_SaoErrados(string toques)
        {
            var resultado = AvaliadorDeResposta.Avaliar(Criar("ABFE"), Toques(toques));

            Assert.That(resultado.Correta, Is.False);
            Assert.That(resultado.FormouAlgumaFace, Is.False);
        }

        [TestCase("ABCD")]
        [TestCase("HGFE")]
        [TestCase("DCGH")]
        public void Qualquer_AceitaQualquerFaceMontada(string toques)
        {
            Assert.That(AvaliadorDeResposta.Avaliar(Criar("qualquer"), Toques(toques)).Correta, Is.True);
        }

        [Test]
        public void Qualquer_RecusaToquesQueNaoFormamFace()
        {
            Assert.That(AvaliadorDeResposta.Avaliar(Criar("qualquer"), Toques("ACGE")).Correta, Is.False);
        }

        [Test]
        public void ToquesNulos_LancaErro()
        {
            Assert.Throws<ArgumentNullException>(() => AvaliadorDeResposta.Avaliar(Criar("ABFE"), null));
            Assert.Throws<ArgumentNullException>(() =>
                AvaliadorDeResposta.Avaliar((DesafioDeMontarFace)null, Toques("ABFE")));
        }

        [Test]
        public void ValidacoesDaBase_ValemTambem()
        {
            Assert.Throws<DesafioInvalidoException>(() => Criar("ABFE", id: ""));
            Assert.Throws<DesafioInvalidoException>(() =>
                new DesafioDeMontarFace("x", Estacao.Elementos, "EF06MA17", "?", 0, 3, 2, "ABFE"));
        }
    }
}
