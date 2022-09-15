using Akka.Event;
using System.Diagnostics;

namespace Actors.Utils
{
    /// <summary>
    /// Tool per effettuare il logging su console da parte
    /// degli attori, ma solo in caso che la configurazione
    /// di debug sia attiva.
    /// </summary>
    public class DebugLog
    {
        private ILoggingAdapter _logger;
        private bool _forceDeactivate = false;


        public DebugLog(ILoggingAdapter logger, bool forceDeactivate = false)
        {
            _logger = logger;
            _forceDeactivate = forceDeactivate;
        }

        [ConditionalAttribute("DEBUG")]
        public void Info(string debugMessage)
        {
            if (!_forceDeactivate) _logger.Info(debugMessage);
        }

        [ConditionalAttribute("DEBUG")]
        public void Warning(string debugMessage)
        {
            if (!_forceDeactivate) _logger.Warning(debugMessage);
        }

        [ConditionalAttribute("DEBUG")]
        public void Error(string debugMessage)
        {
            if (!_forceDeactivate) _logger.Error(debugMessage);
        }

        /// <summary>
        /// Forza la disattivazione del log (indipendentemente dall'ambiente)
        /// </summary>
        public void ForcedDeactivate()
        {
            _forceDeactivate = true;
        }

        /// <summary>
        /// Rimuovi la disattivazione forzata del log 
        /// (ora il log dipenderà solo dall'ambiente). 
        /// </summary>
        public void RemoveForcedDeactivation()
        {
            _forceDeactivate = false;
        }
    }
}
