using System.Linq.Expressions;
using System.Reflection;

namespace NOAM_ASISTENCIA.Server.Models.Utils.ControllerFiltering
{
    class ParsingFilterFormula
    {
        public static List<DynamicLinqExpression.Filter> PrepareFilter(string filter)
        {
            /* possible filters from SfGrid:
            startswith(tolower(Street),'sa')
            endswith(tolower(Zip),'1')
            substringof('11',tolower(Zip))
            tolower(Zip) eq '0-11'
            tolower(Zip) ne '0-11'
            Timestamp lt null
                lt, le, gt, ge

            multiple:  (startswith(tolower(Street),'sa')) and (startswith(tolower(Country),'xy'))
            */
            const string patt_startswith = "startswith(";
            const string patt_endswith = "endswith(";
            const string patt_substringof = "substringof(";
            const string patt_tolower = "tolower(";

            try
            {
                if (filter == null || filter.Length < 2)
                    return new List<DynamicLinqExpression.Filter>();

                List<DynamicLinqExpression.Filter> listFilter = new List<DynamicLinqExpression.Filter>();

                if (filter.Substring(0, 1) == "(" && filter.Substring(filter.Length - 1, 1) == ")")
                    filter = filter.Substring(1, filter.Length - 2); // remove first and last parentheses

                string[] filterparts = filter.Split(") and ("); // get parts for multiple selection
                foreach (string filterpart in filterparts)
                {
                    DynamicLinqExpression.Op operation = DynamicLinqExpression.Op.None;
                    string filterfield = "";
                    string filtervalue = "";

                    if (filterpart.Length > 2 && filterpart.Substring(filterpart.Length - 1) == ")" &&      //startswith(tolower(Street),'sa') or endswith(tolower(Zip),'1')
                        (filterpart.StartsWith(patt_startswith) || filterpart.StartsWith(patt_endswith)))
                    {
                        string s;
                        DynamicLinqExpression.Op op = DynamicLinqExpression.Op.None;
                        if (filterpart.StartsWith(patt_startswith))
                        {
                            s = filterpart.Substring(patt_startswith.Length);
                            op = DynamicLinqExpression.Op.StartsWith;
                        }
                        else
                        {
                            s = filterpart.Substring(patt_endswith.Length);
                            op = DynamicLinqExpression.Op.EndsWith;
                        }
                        // tolower(Street),'sa')
                        s = s.Substring(0, s.Length - 1);
                        // tolower(Street),'sa'
                        string[] p = s.Split(",", 2);
                        if (p.Length == 2)
                        {
                            filterfield = p[0]; // may be "tolower(Street)"
                            if (filterfield.StartsWith(patt_tolower) && filterfield.Contains(")"))
                            {
                                filterfield = filterfield.Split('(', ')')[1];
                            }
                            filtervalue = p[1];
                            if (filtervalue.Length >= 2 && filtervalue[0] == '\'')
                            {
                                filtervalue = filtervalue.Substring(1, filtervalue.Length - 2);
                            }
                            if (filtervalue.Length > 0 && filterfield.Length > 0)
                                operation = op;
                        }
                    }
                    else if (filterpart.Length > 2 && filterpart.Substring(filterpart.Length - 1) == ")" &&   //substringof('11-111',tolower(Zip))
                        filterpart.StartsWith(patt_substringof))
                    {
                        string s = filterpart.Substring(patt_substringof.Length);
                        // '11-111', tolower(Zip))
                        s = s.Substring(0, s.Length - 1);
                        // '11-111', tolower(Zip)
                        string[] p = s.Split(",", 2);
                        if (p.Length == 2)
                        {
                            filterfield = p[1]; // may be "tolower(Street)"
                            if (filterfield.StartsWith(patt_tolower) && filterfield.Contains(")"))
                            {
                                filterfield = filterfield.Split('(', ')')[1];
                            }
                            filtervalue = p[0];
                            if (filtervalue.Length >= 2 && filtervalue[0] == '\'')
                            {
                                filtervalue = filtervalue.Substring(1, filtervalue.Length - 2);
                            }
                            if (filtervalue.Length > 0 && filterfield.Length > 0)
                                operation = DynamicLinqExpression.Op.Contains;
                        }
                    }
                    else if (filterpart.Length > 2)   //tolower(Zip) eq '0-11' or Timestamp lt null
                    {
                        string s = filterpart;
                        string[] p;
                        if (s.StartsWith(patt_tolower) && s.Contains(")")) // tolower(Zip) eq '0-11'
                        {
                            s = s.Substring(patt_tolower.Length);
                            // Zip) eq '0-11'
                            p = s.Split(")", 2);
                        }
                        else // Zip eq '0-11'
                        {
                            p = s.Split(" ", 2);
                        }
                        if (p.Length == 2)
                        {
                            filterfield = p[0];
                            s = p[1].Trim(); // eq '0-11'
                            p = s.Split(" ", 2);
                            if (p.Length == 2)
                            {
                                filtervalue = p[1];
                                if (filtervalue.Length >= 2 && filtervalue[0] == '\'')
                                {
                                    filtervalue = filtervalue.Substring(1, filtervalue.Length - 2);
                                }
                                if (filtervalue.Length > 0 && filterfield.Length > 0)
                                {
                                    switch (p[0])
                                    {
                                        case "eq":
                                            operation = DynamicLinqExpression.Op.Equals;
                                            break;
                                        case "ne":
                                            operation = DynamicLinqExpression.Op.NotEquals;
                                            break;
                                        case "lt":
                                            operation = DynamicLinqExpression.Op.LessThan;
                                            break;
                                        case "le":
                                            operation = DynamicLinqExpression.Op.LessThanOrEqual;
                                            break;
                                        case "gt":
                                            operation = DynamicLinqExpression.Op.GreaterThan;
                                            break;
                                        case "ge":
                                            operation = DynamicLinqExpression.Op.GreaterThanOrEqual;
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    if (operation != DynamicLinqExpression.Op.None)
                    {
                        listFilter.Add(new DynamicLinqExpression.Filter { PropertyName = filterfield, Operation = operation, Value = filtervalue.ToString() });
                    };
                }

                return listFilter;
            }
            catch (Exception)
            {
                return new List<DynamicLinqExpression.Filter>();
            }
        }
    }
}