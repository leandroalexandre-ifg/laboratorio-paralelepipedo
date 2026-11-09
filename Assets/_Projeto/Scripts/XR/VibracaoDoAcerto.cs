using LaboratorioParalelepipedo.Apresentacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>Vibração curta nos dois controles quando o aluno acerta a pergunta da estação.</summary>
    public sealed class VibracaoDoAcerto : MonoBehaviour
    {
        [SerializeField] private EstacaoDasDiagonaisNaCena _estacao;

        private float _fim = -1f;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(EstacaoDasDiagonaisNaCena estacao) => _estacao = estacao;

        private void OnEnable()
        {
            if (_estacao != null) _estacao.Acertou += Vibrar;
        }

        private void OnDisable()
        {
            if (_estacao != null) _estacao.Acertou -= Vibrar;
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
