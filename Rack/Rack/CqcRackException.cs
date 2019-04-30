using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rack;

namespace Rack
{
    public class ShieldBoxNotFoundException : Exception
    {
        public ShieldBoxNotFoundException()
        {
        }

        public ShieldBoxNotFoundException(string message)
            : base(message)
        {
        }

        public ShieldBoxNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class TemplateException : Exception
    {
        public TemplateException()
        {
        }

        public TemplateException(string message)
            : base(message)
        {
        }

        public TemplateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class BoxException : Exception
    {
        public BoxException()
        {
        }

        public BoxException(string message)
            : base(message)
        {
        }

        public BoxException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
