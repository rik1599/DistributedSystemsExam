using Actors.DTO;

namespace TerminalUI.Tools
{
    /// <summary>
    /// Strumento per generare un output a partire 
    /// da un drone state DTO (facendo attenzione al 
    /// tipo)
    /// 
    /// [TODO (poco prioritario): correggere questa soluzione 
    /// terribile con un più elegante Visitor sul DTO.]
    /// </summary>
    public class StringFromDTOOrchestrator
    {
        public enum OutputType
        {
            MINIMAL, SMART, COMPLETE,
        }

        private readonly StringBuilderFromDTO _bobTheBuilder;

        public StringFromDTOOrchestrator(DroneStateDTO dto)
        {
            _bobTheBuilder = new StringBuilderFromDTO(dto)
                .AddBasicInfo();
        }

        private void AddPreferredInfoAccordingToType()
        {
            switch (_bobTheBuilder.DTO)
            {
                case InitStateDTO _:
                    _bobTheBuilder.AddConnectionDetails();
                    break;

                case NegotiateStateDTO _:
                    _bobTheBuilder.AddConflictInfo();
                    break;

                case WaitingStateDTO _:
                    _bobTheBuilder.AddConflictInfo();
                    break;

                case FlyingStateDTO _:
                    _bobTheBuilder.AddFlyDetails();
                    break;

                case ExitStateDTO _:
                    _bobTheBuilder.AddExitDetails();
                    break;
            }
        }

        public string GetString(int indent = 0, OutputType outputType=OutputType.SMART)
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
                    AddPreferredInfoAccordingToType();
                    return _bobTheBuilder.GetString(indent);
                
            }
        }

    }
}
