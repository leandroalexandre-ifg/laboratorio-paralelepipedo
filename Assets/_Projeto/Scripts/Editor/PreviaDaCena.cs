using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor
{
    /// <summary>
    /// Renderiza a cena do laboratório vista da altura dos olhos de um aluno de pé e salva em Logs/previa-*.png.
    /// Serve para conferir o visual sem o óculos. Batchmode (sem -nographics):
    /// -executeMethod LaboratorioParalelepipedo.Editor.PreviaDaCena.Renderizar -quit
    /// </summary>
    public static class PreviaDaCena
    {
        private static readonly (string nome, Vector3 posicao, Vector3 alvo)[] Pontos =
        {
            ("frente", new Vector3(0f, 1.35f, 0f), new Vector3(0f, 1.0f, 1.4f)),
            ("caixa", new Vector3(0.25f, 1.15f, 0.45f), new Vector3(0f, 0.86f, 1.0f)),
            ("sala", new Vector3(1.5f, 1.6f, -1.5f), new Vector3(-0.3f, 1.0f, 1.5f)),
            ("dentro", new Vector3(0.12f, 0.84f, 0.93f), new Vector3(-0.2f, 0.9f, 1.15f)),
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
                    camera.Render();
                    RenderTexture.active = alvo;
                    var imagem = new Texture2D(alvo.width, alvo.height, TextureFormat.RGB24, false);
                    imagem.ReadPixels(new Rect(0, 0, alvo.width, alvo.height), 0, 0);
                    imagem.Apply();
                    File.WriteAllBytes($"Logs/previa-{nome}.png", imagem.EncodeToPNG());
                    Object.DestroyImmediate(imagem);
                }
            }
            finally
            {
                RenderTexture.active = null;
                camera.targetTexture = null;
                Object.DestroyImmediate(alvo);
                Object.DestroyImmediate(objeto);
            }

            Debug.Log("[Prévia] Imagens salvas em Logs/previa-*.png.");
        }
    }
}
