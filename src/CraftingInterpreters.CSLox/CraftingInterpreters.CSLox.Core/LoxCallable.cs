namespace CraftingInterpreters.CSLox.Core;

public class LoxCallable : ILoxCallable
{
    private int _artiy;
    private Func<List<object?>, object?>? _call;
    public LoxCallable()
    {

    }

    public LoxCallable(int arity)
    {
        _artiy = arity;
    }

    public LoxCallable(Func<List<object?>, object?> call)
    {
        _call = call;
    }

    public LoxCallable(int arity, Func<List<object?>, object?> call)
    {
        _artiy = arity;
    }

    public int Arity()
    {
        return _artiy;
    }

    public object? Call(LoxInterpreter interpreter, List<object?> arguments)
    {
        if (_call == null)
            return null;

        return _call(arguments);
    }
}