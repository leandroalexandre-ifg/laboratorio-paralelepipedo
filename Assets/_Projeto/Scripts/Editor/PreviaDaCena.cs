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
    /// como o modo Editor), uma vista da sala de fora e um momento por etapa das Estações 1 (e1-*: abertura, erro, ajuda, face
    /// montada, outra face, pergunta com dica, acerto, fechamento) e 2 (e2-*: passo 1, passo 1 concluído, passo 2 concluído,
    /// pergunta, erro, acerto, fechamento), com as estações conduzidas pelos mesmos métodos que o gatilho chama.
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
            Tocar("AB");
            estacao.TocarVertice("C");
            Salvar(camera, alvo, "e1-2-erro");
            estacao.TocarVertice("H");
            Salvar(camera, alvo, "e1-3-ajuda");
            TerminarAsPiscadas();   // no óculos, o vértice errado pisca por 0,9 s e volta ao normal
            Tocar("FE");
            Salvar(camera, alvo, "e1-4-face-montada");
            estacao.ContinuarAgora();
            Tocar("ADHE");
            Salvar(camera, alvo, "e1-5-outra-face");
            estacao.ContinuarAgora();
            Tocar("ABCD");
            estacao.ContinuarAgora();
            estacao.Responder(estacao.Estacao.Opcoes.ToList().FindIndex(o => o.Texto == "9"));
            Salvar(camera, alvo, "e1-6-pergunta-dica");
            estacao.Responder(Correta());
            Salvar(camera, alvo, "e1-7-acerto");
            estacao.ContinuarAgora();
            estacao.Responder(Correta());
            estacao.ContinuarAgora();
            Salvar(camera, alvo, "e1-8-fechamento");
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
