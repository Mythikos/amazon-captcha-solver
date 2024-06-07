using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonCaptchaSolver
{
    public class MethodResultList<T> : MethodResult
    {
        /// <summary>
        /// The total count of records in the data source
        /// </summary>
        public int TotalRecordCount { get; set; }

        /// <summary>
        /// The total number of records in the result set
        /// </summary>
        public int CurrentRecordCount { get; set; }

        private ICollection<T>? _result;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MethodResultList() : base()
        {
            this.CurrentRecordCount = 0;
            this.TotalRecordCount = 0;
            this._result = null;
        }

        /// <summary>
        /// Default constructor with parameters.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public MethodResultList(bool success, string? message = null, int? currentRecordCount = null, int? totalRecordCount = null, ICollection<T>? result = null) : base(success, message)
        {
            this.CurrentRecordCount = currentRecordCount ?? result?.Count ?? 0;
            this.TotalRecordCount = totalRecordCount ?? result?.Count ?? 0;
            this._result = result;
        }

        /// <summary>
        /// Gets the result of the method call.
        /// </summary>
        /// <returns></returns>
        public ICollection<T>? GetResult()
        {
            return this._result;
        }
    }
}
