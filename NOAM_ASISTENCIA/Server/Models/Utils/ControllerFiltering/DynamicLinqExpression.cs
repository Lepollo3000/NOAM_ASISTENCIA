using System.Linq.Expressions;
using System.Reflection;

namespace NOAM_ASISTENCIA.Server.Models.Utils.ControllerFiltering
{
    class DynamicLinqExpression
    {
        public static string GetSortString(string sSort)
        {
            try
            {
                // "SubStandard,Standard,Class desc"  ->  "SubStandard ASC, Standard ASC, Class DESC"
                string[] listSort = sSort.Split(",");
                if (listSort.Length == 0)
                    return "";

                string sOut = "";
                foreach (string s in listSort)
                {
                    if (s.Length <= 0)
                        return null; // error

                    string[] p = s.Split(" ");
                    if (p.Length < 1 || p[0].Trim().Length == 0)
                        return null; // error
                    bool bDescending = p.Length > 1 && p[1].Contains("desc");
                    string sortColumn = p[0];
                    if (sOut.Length > 0)
                        sOut += ", ";
                    sOut += p[0].Trim();
                    if (p.Length > 1 && p[1].Contains("desc"))
                        sOut += " DESC";
                    else
                        sOut += " ASC";
                }
                return sOut;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //==========  Build Where Clause Dynamically in Linq basing on https://www.codeproject.com/Tips/582450/Build-Where-Clause-Dynamically-in-Linq
        public enum Op
        {
            None,
            Equals,
            NotEquals,
            GreaterThan,
            LessThan,
            GreaterThanOrEqual,
            LessThanOrEqual,
            Contains,
            StartsWith,
            EndsWith
        }

        public class Filter
        {
            public string PropertyName { get; set; }
            public Op Operation { get; set; }
            public object Value { get; set; }
        }

        public static class ExpressionBuilder
        {
            private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
            private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });

            public static Expression<Func<T, bool>> GetExpressionFilter<T>(IList<Filter> filters)
            {
                if (filters.Count == 0)
                    return null;

                ParameterExpression param = Expression.Parameter(typeof(T), "t");
                Expression exp = null;

                if (filters.Count == 1)
                    exp = GetExpressionFilter<T>(param, filters[0]);
                else if (filters.Count == 2)
                    exp = GetExpressionFilter<T>(param, filters[0], filters[1]);
                else
                {
                    while (filters.Count > 0)
                    {
                        var f1 = filters[0];
                        var f2 = filters[1];

                        if (exp == null)
                            exp = GetExpressionFilter<T>(param, filters[0], filters[1]);
                        else
                            exp = Expression.AndAlso(exp, GetExpressionFilter<T>(param, filters[0], filters[1]));

                        filters.Remove(f1);
                        filters.Remove(f2);

                        if (filters.Count == 1)
                        {
                            exp = Expression.AndAlso(exp, GetExpressionFilter<T>(param, filters[0]));
                            filters.RemoveAt(0);
                        }
                    }
                }

                return Expression.Lambda<Func<T, bool>>(exp, param);
            }

            private static Expression GetExpressionFilter<T>(ParameterExpression param, Filter filter)
            {
                MemberExpression member = Expression.Property(param, filter.PropertyName);

                ParameterExpression table = Expression.Parameter(typeof(T), "");
                Expression column = Expression.PropertyOrField(table, filter.PropertyName);

                ConstantExpression constantExpression;
                if (column.Type == typeof(DateTime) || column.Type == typeof(DateTime?))
                {
                    DateTime? d = MyUtils.GetDateFromString(filter.Value.ToString());
                    constantExpression = Expression.Constant(d);
                }
                else if (column.Type == typeof(int) || column.Type == typeof(int?))
                {
                    int? i = MyUtils.StringToInt(filter.Value.ToString(), 0);
                    constantExpression = Expression.Constant(i);
                }
                else
                    constantExpression = Expression.Constant(filter.Value);

                UnaryExpression unaryExpression = Expression.Convert(constantExpression, column.Type);

                switch (filter.Operation)
                {
                    case Op.Equals:
                        return Expression.Equal(member, unaryExpression);

                    case Op.NotEquals:
                        return Expression.NotEqual(member, unaryExpression);

                    case Op.GreaterThan:
                        return Expression.GreaterThan(member, unaryExpression);

                    case Op.GreaterThanOrEqual:
                        return Expression.GreaterThanOrEqual(member, unaryExpression);

                    case Op.LessThan:
                        return Expression.LessThan(member, unaryExpression);

                    case Op.LessThanOrEqual:
                        return Expression.LessThanOrEqual(member, unaryExpression);

                    case Op.Contains:
                        return Expression.Call(member, containsMethod, unaryExpression);

                    case Op.StartsWith:
                        return Expression.Call(member, startsWithMethod, unaryExpression);

                    case Op.EndsWith:
                        return Expression.Call(member, endsWithMethod, unaryExpression);
                }

                return null;
            }

            private static BinaryExpression GetExpressionFilter<T>(ParameterExpression param, Filter filter1, Filter filter2)
            {
                Expression bin1 = GetExpressionFilter<T>(param, filter1);
                Expression bin2 = GetExpressionFilter<T>(param, filter2);

                return Expression.AndAlso(bin1, bin2);
            }
        }
    }
}
