using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftingInterpreters.CSLox.Core;

/// <summary>
/// Walks the nodes tree and checks for block statements, function declerations, variable decleration, and variable and assignment expressions
/// where variables must be resolved.
/// It's there to resolve the issue of having forzen scope for each environment.
/// </summary>
public class LoxResolver : ILoxExpressionVisitor<Unit>, ILoxStatementVisitor<Unit>
{

    private readonly LoxInterpreter _loxInterpreter;
    private readonly List<Dictionary<string, bool>> _scopes = new();

    public LoxResolver(LoxInterpreter interprter)
    {
        _loxInterpreter = interprter;
    }

    public Unit VisitAssignLoxExpression(AssignLoxExpression loxExpression)
    {
        Resolve(loxExpression.Value);
        ResolveLocal(loxExpression, loxExpression.Name);
        return new();
    }

    public Unit VisitBinaryLoxExpression(BinaryLoxExpression loxExpression)
    {
        Resolve(loxExpression.Left);
        Resolve(loxExpression.Right);
        return new();
    }

    public Unit VisitBlockLoxStatement(BlockLoxStatement loxExpression)
    {
        BeginScope();
        Resolve(loxExpression.Statements);
        EndScope();
        return new();
    }

    public Unit VisitCallLoxExpression(CallLoxExpression loxExpression)
    {
        Resolve(loxExpression.Calee);
        foreach (var item in loxExpression.Arguments)
        {
            Resolve(item);
        }

        return new();
    }

    public Unit VisitExpressionLoxStatement(ExpressionLoxStatement loxExpression)
    {
        Resolve(loxExpression.Expression);
        return new();
    }

    public Unit VisitFunctionLoxStatement(FunctionLoxStatement loxExpression)
    {
        Declare(loxExpression.Name);
        Define(loxExpression.Name);
        ResolveFunction(loxExpression);
        return new();
    }

    public Unit VisitGroupingLoxExpression(GroupingLoxExpression loxExpression)
    {
        Resolve(loxExpression.Expression);
        return new();
    }

    public Unit VisitIfLoxStatement(IfLoxStatement loxExpression)
    {
        Resolve(loxExpression.Condition);
        Resolve(loxExpression.ThenBranch);
        if (loxExpression.ElseBranch != null)
            Resolve(loxExpression.ElseBranch);
        return new();
    }

    public Unit VisitLiteralLoxExpression(LiteralLoxExpression loxExpression)
    {
        return new();
    }

    public Unit VisitLogicalLoxExpression(LogicalLoxExpression loxExpression)
    {
        Resolve(loxExpression.Left);
        Resolve(loxExpression.Right);
        return new();
    }

    public Unit VisitPrintLoxStatement(PrintLoxStatement loxExpression)
    {
        Resolve(loxExpression.Expression);
        return new();
    }

    public Unit VisitReturnLoxStatement(ReturnLoxStatement loxExpression)
    {
        if (loxExpression.Value != null)
            Resolve(loxExpression.Value);

        return new();
    }

    public Unit VisitSteppingLoxExpression(SteppingLoxExpression loxExpression)
    {
        return new();
    }

    public Unit VisitUnaryLoxExpression(UnaryLoxExpression loxExpression)
    {
        Resolve(loxExpression.Right);
        return new();
    }

    public Unit VisitVariableLoxExpression(VariableLoxExpression loxExpression)
    {
        // Check if the variable that's being assigned have actually been resolved. 
        if (_scopes.Count != 0 && !_scopes[_scopes.Count - 1][loxExpression.Name.Lexeme])
        {
            // TODO: Throw an error that can't read local variable in the iws own initializer.
        }

        ResolveLocal(loxExpression, loxExpression.Name);
        return new();
    }

    public Unit VisitVariableLoxStatement(VariableLoxStatement loxExpression)
    {
        Declare(loxExpression.Name);
        if (loxExpression.Initializer != null)
            Resolve(loxExpression.Initializer);
        Define(loxExpression.Name);
        return new();
    }

    public Unit VisitWhileLoxStatement(WhileLoxStatement loxExpression)
    {
        Resolve(loxExpression.Condition);
        Resolve(loxExpression.Statement);
        return new();
    }

    public void Resolve(List<LoxStatement> statements)
    {
        foreach (var item in statements)
        {
            Resolve(item);
        }
    }

    public void Resolve(LoxStatement statement)
    {
        statement.Accept(this);
    }

    private void Resolve(LoxExpression expression)
    {
        expression.Accept(this);
    }

    private void BeginScope()
    {
        _scopes.Add(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.RemoveAt(_scopes.Count - 1);
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;

        var scope = _scopes[0];
        scope.Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes[_scopes.Count - 1][name.Lexeme] = true;
    }

    /// <summary>
    /// Define at what level we find this varabile and pass it for the interpreter to resolve for this expression
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="name"></param>
    private void ResolveLocal(LoxExpression expression, Token name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].ContainsKey(name.Lexeme))
            {
                _loxInterpreter.Resolve(expression, _scopes.Count - 1 - i);
                return;
            }
        }
    }

    /// <summary>
    /// The function first we declare and define its name then we open a scope in the body for the parameters then we declare and define them inside.
    /// </summary>
    /// <param name="function"></param>
    private void ResolveFunction(FunctionLoxStatement function)
    {
        BeginScope();
        foreach (var item in function.Paramters)
        {
            Declare(item);
            Define(item);
        }
        EndScope();
    }

}


/// <summary>
/// Void type that means nothing
/// </summary>
public record Unit { }