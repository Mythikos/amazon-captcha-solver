using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonCaptchaSolver
{
    public abstract class MethodResult
    {
        /// <summary>
        /// Indicates if the method call was successful or not
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The message that was returned from the method. In the case of a failure, this will contain the error message. In the case of a success, this will contain a warning or informational message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// THe result of the method call that generated this call. This can be null.
        /// </summary>
        public MethodResult? OriginMethodResult { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MethodResult()
        {
            this.Success = false;
            this.Message = string.Empty;
        }

        /// <summary>
        /// Default constructor with parameters.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <param name="result"></param>
        public MethodResult(bool success, string? message = null)
        {
            this.Success = success;
            this.Message = message ?? string.Empty;
        }

        /// <summary>
        /// Gets the message stack of the method call.
        /// </summary>
        /// <returns></returns>
        public string GetMessageStack(bool indentFirstLine = false, int indentAmount = 1)
        {
            if (this.OriginMethodResult == null)
                return string.Empty;

            var messageStack = new StringBuilder();
            if (string.IsNullOrWhiteSpace(this.Message) == false)
                messageStack.AppendLine($"{(indentFirstLine ? new string('\t', indentAmount) : string.Empty)}{this.Message}");

            var currentResult = this.OriginMethodResult;
            while (currentResult != null)
            {
                if (string.IsNullOrWhiteSpace(currentResult.Message) == false)
                    messageStack.AppendLine($"{new string('\t', indentAmount)}{currentResult.Message}");
                currentResult = currentResult.OriginMethodResult;
            }

            return messageStack.ToString();
        }

        /// <summary>
        /// Creates a new MethodResultSingle with a success status and a result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MethodResultSingle<T> CreateSuccessSingle<T>(T result, string? message = null)
        {
            return new MethodResultSingle<T>(true, message, result);
        }


        /// <summary>
        /// Creates a new MethodResultSingle with a failure status and a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MethodResultSingle<T> CreateFailureSingle<T>(string message, MethodResult? originMethodResult = null)
        {
            return new MethodResultSingle<T>(false, message, default)
            {
                OriginMethodResult = originMethodResult
            };
        }

        /// <summary>
        /// Creates a new MethodResult with a success status and a result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MethodResultList<T> CreateSuccessList<T>(ICollection<T> result, string? message = null)
        {
            return new MethodResultList<T>(true, message, result.Count(), default, result);
        }

        /// <summary>
        /// Creates a new MethodResult with a success status and a result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MethodResultList<T> CreateSuccessList<T>(ICollection<T> result, int totalCount, string? message = null)
        {
            return new MethodResultList<T>(true, message, result.Count(), totalCount, result);
        }

        /// <summary>
        /// Creates a new MethodResult with a failure status and a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MethodResultList<T> CreateFailureList<T>(string message, MethodResult? originMethodResult = null)
        {
            return new MethodResultList<T>(false, message, null, null, default)
            {
                OriginMethodResult = originMethodResult
            };
        }
    }
}
