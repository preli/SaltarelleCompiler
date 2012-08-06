﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.JSModel
{
    public abstract class RewriterVisitorBase<TData> : IExpressionVisitor<JsExpression, TData>, IStatementVisitor<JsStatement, TData> {
        protected static IList<T> VisitCollection<T>(IList<T> orig, Func<T, T> visitor) {
            List<T> list = null;
            for (int i = 0; i < orig.Count; i++) {
                var before = orig[i];
                var after  = visitor(before);
                if (list != null) {
                    list.Add(after);
                }
                else if (!ReferenceEquals(before, after)) {
                    list = new List<T>();
                    for (int j = 0; j < i; j++)
                        list.Add(orig[j]);
                    list.Add(after);
                }
            }
            return list ?? orig;
        }

        public virtual IList<JsExpression> Visit(IList<JsExpression> expressions, TData data) {
            return VisitCollection(expressions, expr => VisitExpression(expr, data));
        }

        public virtual IList<JsObjectLiteralProperty> Visit(IList<JsObjectLiteralProperty> values, TData data) {
            return VisitCollection(values, v => Visit(v, data));
        }

        public virtual JsObjectLiteralProperty Visit(JsObjectLiteralProperty value, TData data) {
            var after = VisitExpression(value.Value, data);
            return ReferenceEquals(after, value.Value) ? value : new JsObjectLiteralProperty(value.Name, after);
        }

        public virtual JsExpression VisitExpression(JsExpression expression, TData data) {
            return expression.Accept(this, data);
        }

        public virtual JsExpression VisitArrayLiteralExpression(JsArrayLiteralExpression expression, TData data) {
            var after  = Visit(expression.Elements, data);
            return ReferenceEquals(after, expression.Elements) ? expression : JsExpression.ArrayLiteral(after);
        }

        public virtual JsExpression VisitBinaryExpression(JsBinaryExpression expression, TData data) {
            var left  = VisitExpression(expression.Left, data);
            var right = VisitExpression(expression.Right, data);
            return ReferenceEquals(left, expression.Left) && ReferenceEquals(right, expression.Right) ? expression : JsExpression.Binary(expression.NodeType, left, right);
        }

        public virtual JsExpression VisitCommaExpression(JsCommaExpression expression, TData data) {
            var after = Visit(expression.Expressions, data);
            return ReferenceEquals(after, expression.Expressions) ? expression : JsExpression.Comma(after);
        }

        public virtual JsExpression VisitConditionalExpression(JsConditionalExpression expression, TData data) {
            var test      = VisitExpression(expression.Test, data);
            var truePart  = VisitExpression(expression.TruePart, data);
            var falsePart = VisitExpression(expression.FalsePart, data);
            return ReferenceEquals(test, expression.Test) && ReferenceEquals(truePart, expression.TruePart) && ReferenceEquals(falsePart, expression.FalsePart) ? expression : JsExpression.Conditional(test, truePart, falsePart);
        }

        public virtual JsExpression VisitConstantExpression(JsConstantExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression VisitFunctionDefinitionExpression(JsFunctionDefinitionExpression expression, TData data) {
            var body = VisitBlockStatement(expression.Body, data);
            return ReferenceEquals(body, expression.Body) ? expression : JsExpression.FunctionDefinition(expression.ParameterNames, body, expression.Name);
        }

        public virtual JsExpression VisitIdentifierExpression(JsIdentifierExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression VisitInvocationExpression(JsInvocationExpression expression, TData data) {
            var method    = VisitExpression(expression.Method, data);
            var arguments = Visit(expression.Arguments, data);
            return ReferenceEquals(method, expression.Method) && ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.Invocation(method, arguments);
        }

        public virtual JsExpression VisitObjectLiteralExpression(JsObjectLiteralExpression expression, TData data) {
            var values = Visit(expression.Values, data);
            return ReferenceEquals(values, expression.Values) ? expression : JsExpression.ObjectLiteral(values);
        }

        public virtual JsExpression VisitMemberAccessExpression(JsMemberAccessExpression expression, TData data) {
            var target = VisitExpression(expression.Target, data);
            return ReferenceEquals(target, expression.Target) ? expression : JsExpression.MemberAccess(target, expression.Member);
        }

        public virtual JsExpression VisitNewExpression(JsNewExpression expression, TData data) {
            var constructor = VisitExpression(expression.Constructor, data);
            var arguments   = Visit(expression.Arguments, data);
            return ReferenceEquals(constructor, expression.Constructor) && ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.New(constructor, arguments);
        }

        public virtual JsExpression VisitUnaryExpression(JsUnaryExpression expression, TData data) {
            var operand = VisitExpression(expression.Operand, data);
            return ReferenceEquals(operand, expression.Operand) ? expression : JsExpression.Unary(expression.NodeType, operand);
        }

        public virtual JsExpression VisitTypeReferenceExpression(JsTypeReferenceExpression expression, TData data) {
            return expression;
        }

        public virtual JsExpression VisitThisExpression(JsThisExpression expression, TData data) {
            return expression;
        }

		public virtual JsExpression VisitLiteralExpression(JsLiteralExpression expression, TData data) {
			var arguments = Visit(expression.Arguments, data);
			return ReferenceEquals(arguments, expression.Arguments) ? expression : JsExpression.Literal(expression.Format, arguments);
		}

        public virtual IList<JsStatement> Visit(IList<JsStatement> statements, TData data) {
            return VisitCollection(statements, s => VisitStatement(s, data));
        }

        public virtual IList<JsSwitchSection> Visit(IList<JsSwitchSection> clauses, TData data) {
            return VisitCollection(clauses, c => Visit(c, data));
        }

        public virtual IList<JsVariableDeclaration> Visit(IList<JsVariableDeclaration> declarations, TData data) {
            return VisitCollection(declarations, d => Visit(d, data));
        }

        public virtual JsSwitchSection Visit(JsSwitchSection clause, TData data) {
            var values = VisitCollection(clause.Values, x => x != null ? x.Accept(this, data) : null);
            var body  = VisitBlockStatement(clause.Body, data);
            return ReferenceEquals(values, clause.Values) && ReferenceEquals(body, clause.Body) ? clause : new JsSwitchSection(values, body);
        }

        public virtual JsCatchClause Visit(JsCatchClause clause, TData data) {
            var body = VisitBlockStatement(clause.Body, data);
            return ReferenceEquals(body, clause.Body) ? clause : new JsCatchClause(clause.Identifier, body);
        }

        public virtual JsVariableDeclaration Visit(JsVariableDeclaration declaration, TData data) {
            var after = (declaration.Initializer != null ? VisitExpression(declaration.Initializer, data) : null);
            return ReferenceEquals(after, declaration.Initializer) ? declaration : new JsVariableDeclaration(declaration.Name, after);
        }

        public virtual JsStatement VisitStatement(JsStatement statement, TData data) {
            return statement.Accept(this, data);
        }

        public virtual JsStatement VisitComment(JsComment comment, TData data) {
            return comment;
        }

        public virtual JsStatement VisitBlockStatement(JsBlockStatement statement, TData data) {
            var after = Visit(statement.Statements, data);
            return ReferenceEquals(after, statement.Statements) ? statement : new JsBlockStatement(after);
        }

        public virtual JsStatement VisitBreakStatement(JsBreakStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement VisitContinueStatement(JsContinueStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement VisitDoWhileStatement(JsDoWhileStatement statement, TData data) {
            var condition = VisitExpression(statement.Condition, data);
            var body      = VisitBlockStatement(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new JsDoWhileStatement(condition, body);
        }

        public virtual JsStatement VisitEmptyStatement(JsEmptyStatement statement, TData data) {
            return statement;
        }

        public virtual JsStatement VisitExpressionStatement(JsExpressionStatement statement, TData data) {
            var after = VisitExpression(statement.Expression, data);
            return ReferenceEquals(after, statement.Expression) ? statement : new JsExpressionStatement(after);
        }

        public virtual JsStatement VisitForEachInStatement(JsForEachInStatement statement, TData data) {
            var objectToIterateOver = VisitExpression(statement.ObjectToIterateOver, data);
            var body = VisitStatement(statement.Body, data);
            return ReferenceEquals(objectToIterateOver, statement.ObjectToIterateOver) && ReferenceEquals(body, statement.Body) ? statement : new JsForEachInStatement(statement.LoopVariableName, objectToIterateOver, body, statement.IsLoopVariableDeclared);
        }

        public virtual JsStatement VisitForStatement(JsForStatement statement, TData data) {
            var initStatement = statement.InitStatement       != null ? VisitStatement(statement.InitStatement, data)       : null;
            var condition     = statement.ConditionExpression != null ? VisitExpression(statement.ConditionExpression, data) : null;
            var iterator      = statement.IteratorExpression  != null ? VisitExpression(statement.IteratorExpression, data)  : null;
            var body          = VisitBlockStatement(statement.Body, data);
            return ReferenceEquals(initStatement, statement.InitStatement) && ReferenceEquals(condition, statement.ConditionExpression) && ReferenceEquals(iterator, statement.IteratorExpression) && ReferenceEquals(body, statement.Body)
                 ? statement
                 : new JsForStatement(initStatement, condition, iterator, body);
        }

        public virtual JsStatement VisitIfStatement(JsIfStatement statement, TData data) {
            var test  = VisitExpression(statement.Test, data);
            var then  = VisitBlockStatement(statement.Then, data);
            var @else = statement.Else != null ? VisitBlockStatement(statement.Else, data) : null;
            return ReferenceEquals(test, statement.Test) && ReferenceEquals(then, statement.Then) && ReferenceEquals(@else, statement.Else) ? statement : new JsIfStatement(test, then, @else);
        }

        public virtual JsStatement VisitReturnStatement(JsReturnStatement statement, TData data) {
            var value = (statement.Value != null ? VisitExpression(statement.Value, data) : null);
            return ReferenceEquals(value, statement.Value) ? statement : new JsReturnStatement(value);
        }

        public virtual JsStatement VisitSwitchStatement(JsSwitchStatement statement, TData data) {
            var test = VisitExpression(statement.Expression, data);
            var clauses = Visit(statement.Clauses, data);
            return ReferenceEquals(test, statement.Expression) && ReferenceEquals(clauses, statement.Clauses) ? statement : new JsSwitchStatement(test, clauses);
        }

        public virtual JsStatement VisitThrowStatement(JsThrowStatement statement, TData data) {
            var expr = VisitExpression(statement.Expression, data);
            return ReferenceEquals(expr, statement.Expression) ? statement : new JsThrowStatement(expr);
        }

        public virtual JsStatement VisitTryStatement(JsTryStatement statement, TData data) {
            var guarded  = VisitBlockStatement(statement.GuardedStatement, data);
            var @catch   = statement.Catch != null ? Visit(statement.Catch, data) : null;
            var @finally = statement.Finally != null ? VisitBlockStatement(statement.Finally, data) : null;
            return ReferenceEquals(guarded, statement.GuardedStatement) && ReferenceEquals(@catch, statement.Catch) && ReferenceEquals(@finally, statement.Finally) ? statement : new JsTryStatement(guarded, @catch, @finally);
        }

        public virtual JsStatement VisitVariableDeclarationStatement(JsVariableDeclarationStatement statement, TData data) {
            var declarations = Visit(statement.Declarations, data);
            return ReferenceEquals(declarations, statement.Declarations) ? statement : new JsVariableDeclarationStatement(declarations);
        }

        public virtual JsStatement VisitWhileStatement(JsWhileStatement statement, TData data) {
            var condition = VisitExpression(statement.Condition, data);
            var body      = VisitBlockStatement(statement.Body, data);
            return ReferenceEquals(condition, statement.Condition) && ReferenceEquals(body, statement.Body) ? statement : new JsWhileStatement(condition, body);
        }

        public virtual JsStatement VisitWithStatement(JsWithStatement statement, TData data) {
            var @object = VisitExpression(statement.Object, data);
            var body    = VisitStatement(statement.Body, data);
            return ReferenceEquals(@object, statement.Object) && ReferenceEquals(body, statement.Body) ? statement : new JsWithStatement(@object, body);
        }

    	public virtual JsStatement VisitLabelledStatement(JsLabelledStatement statement, TData data) {
			var stmt = VisitStatement(statement.Statement, data);
    		return ReferenceEquals(stmt, statement.Statement) ? statement : new JsLabelledStatement(statement.Label, stmt);
    	}

		public virtual JsStatement VisitFunctionStatement(JsFunctionStatement statement, TData data) {
			var body = VisitBlockStatement(statement.Body, data);
			return ReferenceEquals(body, statement.Body) ? statement : new JsFunctionStatement(statement.Name, statement.ParameterNames, body);
		}

    	public virtual JsStatement VisitGotoStatement(JsGotoStatement statement, TData data) {
    		return statement;
    	}

    	public virtual JsStatement VisitYieldStatement(JsYieldStatement statement, TData data) {
    		var value = statement.Value != null ? VisitExpression(statement.Value, data) : null;
			return ReferenceEquals(value, statement.Value) ? statement : new JsYieldStatement(value);
    	}
    }
}
