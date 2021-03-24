using System.Net;

namespace NetComModels
{
    public class TwoEndPoints
    {
        public IPEndPoint Source { get; set; }
        public IPEndPoint Destination { get; set; }

        public TwoEndPoints()
        {
        }

        public TwoEndPoints(IPAddress source, int sourcePort, IPEndPoint destination)
        {
            Source = new IPEndPoint(source, sourcePort);
            Destination = destination;
        }

        public TwoEndPoints(IPEndPoint source, IPEndPoint destination)
        {
            Source = source;
            Destination = destination;
        }

        public override string ToString()
        {
            return $"Source = {Source}, Destination = {Destination}";
        }
    }
}