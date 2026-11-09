using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class ValidadorDeFaceTests
    {
        private ValidadorDeFace _validador;

        [SetUp]
        public void Preparar() => _validador = new ValidadorDeFace(new Paralelepipedo(4, 3, 2));

        // Todas as faces da convenção, na ordem cíclica.
        [TestCase("ABCD")]
        [TestCase("EFGH")]
        [TestCase("ADHE")]
        [TestCase("BCGF")]
        [TestCase("ABFE")]
        [TestCase("DCGH")]
        public void FacesDaConvencao_SaoValidas(string face) => AssertForma(face, face);

        // A → B → F → E e as outras partidas e o sentido inverso.
        [TestCase("ABFE")]
        [TestCase("BFEA")]
        [TestCase("FEAB")]
        [TestCase("EABF")]
        [TestCase("EFBA")]
        [TestCase("AEFB")]
        [TestCase("BAEF")]
        [TestCase("FBAE")]
        public void QualquerPartidaEOsDoisSentidos_IdentificamABFE(string toques) => AssertForma(toques, "ABFE");

        [TestCase("HGFE", "EFGH")]
        [TestCase("DABC", "ABCD")]
        [TestCase("EHDA", "ADHE")]
        [TestCase("GCBF", "BCGF")]
        [TestCase("HGCD", "DCGH")]
        public void OrdemInvertidaOuDeslocada_IdentificaAFace(string toques, string esperada) =>
            AssertForma(toques, esperada);

        [TestCase("ABFEA", "ABFE")]
        [TestCase("EFBAE", "ABFE")]
        [TestCase("CDABC", "ABCD")]
        public void VoltarAoVerticeInicial_TambemVale(string toques, string esperada) => AssertForma(toques, esperada);

        [TestCase("AFBE", Description = "fora de ordem: cruza a face")]
        [TestCase("ABFG", Description = "G não é da face ABFE")]
        [TestCase("ABCG", Description = "não é coplanar")]
        [TestCase("AGCE", Description = "plano diagonal, não é face")]
        [TestCase("ACEG", Description = "diagonais")]
        [TestCase("ABF", Description = "incompleta")]
        [TestCase("ABFEB", Description = "fecha no vértice errado")]
        [TestCase("ABFEAB", Description = "passou de uma volta")]
        [TestCase("AABF", Description = "vértice repetido")]
        [TestCase("ABAB", Description = "vai e volta")]
        [TestCase("", Description = "nenhum toque")]
        public void SequenciasInvalidas_NaoFormamFace(string toques)
        {
            Assert.That(_validador.TentarIdentificar(Toques(toques), out var face), Is.False);
            Assert.That(face, Is.Null);
        }

        [Test]
        public void VerticeInexistente_NaoFormaFace()
        {
            Assert.That(_validador.TentarIdentificar(new[] { "A", "B", "F", "X" }, out _), Is.False);
        }

        [TestCase("")]
        [TestCase("A")]
        [TestCase("AB")]
        [TestCase("ABF")]
        [TestCase("FE")]
        [TestCase("ABFE")]
        [TestCase("ABFEA")]
        public void ToquesQueAindaPodemFormarFace_PodemContinuar(string toques)
        {
            Assert.That(_validador.PodeContinuar(Toques(toques)), Is.True);
        }

        [TestCase("AG", Description = "diagonal do paralelepípedo")]
        [TestCase("AC", Description = "diagonal da base")]
        [TestCase("ABG")]
        [TestCase("AA")]
        [TestCase("ABFEB")]
        public void ToquesQueNaoFormamFace_NaoPodemContinuar(string toques)
        {
            Assert.That(_validador.PodeContinuar(Toques(toques)), Is.False);
        }

        [Test]
        public void SequenciaNula_LancaErro()
        {
            Assert.Throws<ArgumentNullException>(() => _validador.TentarIdentificar(null, out _));
            Assert.Throws<ArgumentNullException>(() => _validador.PodeContinuar(null));
            Assert.Throws<ArgumentNullException>(() => new ValidadorDeFace(null));
        }

        [TestCase("", true)]
        [TestCase("A", true)]
        [TestCase("AB", true)]
        [TestCase("FB", true)]
        [TestCase("EAB", true)]
        [TestCase("ABFE", true)]
        [TestCase("C", false)]
        [TestCase("AF", false)]
        [TestCase("ABC", false)]
        public void PodeContinuar_UmaFaceEspecifica(string toques, bool esperado)
        {
            var face = new Paralelepipedo(4, 3, 2).ObterFace("ABFE");
            Assert.That(_validador.PodeContinuar(face, Toques(toques)), Is.EqualTo(esperado), toques);
            Assert.Throws<ArgumentNullException>(() => _validador.PodeContinuar(null, Toques(toques)));
        }

        private void AssertForma(string toques, string faceEsperada)
        {
            Assert.That(_validador.TentarIdentificar(Toques(toques), out var face), Is.True, toques);
            Assert.That(face.Nome, Is.EqualTo(faceEsperada), toques);
        }

        private static string[] Toques(string letras)
        {
            var nomes = new string[letras.Length];
            for (int i = 0; i < letras.Length; i++) nomes[i] = letras[i].ToString();
            return nomes;
        }
    }
}
