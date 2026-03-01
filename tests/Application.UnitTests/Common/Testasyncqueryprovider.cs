using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.UnitTests.Common;

public class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<T>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        // Определяем тип результата (например, для Task<User> это будет User)
        var resultType = typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>)
            ? typeof(TResult).GetGenericArguments()[0]
            : typeof(TResult);

        try
        {
            // Находим метод Execute
            var executeMethod = typeof(IQueryProvider).GetMethods()
                .First(m => m.Name == "Execute"
                    && m.IsGenericMethod
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(Expression));

            var genericExecuteMethod = executeMethod.MakeGenericMethod(resultType);

            // Выполняем запрос
            var result = genericExecuteMethod.Invoke(_inner, new[] { expression });

            // Возвращаем Task.FromResult
            var fromResultMethod = typeof(Task).GetMethods()
                .First(m => m.Name == "FromResult" && m.IsGenericMethod)
                .MakeGenericMethod(resultType);

            var taskResult = fromResultMethod.Invoke(null, new[] { result });

            return (TResult)taskResult;
        }
        catch (TargetInvocationException ex)
        {
            // Если это исключение "Sequence contains no matching element"
            if (ex.InnerException is InvalidOperationException)
            {
                // Находим метод FromException<T>
                var fromExceptionMethod = typeof(Task).GetMethods()
                    .First(m => m.Name == "FromException"
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == typeof(Exception))
                    .MakeGenericMethod(resultType);

                // Создаем Task, который завершится с исключением
                var task = fromExceptionMethod.Invoke(null, new[] { ex.InnerException });

                return (TResult)task;
            }

            // Для других исключений - пробрасываем внутреннее
            throw ex.InnerException ?? ex;
        }
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}