﻿using System;
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
        internal async ValueTask<TResult> ExecuteAsync<TResult>(ValueTask<TResult> task, TResult defaultResult = default)
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
        /// <param name="iterations">Number of executing Action iterations.</param>
        internal void Execute(Action action, uint iterations = 1)
        {
            if (this._useExceptionHandler)
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        action();
                    }
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
                for (var i = 0; i < iterations; i++)
                {
                    action();
                }
                this.PerformanceMeter.InnerStopwatch.Start();
            }
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <param name="task">Executed Task.</param>
        /// <param name="iterations">Number of executing Task iterations.</param>
        internal async ValueTask ExecuteAsync(ValueTask task, uint iterations = 1)
        {
            if (this._useExceptionHandler)
            {
                if (this._stopWatching)
                    this.PerformanceMeter.InnerStopwatch.Stop();
                try
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        await task;
                    }
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
                for (var i = 0; i < iterations; i++)
                {
                    await task;
                }
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
        /// <param name="iterations">Number of executing Action iterations.</param>
        public static void Start<TClass, TException>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, Action action, uint iterations = 1) where TClass : class where TException : Exception
        {
            codeExecutorBuilder.Execute(action, iterations);
        }

        /// <summary>
        /// Execute the Task.
        /// </summary>
        /// <typeparam name="TClass">Class with methods.</typeparam>
        /// <typeparam name="TException">Type of exception of the exception handler.</typeparam>
        /// <param name="codeExecutorBuilder"><see cref="CodeExecutorBuilder{TClass, TException}"/>.</param>
        /// <param name="task">Executed Task.</param>
        /// <param name="iterations">Number of executing Task iterations.</param>
        public static async ValueTask StartAsync<TClass, TException>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, ValueTask task, uint iterations = 1) where TClass : class where TException : Exception
        {
            await codeExecutorBuilder.ExecuteAsync(task, iterations).ConfigureAwait(false);
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
        public static async ValueTask<TResult> StartAsync<TClass, TException, TResult>(this CodeExecutorBuilder<TClass, TException> codeExecutorBuilder, ValueTask<TResult> task, TResult defaultResult = default) where TClass : class where TException : Exception
        {
            return await codeExecutorBuilder.ExecuteAsync(task, defaultResult).ConfigureAwait(false);
        }

        #endregion
    }
}
