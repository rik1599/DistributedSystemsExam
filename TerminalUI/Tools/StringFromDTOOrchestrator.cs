using Actors.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalUI.Tools
{   
    /// <summary>
    /// Strumento per generare un output a partire 
    /// da un drone state DTO (facendo attenzione al 
    /// tipo)
    /// 
    /// [TODO (poco prioritario): correggere questa soluzione 
    /// terribile con un più elegante Vistor sul DTO.]
    /// </summary>
    public class StringFromDTOOrchestrator
    {
        public enum OutputType
        {
            MINIMAL, SMART, COMPLETE,
        }

        private StringBuilderFromDTO _bobTheBuilder;

        public StringFromDTOOrchestrator(DroneStateDTO dto)
        {
            _bobTheBuilder = new StringBuilderFromDTO(dto)
                .AddBasicInfo();
        }

        private void _addPreferredInfoAccordingToType()
        {
            if (_bobTheBuilder.DTO is InitStateDTO)
            {
                _bobTheBuilder.AddConnectionDetails();

            } 
            else if (_bobTheBuilder.DTO is NegotiateStateDTO || 
                _bobTheBuilder.DTO is WaitingStateDTO)
            {
                _bobTheBuilder.AddConflictInfo();

            } 
            else if (_bobTheBuilder.DTO is FlyingStateDTO)
            {
                _bobTheBuilder.AddFlyDetails();

            } 
            else if (_bobTheBuilder.DTO is ExitStateDTO)
            {
                _bobTheBuilder.AddExitDetails();
            }
        }

        public String GetString(int indent = 0, OutputType outputType=OutputType.SMART)
        {
            switch (outputType)
            {
                case OutputType.MINIMAL: 
                    return _bobTheBuilder.GetString(indent);
                case OutputType.COMPLETE:
                    return _bobTheBuilder
                        .AddConnectionDetails()
                        .AddConflictInfo()
                        .AddFlyDetails()
                        .AddExitDetails()
                        .GetString(indent);
                default:
                    _addPreferredInfoAccordingToType();
                    return _bobTheBuilder.GetString(indent);
                
            }
        }

    }
}
