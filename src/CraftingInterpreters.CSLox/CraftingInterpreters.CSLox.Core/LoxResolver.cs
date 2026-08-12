using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
    private LoxFunctionType _currentFunction = LoxFunctionType.None;
    private List<string> _errors = new();

    public IEnumerable<string> Errors => _errors;   
    public bool HadError => _errors.Count > 0;


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
        ResolveFunction(loxExpression, LoxFunctionType.Function);
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
        if (_currentFunction == LoxFunctionType.None)
        {
            Error(loxExpression.Keyword, "Can't return from a top-level code.");
        }
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
            Error(loxExpression.Name, "Can't read local variable in its own initializer"); 
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

        var scope = _scopes[_scopes.Count - 1];
        if (scope.ContainsKey(name.Lexeme))
        {
            Error(name, "Alraedy a variable with this name in this scope.");
        }
        scope.Add(name.Lexeme, false);
    }

    private void Error(Token token, string message)
    {
        var errorMessage = $"Error at line {token.Line} on {token.Lexeme}: {message}";
        _errors.Add(errorMessage);
        throw new LoxParserException(errorMessage);
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes[_scopes.Count - 1].TryAdd(name.Lexeme,true);
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
    private void ResolveFunction(FunctionLoxStatement function, LoxFunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope();
        foreach (var item in function.Paramters)
        {
            Declare(item);
            Define(item);
        }
        Resolve(function.Body);
        EndScope();
        _currentFunction = enclosingFunction;
    }

}
