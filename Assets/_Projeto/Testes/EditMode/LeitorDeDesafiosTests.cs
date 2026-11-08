using System;
using System.Linq;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class LeitorDeDesafiosTests
    {
        private static DesafioDto Resposta(string id = "diag-01", string grandeza = "diagonalDaBase",
            string a = "2", string b = "3", string c = "6", string estacao = "diagonais",
            string[] distratores = null, string tolerancia = null) =>
            new DesafioDto
            {
                id = id, estacao = estacao, bncc = "EF09MA14", enunciado = "Quanto mede?",
                dimensoes = new DimensoesDto { a = a, b = b, c = c },
                grandeza = grandeza, distratores = distratores, tolerancia = tolerancia,
            };

        private static DesafioDto MontarFace(string id = "elem-01", string face = "ABFE") =>
            new DesafioDto
            {
                id = id, tipo = "montarFace", estacao = "elementos", bncc = "EF06MA17",
                enunciado = "Monte a face.", dimensoes = new DimensoesDto { a = "4", b = "3", c = "2" }, face = face,
            };

        private static ArquivoDeDesafiosDto Arquivo(params DesafioDto[] desafios) =>
            new ArquivoDeDesafiosDto { versao = 1, desafios = desafios };

        private static DadosInvalidosException Erro(params DesafioDto[] desafios) =>
            Assert.Throws<DadosInvalidosException>(() => LeitorDeDesafios.Ler(Arquivo(desafios)));

        [Test]
        public void DesafioDeResposta_EhLidoComMedidasEmTexto()
        {
            var desafio = (DesafioDeResposta)LeitorDeDesafios.Ler(Arquivo(
                Resposta(distratores: new[] { "5", "√5", "13" }, tolerancia: "0,05")))[0];

            Assert.That(desafio.Tipo, Is.EqualTo(TipoDeDesafio.Resposta));
            Assert.That(desafio.Estacao, Is.EqualTo(Estacao.Diagonais));
            Assert.That(desafio.Grandeza, Is.EqualTo(Grandeza.DiagonalDaBase));
            Assert.That(desafio.RespostaCorreta, Is.EqualTo(Math.Sqrt(13)).Within(1e-9));
            Assert.That(desafio.Distratores.Select(d => d.Texto), Is.EqualTo(new[] { "5", "√5", "13" }));
            Assert.That(desafio.Tolerancia, Is.EqualTo(0.05).Within(1e-12));
        }

        [Test]
        public void TipoAusente_EhResposta()
        {
            Assert.That(LeitorDeDesafios.Ler(Arquivo(Resposta()))[0], Is.InstanceOf<DesafioDeResposta>());
        }

        [Test]
        public void DimensoesIrracionaisEDecimaisComVirgulaOuPonto()
        {
            var desafio = LeitorDeDesafios.Ler(Arquivo(
                Resposta("vol-02", "volume", "2,5", "1.2", "√2", "volume")))[0];

            Assert.That(desafio.Comprimento.Texto, Is.EqualTo("2,5"));
            Assert.That(desafio.Largura.Texto, Is.EqualTo("1,2"));
            Assert.That(desafio.Altura.Texto, Is.EqualTo("√2"));
            Assert.That(desafio.Caixa.Volume, Is.EqualTo(3 * Math.Sqrt(2)).Within(1e-9));
        }

        [Test]
        public void DesafioDeMontarFace_EhLido()
        {
            var desafio = (DesafioDeMontarFace)LeitorDeDesafios.Ler(Arquivo(MontarFace(face: "FEAB")))[0];

            Assert.That(desafio.FaceAlvo.Nome, Is.EqualTo("ABFE"));
            Assert.That(desafio.CodigoBncc, Is.EqualTo("EF06MA17"));
        }

        [TestCase("diagonalDaBase")]
        [TestCase("diagonal")]
        [TestCase("areaDaBase")]
        [TestCase("areaLateral")]
        [TestCase("areaTotal")]
        [TestCase("volume")]
        [TestCase("numeroDeVertices")]
        [TestCase("numeroDeArestas")]
        [TestCase("numeroDeFaces")]
        [TestCase("AreaTotal")]
        public void GrandezasDaListaFechada_SaoAceitas(string grandeza)
        {
            Assert.DoesNotThrow(() => LeitorDeDesafios.Ler(Arquivo(Resposta(grandeza: grandeza))));
        }

        [Test]
        public void ListaFechadaDeGrandezas_TemNoveNomes()
        {
            Assert.That(LeitorDeDesafios.NomesDasGrandezas(), Is.EqualTo(new[]
            {
                "diagonalDaBase", "diagonal", "areaDaBase", "areaLateral", "areaTotal", "volume",
                "numeroDeVertices", "numeroDeArestas", "numeroDeFaces",
            }));
        }

        [TestCase("perimetro")]
        [TestCase("area")]
        [TestCase("diagonal da base")]
        public void GrandezaForaDaLista_DaErroComAsOpcoes(string grandeza)
        {
            var erro = Erro(Resposta(grandeza: grandeza));

            Assert.That(erro.Erros[0], Does.Contain($"grandeza \"{grandeza}\" desconhecida")
                .And.Contain("\"diagonalDaBase\"").And.Contain("\"numeroDeFaces\""));
        }

        [Test]
        public void SemGrandeza_DaErro()
        {
            Assert.That(Erro(Resposta(grandeza: null)).Erros[0], Does.Contain("falta a \"grandeza\""));
        }

        [Test]
        public void TipoDesconhecido_DaErro()
        {
            var dto = Resposta();
            dto.tipo = "escolha";

            Assert.That(Erro(dto).Erros[0], Does.Contain("tipo \"escolha\" desconhecido")
                .And.Contain("\"resposta\"").And.Contain("\"montarFace\""));
        }

        [Test]
        public void EstacaoDesconhecidaOuAusente_DaErro()
        {
            Assert.That(Erro(Resposta(estacao: "planificacao")).Erros[0], Does.Contain("estação \"planificacao\""));
            Assert.That(Erro(Resposta(estacao: null)).Erros[0], Does.Contain("falta a \"estacao\""));
        }

        [Test]
        public void FaceInexistente_DaErro()
        {
            Assert.That(Erro(MontarFace(face: "ABCG")).Erros[0],
                Does.Contain("Desafio \"elem-01\"").And.Contain("não existe na caixa"));
        }

        [Test]
        public void MontarFaceComCamposDeResposta_DaUmErroPorCampo()
        {
            var dto = MontarFace();
            dto.grandeza = "numeroDeFaces";
            dto.distratores = new[] { "4" };
            dto.tolerancia = "0";

            var erro = Erro(dto);

            Assert.That(erro.Erros, Is.EqualTo(new[]
            {
                "Desafio \"elem-01\": o campo \"grandeza\" não pertence a desafios do tipo \"montarFace\".",
                "Desafio \"elem-01\": o campo \"distratores\" não pertence a desafios do tipo \"montarFace\".",
                "Desafio \"elem-01\": o campo \"tolerancia\" não pertence a desafios do tipo \"montarFace\".",
            }));
        }

        [Test]
        public void RespostaComFace_DaErro()
        {
            var dto = Resposta();
            dto.face = "ABFE";

            Assert.That(Erro(dto).Erros[0],
                Does.Contain("o campo \"face\" não pertence a desafios do tipo \"resposta\""));
        }

        [Test]
        public void ListaDeDistratoresVazia_ContaComoAusente()
        {
            var dto = MontarFace();
            dto.distratores = new string[0];

            Assert.DoesNotThrow(() => LeitorDeDesafios.Ler(Arquivo(dto)));
        }

        [Test]
        public void DistratorInvalido_DaErroComAPosicao()
        {
            var erro = Erro(Resposta(distratores: new[] { "5", "raiz de 5" }));

            Assert.That(erro.Erros[0], Does.StartWith("Desafio \"diag-01\": distrator 2:")
                .And.Contain("\"raiz de 5\" não é uma medida válida"));
        }

        [Test]
        public void DistratorIgualARespostaDentroDaTolerancia_DaErro()
        {
            var erro = Erro(Resposta(distratores: new[] { "3,61" }));

            Assert.That(erro.Erros[0], Does.Contain("distrator 1 (3,61) é igual à resposta correta"));
        }

        [Test]
        public void DistratorIgualAoValorExatoEscritoDeOutraForma_DaErro()
        {
            // 2√2 = √8: na caixa 2 × 2 × 1, d = √8.
            var erro = Erro(Resposta(a: "2", b: "2", c: "1", distratores: new[] { "2√2" }));

            Assert.That(erro.Erros[0], Does.Contain("distrator 1 (2√2) é igual à resposta correta"));
        }

        [Test]
        public void ToleranciaInvalida_DaErro()
        {
            Assert.That(Erro(Resposta(tolerancia: "pouca")).Erros[0], Does.Contain("tolerância:"));
        }

        [Test]
        public void ToleranciaZeroNumaContagem_EhAceita()
        {
            var desafio = (DesafioDeResposta)LeitorDeDesafios.Ler(Arquivo(
                Resposta(grandeza: "numeroDeArestas", tolerancia: "0", distratores: new[] { "8", "6" })))[0];

            Assert.That(desafio.Tolerancia, Is.EqualTo(0));
        }

        [Test]
        public void DimensoesAusentesOuInvalidas_DaoErroPorDimensao()
        {
            var dto = Resposta(a: null, b: "2,,5", c: "0");

            var erro = Erro(dto);

            Assert.That(erro.Erros[0], Does.Contain("falta a dimensão a"));
            Assert.That(erro.Erros[1], Does.Contain("dimensão b:").And.Contain("\"2,,5\""));
            Assert.That(erro.Erros.Count, Is.EqualTo(2), "c = 0 só é conferido quando a e b estão certos");
        }

        [Test]
        public void DimensaoZero_DaErro()
        {
            Assert.That(Erro(Resposta(c: "0")).Erros[0], Does.Contain("dimensão c deve ser um número positivo"));
        }

        [Test]
        public void SemDimensoes_DaErroParaAsTres()
        {
            var dto = Resposta();
            dto.dimensoes = null;

            Assert.That(Erro(dto).Erros.Count, Is.EqualTo(3));
        }

        [Test]
        public void EstacaoDeVolumeComDimensaoAcimaDe10_DaErro()
        {
            Assert.That(Erro(Resposta("vol-09", "volume", "12", "3", "3", "volume")).Erros[0],
                Does.Contain("no máximo 10"));
        }

        [Test]
        public void IdAusenteOuRepetido_DaErro()
        {
            var erro = Erro(Resposta(id: ""), Resposta(id: "x"), Resposta(id: "x"));

            Assert.That(erro.Erros, Is.EqualTo(new[]
            {
                "Desafio na posição 1: falta o \"id\".",
                "Desafio \"x\": id repetido (já usado na posição 2).",
            }));
        }

        [Test]
        public void ReuneOsErrosDeTodosOsDesafios()
        {
            var erro = Erro(Resposta(id: "a1", grandeza: "x"), Resposta(id: "ok"), MontarFace(id: "f1", face: "XX"));

            Assert.That(erro.Erros.Count, Is.EqualTo(2));
            Assert.That(erro.Message, Does.StartWith("desafios.json tem 2 erros:"));
        }

        [Test]
        public void ArquivoVazioVersaoErradaOuSemDesafios_DaErro()
        {
            Assert.Throws<DadosInvalidosException>(() => LeitorDeDesafios.Ler(null));
            Assert.Throws<DadosInvalidosException>(() =>
                LeitorDeDesafios.Ler(new ArquivoDeDesafiosDto { versao = 2, desafios = new[] { Resposta() } }));
            Assert.Throws<DadosInvalidosException>(() => LeitorDeDesafios.Ler(Arquivo()));
        }

        [Test]
        public void DesafioNuloNaLista_DaErroComAPosicao()
        {
            Assert.That(Erro(Resposta(), null).Erros[0], Does.Contain("posição 2: está vazio"));
        }
    }
}
