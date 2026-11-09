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
    /// como o modo Editor), uma vista da sala de fora e um momento por etapa da Estação 2 (abertura, passo 1 concluído, passo 2 concluído,
    /// pergunta, erro, acerto, fechamento), com a estação conduzida pelos mesmos métodos que o gatilho chama.
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
            var estacao = UnityEngine.Object.FindFirstObjectByType<EstacaoDasDiagonaisNaCena>();
            if (estacao == null)
            {
                Debug.LogError("[Prévia] A cena não tem a Estação 2.");
                return;
            }

            camera.transform.position = OlhosDoAluno;
            camera.transform.LookAt(new Vector3(0.45f, 1.1f, 1.2f));
            estacao.Inicializar();
            var opcoes = estacao.Estacao.Opcoes;
            int Indice(Func<Nucleo.Estacoes.OpcaoDeResposta, bool> condicao) => opcoes.ToList().FindIndex(o => condicao(o));

            Salvar(camera, alvo, "1-abertura");
            estacao.Comecar();
            estacao.TocarVertice("E");
            estacao.TocarVertice("G");
            Salvar(camera, alvo, "2-passo1-concluido");
            estacao.TocarVertice("E");
            estacao.TocarVertice("C");
            UnityEngine.Object.FindFirstObjectByType<DesenhoDasDiagonais>().TerminarSubida();
            Salvar(camera, alvo, "3-passo2-concluido");
            estacao.MostrarOpcoesAgora();
            Salvar(camera, alvo, "4-pergunta");
            int errada = Indice(o => !o.Correta && o.ChaveDaDica == Nucleo.Dados.ChavesDeTexto.DiagonaisDicaSemRaiz);
            estacao.Responder(errada >= 0 ? errada : Indice(o => !o.Correta));
            Salvar(camera, alvo, "5-erro");
            estacao.Responder(Indice(o => o.Correta));
            Salvar(camera, alvo, "6-acerto");
            estacao.ConcluirAgora();
            Salvar(camera, alvo, "7-fechamento");
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
