using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ML.Infrastructure.Helpers
{
    public static class PagingExtension
    {
        public static IQueryable<TSource> Page<TSource>(IQueryable<TSource> source, int skip, int take)
        {
            return source.Skip(((skip - 1) * take)).Take(take);
        }

        public static IEnumerable<TSource> Page<TSource>(IEnumerable<TSource> source, int skip, int take)
        {
            return source.Skip(((skip - 1) * take)).Take(take);
        }

        public static IEnumerable<TProjection> DoPaging<TDocument, TProjection>(IFindFluent<TDocument, TProjection> source, int? skip, int? take)
        {
            if (source == null || source.CountDocuments() <= 0) return Enumerable.Empty<TProjection>();

            if (skip.HasValue && take.HasValue)
            {
                skip = skip.Value <= 0 ? 1 : skip;
                take = take.Value <= 0 ? 10 : take;
                source = source.Skip(((skip - 1) * take)).Limit(take);
            }

            return source.ToEnumerable();

        }

        public static List<T> DoPaging<T>(List<T> source, int? skip, int? take)
        {
            if (source == null || !source.Any()) return new List<T>();

            if (skip.HasValue && take.HasValue)
            {
                skip = skip.Value <= 0 ? 1 : skip ?? 1;
                take = take.Value <= 0 ? 10 : take ?? 10;
                source = source.Count <= take ? source : source.GetRange(skip.Value, take.Value);
            }

            return source;
        }
    }
}
