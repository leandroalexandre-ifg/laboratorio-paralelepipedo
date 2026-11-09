using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Desenha um prisma do núcleo: arestas, faces translúcidas, vértices e os rótulos A–H (sempre virados para o aluno).
    /// A base fica apoiada em y = 0 do objeto e centrada em x e z; a frente (z = 0 do núcleo) fica do lado do aluno.
    /// A escala vem do núcleo (<see cref="EscalaDaCaixa"/>): a maior dimensão ocupa no máximo 0,6 m.
    /// Monta os filhos uma vez (no Awake ou ao chamar <see cref="Construir"/>); nada roda por frame.
    /// </summary>
    public sealed class VisualDaCaixa : MonoBehaviour
    {
        /// <summary>Nomes dos grupos de filhos.</summary>
        public const string GrupoArestas = "Arestas";
        public const string GrupoFaces = "Faces";
        public const string GrupoVertices = "Vertices";
        public const string GrupoRotulos = "Rotulos";

        [Header("Dimensões (unidades do núcleo)")]
        [SerializeField] private float _comprimento = 4f;
        [SerializeField] private float _largura = 3f;
        [SerializeField] private float _altura = 2f;

        [Header("Aparência")]
        [SerializeField] private Material _materialDaAresta;
        [SerializeField] private Material _materialDaFace;
        [SerializeField] private Material _materialDoVertice;
        [SerializeField] private TMP_FontAsset _fonte;
        [SerializeField, Tooltip("Material da fonte que aparece através da caixa (opcional).")]
        private Material _materialDoRotulo;
        [SerializeField] private float _espessuraDaAresta = 0.008f;
        [SerializeField] private float _diametroDoVertice = 0.02f;
        [SerializeField] private float _tamanhoDoRotulo = 0.4f;
        [SerializeField] private float _afastamentoDoRotulo = 0.05f;

        private readonly List<Mesh> _malhasCriadas = new List<Mesh>();
        // Esferas da caixa atual, por nome. Não se procura pelo nome na hierarquia: rodando, o Destroy só apaga no fim
        // do quadro, e a busca achava a esfera da caixa anterior (que sumia logo depois, levando o alvo do gatilho).
        private readonly Dictionary<string, Transform> _vertices = new Dictionary<string, Transform>(8);
        private IPrisma _prisma;
        private float _metrosPorUnidade;
        private Vector3 _centroDaBase;

        /// <summary>Prisma desenhado agora (nulo antes de construir).</summary>
        public IPrisma Prisma => _prisma;

        /// <summary>Metros na cena por unidade de medida, calculados por <see cref="EscalaDaCaixa"/> para cada caixa.</summary>
        public float MetrosPorUnidade => _metrosPorUnidade;

        /// <summary>Tamanho da caixa na cena, em metros (x: comprimento, y: altura, z: largura).</summary>
        public Vector3 TamanhoNaCena => new Vector3(_comprimento, _altura, _largura) * _metrosPorUnidade;

        /// <summary>Liga a aparência (usado pelo gerador da cena e pelos testes).</summary>
        public void Configurar(Material aresta, Material face, Material vertice, TMP_FontAsset fonte,
            Material materialDoRotulo = null)
        {
            _materialDaAresta = aresta;
            _materialDaFace = face;
            _materialDoVertice = vertice;
            _fonte = fonte;
            _materialDoRotulo = materialDoRotulo;
        }

        /// <summary>Material dos rótulos, que aparece através da caixa (nulo: o material padrão da fonte).</summary>
        public Material MaterialDoRotulo => _materialDoRotulo;

        private void Awake()
        {
            if (_prisma == null) Construir(new Paralelepipedo(_comprimento, _largura, _altura));
        }

        private void OnDestroy() => LiberarMalhas();

        /// <summary>Apaga o desenho anterior e desenha o prisma dado.</summary>
        public void Construir(IPrisma prisma)
        {
            _prisma = prisma ?? throw new System.ArgumentNullException(nameof(prisma));
            if (prisma is Paralelepipedo caixa)
            {
                _comprimento = (float)caixa.A;
                _largura = (float)caixa.B;
                _altura = (float)caixa.C;
            }

            Limpar();
            _metrosPorUnidade = (float)EscalaDaCaixa.MetrosPorUnidade(prisma);
            _centroDaBase = CentroDaBase(prisma);

            Transform arestas = CriarGrupo(GrupoArestas);
            foreach (var aresta in prisma.Arestas) CriarAresta(arestas, aresta);

            Transform faces = CriarGrupo(GrupoFaces);
            foreach (var face in prisma.Faces) CriarFace(faces, face);

            Transform vertices = CriarGrupo(GrupoVertices);
            Transform rotulos = CriarGrupo(GrupoRotulos);
            Vector3 centro = Vector3.up * (float)(prisma.Altura * _metrosPorUnidade / 2);
            foreach (var vertice in prisma.Vertices)
            {
                _vertices[vertice.Nome] = CriarVertice(vertices, vertice);
                CriarRotulo(rotulos, vertice, centro);
            }
        }

        /// <summary>Posição local, em metros, de um ponto do núcleo.</summary>
        public Vector3 PosicaoNaCena(Vetor3 ponto) =>
            (new Vector3((float)ponto.X, (float)ponto.Y, (float)ponto.Z) - _centroDaBase) * _metrosPorUnidade;

        private static Vector3 CentroDaBase(IPrisma prisma)
        {
            double minX = double.MaxValue, maxX = double.MinValue, minZ = double.MaxValue, maxZ = double.MinValue;
            foreach (var v in prisma.Vertices)
            {
                if (v.Posicao.X < minX) minX = v.Posicao.X;
                if (v.Posicao.X > maxX) maxX = v.Posicao.X;
                if (v.Posicao.Z < minZ) minZ = v.Posicao.Z;
                if (v.Posicao.Z > maxZ) maxZ = v.Posicao.Z;
            }

            return new Vector3((float)((minX + maxX) / 2), 0f, (float)((minZ + maxZ) / 2));
        }

        private void CriarAresta(Transform grupo, Aresta aresta)
        {
            Vector3 inicio = PosicaoNaCena(aresta.Inicio.Posicao);
            Vector3 fim = PosicaoNaCena(aresta.Fim.Posicao);
            Vector3 direcao = fim - inicio;
            var objeto = CriarMalha(grupo, $"Aresta {aresta.Nome}", MalhaEmbutida("Cube.fbx"), _materialDaAresta);
            objeto.transform.localPosition = (inicio + fim) / 2;
            objeto.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, direcao);
            // Mais comprida que a aresta pela própria espessura: os cantos ficam fechados.
            objeto.transform.localScale = new Vector3(_espessuraDaAresta, _espessuraDaAresta,
                direcao.magnitude + _espessuraDaAresta);
        }

        private void CriarFace(Transform grupo, Face face)
        {
            var pontos = new Vector3[face.Vertices.Count];
            for (int i = 0; i < pontos.Length; i++) pontos[i] = PosicaoNaCena(face.Vertices[i].Posicao);

            var triangulos = new int[(pontos.Length - 2) * 3];
            for (int i = 0; i < pontos.Length - 2; i++)
            {
                triangulos[i * 3] = 0;
                triangulos[i * 3 + 1] = i + 1;
                triangulos[i * 3 + 2] = i + 2;
            }

            var malha = new Mesh { name = $"Face {face.Nome}", vertices = pontos, triangles = triangulos };
            malha.RecalculateNormals();
            malha.RecalculateBounds();
            _malhasCriadas.Add(malha);
            CriarMalha(grupo, $"Face {face.Nome}", malha, _materialDaFace);
        }

        private Transform CriarVertice(Transform grupo, Vertice vertice)
        {
            var objeto = CriarMalha(grupo, $"Vertice {vertice.Nome}", MalhaEmbutida("Sphere.fbx"), _materialDoVertice);
            objeto.transform.localPosition = PosicaoNaCena(vertice.Posicao);
            objeto.transform.localScale = Vector3.one * _diametroDoVertice;
            return objeto.transform;
        }

        private void CriarRotulo(Transform grupo, Vertice vertice, Vector3 centro)
        {
            Vector3 posicao = PosicaoNaCena(vertice.Posicao);
            var objeto = new GameObject($"Rotulo {vertice.Nome}", typeof(RectTransform));
            objeto.transform.SetParent(grupo, false);
            // Para o lado (esquerda para A, B, E, F; direita para C, D, G, H), não para a frente: com a caixa girada na
            // mesa, "para fora na diagonal" punha o rótulo da frente na linha do olhar, sobre a esfera. Os de cima sobem
            // um pouco e os de baixo ficam rente à mesa; os de cima do fundo (B, C) sobem mais que os da frente (A, D).
            var paraFora = new Vector3(posicao.x >= centro.x ? 1f : -1f, 0f, 0f);
            bool emCima = posicao.y > centro.y;
            bool noFundo = posicao.z > centro.z;
            float subida = emCima ? _afastamentoDoRotulo * (noFundo ? 1.6f : 0.9f) : _tamanhoDoRotulo * 0.05f;
            objeto.transform.localPosition = posicao + paraFora.normalized * _afastamentoDoRotulo + Vector3.up * subida;

            var texto = objeto.AddComponent<TextMeshPro>();
            if (_fonte != null) texto.font = _fonte;
            if (_materialDoRotulo != null) texto.fontSharedMaterial = _materialDoRotulo;
            texto.text = vertice.Nome;
            texto.fontSize = _tamanhoDoRotulo;
            texto.color = PaletaDoLaboratorio.Texto;
            texto.alignment = TextAlignmentOptions.Center;
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            texto.rectTransform.sizeDelta = new Vector2(0.06f, 0.05f);
            objeto.AddComponent<RotuloVoltadoParaOAluno>();
        }

        /// <summary>Esfera do vértice com o nome dado (A a H), ou nula.</summary>
        public Transform Vertice(string nome)
        {
            if (_vertices.TryGetValue(nome, out Transform esfera) && esfera != null) return esfera;
            // Caixa salva na cena e ainda não reconstruída nesta execução: só a desenhada pelo gerador existe.
            return _vertices.Count == 0 ? transform.Find($"{GrupoVertices}/Vertice {nome}") : null;
        }

        /// <summary>Diâmetro da esfera de cada vértice, em metros.</summary>
        public float DiametroDoVertice => _diametroDoVertice;

        private GameObject CriarMalha(Transform grupo, string nome, Mesh malha, Material material)
        {
            var objeto = new GameObject(nome);
            objeto.transform.SetParent(grupo, false);
            objeto.AddComponent<MeshFilter>().sharedMesh = malha;
            var renderizador = objeto.AddComponent<MeshRenderer>();
            renderizador.sharedMaterial = material;
            renderizador.shadowCastingMode = ShadowCastingMode.Off;
            renderizador.receiveShadows = false;
            renderizador.lightProbeUsage = LightProbeUsage.Off;
            renderizador.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return objeto;
        }

        private Transform CriarGrupo(string nome)
        {
            var grupo = new GameObject(nome).transform;
            grupo.SetParent(transform, false);
            return grupo;
        }

        private void Limpar()
        {
            _vertices.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--) Destruir(transform.GetChild(i).gameObject);
            LiberarMalhas();
        }

        private void LiberarMalhas()
        {
            foreach (var malha in _malhasCriadas) Destruir(malha);
            _malhasCriadas.Clear();
        }

        /// <summary>
        /// Rodando, o Destroy só age no fim do quadro: o objeto sai da hierarquia e se esconde já, para nada no mesmo
        /// quadro (buscas, colisores, raio) o confundir com a caixa nova.
        /// </summary>
        internal static void Destruir(Object objeto)
        {
            if (objeto == null) return;
            if (!Application.isPlaying)
            {
                DestroyImmediate(objeto);
                return;
            }

            if (objeto is GameObject jogo)
            {
                jogo.SetActive(false);
                jogo.transform.SetParent(null, false);
            }
            Destroy(objeto);
        }

        private static Mesh MalhaEmbutida(string nome) => Resources.GetBuiltinResource<Mesh>(nome);
    }
}
