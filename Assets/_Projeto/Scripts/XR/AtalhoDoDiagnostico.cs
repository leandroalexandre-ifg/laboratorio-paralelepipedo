using LaboratorioParalelepipedo.Nucleo.Interacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Atalho do professor: os dois analógicos afundados ao mesmo tempo por <see cref="Segundos"/> s mostram ou escondem
    /// o painel de diagnóstico, com uma vibração nos dois controles para confirmar. Difícil de acionar sem querer.
    /// Lê os botões "crus" de cada analógico (RawButton), que não dependem de qual controle é o primário.
    /// Sem alocação por frame.
    /// </summary>
    public sealed class AtalhoDoDiagnostico : MonoBehaviour
    {
        public const double Segundos = 3;

        [SerializeField] private PainelDeDiagnostico _painel;

        private readonly SeguraPorTempo _segura = new SeguraPorTempo(Segundos);
        private float _fimDaVibracao = -1f;

        /// <summary>Analógico esquerdo afundado agora (também mostrado no diagnóstico).</summary>
        public static bool EsquerdoAfundado => OVRInput.Get(OVRInput.RawButton.LThumbstick, OVRInput.Controller.Touch);

        /// <summary>Analógico direito afundado agora (também mostrado no diagnóstico).</summary>
        public static bool DireitoAfundado => OVRInput.Get(OVRInput.RawButton.RThumbstick, OVRInput.Controller.Touch);

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(PainelDeDiagnostico painel) => _painel = painel;

        private void Update()
        {
            if (_fimDaVibracao >= 0f && Time.unscaledTime >= _fimDaVibracao)
            {
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
                _fimDaVibracao = -1f;
            }

            if (!_segura.Atualizar(EsquerdoAfundado && DireitoAfundado, Time.unscaledDeltaTime) || _painel == null) return;
            _painel.Alternar();
            PulsoDeVibracao pulso = Vibracao.AoConfirmar;
            OVRInput.SetControllerVibration(pulso.Frequencia, pulso.Forca, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(pulso.Frequencia, pulso.Forca, OVRInput.Controller.RTouch);
            _fimDaVibracao = Time.unscaledTime + Mathf.Min(pulso.Duracao, Vibracao.DuracaoMaxima);
        }
    }
}
