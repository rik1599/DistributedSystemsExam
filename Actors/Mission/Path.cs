﻿using MathNet.Spatial.Euclidean;

namespace Actors.Mission
{
    /// <summary>
    /// Tratta che un drone vuole percorrere, si tratta
    /// di uno spostamento da un punto A ad un punto B ad una certa
    /// velocità. 
    /// 
    /// Permette di calcolare agevolmente conflitti (con un margine) e 
    /// di ottenere la distanza (in termini temporali) da un certo punto.
    /// </summary>
    public class Path
    {
        public Point2D StartPoint { get; private set; }
        public Point2D EndPoint { get; private set; }
        
        /// <summary>
        /// Velocità alla quale il drone si sposta. Si misura in 
        /// unità spaziali al secondo.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="speed">Velocità in unità spaziali al secondo</param>
        public Path(Point2D startPoint, Point2D endPoint, float speed)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Speed = speed;
        }

        /// <summary>
        /// Calcola se esiste un punto di conflitto con un altra tratta 
        /// e - in caso - determina qual è il punto di conflitto più 
        /// vicino al mio punto di partenza. 
        /// </summary>
        /// <param name="p"></param>
        /// <returns>
        /// L'eventuale punto di conflitto più vicino al mio punto di partenza.
        /// 
        /// Se il punto coincide con il mio punto di partenza o quello 
        /// di arrivo si ritorna direttamente il punto di partenza o di arrivo.
        /// 
        /// Se non ci sono conflitti tra le due tratte, si ritorna null.
        /// </returns>
        public Point2D? ClosestConflictPoint(Path p)
        {
            return null;
        }

        /// <summary>
        /// Calcola quanto tempo ci metto a raggiungere
        /// un certo punto a partire dal mio punto 
        /// di partenza.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public TimeSpan TimeDistance(Point2D p)
        {
            return TimeSpan.Zero;
        }
    }
}
