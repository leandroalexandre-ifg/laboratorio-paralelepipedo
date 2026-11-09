using System;
using System.IO;
using System.Linq;
using LaboratorioParalelepipedo.Apresentacao;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor
{
    /// <summary>
    /// Renderiza a cena do laboratório e salva em Logs/previa-*.png: a vista inicial do aluno (origem, olhos a 1,4 m,
    /// como o modo Editor), uma vista da sala de fora e um momento por etapa das Estações 1 (e1-*: abertura, face da frente em
    /// montagem e pronta, erro, ajuda, face ABFE montada, outra face, pergunta com dica, acerto, fechamento) e 2 (e2-*: passo 1, passo 1 concluído, passo 2 concluído,
    /// pergunta, erro, acerto, fechamento) e 3 (e3-*: caixa fechada, abrindo, aberta, pares, pergunta com dica, acerto,
        /// fechamento, fechando, mais a planificação vista de frente), com as estações conduzidas pelos mesmos métodos que o gatilho chama.
    /// Serve para conferir o visual sem o óculos. Nada é salvo na cena. Batchmode (sem -nographics):
    /// -executeMethod LaboratorioParalelepipedo.Editor.PreviaDaCena.Renderizar -quit
    /// </summary>
    public static class PreviaDaCena
    {
        private static readonly Vector3 OlhosDoAluno = new Vector3(0f, 1.4f, 0f);

        private static readonly (string nome, Vector3 posicao, Vector3 alvo)[] Pontos =
        {
            // O aluno começa sempre na origem, olhando para +z (RecentralizacaoDoAluno), a ~0,9 m da caixa.
            ("frente", OlhosDoAluno, new Vector3(0f, 1.3f, 1.4f)),
            // Vista de fora, de trás e acima do aluno, para ver a sala toda (nenhum aluno fica aqui).
            ("sala", new Vector3(1.5f, 1.6f, -1.5f), new Vector3(-0.3f, 1.0f, 1.5f)),
        };

        [MenuItem("Laboratório/Renderizar prévia da cena")]
        public static void Renderizar()
        {
            EditorSceneManager.OpenScene(GeradorDaCena.CaminhoDaCena);
            Directory.CreateDirectory("Logs");

            var objeto = new GameObject("CameraDaPrevia");
            var camera = objeto.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Apresentacao.PaletaDoLaboratorio.Parede;
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            var alvo = new RenderTexture(1600, 1000, 24) { antiAliasing = 4 };
            camera.targetTexture = alvo;
            try
            {
                camera.Render(); // aquecimento: a luz ambiente só fica pronta depois da primeira renderização
                foreach (var (nome, posicao, ponto) in Pontos)
                {
                    objeto.transform.position = posicao;
                    objeto.transform.LookAt(ponto);
                    Salvar(camera, alvo, nome);
                }

                RenderizarAEstacao(camera, alvo);
            }
            finally
            {
                RenderTexture.active = null;
                camera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(alvo);
                UnityEngine.Object.DestroyImmediate(objeto);
                // Volta a cena ao estado salvo: a prévia mexe na caixa e nos painéis.
                EditorSceneManager.OpenScene(GeradorDaCena.CaminhoDaCena);
            }

            Debug.Log("[Prévia] Imagens salvas em Logs/previa-*.png.");
        }

        // Aluno no ponto de partida, com a cabeça virada ~20° à direita, entre a caixa e o painel da estação.
        private static void RenderizarAEstacao(Camera camera, RenderTexture alvo)
        {
            var elementos = UnityEngine.Object.FindFirstObjectByType<EstacaoDosElementosNaCena>();
            var estacao = UnityEngine.Object.FindFirstObjectByType<EstacaoDasDiagonaisNaCena>();
            if (elementos == null || estacao == null)
            {
                Debug.LogError("[Prévia] A cena não tem as Estações 1 e 2.");
                return;
            }

            camera.transform.position = OlhosDoAluno;
            camera.transform.LookAt(new Vector3(0.45f, 1.1f, 1.2f));
            elementos.Inicializar();
            estacao.Inicializar();
            RenderizarAEstacao1(camera, alvo, elementos);

            var opcoes = estacao.Estacao.Opcoes;
            int Indice(Func<Nucleo.Estacoes.OpcaoDeResposta, bool> condicao) => opcoes.ToList().FindIndex(o => condicao(o));

            elementos.IrParaAProximaEstacao();   // o botão "Ir para a Estação 2"
            Salvar(camera, alvo, "e2-1-abertura");
            estacao.TocarVertice("E");
            estacao.TocarVertice("G");
            Salvar(camera, alvo, "e2-2-passo1-concluido");
            estacao.TocarVertice("E");
            estacao.TocarVertice("C");
            UnityEngine.Object.FindFirstObjectByType<DesenhoDasDiagonais>().TerminarSubida();
            Salvar(camera, alvo, "e2-3-passo2-concluido");
            estacao.MostrarOpcoesAgora();
            Salvar(camera, alvo, "e2-4-pergunta");
            int errada = Indice(o => !o.Correta && o.ChaveDaDica == Nucleo.Dados.ChavesDeTexto.DiagonaisDicaSemRaiz);
            estacao.Responder(errada >= 0 ? errada : Indice(o => !o.Correta));
            Salvar(camera, alvo, "e2-5-erro");
            estacao.Responder(Indice(o => o.Correta));
            Salvar(camera, alvo, "e2-6-acerto");
            estacao.ConcluirAgora();
            Salvar(camera, alvo, "e2-7-fechamento");

            var areas = UnityEngine.Object.FindFirstObjectByType<EstacaoDasAreasNaCena>();
            if (areas == null)
            {
                Debug.LogError("[Prévia] A cena não tem a Estação 3.");
                return;
            }
            areas.Inicializar();
            estacao.IrParaAProximaEstacao();   // o botão "Ir para a Estação 3"
            RenderizarAEstacao3(camera, alvo, areas);
        }

        // Estação 3: da caixa fechada ao fechamento, olhando como o aluno (entre a mesa e o painel) e, numa vista
        // extra, de frente para a planificação, da mesma posição inicial.
        private static void RenderizarAEstacao3(Camera camera, RenderTexture alvo, EstacaoDasAreasNaCena estacao)
        {
            var planificacao = UnityEngine.Object.FindFirstObjectByType<PlanificacaoNaCena>();
            int Indice(Func<Nucleo.Estacoes.OpcaoDeResposta, bool> condicao) =>
                estacao.Estacao.Opcoes.ToList().FindIndex(o => condicao(o));

            Salvar(camera, alvo, "e3-1-caixa-fechada");
            estacao.Abrir();
            planificacao.Avancar(PlanificacaoNaCena.DuracaoDaAbertura / 2);
            Salvar(camera, alvo, "e3-2-abrindo");
            estacao.TerminarAnimacao();
            Salvar(camera, alvo, "e3-3-aberta");
            Quaternion comoOAluno = camera.transform.rotation;
            camera.transform.LookAt(new Vector3(0f, 0.95f, 1.0f));
            Salvar(camera, alvo, "e3-3b-aberta-de-frente");
            estacao.TocarFace("ABCD");
            estacao.TocarFace("ADHE");
            Salvar(camera, alvo, "e3-4-dois-pares-de-frente");
            camera.transform.rotation = comoOAluno;
            estacao.TocarFace("ABFE");
            Salvar(camera, alvo, "e3-5-tres-pares");
            estacao.ContinuarAgora();
            int errada = Indice(o => !o.Correta && o.ChaveDaDica == Nucleo.Dados.ChavesDeTexto.AreasDicaSomou);
            estacao.Responder(errada >= 0 ? errada : Indice(o => !o.Correta));
            Salvar(camera, alvo, "e3-6-pergunta-dica");
            estacao.Responder(Indice(o => o.Correta));
            Salvar(camera, alvo, "e3-7-acerto");
            estacao.ContinuarAgora();
            estacao.Responder(Indice(o => o.Correta));
            estacao.ContinuarAgora();
            estacao.Responder(Indice(o => o.Correta));
            Salvar(camera, alvo, "e3-8-acerto-da-total");
            estacao.ContinuarAgora();
            Salvar(camera, alvo, "e3-9-fechamento");
            estacao.Fechar();
            planificacao.Avancar(PlanificacaoNaCena.DuracaoDaAbertura / 2);
            Salvar(camera, alvo, "e3-10-fechando");
            estacao.TerminarAnimacao();
            Salvar(camera, alvo, "e3-11-fechada-de-novo");
        }

        // Estação 1: abertura, erro, ajuda, face montada, outra face, pergunta com dica, acerto e fechamento.
        private static void RenderizarAEstacao1(Camera camera, RenderTexture alvo, EstacaoDosElementosNaCena estacao)
        {
            int Correta() => estacao.Estacao.Opcoes.ToList().FindIndex(o => o.Correta);
            void Tocar(string letras)
            {
                foreach (char letra in letras) estacao.TocarVertice(letra.ToString());
            }

            Salvar(camera, alvo, "e1-1-abertura");
            estacao.Comecar();
            Tocar("ADH");
            Salvar(camera, alvo, "e1-2-montando-a-frente");
            estacao.TocarVertice("E");
            Salvar(camera, alvo, "e1-3-frente-montada");
            estacao.ContinuarAgora();
            Tocar("AB");
            estacao.TocarVertice("C");
            Salvar(camera, alvo, "e1-4-erro");
            estacao.TocarVertice("H");
            Salvar(camera, alvo, "e1-5-ajuda");
            TerminarAsPiscadas();   // no óculos, o vértice errado pisca por 0,9 s e volta ao normal
            Tocar("FE");
            Salvar(camera, alvo, "e1-6-face-montada");
            estacao.ContinuarAgora();
            Tocar("ABCD");   // qualquer face: a de cima, outra cor de par
            Salvar(camera, alvo, "e1-7-outra-face");
            estacao.ContinuarAgora();
            estacao.Responder(estacao.Estacao.Opcoes.ToList().FindIndex(o => o.Texto == "9"));
            Salvar(camera, alvo, "e1-8-pergunta-dica");
            estacao.Responder(Correta());
            Salvar(camera, alvo, "e1-9-acerto");
            estacao.ContinuarAgora();
            estacao.Responder(Correta());
            estacao.ContinuarAgora();
            Salvar(camera, alvo, "e1-10-fechamento");
        }

        private static void TerminarAsPiscadas()
        {
            foreach (var vertice in UnityEngine.Object.FindObjectsByType<AlvoDeVertice>(FindObjectsSortMode.None))
                if (vertice.Piscando) vertice.Avancar(AlvoDeVertice.DuracaoDaPiscada);
        }

        private static void Salvar(Camera camera, RenderTexture alvo, string nome)
        {
            // No Editor parado não há LateUpdate: vira os rótulos e refaz as malhas de texto à mão.
            foreach (var rotulo in UnityEngine.Object.FindObjectsByType<RotuloVoltadoParaOAluno>(FindObjectsSortMode.None))
                rotulo.VirarPara(camera.transform.position);
            foreach (var texto in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
                texto.ForceMeshUpdate();
            // O vértice pedido aparece no auge do pulso (no óculos ele cresce e volta a cada segundo).
            foreach (var vertice in UnityEngine.Object.FindObjectsByType<AlvoDeVertice>(FindObjectsSortMode.None))
                if (vertice.Pulsando)
                {
                    vertice.DefinirPulsando(false);   // recomeça o pulso e vai ao meio do período (o auge)
                    vertice.DefinirPulsando(true);
                    vertice.Avancar(AlvoDeVertice.PeriodoDoPulso / 2);
                }

            camera.Render();
            RenderTexture.active = alvo;
            var imagem = new Texture2D(alvo.width, alvo.height, TextureFormat.RGB24, false);
            imagem.ReadPixels(new Rect(0, 0, alvo.width, alvo.height), 0, 0);
            imagem.Apply();
            File.WriteAllBytes($"Logs/previa-{nome}.png", imagem.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(imagem);
        }
    }
}
