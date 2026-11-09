using System;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// A caixa aberta da Estação 3: as 6 faces giram pelas arestas (<see cref="Planificacao"/>) em
    /// <see cref="DuracaoDaAbertura"/> s, e a planificação, ao mesmo tempo, vai para o meio da mesa, endireita o giro
    /// da caixa, muda para uma escala que cabe no tampo e inclina <see cref="Inclinacao"/>° para o aluno, como um tampo
    /// de prancheta. "Fechar" faz o caminho inverso, dobrando as faces de volta. Aberta, cada face tem colisor
    /// (o gatilho escolhe) e um rótulo com o nome e as medidas (ou a área, depois de o par ser achado).
    /// Sem alocação por quadro: malhas e listas são criadas uma vez por caixa.
    /// </summary>
    public sealed class PlanificacaoNaCena : MonoBehaviour
    {
        public const float DuracaoDaAbertura = 1f;
        public const string NomeDoGrupo = "Faces da planificacao";
        private const float EspessuraDaAresta = 0.006f;
        private const float EspessuraDoColisor = 0.02f;
        private const float AfastamentoDoRotulo = 0.015f;
        private const float OcupacaoDoRotulo = 0.85f;

        [SerializeField] private Material _neutra;
        [SerializeField] private Material _bases;
        [SerializeField] private Material _frenteEFundo;
        [SerializeField] private Material _esquerdaEDireita;
        [SerializeField] private Material _apontada;
        [SerializeField] private Material _aresta;
        [SerializeField] private TMP_FontAsset _fonte;
        [SerializeField] private Material _materialDoRotulo;
        [SerializeField, Tooltip("Inclinação da planificação aberta para o aluno, em graus.")]
        private float _inclinacao = 35f;
        [SerializeField, Tooltip("Centro da planificação aberta, no mundo (acima do meio da mesa).")]
        private Vector3 _centroAberta = new Vector3(0f, 1f, 1f);
        [SerializeField, Tooltip("Maior largura e profundidade da planificação aberta, em metros.")]
        private Vector2 _tamanhoMaximo = new Vector2(1f, 0.75f);
        [SerializeField] private float _tamanhoDoRotulo = 0.45f;

        private readonly TransicaoSuave _transicao = new TransicaoSuave(DuracaoDaAbertura);
        private readonly Vetor3[] _pontosDoNucleo = new Vetor3[4];
        private Paralelepipedo _caixa;
        private Planificacao _planificacao;
        private Transform _grupo;
        private Face[] _faces = new Face[0];
        private Mesh[] _malhas = new Mesh[0];
        private Vector3[][] _pontos = new Vector3[0][];
        private Transform[][] _arestas = new Transform[0][];
        private AlvoDeFace[] _alvos = new AlvoDeFace[0];
        private BoxCollider[] _colisores = new BoxCollider[0];
        private TextMeshPro[] _rotulos = new TextMeshPro[0];
        private float[] _larguras = new float[0];
        private Vector3 _posicaoFechada;
        private Quaternion _rotacaoFechada = Quaternion.identity;
        private float _escalaFechada = 0.1f, _escalaAberta = 0.075f;
        private float _fracao, _inicio, _fim;

        /// <summary>Quanto está aberta: 0 = fechada (no lugar da caixa), 1 = aberta e inclinada.</summary>
        public float Fracao => _fracao;

        /// <summary>Totalmente aberta (as faces aceitam o gatilho).</summary>
        public bool Aberta => _fracao >= 1f && !_transicao.Ativa;

        /// <summary>Abrindo ou fechando agora.</summary>
        public bool Animando => _transicao.Ativa;

        /// <summary>Inclinação da planificação aberta, em graus.</summary>
        public float Inclinacao => _inclinacao;

        /// <summary>Metros por unidade com a caixa aberta (menor que a da caixa, para caber na mesa).</summary>
        public float EscalaAberta => _escalaAberta;

        /// <summary>Uma face aberta foi escolhida pelo gatilho (o nome dela).</summary>
        public event Action<string> FaceTocada;

        /// <summary>Terminou de abrir (verdadeiro) ou de fechar (falso).</summary>
        public event Action<bool> Terminou;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(Material neutra, Material bases, Material frenteEFundo, Material esquerdaEDireita, Material apontada,
            Material aresta, TMP_FontAsset fonte, Material materialDoRotulo, Vector3 centroAberta, Vector2 tamanhoMaximo,
            float inclinacao)
        {
            _neutra = neutra;
            _bases = bases;
            _frenteEFundo = frenteEFundo;
            _esquerdaEDireita = esquerdaEDireita;
            _apontada = apontada;
            _aresta = aresta;
            _fonte = fonte;
            _materialDoRotulo = materialDoRotulo;
            _centroAberta = centroAberta;
            _tamanhoMaximo = tamanhoMaximo;
            _inclinacao = inclinacao;
        }

        /// <summary>
        /// Monta a planificação fechada no lugar da caixa (base no ponto <paramref name="posicaoDaCaixa"/>, com o giro
        /// dela e <paramref name="metrosPorUnidade"/>), tudo escondido até abrir.
        /// </summary>
        public void Construir(Paralelepipedo caixa, Vector3 posicaoDaCaixa, Quaternion rotacaoDaCaixa, float metrosPorUnidade)
        {
            Limpar();
            _caixa = caixa ?? throw new ArgumentNullException(nameof(caixa));
            _planificacao = new Planificacao(caixa);
            _posicaoFechada = posicaoDaCaixa;
            _rotacaoFechada = rotacaoDaCaixa;
            _escalaFechada = metrosPorUnidade;
            _escalaAberta = Mathf.Min(metrosPorUnidade, _tamanhoMaximo.x / (float)_planificacao.LarguraAberta,
                _tamanhoMaximo.y / (float)_planificacao.ProfundidadeAberta);

            _grupo = new GameObject(NomeDoGrupo).transform;
            _grupo.SetParent(transform, false);
            int n = caixa.Faces.Count;
            _faces = new Face[n];
            _malhas = new Mesh[n];
            _pontos = new Vector3[n][];
            _arestas = new Transform[n][];
            _alvos = new AlvoDeFace[n];
            _colisores = new BoxCollider[n];
            _rotulos = new TextMeshPro[n];
            _larguras = new float[n];
            Mesh cubo = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            for (int i = 0; i < n; i++)
            {
                Face face = caixa.Faces[i];
                _faces[i] = face;
                _pontos[i] = new Vector3[4];
                _malhas[i] = new Mesh { name = $"Face {face.Nome}" };
                _malhas[i].MarkDynamic();
                _malhas[i].vertices = _pontos[i];
                _malhas[i].triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };   // as duas faces

                Renderer renderizador = DesenhoDasDiagonais.Desenhar(_grupo, $"Face {face.Nome}", _malhas[i], _neutra);
                GameObject objeto = renderizador.gameObject;
                _colisores[i] = objeto.AddComponent<BoxCollider>();
                _colisores[i].enabled = false;
                _alvos[i] = objeto.AddComponent<AlvoDeFace>();
                _alvos[i].Configurar(face.Nome, renderizador, _neutra, _apontada);
                _alvos[i].Acionado += AoTocarFace;

                _arestas[i] = new Transform[4];
                for (int k = 0; k < 4; k++)
                    _arestas[i][k] = DesenhoDasDiagonais.Desenhar(objeto.transform, $"Aresta {k + 1}", cubo, _aresta).transform;

                _rotulos[i] = CriarRotulo(objeto.transform, face.Nome);
            }

            _transicao.Parar();
            Posicionar(0f);
            _grupo.gameObject.SetActive(false);
        }

        private TextMeshPro CriarRotulo(Transform pai, string nome)
        {
            var objeto = new GameObject("Rotulo", typeof(RectTransform));
            objeto.transform.SetParent(pai, false);
            var tmp = objeto.AddComponent<TextMeshPro>();
            if (_fonte != null) tmp.font = _fonte;
            if (_materialDoRotulo != null) tmp.fontSharedMaterial = _materialDoRotulo;
            tmp.fontSize = _tamanhoDoRotulo;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = PaletaDoLaboratorio.Texto;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.richText = true;
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.1f);
            tmp.text = nome;
            objeto.AddComponent<RotuloVoltadoParaOAluno>();
            objeto.SetActive(false);
            return tmp;
        }

        /// <summary>Texto do rótulo de uma face (nome e medidas, ou a área), escrito só quando muda.</summary>
        public void DefinirRotulo(string face, string texto)
        {
            int i = Indice(face);
            if (i < 0 || _rotulos[i].text == texto) return;
            _rotulos[i].text = texto;
            AjustarRotulo(i);
        }

        // O rótulo cabe na largura da face: encolhe só quando o texto ("3 · 2 = 6") é mais largo que ela.
        private void AjustarRotulo(int i)
        {
            if (_larguras[i] <= 0f) return;
            float largura = _rotulos[i].GetPreferredValues(_rotulos[i].text).x;
            float fator = largura > 0f ? Mathf.Min(1f, OcupacaoDoRotulo * _larguras[i] / largura) : 1f;
            _rotulos[i].transform.localScale = Vector3.one * (fator / _escalaAberta);
        }

        /// <summary>Pinta as duas faces de um par com a cor dele (achado) ou volta ao neutro.</summary>
        public void DefinirPar(ParDeFaces par, bool encontrado)
        {
            Material material = !encontrado ? _neutra
                : par == ParDeFaces.Bases ? _bases : par == ParDeFaces.FrenteEFundo ? _frenteEFundo : _esquerdaEDireita;
            for (int i = 0; i < _faces.Length; i++)
                if (_faces[i].Par == par) _alvos[i].DefinirMaterial(material);
        }

        /// <summary>Material da face agora, sem o raio (para os testes).</summary>
        public Material MaterialDaFace(string face)
        {
            int i = Indice(face);
            return i >= 0 ? _alvos[i].GetComponent<Renderer>().sharedMaterial : null;
        }

        /// <summary>Texto do rótulo de uma face (para os testes).</summary>
        public string RotuloDaFace(string face)
        {
            int i = Indice(face);
            return i >= 0 ? _rotulos[i].text : null;
        }

        /// <summary>A face aberta (para os testes e a prévia).</summary>
        public Transform Face(string face)
        {
            int i = Indice(face);
            return i >= 0 ? _alvos[i].transform : null;
        }

        /// <summary>Rótulo de uma face (para os testes de legibilidade).</summary>
        public TextMeshPro Rotulo(string face)
        {
            int i = Indice(face);
            return i >= 0 ? _rotulos[i] : null;
        }

        private int Indice(string face)
        {
            for (int i = 0; i < _faces.Length; i++)
                if (_faces[i].Nome == face) return i;
            return -1;
        }

        /// <summary>Começa a abrir (de onde estiver).</summary>
        public void Abrir() => Animar(1f);

        /// <summary>Começa a fechar, dobrando as faces de volta (de onde estiver).</summary>
        public void Fechar() => Animar(0f);

        private void Animar(float destino)
        {
            if (_grupo == null) return;
            if (Mathf.Approximately(_fracao, destino) && !_transicao.Ativa) return;
            _inicio = _fracao;
            _fim = destino;
            _transicao.Iniciar();
            _grupo.gameObject.SetActive(true);
            AtivarAlvos(false);
        }

        /// <summary>Termina a animação já (para a prévia e os testes).</summary>
        public void TerminarAgora()
        {
            if (!_transicao.Ativa) return;
            Avancar(DuracaoDaAbertura * 2);
        }

        private void Update() => Avancar(Time.deltaTime);

        /// <summary>Avança a animação (o Update chama; público para a prévia e os testes).</summary>
        public void Avancar(float segundos)
        {
            if (!_transicao.Ativa) return;
            float t = (float)_transicao.Avancar(segundos);
            Posicionar(Mathf.Lerp(_inicio, _fim, t));
            if (_transicao.Ativa) return;

            bool aberta = _fim >= 1f;
            AtivarAlvos(aberta);
            if (!aberta) _grupo.gameObject.SetActive(false);
            Terminou?.Invoke(aberta);
        }

        private void AtivarAlvos(bool ativos)
        {
            for (int i = 0; i < _alvos.Length; i++)
            {
                if (_colisores[i].enabled != ativos) _colisores[i].enabled = ativos;
                if (_rotulos[i].gameObject.activeSelf != ativos) _rotulos[i].gameObject.SetActive(ativos);
            }
        }

        // Faces na fração dada; o grupo vai do lugar da caixa (escala e giro dela) ao meio da mesa, inclinado.
        private void Posicionar(float fracao)
        {
            _fracao = fracao;
            float escala = Mathf.Lerp(_escalaFechada, _escalaAberta, fracao);
            _grupo.SetPositionAndRotation(Vector3.Lerp(_posicaoFechada, _centroAberta, fracao),
                Quaternion.Slerp(_rotacaoFechada, Quaternion.Euler(-_inclinacao, 0f, 0f), fracao));
            _grupo.localScale = Vector3.one * escala;

            Vector3 centro = ConversaoDeVetor.ParaUnity(_planificacao.Centro(fracao));
            float espessura = EspessuraDaAresta / escala;
            for (int i = 0; i < _faces.Length; i++)
            {
                _planificacao.PosicoesDaFace(_faces[i], fracao, _pontosDoNucleo);
                Vector3[] pontos = _pontos[i];
                for (int k = 0; k < 4; k++) pontos[k] = ConversaoDeVetor.ParaUnity(_pontosDoNucleo[k]) - centro;
                _malhas[i].vertices = pontos;
                _malhas[i].RecalculateBounds();
                for (int k = 0; k < 4; k++)
                    DesenhoDasDiagonais.Posicionar(_arestas[i][k], pontos[k], pontos[(k + 1) % 4], espessura);

                if (fracao < 1f) continue;
                // Aberta e plana (y = 0 no grupo): colisor fino sobre a face e o rótulo logo acima do meio dela.
                Vector3 minimo = Vector3.Min(Vector3.Min(pontos[0], pontos[1]), Vector3.Min(pontos[2], pontos[3]));
                Vector3 maximo = Vector3.Max(Vector3.Max(pontos[0], pontos[1]), Vector3.Max(pontos[2], pontos[3]));
                _colisores[i].center = (minimo + maximo) / 2;
                _colisores[i].size = new Vector3(maximo.x - minimo.x, EspessuraDoColisor / escala, maximo.z - minimo.z);
                Transform rotulo = _rotulos[i].transform;
                rotulo.localPosition = (minimo + maximo) / 2 + Vector3.up * (AfastamentoDoRotulo / escala);
                _larguras[i] = (maximo.x - minimo.x) * escala;
                AjustarRotulo(i);   // o texto mantém o tamanho em metros, encolhido se for mais largo que a face
            }
        }

        private void AoTocarFace(AlvoDeFace alvo)
        {
            if (Aberta) FaceTocada?.Invoke(alvo.Nome);
        }

        /// <summary>Apaga a planificação (ao trocar de caixa ou sair da estação).</summary>
        public void Limpar()
        {
            _transicao.Parar();
            _fracao = 0f;
            if (_grupo != null) VisualDaCaixa.Destruir(_grupo.gameObject);
            _grupo = null;
            foreach (Mesh malha in _malhas) VisualDaCaixa.Destruir(malha);
            _faces = new Face[0];
            _malhas = new Mesh[0];
            _alvos = new AlvoDeFace[0];
            _colisores = new BoxCollider[0];
            _rotulos = new TextMeshPro[0];
            _arestas = new Transform[0][];
            _pontos = new Vector3[0][];
        }

        private void OnDestroy() => Limpar();
    }
}
