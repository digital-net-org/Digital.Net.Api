using System.Linq.Expressions;

namespace Digital.Net.Lib.Models;

public static class PayloadBinder
{
    public static T Bind<TPayload, T>(TPayload payload)
        where TPayload : class
        where T : class
        => Binder<TPayload, T>.Map(payload);

    private static class Binder<TPayload, T>
        where TPayload : class
        where T : class
    {
        public static readonly Func<TPayload, T> Map = Build();

        private static Func<TPayload, T> Build()
        {
            var param = Expression.Parameter(typeof(TPayload), "p");
            var sourceProperties = typeof(TPayload).GetProperties();
            var bindings = typeof(T)
                .GetProperties()
                .Where(target => target.CanWrite)
                .Select(target => (target, source: sourceProperties.FirstOrDefault(s =>
                    s.Name == target.Name && s.PropertyType == target.PropertyType && s.CanRead))
                )
                .Where(x => x.source is not null)
                .Select(x => (MemberBinding)Expression.Bind(x.target, Expression.Property(param, x.source!)));

            var body = Expression.MemberInit(Expression.New(typeof(T)), bindings);
            return Expression.Lambda<Func<TPayload, T>>(body, param).Compile();
        }
    }
}