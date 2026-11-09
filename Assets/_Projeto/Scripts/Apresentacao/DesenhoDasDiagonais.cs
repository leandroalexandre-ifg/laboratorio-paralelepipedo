using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Desenho da Estação 2 dentro da caixa (filho dela: anda e gira junto quando a caixa é pega).
    /// Base EFGH destacada no passo 1. Passo 1 concluído: segmento d (azul) de E a G, triângulo EHG preenchido, marca de
    /// ângulo reto em H e os rótulos a, b e d. Passo 2: segmento D (laranja) de E a C e o triângulo EGC, que "se levanta"
    /// de dentro da caixa em <see cref="DuracaoDaSubida"/> s (C sobe de G até o lugar), com a marca de ângulo reto em G
    /// e os rótulos c e D no fim. As malhas são criadas só ao trocar de caixa; na subida, só os vértices mudam.
    /// </summary>
    public sealed class DesenhoDasDiagonais : MonoBehaviour
    {
        public const string NomeDoGrupo = "Diagonais";
        public const float DuracaoDaSubida = 0.6f;
        private const float Espessura = 0.01f;
        private const float LadoDaMarca = 0.045f;
        private const float EspessuraDaMarca = 0.008f;
        // Folga entre a aresta e o começo do rótulo de medida (o rótulo se afasta mais a metade da própria largura).
        private const float FolgaDaMedida = 0.03f;

        [SerializeField] private VisualDaCaixa _visual;
        [SerializeField] private Material _linhaDaBase;
        [SerializeField] private Material _linhaDaDiagonal;
        [SerializeField] private Material _trianguloDaBase;
        [SerializeField] private Material _trianguloLevantado;
        [SerializeField] private Material _baseDestacada;
        [SerializeField] private Material _marca;
        [SerializeField] private TMP_FontAsset _fonte;
        [SerializeField] private float _tamanhoDoRotulo = 0.4f;

        private readonly TransicaoSuave _subida = new TransicaoSuave(DuracaoDaSubida);
        private readonly Vector3[] _pontosDoLevantado = new Vector3[3];
        private Transform _grupo;
        private GameObject _base, _passo1, _passo2, _fimDoPasso2;
        private Transform _segmentoGrande;
        private Mesh _malhaDoLevantado, _malhaDaBase, _malhaDoTrianguloDaBase;
        private Vector3 _e, _g, _c;

        public GameObject Base => _base;
        public GameObject Passo1 => _passo1;
        public GameObject Passo2 => _passo2;

        /// <summary>Se o triângulo EGC ainda está subindo.</summary>
        public bool Subindo => _subida.Ativa;

        /// <summary>Liga a aparência (usado pelo gerador da cena).</summary>
        public void Configurar(VisualDaCaixa visual, Material linhaDaBase, Material linhaDaDiagonal, Material trianguloDaBase,
            Material trianguloLevantado, Material baseDestacada, Material marca, TMP_FontAsset fonte)
        {
            _visual = visual;
            _linhaDaBase = linhaDaBase;
            _linhaDaDiagonal = linhaDaDiagonal;
            _trianguloDaBase = trianguloDaBase;
            _trianguloLevantado = trianguloLevantado;
            _baseDestacada = baseDestacada;
            _marca = marca;
            _fonte = fonte;
        }

        /// <summary>
        /// Monta o desenho para a caixa que o <see cref="VisualDaCaixa"/> acabou de construir, tudo escondido.
        /// Os rótulos vêm prontos ("a = 2", "d", "D").
        /// </summary>
        public void Construir(string rotuloA, string rotuloB, string rotuloC, string rotuloD, string rotuloDaDiagonal)
        {
            Limpar();
            IPrisma prisma = _visual.Prisma;
            Vector3 P(string nome) => _visual.PosicaoNaCena(((Paralelepipedo)prisma).ObterVertice(nome).Posicao);
            _e = P("E");
            Vector3 f = P("F");
            _g = P("G");
            Vector3 h = P("H");
            _c = P("C");
            Vector3 centro = (_e + _g) / 2;

            _grupo = new GameObject(NomeDoGrupo).transform;
            _grupo.SetParent(transform, false);

            // Base EFGH, rente ao fundo.
            _base = Grupo("BaseDestacada");
            Vector3 acima = Vector3.up * 0.002f;
            _malhaDaBase = Malha("Base EFGH", new[] { _e + acima, f + acima, _g + acima, h + acima }, new[] { 0, 1, 2, 0, 2, 3 });
            Desenhar(_base.transform, "Base EFGH", _malhaDaBase, _baseDestacada);

            // Passo 1: d, triângulo EHG, ângulo reto em H, rótulos a, b, d.
            _passo1 = Grupo("Passo1");
            Segmento(_passo1.transform, "Segmento d", _e, _g, _linhaDaBase);
            Vector3 levinho = Vector3.up * 0.004f;
            _malhaDoTrianguloDaBase = Malha("Triangulo EHG", new[] { _e + levinho, h + levinho, _g + levinho }, new[] { 0, 1, 2 });
            Desenhar(_passo1.transform, "Triangulo EHG", _malhaDoTrianguloDaBase, _trianguloDaBase);
            MarcaDeAnguloReto(_passo1.transform, "Angulo reto em H", h + levinho, (_e - h).normalized, (_g - h).normalized);
            RotuloDeMedida(_passo1.transform, "Rotulo a", (_e + h) / 2, centro, rotuloA);
            RotuloDeMedida(_passo1.transform, "Rotulo b", (h + _g) / 2, centro, rotuloB);
            Rotulo(_passo1.transform, "Rotulo d", centro + Vector3.up * 0.03f, rotuloD, PaletaDoLaboratorio.DestaqueAzul);

            // Passo 2: D e o triângulo EGC (C sobe de G); no fim, ângulo reto em G e rótulos c, D.
            _passo2 = Grupo("Passo2");
            _segmentoGrande = Segmento(_passo2.transform, "Segmento D", _e, _c, _linhaDaDiagonal);
            _pontosDoLevantado[0] = _e;
            _pontosDoLevantado[1] = _g;
            _pontosDoLevantado[2] = _c;
            _malhaDoLevantado = Malha("Triangulo EGC", _pontosDoLevantado, new[] { 0, 1, 2 });
            _malhaDoLevantado.MarkDynamic();
            Desenhar(_passo2.transform, "Triangulo EGC", _malhaDoLevantado, _trianguloLevantado);
            _fimDoPasso2 = new GameObject("FimDoPasso2");
            _fimDoPasso2.transform.SetParent(_passo2.transform, false);
            MarcaDeAnguloReto(_fimDoPasso2.transform, "Angulo reto em G", _g, (_e - _g).normalized, Vector3.up);
            Vector3 meioDeGC = (_g + _c) / 2;
            RotuloDeMedida(_fimDoPasso2.transform, "Rotulo c", meioDeGC, new Vector3(centro.x, meioDeGC.y, centro.z), rotuloC);
            Rotulo(_fimDoPasso2.transform, "Rotulo D", (_e + _c) / 2 + Vector3.up * 0.03f, rotuloDaDiagonal,
                PaletaDoLaboratorio.DestaqueLaranja);

            MostrarBase(false);
            MostrarPasso1(false);
            MostrarPasso2(false, false);
        }

        public void MostrarBase(bool visivel) => Ativar(_base, visivel);

        public void MostrarPasso1(bool visivel) => Ativar(_passo1, visivel);

        /// <summary>Mostra o passo 2; com <paramref name="animar"/>, o triângulo EGC se levanta.</summary>
        public void MostrarPasso2(bool visivel, bool animar)
        {
            Ativar(_passo2, visivel);
            if (!visivel)
            {
                _subida.Parar();
                return;
            }

            if (animar)
            {
                _subida.Iniciar();
                Levantar(0f);
            }
            else
            {
                _subida.Parar();
                Levantar(1f);
            }
        }

        /// <summary>Termina a subida agora (para a prévia e os testes).</summary>
        public void TerminarSubida()
        {
            if (!_subida.Ativa) return;
            _subida.Parar();
            Levantar(1f);
        }

        private void Update() => Avancar(Time.deltaTime);

        /// <summary>Avança a subida (público para os testes no Editor).</summary>
        public void Avancar(float segundos)
        {
            if (!_subida.Ativa) return;
            float fracao = (float)_subida.Avancar(segundos);
            Levantar(_subida.Ativa ? fracao : 1f);
        }

        private void Levantar(float fracao)
        {
            if (_malhaDoLevantado == null) return;
            Vector3 c = Vector3.LerpUnclamped(_g, _c, fracao);
            _pontosDoLevantado[2] = c;
            _malhaDoLevantado.vertices = _pontosDoLevantado;
            _malhaDoLevantado.RecalculateBounds();
            Posicionar(_segmentoGrande, _e, c);
            Ativar(_fimDoPasso2, fracao >= 1f);
        }

        private void OnDestroy() => Limpar();

        private void Limpar()
        {
            if (_grupo != null) Destruir(_grupo.gameObject);
            _grupo = null;
            Destruir(_malhaDaBase);
            Destruir(_malhaDoTrianguloDaBase);
            Destruir(_malhaDoLevantado);
            _malhaDaBase = _malhaDoTrianguloDaBase = _malhaDoLevantado = null;
        }

        private GameObject Grupo(string nome)
        {
            var objeto = new GameObject(nome);
            objeto.transform.SetParent(_grupo, false);
            return objeto;
        }

        private static void Ativar(GameObject objeto, bool ativo)
        {
            if (objeto != null && objeto.activeSelf != ativo) objeto.SetActive(ativo);
        }

        private static Mesh Malha(string nome, Vector3[] pontos, int[] frente)
        {
            // As duas faces: o triângulo aparece de qualquer lado.
            var triangulos = new int[frente.Length * 2];
            for (int i = 0; i < frente.Length; i += 3)
            {
                triangulos[i] = frente[i];
                triangulos[i + 1] = frente[i + 1];
                triangulos[i + 2] = frente[i + 2];
                triangulos[frente.Length + i] = frente[i];
                triangulos[frente.Length + i + 1] = frente[i + 2];
                triangulos[frente.Length + i + 2] = frente[i + 1];
            }

            var malha = new Mesh { name = nome, vertices = (Vector3[])pontos.Clone(), triangles = triangulos };
            malha.RecalculateNormals();
            malha.RecalculateBounds();
            return malha;
        }

        private static Renderer Desenhar(Transform pai, string nome, Mesh malha, Material material)
        {
            var objeto = new GameObject(nome);
            objeto.transform.SetParent(pai, false);
            objeto.AddComponent<MeshFilter>().sharedMesh = malha;
            var renderizador = objeto.AddComponent<MeshRenderer>();
            renderizador.sharedMaterial = material;
            renderizador.shadowCastingMode = ShadowCastingMode.Off;
            renderizador.receiveShadows = false;
            renderizador.lightProbeUsage = LightProbeUsage.Off;
            renderizador.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return renderizador;
        }

        private static Transform Segmento(Transform pai, string nome, Vector3 inicio, Vector3 fim, Material material)
        {
            Transform segmento = Desenhar(pai, nome, Resources.GetBuiltinResource<Mesh>("Cube.fbx"), material).transform;
            Posicionar(segmento, inicio, fim);
            return segmento;
        }

        private static void Posicionar(Transform segmento, Vector3 inicio, Vector3 fim)
        {
            Vector3 direcao = fim - inicio;
            float comprimento = direcao.magnitude;
            segmento.localPosition = (inicio + fim) / 2;
            segmento.localRotation = comprimento > 1e-6f ? Quaternion.LookRotation(direcao, Vector3.up) : Quaternion.identity;
            if (comprimento > 1e-6f && Mathf.Abs(Vector3.Dot(direcao / comprimento, Vector3.up)) > 0.999f)
                segmento.localRotation = Quaternion.LookRotation(direcao, Vector3.forward);
            segmento.localScale = new Vector3(Espessura, Espessura, comprimento + Espessura);
        }

        private void MarcaDeAnguloReto(Transform pai, string nome, Vector3 vertice, Vector3 u, Vector3 v)
        {
            var marca = new GameObject(nome).transform;
            marca.SetParent(pai, false);
            Vector3 canto = vertice + (u + v) * LadoDaMarca;
            Segmento(marca, "Lado 1", vertice + u * LadoDaMarca, canto, _marca).localScale =
                new Vector3(EspessuraDaMarca, EspessuraDaMarca, LadoDaMarca + EspessuraDaMarca);
            Segmento(marca, "Lado 2", vertice + v * LadoDaMarca, canto, _marca).localScale =
                new Vector3(EspessuraDaMarca, EspessuraDaMarca, LadoDaMarca + EspessuraDaMarca);
        }

        /// <summary>Rótulo de medida (a, b, c) fora da aresta: afasta-se do eixo da caixa a folga mais meia largura do texto.</summary>
        private void RotuloDeMedida(Transform pai, string nome, Vector3 meioDaAresta, Vector3 centro, string texto)
        {
            TextMeshPro tmp = Rotulo(pai, nome, meioDaAresta, texto, PaletaDoLaboratorio.Texto);
            Vector3 direcao = meioDaAresta - centro;
            direcao.y = 0f;
            direcao = direcao.sqrMagnitude > 1e-8f ? direcao.normalized : Vector3.forward;
            float meiaLargura = tmp.GetPreferredValues(texto).x / 2;
            tmp.transform.localPosition = meioDaAresta + direcao * (FolgaDaMedida + meiaLargura) + Vector3.up * 0.015f;
        }

        private TextMeshPro Rotulo(Transform pai, string nome, Vector3 posicao, string texto, Color cor)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            objeto.transform.SetParent(pai, false);
            objeto.transform.localPosition = posicao;
            var tmp = objeto.AddComponent<TextMeshPro>();
            if (_fonte != null) tmp.font = _fonte;
            if (_visual != null && _visual.MaterialDoRotulo != null) tmp.fontSharedMaterial = _visual.MaterialDoRotulo;
            tmp.text = texto;
            tmp.fontSize = _tamanhoDoRotulo;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = cor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(0.2f, 0.05f);
            objeto.AddComponent<RotuloVoltadoParaOAluno>();
            return tmp;
        }

        private static void Destruir(Object objeto)
        {
            if (objeto == null) return;
            if (Application.isPlaying) Object.Destroy(objeto);
            else Object.DestroyImmediate(objeto);
        }
    }
}
