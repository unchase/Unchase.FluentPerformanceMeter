using System;
using System.Threading.Tasks;

namespace Unchase.FluentPerformanceMeter.Builders
{
    /// <summary>
    /// Class for configure and execute the code.
    /// </summary>
    /// <typeparam name="TClass">Class with methods.</typeparam>
    /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
    public class CodeExecutorBuilder<TClass, TException> where TClass : class where TException : Exception
    {
        #region Properties and fields

        internal PerformanceMeter<TClass> PerformanceMeter;

        internal Action<TException> _exceptionHandler;

        private bool _useExceptionHandler;

        private bool _stopWatching;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for <see cref="CodeExecutorBuilder{TCalss, TException}"/>.
        /// </summary>
        internal CodeExecutorBuilder() { }

        /// <summary>
        /// Constructor for <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </summary>
        /// <param name="performanceMeter"><see cref="PerformanceMeter{TClass}"/>.</param>
        public CodeExecutorBuilder(PerformanceMeter<TClass> performanceMeter)
        {
            this.PerformanceMeter = performanceMeter;
        }

        #endregion

        #region Main

        /// <summary>
        /// Stop watching when executing the code.
        /// </summary>
        /// <returns>
        /// Returns <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </returns>
        internal CodeExecutorBuilder<TClass, TException> WithoutWatching()
        {
            this._stopWatching = true;
            return this;
        }

        /// <summary>
        /// Set exception handler for executed code.
        /// </summary>
        /// <param name="exceptionHandler">Exception handler for executed code.</param>
        /// <returns>
        /// Returns <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </returns>
        internal CodeExecutorBuilder<TClass, TException> WithExceptionHandler(Action<TException> exceptionHandler)
        {
            this._useExceptionHandler = true;
            this._exceptionHandler = exceptionHandler;
            return this;
        }

        /// <summary>
        /// Execute the Func.
        /// </summary>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="func">Executed Func.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Returns result.
        /// </returns>
        internal TResult Execute<TResult>(Func<TResult> func, TResult defaultResult = default)
        {
            if (this._useExceptionHandler)
            {
                TResult result = defaultResult;
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                try
                {
                    result = func();
                }
                catch (TException ex)
                {
                    if (this._exceptionHandler != null)
                        this._exceptionHandler(ex);
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    if (this.PerformanceMeter.ExceptionHandler != null)
                        this.PerformanceMeter.ExceptionHandler(ex);
                    else
                        throw;
                }
                this.PerformanceMeter.InnerStopwatch.Start();
                return result;
            }
            else
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                var result = func();
                this.PerformanceMeter.InnerStopwatch.Start();
                return result;
            }
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="task">Executed Task.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Returns result.
        /// </returns>
        internal async Task<TResult> ExecuteAsync<TResult>(Task<TResult> task, TResult defaultResult = default)
        {
            if (this._useExceptionHandler)
            {
                TResult result = defaultResult;
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                try
                {
                    result = await task;
                }
                catch (TException ex)
                {
                    if (this._exceptionHandler != null)
                        this._exceptionHandler(ex);
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    if (this.PerformanceMeter.ExceptionHandler != null)
                        this.PerformanceMeter.ExceptionHandler(ex);
                    else
                        throw;
                }
                this.PerformanceMeter.InnerStopwatch.Start();
                return result;
            }
            else
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                var result = await task;
                this.PerformanceMeter.InnerStopwatch.Start();
                return result;
            }
        }

        /// <summary>
        /// Execute the Action.
        /// </summary>
        /// <param name="action">Executed Action.</param>
        internal void Execute(Action action)
        {
            if (this._useExceptionHandler)
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                try
                {
                    action();
                }
                catch (TException ex)
                {
                    if (this._exceptionHandler != null)
                        this._exceptionHandler(ex);
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    if (this.PerformanceMeter.ExceptionHandler != null)
                        this.PerformanceMeter.ExceptionHandler(ex);
                    else
                        throw;
                }
                this.PerformanceMeter.InnerStopwatch.Start();
            }
            else
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                action();
                this.PerformanceMeter.InnerStopwatch.Start();
            }
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <param name="task">Executed Task.</param>
        internal async Task ExecuteAsync(Task task)
        {
            if (this._useExceptionHandler)
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                try
                {
                    await task;
                }
                catch (TException ex)
                {
                    if (this._exceptionHandler != null)
                        this._exceptionHandler(ex);
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    if (this.PerformanceMeter.ExceptionHandler != null)
                        this.PerformanceMeter.ExceptionHandler(ex);
                    else
                        throw;
                }
                this.PerformanceMeter.InnerStopwatch.Start();
            }
            else
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                await task;
                this.PerformanceMeter.InnerStopwatch.Start();
            }
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for <see cref="CodeExecutorBuilder{TClass, TException}"/>.
    /// </summary>
    public static class CodeExecutorBuilderExtensions
    {
        #region Extension methods

        /// <summary>
        /// Stop watching when executing the code.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <returns>
        /// Returns <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </returns>
        public static CodeExecutorBuilder<TClass, TException> WithoutWatching<TClass, TException>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder) where TClass : class where TException : Exception
        {
            return codeExecutorBuilder.WithoutWatching();
        }

        /// <summary>
        /// Set exception handler for executed code.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <param name="exceptionHandler">Exception handler for executed code.</param>
        /// <returns>
        /// Returns <see cref="CodeExecutorBuilder{TClass, TException}"/>.
        /// </returns>
        public static CodeExecutorBuilder<TClass, TException> WithExceptionHandler<TClass, TException>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, Action<TException> exceptionHandler) where TClass : class where TException : Exception
        {
            return codeExecutorBuilder.WithExceptionHandler(exceptionHandler);
        }

        /// <summary>
        /// Execute the Action.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <param name="action">Executed Action.</param>
        public static void Start<TClass, TException>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, Action action) where TClass : class where TException : Exception
        {
            codeExecutorBuilder.Execute(action);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <param name="task">Executed Task.</param>
        public static async Task StartAsync<TClass, TException>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, Task task) where TClass : class where TException : Exception
        {
            await codeExecutorBuilder.ExecuteAsync(task);
        }

        /// <summary>
        /// Execute the Func.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <param name="func">Executed Func.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Returns result.
        /// </returns>
        public static TResult Start<TClass, TException, TResult>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, Func<TResult> func, TResult defaultResult = default) where TClass : class where TException : Exception
        {
            return codeExecutorBuilder.Execute(func, defaultResult);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <param name="task">Executed Task.</param>
        /// <param name="defaultResult">Default result if exception will occured.</param>
        /// <returns>
        /// Returns result.
        /// </returns>
        public static async Task<TResult> StartAsync<TClass, TException, TResult>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, Task<TResult> task, TResult defaultResult = default) where TClass : class where TException : Exception
        {
            return await codeExecutorBuilder.ExecuteAsync(task, defaultResult);
        }

        #endregion
    }
}
