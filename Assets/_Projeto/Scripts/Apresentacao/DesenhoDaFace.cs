using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo;
using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Desenho da Estação 1 sobre a caixa: uma aresta grossa azul entre cada vértice tocado e o anterior e, com a face
    /// pronta, a aresta que fecha o contorno, a face preenchida com a cor do par dela (bases, a·c, b·c) e o nome no
    /// meio. Os objetos são criados uma vez por caixa e só mudam quando o aluno toca um vértice, nunca a cada quadro.
    /// </summary>
    public sealed class DesenhoDaFace : MonoBehaviour
    {
        public const string NomeDoGrupo = "Face";
        private const float Espessura = 0.014f;
        private const float AfastamentoDaFace = 0.003f;
        private const float AfastamentoDoNome = 0.03f;

        [SerializeField] private VisualDaCaixa _visual;
        [SerializeField] private Material _aresta;
        [SerializeField] private Material _bases;
        [SerializeField] private Material _frenteEFundo;
        [SerializeField] private Material _esquerdaEDireita;
        [SerializeField] private TMP_FontAsset _fonte;
        [SerializeField] private float _tamanhoDoNome = 0.5f;

        private readonly Transform[] _segmentos = new Transform[4];
        private readonly Vector3[] _cantos = new Vector3[4];
        private Transform _grupo;
        private Renderer _preenchimento;
        private TextMeshPro _nome;
        private Mesh _malha;

        /// <summary>Arestas desenhadas agora (para os testes).</summary>
        public int ArestasVisiveis
        {
            get
            {
                int total = 0;
                foreach (Transform segmento in _segmentos)
                    if (segmento != null && segmento.gameObject.activeSelf) total++;
                return total;
            }
        }

        /// <summary>Se a face montada está preenchida agora.</summary>
        public bool FacePreenchida => _preenchimento != null && _preenchimento.gameObject.activeSelf;

        /// <summary>Material do preenchimento (a cor do par da face).</summary>
        public Material MaterialDaFace => _preenchimento != null ? _preenchimento.sharedMaterial : null;

        /// <summary>Nome escrito na face montada (vazio se não há face).</summary>
        public string NomeDaFace => _nome != null && _nome.gameObject.activeSelf ? _nome.text : string.Empty;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(VisualDaCaixa visual, Material aresta, Material bases, Material frenteEFundo,
            Material esquerdaEDireita, TMP_FontAsset fonte)
        {
            _visual = visual;
            _aresta = aresta;
            _bases = bases;
            _frenteEFundo = frenteEFundo;
            _esquerdaEDireita = esquerdaEDireita;
            _fonte = fonte;
        }

        /// <summary>Monta o desenho (tudo escondido) para a caixa que o <see cref="VisualDaCaixa"/> acabou de construir.</summary>
        public void Construir()
        {
            Limpar();
            _grupo = new GameObject(NomeDoGrupo).transform;
            _grupo.SetParent(transform, false);

            Mesh cubo = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            for (int i = 0; i < _segmentos.Length; i++)
            {
                _segmentos[i] = DesenhoDasDiagonais.Desenhar(_grupo, $"Aresta {i + 1}", cubo, _aresta).transform;
                _segmentos[i].gameObject.SetActive(false);
            }

            _malha = new Mesh { name = "Face montada" };
            _malha.MarkDynamic();
            _malha.vertices = _cantos;
            // As duas faces: o preenchimento aparece de dentro e de fora da caixa.
            _malha.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };
            _preenchimento = DesenhoDasDiagonais.Desenhar(_grupo, "Preenchimento", _malha, _bases);
            _preenchimento.gameObject.SetActive(false);

            var objeto = new GameObject("Nome da face", typeof(RectTransform));
            objeto.transform.SetParent(_grupo, false);
            _nome = objeto.AddComponent<TextMeshPro>();
            if (_fonte != null) _nome.font = _fonte;
            if (_visual != null && _visual.MaterialDoRotulo != null) _nome.fontSharedMaterial = _visual.MaterialDoRotulo;
            _nome.fontSize = _tamanhoDoNome;
            _nome.fontStyle = FontStyles.Bold;
            _nome.alignment = TextAlignmentOptions.Center;
            _nome.textWrappingMode = TextWrappingModes.NoWrap;
            _nome.rectTransform.sizeDelta = new Vector2(0.3f, 0.08f);
            objeto.AddComponent<RotuloVoltadoParaOAluno>();
            objeto.SetActive(false);
        }

        /// <summary>
        /// Mostra as arestas entre os vértices tocados, em ordem; com <paramref name="face"/>, fecha o contorno,
        /// preenche a face e escreve o nome.
        /// </summary>
        public void Mostrar(IReadOnlyList<string> tocados, Face face)
        {
            if (_grupo == null || _visual == null || !(_visual.Prisma is Paralelepipedo caixa)) return;
            int quantos = tocados != null ? tocados.Count : 0;
            for (int i = 0; i < _segmentos.Length; i++)
            {
                bool ultima = i == _segmentos.Length - 1;
                bool visivel = ultima ? face != null && quantos == _segmentos.Length : i + 1 < quantos;
                if (visivel)
                {
                    Vector3 inicio = Posicao(caixa, tocados[i]);
                    Vector3 fim = Posicao(caixa, tocados[ultima ? 0 : i + 1]);
                    DesenhoDasDiagonais.Posicionar(_segmentos[i], inicio, fim, Espessura);
                }
                if (_segmentos[i].gameObject.activeSelf != visivel) _segmentos[i].gameObject.SetActive(visivel);
            }

            bool comFace = face != null;
            if (comFace) Preencher(caixa, face);
            if (_preenchimento.gameObject.activeSelf != comFace) _preenchimento.gameObject.SetActive(comFace);
            if (_nome.gameObject.activeSelf != comFace) _nome.gameObject.SetActive(comFace);
        }

        private void Preencher(Paralelepipedo caixa, Face face)
        {
            Vector3 centroDaFace = Vector3.zero;
            for (int i = 0; i < _cantos.Length; i++)
            {
                _cantos[i] = _visual.PosicaoNaCena(face.Vertices[i].Posicao);
                centroDaFace += _cantos[i] / _cantos.Length;
            }

            // Um pouco para fora da caixa: não briga com a face translúcida pela profundidade.
            Vector3 centroDaCaixa = (Posicao(caixa, "E") + Posicao(caixa, "C")) / 2;
            Vector3 paraFora = (centroDaFace - centroDaCaixa).normalized;
            Vector3 normal = Vector3.Cross(_cantos[1] - _cantos[0], _cantos[2] - _cantos[0]).normalized;
            if (Vector3.Dot(normal, paraFora) < 0f) normal = -normal;
            for (int i = 0; i < _cantos.Length; i++) _cantos[i] += normal * AfastamentoDaFace;
            _malha.vertices = _cantos;
            _malha.RecalculateNormals();
            _malha.RecalculateBounds();

            _preenchimento.sharedMaterial = face.Par == ParDeFaces.Bases ? _bases
                : face.Par == ParDeFaces.FrenteEFundo ? _frenteEFundo : _esquerdaEDireita;
            _nome.text = face.Nome;
            _nome.color = face.Par == ParDeFaces.Bases ? PaletaDoLaboratorio.BasesEscuro
                : face.Par == ParDeFaces.FrenteEFundo ? PaletaDoLaboratorio.FrenteEFundoEscuro
                : PaletaDoLaboratorio.EsquerdaEDireitaEscuro;
            _nome.transform.localPosition = centroDaFace + normal * AfastamentoDoNome;
        }

        private Vector3 Posicao(Paralelepipedo caixa, string nome) => _visual.PosicaoNaCena(caixa.ObterVertice(nome).Posicao);

        /// <summary>Apaga o desenho (ao trocar de caixa ou de estação).</summary>
        public void Limpar()
        {
            if (_grupo != null) VisualDaCaixa.Destruir(_grupo.gameObject);
            _grupo = null;
            for (int i = 0; i < _segmentos.Length; i++) _segmentos[i] = null;
            _preenchimento = null;
            _nome = null;
            VisualDaCaixa.Destruir(_malha);
            _malha = null;
        }

        private void OnDestroy() => Limpar();
    }
}
