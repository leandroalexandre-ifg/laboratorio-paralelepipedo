using System.Linq;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    public class EstacaoDosElementosTests
    {
        private static Medida M(string texto) => InterpretadorDeMedida.Interpretar(texto);

        private static DesafioDeMontarFace Face(string id, string face) =>
            new DesafioDeMontarFace(id, Nucleo.Estacao.Elementos, "EF06MA17", "Monte a face.", 4, 3, 2, face);

        private static DesafioDeResposta Contagem(string id, Grandeza grandeza, params string[] distratores) =>
            new DesafioDeResposta(id, Nucleo.Estacao.Elementos, "EF06MA17", "Quantos?", 4, 3, 2, grandeza,
                distratores.Select(M).ToArray());

        // Os 5 desafios do desafios.json, na mesma ordem.
        private static EstacaoDosElementos Nova(int semente = 0) => new EstacaoDosElementos(new Desafio[]
        {
            Face("elem-01", "ADHE"),
            Face("elem-02", "ABFE"),
            Face("elem-03", "qualquer"),
            Contagem("elem-04", Grandeza.NumeroDeArestas, "8", "6", "9"),
            Contagem("elem-05", Grandeza.NumeroDeFaces, "4", "8", "12"),
        }, semente);

        private static void Tocar(EstacaoDosElementos estacao, string letras)
        {
            foreach (char letra in letras) estacao.TocarVertice(letra.ToString());
        }

        // Primeiro desafio (ADHE, a face da frente) pronto e já no segundo (ABFE).
        private static EstacaoDosElementos NaFaceABFE(int semente = 0)
        {
            var estacao = Nova(semente);
            estacao.Comecar();
            Tocar(estacao, "ADHE");
            estacao.Continuar();
            Assert.That(estacao.Desafio.Id, Is.EqualTo("elem-02"));
            return estacao;
        }

        private static int Indice(EstacaoDosElementos estacao, string texto) =>
            estacao.Opcoes.ToList().FindIndex(o => o.Texto == texto);

        private static int Correta(EstacaoDosElementos estacao) => estacao.Opcoes.ToList().FindIndex(o => o.Correta);

        [Test]
        public void ComecaNaAbertura_EComecarLevaAMontarAPrimeiraFace()
        {
            var estacao = Nova();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Abertura));
            Assert.That(estacao.TocarVertice("A"), Is.EqualTo(ResultadoDoToque.Ignorado), "vértices não valem na abertura");

            estacao.Comecar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.MontarFace));
            Assert.That(estacao.Desafio.Id, Is.EqualTo("elem-01"));
            Assert.That(estacao.Tocados, Is.Empty);
            Assert.That(estacao.VerticesDeAjuda, Is.Empty, "no começo nada pulsa");
        }

        [Test]
        public void PrimeiroDesafio_AFaceDaFrenteADHE()
        {
            var estacao = Nova();
            estacao.Comecar();
            Assert.That(estacao.DesafioDeFace.FaceAlvo.Nome, Is.EqualTo("ADHE"));
            Tocar(estacao, "HDAE");
            Assert.That(estacao.FaceMontada?.Nome, Is.EqualTo("ADHE"));
            estacao.Continuar();
            Assert.That(estacao.DesafioDeFace.FaceAlvo.Nome, Is.EqualTo("ABFE"));
        }

        [TestCase("ABFE")]
        [TestCase("EFBA")]
        [TestCase("FEAB")]
        [TestCase("BAEF")]
        public void FacePedida_QualquerInicioEQualquerSentido(string toques)
        {
            var estacao = NaFaceABFE();
            for (int i = 0; i < 3; i++)
                Assert.That(estacao.TocarVertice(toques[i].ToString()), Is.EqualTo(ResultadoDoToque.Escolheu));
            Assert.That(estacao.TocarVertice(toques[3].ToString()), Is.EqualTo(ResultadoDoToque.Ligou));
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Acertou));
            Assert.That(estacao.FaceMontada.Nome, Is.EqualTo("ABFE"));
        }

        [Test]
        public void VerticeForaDaFace_PiscaEDescartaSoEsseToque()
        {
            var estacao = NaFaceABFE();
            Tocar(estacao, "AB");

            Assert.That(estacao.TocarVertice("C"), Is.EqualTo(ResultadoDoToque.Errado));
            Assert.That(estacao.VerticeErrado, Is.EqualTo("C"));
            Assert.That(estacao.ChaveDoVerticeErrado, Is.EqualTo(ChavesDeTexto.ElementosForaDaFace));
            Assert.That(estacao.Tocados, Is.EqualTo(new[] { "A", "B" }), "os vértices certos ficam");

            Assert.That(estacao.TocarVertice("F"), Is.EqualTo(ResultadoDoToque.Escolheu));
            Assert.That(estacao.VerticeErrado, Is.Null, "o toque certo apaga a dica");
            Assert.That(estacao.TocarVertice("E"), Is.EqualTo(ResultadoDoToque.Ligou));
        }

        [Test]
        public void VerticeDaFaceForaDeOrdem_NaoContinuaAFace()
        {
            var estacao = NaFaceABFE();
            estacao.TocarVertice("A");
            Assert.That(estacao.TocarVertice("F"), Is.EqualTo(ResultadoDoToque.Errado), "F é da face, mas não vizinho de A");
            Assert.That(estacao.ChaveDoVerticeErrado, Is.EqualTo(ChavesDeTexto.ElementosNaoEhFace));
            Assert.That(estacao.Tocados, Is.EqualTo(new[] { "A" }));
        }

        [Test]
        public void TocarDeNovoUmVerticeJaEscolhido_NaoMudaNada()
        {
            var estacao = NaFaceABFE();
            Tocar(estacao, "AB");
            Assert.That(estacao.TocarVertice("A"), Is.EqualTo(ResultadoDoToque.Ignorado));
            Assert.That(estacao.TocarVertice("B"), Is.EqualTo(ResultadoDoToque.Ignorado));
            Assert.That(estacao.Tocados, Is.EqualTo(new[] { "A", "B" }));
            Assert.That(estacao.Erros, Is.Zero);
        }

        [Test]
        public void DoisErrosSeguidos_OsQuatroVerticesDaFacePedidaPulsam()
        {
            var estacao = NaFaceABFE();
            estacao.TocarVertice("C");
            Assert.That(estacao.VerticesDeAjuda, Is.Empty, "um erro ainda não ajuda");
            estacao.TocarVertice("D");
            Assert.That(estacao.VerticesDeAjuda, Is.EquivalentTo(new[] { "A", "B", "F", "E" }));

            estacao.TocarVertice("A");
            Assert.That(estacao.VerticesDeAjuda, Is.EquivalentTo(new[] { "A", "B", "F", "E" }), "a ajuda continua");
            Tocar(estacao, "BFE");
            Assert.That(estacao.VerticesDeAjuda, Is.Empty, "a face pronta não pulsa");
        }

        [Test]
        public void ErrosSeparadosPorUmAcerto_NaoContamComoSeguidos()
        {
            var estacao = NaFaceABFE();
            estacao.TocarVertice("C");
            estacao.TocarVertice("A");
            estacao.TocarVertice("G");
            Assert.That(estacao.ErrosSeguidos, Is.EqualTo(1));
            Assert.That(estacao.VerticesDeAjuda, Is.Empty);
            Assert.That(estacao.Erros, Is.EqualTo(2));
        }

        [Test]
        public void QualquerFace_AceitaUmaFaceDeCadaPar()
        {
            foreach (string toques in new[] { "ABCD", "HGFE", "ADHE", "GCBF", "ABFE", "CDHG" })
            {
                var estacao = NaFaceABFE();
                Tocar(estacao, "ABFE");
                estacao.Continuar();
                Assert.That(estacao.Desafio.Id, Is.EqualTo("elem-03"));
                Tocar(estacao, toques);
                Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Acertou), toques);
                Assert.That(estacao.FaceMontada, Is.Not.Null, toques);
            }
        }

        [Test]
        public void QualquerFace_VerticeQueNaoContinuaNenhuma_EAAjudaSegueOQueJaFoiTocado()
        {
            var estacao = NaFaceABFE();
            Tocar(estacao, "ABFE");
            estacao.Continuar();

            Tocar(estacao, "AB");
            Assert.That(estacao.TocarVertice("H"), Is.EqualTo(ResultadoDoToque.Errado), "A–B–H não segue nenhuma face");
            Assert.That(estacao.ChaveDoVerticeErrado, Is.EqualTo(ChavesDeTexto.ElementosNaoEhFace));
            estacao.TocarVertice("G");
            Assert.That(estacao.VerticesDeAjuda, Has.Member("A").And.Member("B"), "a face da ajuda contém A e B");
            Assert.That(estacao.VerticesDeAjuda, Has.Count.EqualTo(4));
        }

        [Test]
        public void Pergunta_OpcoesComARespostaCalculadaEOsDistratores_Embaralhadas()
        {
            var estacao = NaFaceABFE();
            Tocar(estacao, "ABFE");
            estacao.Continuar();
            Tocar(estacao, "ABCD");
            estacao.Continuar();

            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Pergunta));
            Assert.That(estacao.Opcoes.Select(o => o.Texto), Is.EquivalentTo(new[] { "12", "8", "6", "9" }));
            Assert.That(estacao.Opcoes.Single(o => o.Correta).Texto, Is.EqualTo("12"));
            Assert.That(estacao.TocarVertice("A"), Is.EqualTo(ResultadoDoToque.Ignorado), "vértices não valem na pergunta");
        }

        [Test]
        public void MesmaSemente_MesmaOrdem()
        {
            EstacaoDosElementos NaPergunta(int semente)
            {
                var estacao = NaFaceABFE(semente);
                Tocar(estacao, "ABFE");
                estacao.Continuar();
                Tocar(estacao, "ABCD");
                estacao.Continuar();
                return estacao;
            }

            Assert.That(NaPergunta(3).Opcoes.Select(o => o.Texto), Is.EqualTo(NaPergunta(3).Opcoes.Select(o => o.Texto)));
        }

        [TestCase("8", ChavesDeTexto.ElementosDicaVertices)]
        [TestCase("6", ChavesDeTexto.ElementosDicaFaces)]
        [TestCase("9", ChavesDeTexto.ElementosDicaArestasEscondidas)]
        public void Arestas_CadaDistratorTemADicaDoErroQueImita(string distrator, string chave)
        {
            var estacao = NaFaceABFE();
            Tocar(estacao, "ABFE");
            estacao.Continuar();
            Tocar(estacao, "ABCD");
            estacao.Continuar();

            Assert.That(estacao.Responder(Indice(estacao, distrator)), Is.False);
            Assert.That(estacao.ChaveDaDica, Is.EqualTo(chave));
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Pergunta), "o erro fica na pergunta");
        }

        [TestCase("4", ChavesDeTexto.ElementosDicaSemAsBases)]
        [TestCase("8", ChavesDeTexto.ElementosDicaVertices)]
        [TestCase("12", ChavesDeTexto.ElementosDicaArestas)]
        public void Faces_CadaDistratorTemADicaDoErroQueImita(string distrator, string chave)
        {
            var estacao = NaUltimaPergunta();
            Assert.That(estacao.Responder(Indice(estacao, distrator)), Is.False);
            Assert.That(estacao.ChaveDaDica, Is.EqualTo(chave));
        }

        private static EstacaoDosElementos NaUltimaPergunta()
        {
            var estacao = NaFaceABFE();
            Tocar(estacao, "ABFE");
            estacao.Continuar();
            Tocar(estacao, "ABCD");
            estacao.Continuar();
            estacao.Responder(Correta(estacao));
            estacao.Continuar();
            Assert.That(estacao.Desafio.Id, Is.EqualTo("elem-05"));
            return estacao;
        }

        [Test]
        public void UltimoAcerto_LevaAoFechamento_ERefazerVoltaAoPrimeiroDesafio()
        {
            var estacao = NaUltimaPergunta();
            estacao.Responder(Indice(estacao, "4"));
            Assert.That(estacao.Responder(Correta(estacao)), Is.True);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Acertou));
            Assert.That(estacao.ChaveDaDica, Is.Null);
            Assert.That(estacao.NoUltimoDesafio, Is.True);

            estacao.Continuar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Fechamento));

            estacao.Refazer();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.MontarFace));
            Assert.That(estacao.Desafio.Id, Is.EqualTo("elem-01"));
            Assert.That(estacao.Erros, Is.Zero);
            Assert.That(estacao.Tocados, Is.Empty);
        }

        [Test]
        public void ForaDaEtapa_ComandosNaoFazemNada()
        {
            var estacao = Nova();
            estacao.Continuar();
            estacao.Refazer();
            Assert.That(estacao.Responder(0), Is.False);
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.Abertura));

            estacao.Comecar();
            estacao.Comecar();
            estacao.Refazer();
            estacao.Continuar();
            Assert.That(estacao.Etapa, Is.EqualTo(EtapaDosElementos.MontarFace));
            Assert.That(estacao.IndiceDoDesafio, Is.Zero);
        }

        [Test]
        public void DesafiosDaEstacao_SoMontarFaceEContagensDaEstacaoDeElementos_NaOrdem()
        {
            var todos = new Desafio[]
            {
                Contagem("c1", Grandeza.NumeroDeVertices, "6", "12", "4"),
                new DesafioDeResposta("d1", Nucleo.Estacao.Diagonais, "EF09MA14", "?", 2, 3, 6, Grandeza.Diagonal, new[] { M("11") }),
                new DesafioDeResposta("a1", Nucleo.Estacao.Elementos, "EF06MA17", "?", 4, 3, 2, Grandeza.AreaTotal, new[] { M("24") }),
                Face("f1", "ABFE"),
            };
            Assert.That(EstacaoDosElementos.DesafiosDaEstacao(todos).Select(d => d.Id), Is.EqualTo(new[] { "c1", "f1" }));
            Assert.That(EstacaoDosElementos.DesafiosDaEstacao(null), Is.Empty);
        }

        [Test]
        public void SemDesafios_OuComDesafioDeOutraEstacao_Recusa()
        {
            Assert.Throws<System.ArgumentException>(() => new EstacaoDosElementos(new Desafio[0]));
            Assert.Throws<System.ArgumentNullException>(() => new EstacaoDosElementos(null));
            Assert.Throws<System.ArgumentException>(() => new EstacaoDosElementos(new Desafio[]
            {
                new DesafioDeResposta("d1", Nucleo.Estacao.Diagonais, "EF09MA14", "?", 2, 3, 6, Grandeza.Diagonal, new[] { M("11") }),
            }));
        }

        [Test]
        public void DesafiosDoJson_AEstacaoUsaOsCinco_ComecandoPelaFaceDaFrente()
        {
            var desafios = Apresentacao.CarregadorDeDados.CarregarDesafios(
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>("Assets/_Projeto/Dados/desafios.json"));
            Assert.That(EstacaoDosElementos.DesafiosDaEstacao(desafios).Select(d => d.Id),
                Is.EqualTo(new[] { "elem-01", "elem-02", "elem-03", "elem-04", "elem-05" }));
        }

        [Test]
        public void DicaDaContagem_VerticesComDistratoresDeArestasEFaces()
        {
            var caixa = new Paralelepipedo(4, 3, 2);
            Assert.That(DicaDaContagem.Chave(caixa, Grandeza.NumeroDeVertices, 12), Is.EqualTo(ChavesDeTexto.ElementosDicaArestas));
            Assert.That(DicaDaContagem.Chave(caixa, Grandeza.NumeroDeVertices, 6), Is.EqualTo(ChavesDeTexto.ElementosDicaFaces));
            Assert.That(DicaDaContagem.Chave(caixa, Grandeza.NumeroDeVertices, 7), Is.EqualTo(ChavesDeTexto.ElementosDicaGeral));
            Assert.That(DicaDaContagem.Chave(caixa, Grandeza.NumeroDeFaces, 5), Is.EqualTo(ChavesDeTexto.ElementosDicaGeral));
        }
    }
}
