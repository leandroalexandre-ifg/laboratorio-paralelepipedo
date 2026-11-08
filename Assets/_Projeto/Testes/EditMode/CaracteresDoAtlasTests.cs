using LaboratorioParalelepipedo.Nucleo.Dados;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class CaracteresDoAtlasTests
    {
        [Test]
        public void JuntaExtrasEValoresDeTexto_SemRepetir_EmOrdem()
        {
            string caracteres = CaracteresDoAtlas.Coletar("ba", "{ \"t\": [\"cã\", { \"x\": \"√a\" }] }");

            Assert.That(caracteres, Is.EqualTo("abcã√"));
        }

        [Test]
        public void IgnoraAsChavesDoJson()
        {
            Assert.That(CaracteresDoAtlas.Coletar("", "{ \"chave\": \"x\" }"), Is.EqualTo("x"));
        }

        [Test]
        public void IncluiNumerosSemAspas()
        {
            Assert.That(CaracteresDoAtlas.Coletar("", "{ \"a\": 2.5 }"), Is.EqualTo(".25"));
        }

        [Test]
        public void IgnoraCaracteresDeControle()
        {
            Assert.That(CaracteresDoAtlas.Coletar("a\n\tb\r", "{ \"a\": \"c\\nd\" }"), Is.EqualTo("abcd"));
        }

        [Test]
        public void JsonMalFormado_DaErroClaro()
        {
            var erro = Assert.Throws<DadosInvalidosException>(() => CaracteresDoAtlas.Coletar("", "{ \"a\": }"));

            Assert.That(erro.Message, Does.Contain("mal formado"));
        }

        [TestCase("D² = d² + c²", "abcdD =+", "²")]
        [TestCase("φ φ ψ", "", " φψ")]
        [TestCase("abc", "abc", "")]
        [TestCase("a\nb", "ab", "")]
        public void QueFaltam_ListaCadaCaractereUmaVez(string texto, string atlas, string esperado)
        {
            Assert.That(CaracteresDoAtlas.QueFaltam(texto, atlas), Is.EqualTo(esperado));
        }
    }
}
