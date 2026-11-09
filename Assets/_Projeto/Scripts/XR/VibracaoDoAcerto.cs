using LaboratorioParalelepipedo.Apresentacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>Vibração curta nos dois controles quando o aluno acerta um desafio da Estação 1 ou a pergunta da Estação 2.</summary>
    public sealed class VibracaoDoAcerto : MonoBehaviour
    {
        [SerializeField] private EstacaoDasDiagonaisNaCena _estacao;
        [SerializeField] private EstacaoDosElementosNaCena _estacaoDosElementos;

        private float _fim = -1f;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(EstacaoDasDiagonaisNaCena estacao, EstacaoDosElementosNaCena estacaoDosElementos = null)
        {
            _estacao = estacao;
            _estacaoDosElementos = estacaoDosElementos;
        }

        private void OnEnable()
        {
            if (_estacao != null) _estacao.Acertou += Vibrar;
            if (_estacaoDosElementos != null) _estacaoDosElementos.Acertou += Vibrar;
        }

        private void OnDisable()
        {
            if (_estacao != null) _estacao.Acertou -= Vibrar;
            if (_estacaoDosElementos != null) _estacaoDosElementos.Acertou -= Vibrar;
            Parar();
        }

        private void Vibrar()
        {
            if (!Application.isPlaying) return;
            var pulso = Vibracao.AoAcertar;
            OVRInput.SetControllerVibration(pulso.Frequencia, pulso.Forca, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(pulso.Frequencia, pulso.Forca, OVRInput.Controller.RTouch);
            _fim = Time.unscaledTime + Mathf.Min(pulso.Duracao, Vibracao.DuracaoMaxima);
        }

        private void Update()
        {
            if (_fim >= 0f && Time.unscaledTime >= _fim) Parar();
        }

        private void Parar()
        {
            if (_fim < 0f) return;
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
            _fim = -1f;
        }
    }
}
