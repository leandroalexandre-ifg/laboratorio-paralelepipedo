using System;
using LaboratorioParalelepipedo.Nucleo;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class InterpretadorDeMedidaTests
    {
        private const double Eps = 1e-12;

        [TestCase("2", 2, "2")]
        [TestCase("12", 12, "12")]
        [TestCase("0", 0, "0")]
        [TestCase(" 6 ", 6, "6")]
        public void Inteiros(string texto, double valor, string rotulo) => AssertMedida(texto, valor, rotulo);

        [TestCase("2,5", 2.5, "2,5")]
        [TestCase("2.5", 2.5, "2,5")]
        [TestCase("1,2", 1.2, "1,2")]
        [TestCase("0.05", 0.05, "0,05")]
        [TestCase("3,60", 3.6, "3,6")]
        [TestCase("2.500000", 2.5, "2,5")]
        public void DecimaisComVirgulaOuPonto(string texto, double valor, string rotulo) =>
            AssertMedida(texto, valor, rotulo);

        [Test]
        public void VirgulaEPonto_DaoOMesmoValor()
        {
            Assert.That(InterpretadorDeMedida.Interpretar("2,5").Valor,
                Is.EqualTo(InterpretadorDeMedida.Interpretar("2.5").Valor));
        }

        [TestCase("5/2", 2.5, "5/2")]
        [TestCase("1/3", 1.0 / 3, "1/3")]
        [TestCase("12 / 4", 3, "12/4")]
        public void Fracoes(string texto, double valor, string rotulo) => AssertMedida(texto, valor, rotulo);

        [TestCase("√2", 1.4142135623730951, "√2")]
        [TestCase("√13", 3.6055512754639891, "√13")]
        [TestCase("√(13)", 3.6055512754639891, "√13")]
        [TestCase("√ 2", 1.4142135623730951, "√2")]
        [TestCase("√2,25", 1.5, "√2,25")]
        public void Raizes(string texto, double valor, string rotulo) => AssertMedida(texto, valor, rotulo);

        [TestCase("3√2", 3 * 1.4142135623730951, "3√2")]
        [TestCase("2√3", 2 * 1.7320508075688772, "2√3")]
        [TestCase("3 √ 2", 3 * 1.4142135623730951, "3√2")]
        [TestCase("2,5√2", 2.5 * 1.4142135623730951, "2,5√2")]
        [TestCase("2.5√2", 2.5 * 1.4142135623730951, "2,5√2")]
        [TestCase("2√(2)", 2 * 1.4142135623730951, "2√2")]
        public void MultiplosDeRaiz(string texto, double valor, string rotulo) => AssertMedida(texto, valor, rotulo);

        [TestCase("abc")]
        [TestCase("2,,5")]
        [TestCase("2,5.1")]
        [TestCase("1.000,5")]
        [TestCase(",5")]
        [TestCase("2,")]
        [TestCase("1e3")]
        [TestCase("√")]
        [TestCase("3√")]
        [TestCase("√√2")]
        [TestCase("2√√2")]
        [TestCase("√2/2")]
        [TestCase("2/3√2")]
        [TestCase("5/2/3")]
        [TestCase("2,5/2")]
        [TestCase("√(2")]
        [TestCase("raiz de 2")]
        [TestCase("∞")]
        [TestCase("٢", Description = "dígito arábico")]
        public void TextosInvalidos_DaoErroComExemplos(string texto)
        {
            AssertErro(texto, "não é uma medida válida");
            AssertErro(texto, InterpretadorDeMedida.Exemplos);
        }

        [TestCase("-2")]
        [TestCase("−2")]
        [TestCase("-√2")]
        public void Negativas_DaoErroEspecifico(string texto) => AssertErro(texto, "não podem ser negativas");

        [TestCase("5/0")]
        [TestCase("0/0")]
        public void DenominadorZero_DaErroEspecifico(string texto) => AssertErro(texto, "denominador");

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Vazia_DaErro(string texto) => AssertErro(texto, "vazia");

        [Test]
        public void NumeroGrandeDemais_DaErro()
        {
            AssertErro(new string('9', 400), "grande demais");
        }

        [Test]
        public void Interpretar_LancaFormatExceptionComAMesmaMensagem()
        {
            var erro = Assert.Throws<FormatException>(() => InterpretadorDeMedida.Interpretar("2,,5"));
            Assert.That(erro.Message, Does.Contain("\"2,,5\""));
        }

        [Test]
        public void MedidaDeNumero_TemTextoComVirgula()
        {
            Medida medida = 2.5;

            Assert.That(medida.Valor, Is.EqualTo(2.5));
            Assert.That(medida.Texto, Is.EqualTo("2,5"));
        }

        private static void AssertMedida(string texto, double valor, string rotulo)
        {
            Assert.That(InterpretadorDeMedida.TentarInterpretar(texto, out var medida, out var erro), Is.True,
                $"{texto}: {erro}");
            Assert.That(medida.Valor, Is.EqualTo(valor).Within(Eps), texto);
            Assert.That(medida.Texto, Is.EqualTo(rotulo), texto);
            Assert.That(erro, Is.Null);
        }

        private static void AssertErro(string texto, string trecho)
        {
            Assert.That(InterpretadorDeMedida.TentarInterpretar(texto, out _, out var erro), Is.False, texto);
            Assert.That(erro, Does.Contain(trecho), texto);
        }
    }
}
