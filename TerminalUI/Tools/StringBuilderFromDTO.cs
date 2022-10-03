using Actors.DTO;

namespace TerminalUI.Tools
{
    public class StringBuilderFromDTO
    {
        private readonly DroneStateDTO _dto;
        private string _dtoAsString;

        public DroneStateDTO DTO { get { return _dto; } }   

        public string GetString(int indent=0) 
        { 
            if (_dtoAsString == "")
            {
                throw new Exception("Usa i metodi per aggiungere qualche informazione prima di estrarre la stringa.");
            }
            
            return ApplyIndent("\n{" + _dtoAsString + "\n}", indent);
        }

        public StringBuilderFromDTO(DroneStateDTO dto)
        {
            _dto = dto;
            _dtoAsString = "";
        }

        public StringBuilderFromDTO AddBasicInfo()
        {
            _dtoAsString = _dtoAsString 
                + $"\n\tType: {_dto.GetType()},"
                + $"\n\tAge: {_dto.Age},"
                + $"\n\tPosizione attuale: {_dto.CurrentPosition},";

            return this;
        }

        public StringBuilderFromDTO AddConflictInfo() 
        {
            _dtoAsString = _dtoAsString
                + $"\n\tNumero Negoziazione: {_dto.NegotiationsCount}, "
                + $"\n\tPriorità: {ApplyIndent(_dto.CurrentPriority.ToString()!)}, "
                + "\n\tMissioni che attendo (p>mia): ["
                    + string.Join(", ", _dto.GetGreaterPriorityMissions().Select(m =>
                    {
                        return ApplyIndent("\n{"
                            + $"\n\tNodeRef: {ApplyIndent(m.NodeRef.ToString()!)}, "
                            + $"\n\tPath: {ApplyIndent(m.MissionPath.ToString()!)}, "
                            + $"\n\tPriority: {ApplyIndent(m.Priority.ToString()!)}, " 
                            + "\n}", 2);
                    })) + "\n\t], "
                + "\n\tMissioni che MI attendono (p<mia): ["
                    + string.Join(", ", _dto.GetGreaterPriorityMissions().Select(m =>
                    {
                        return ApplyIndent("\n{"
                            + $"\n\tNodeRef: {ApplyIndent(m.NodeRef.ToString()!)}, "
                            + $"\n\tPath: {ApplyIndent(m.MissionPath.ToString()!)}, "
                            + $"\n\tPriority: {ApplyIndent(m.Priority.ToString()!)}, "
                            + "\n}", 2);
                    })) + "\n\t], "
                + "\n\tMissioni in volo: ["
                    + string.Join(", ", _dto.FlyingConflictMissions.Select(m =>
                    {
                        return ApplyIndent("\n{"
                            + $"\n\tNodeRef: {ApplyIndent(m.NodeRef.ToString()!)}, "
                            + $"\n\tPath: {ApplyIndent(m.MissionPath.ToString()!)}, "
                            + $"\n\tSafe-Time-To-Fly: {m.RemainingTimeForSafeStart}, "
                            + "\n}, ", 2);
                    })) + "\n\t], ";

            return this;
        }

        public StringBuilderFromDTO AddConnectionDetails()
        {
            _dtoAsString = _dtoAsString
                + $"\n\tNodi noti: ["
                    + string.Join(", ", _dto.KnownNodes.Select(n =>
                    {
                        return $"\n\t\t{n}";
                    })) + "\n\t], ";

            if (_dto is InitStateDTO @dto)
            {
                _dtoAsString = _dtoAsString + 
                    $"\n\tConnessioni mancanti: ["
                    + string.Join(", ", @dto.MissingConnections.Select(n =>
                    {
                        return $"\n\t\t{n}";
                    })) + "\n\t], ";
            } else
            {
                _dtoAsString += $"\n\tConnessioni mancanti: [ ]";
            }

            return this;
        }


        public StringBuilderFromDTO AddFlyDetails()
        {
            _dtoAsString = _dtoAsString
                + $"\n\tAttesa pre-volo: {_dto.TotalWaitTime};"
                + $"\n\tTempo di volo: {_dto.DoneFlyTime};"
                + $"\n\tStima t. rimanente: {_dto.Path.ExpectedDuration() - _dto.DoneFlyTime}; ";

            return this;
        }

        public StringBuilderFromDTO AddExitDetails()
        {
            if (_dto is ExitStateDTO @dto)
            {
                _dtoAsString = _dtoAsString
                    + $"\n\tMissione completata: {@dto.IsMissionAccomplished};" 
                    + $"\n\tMotivazione: {@dto.Motivation};"
                    + $"\n\tTerm. per errore: {@dto.Error};"
                    + "\n\tStato precedente: "
                    + new StringBuilderFromDTO(@dto)
                        .AddBasicInfo()
                        .AddConflictInfo()
                        .AddFlyDetails()
                        .GetString(2) + ";"; 
            }

            return this;
        }

        private static string ApplyIndent(string str, int indent=1)
        {
            if (indent == 0)
            {
                return str;
            } else if (indent > 0)
            {
                return ApplyIndent(
                    str.Replace("\n", "\n\t"), 
                    indent-1
                    );
            } else
            {
                return ApplyIndent(
                    str.Replace("\n\t", "\n"),
                    indent + 1
                    );
            }
        }
    }
}
