using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Esfera escura presa à cabeça, usada pelo giro por passos (~100 ms) e pela recentralização (escurece, repõe o
    /// aluno e clareia). Mostra a maior das duas opacidades e fica desligada quando as duas são zero: sem vinheta
    /// parado nem no teleporte. Só troca o MaterialPropertyBlock quando a opacidade muda. Sem alocação por frame.
    /// </summary>
    public sealed class TelaEscura : MonoBehaviour
    {
        [SerializeField] private Renderer _esfera;

        private static readonly int CorBase = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock _bloco;
        private float _mostrada = -1f;

        /// <summary>Opacidade pedida pelo giro por passos (0 a 1).</summary>
        public float OpacidadeDoGiro { get; set; }

        /// <summary>Opacidade pedida pela recentralização (0 a 1).</summary>
        public float OpacidadeDaRecentralizacao { get; set; }

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(Renderer esfera) => _esfera = esfera;

        private void Awake() => Aplicar();

        private void LateUpdate() => Aplicar();

        /// <summary>Mostra a opacidade atual (público para os testes no Editor).</summary>
        public void Aplicar()
        {
            if (_esfera == null) return;
            float opacidade = Mathf.Clamp01(Mathf.Max(OpacidadeDoGiro, OpacidadeDaRecentralizacao));
            if (opacidade == _mostrada) return;
            _mostrada = opacidade;

            _esfera.enabled = opacidade > 0f;
            if (!_esfera.enabled) return;
            if (_bloco == null) _bloco = new MaterialPropertyBlock();
            _bloco.SetColor(CorBase, new Color(0f, 0f, 0f, opacidade));
            _esfera.SetPropertyBlock(_bloco);
        }
    }
}
