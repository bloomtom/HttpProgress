using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProgressTests
{
    /// <summary>
    /// A really naive progress class which doesn't do any surprising threading, pooling, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class NaiveProgress<T> : IProgress<T>
    {
        private readonly Action<T> action;

        public NaiveProgress(Action<T> action)
        {
            this.action = action;
        }

        public void Report(T value)
        {
            action.Invoke(value);
        }
    }
}
