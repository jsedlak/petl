using Petl.Activators;

namespace Petl
{
    public class EvalOptions
    {
        public IActivator Activator { get; set; }

        public bool ContinueOnException { get; set; } = true;
    }
}
