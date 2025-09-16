using System;
using System.Linq.Expressions;
using System.Text;

namespace DPS.MRPReport.Utils
{
	public static class PropertyPathUtil
	{
		public static string GetPropertyPath<T>(Expression<Func<T, object>> expression)
		{
			Expression expr = expression.Body;
			if(expr is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
			{
				expr = unaryExpr.Operand;
			}

			StringBuilder path = new StringBuilder();
			while(expr is MemberExpression memberExpr)
			{
				if(path.Length > 0)
				{
					path.Insert(0, ".");
				}
				path.Insert(0, memberExpr.Member.Name);
				expr = memberExpr.Expression;
			}

			return path.ToString();
		}
	}
}
