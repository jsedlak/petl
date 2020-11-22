﻿using Petl.Builder;
using Petl.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Petl.Targets
{
    public abstract class ProgrammableTarget<TInput, TOutput> : ICanAddConverter<TInput, TOutput>
    {
        public ProgrammableTarget()
            : this(new IValueConverter[] { })
        {
        }

        public ProgrammableTarget(IEnumerable<IValueConverter> converters)
        {
            Converters = converters;
        }

        protected abstract void SetValue(EvalContext<TInput, TOutput> context, object value);

        public void Eval(EvalContext<TInput, TOutput> context, object value)
        {
            foreach(var converter in Converters)
            {
                value = converter.Convert(value);
            }

            SetValue(context, value);
        }

        public void AddConverter(IValueConverter valueConverter)
        {
            Converters = Converters == null ? new[] { valueConverter } : Converters.Union(new[] { valueConverter });
        }

        public IEnumerable<IValueConverter> Converters { get; set; }
    }
}
