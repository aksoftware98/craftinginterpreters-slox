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
internal class LoxResolver : ILoxExpressionVisitor<Unit>, ILoxStatementVisitor<Unit>
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public Unit VisitExpressionLoxStatement(ExpressionLoxStatement loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitFunctionLoxStatement(FunctionLoxStatement loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitGroupingLoxExpression(GroupingLoxExpression loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitIfLoxStatement(IfLoxStatement loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitLiteralLoxExpression(LiteralLoxExpression loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitLogicalLoxExpression(LogicalLoxExpression loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitPrintLoxStatement(PrintLoxStatement loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitReturnLoxStatement(ReturnLoxStatement loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitSteppingLoxExpression(SteppingLoxExpression loxExpression)
    {
        throw new NotImplementedException();
    }

    public Unit VisitUnaryLoxExpression(UnaryLoxExpression loxExpression)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    private void Resolve(List<LoxStatement> statements)
    {
        foreach (var item in statements)
        {
            Resolve(item);
        }
    }

    private void Resolve(LoxStatement statement)
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


}


/// <summary>
/// Void type that means nothing
/// </summary>
record Unit { }