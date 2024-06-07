using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonCaptchaSolver
{
    public class MethodResultSingle<T> : MethodResult
    {
        private T? _result;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MethodResultSingle() : base()
        {
            this._result = default;
        }

        /// <summary>
        /// Default constructor with parameters.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public MethodResultSingle(bool success, string? message = null, T? result = default) : base(success, message)
        {
            this._result = result;
        }

        /// <summary>
        /// Gets the result of the method call.
        /// </summary>
        /// <returns></returns>
        public T? GetResult()
        {
            return this._result;
        }
    }
}
