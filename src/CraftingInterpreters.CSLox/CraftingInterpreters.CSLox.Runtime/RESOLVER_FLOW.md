# Resolver & Environment — One Pocket Reference

The resolver is a **static pre-pass** that runs between the parser and the interpreter.
It walks the same AST the interpreter will walk, but instead of *running* code it just
answers one question for every variable use: **"how many scopes up do I find this name?"**
That number (the *distance*) is handed to the interpreter so that at runtime a variable
lookup is an exact hop count instead of a search — this is what freezes each variable to
the scope it was written in (fixes the closure-capture bug).

```
Source ──> Scanner ──> Parser ──> AST ──> [ LoxResolver ] ──> LoxInterpreter
                                            (static pass)        (runtime)
                                                 │                   ▲
                                                 └── Resolve(expr, depth) ──> _locals map
```

---

## Pipeline in one picture

```
LoxResolver.Resolve(statements)          LoxInterpreter (later, at runtime)
        │                                         │
        │  walks AST, tracks scopes               │  reads _locals[expr] = distance
        │                                         │
        └── ResolveLocal(expr, name) ────────────▶ Resolve(expr, distance)
                    finds distance                   stores in _locals dict
                                                          │
                                                          ▼
                                             LookupVariable / Assign
                                                          │
                                                          ▼
                                             Environment.GetAt(distance)
                                                          │
                                                          ▼
                                             Ancestor(distance).Get(name)
                                               hop N enclosing envs, read value
```

---

## The scope stack

`_scopes` is a `List<Dictionary<string, bool>>` used as a **stack** (last = innermost).
The `bool` per name is the **declare/define** state:

| Value        | Meaning                                                        |
|--------------|---------------------------------------------------------------|
| `false`      | **Declared** — name exists but its initializer isn't ready yet |
| `true`       | **Defined** — initializer finished, safe to read               |
| not present  | Not in this scope                                              |

This two-step is what catches `var a = a;` (reading a variable inside its own initializer).

---

## Core helpers (the plumbing)

| Method            | Timeline / what it does                                                                 |
|-------------------|------------------------------------------------------------------------------------------|
| `BeginScope()`    | push a new empty dictionary onto `_scopes`                                                |
| `EndScope()`      | pop the innermost dictionary off                                                          |
| `Declare(name)`   | add `name -> false` to innermost scope; errors on duplicate name in same scope           |
| `Define(name)`    | flip `name -> true` in innermost scope                                                    |
| `Resolve(...)`    | dispatch — calls `Accept(this)` so the right `Visit*` runs (list / stmt / expr overloads) |
| `ResolveLocal`    | walk `_scopes` innermost→outermost; on first hit, tell interpreter the distance           |
| `ResolveFunction` | open a scope, declare+define params, resolve body, close scope (tracks function type)     |

### `ResolveLocal` — the heart of it

```
for i from innermost..outermost:
    if scope[i] contains name:
        interpreter.Resolve(expr, (top index) - i)   // distance = hops from current scope
        return
# never found -> leave it out of _locals -> interpreter treats it as GLOBAL
```

`distance = _scopes.Count - 1 - i` → `0` means "this very scope", `1` means "one scope out", etc.

---

## Visitor methods — grouped by what they do

**1. Pure pass-through** (just resolve children, no scope logic):
`Binary, Unary, Grouping, Logical, Call, Print, Expression, If, While`
→ they only call `Resolve(child)` on their sub-parts. Literals/stepping resolve to nothing.

**2. The 4 that actually matter** (these carry the whole idea):

| Visit method               | Timeline                                                                                              |
|----------------------------|-------------------------------------------------------------------------------------------------------|
| `VisitBlockLoxStatement`   | `BeginScope` → resolve inner statements → `EndScope`                                                   |
| `VisitVariableLoxStatement`| `Declare(name)` → resolve initializer (if any) → `Define(name)` — the declare-before-define split      |
| `VisitVariableLoxExpression` (a *read*) | if name is declared-but-not-defined in current scope → error; else `ResolveLocal`         |
| `VisitAssignLoxExpression` (a *write*)  | resolve the value expression → `ResolveLocal` to bind the target                         |

**3. Functions:**

| Visit method                 | Timeline                                                                                  |
|------------------------------|--------------------------------------------------------------------------------------------|
| `VisitFunctionLoxStatement`  | `Declare` + `Define` the name **immediately** (so the body can recurse) → `ResolveFunction` |
| `ReturnStatement`            | error if `_currentFunction == None` (return outside a function) → resolve the value         |

> Why declare+define the function name up front but split it for variables?
> A function should be able to refer to itself (recursion); a variable's initializer should not.

---

## `_currentFunction` — the guard flag

`ResolveFunction` saves the old value, sets the new type, and restores it on the way out
(a manual stack). Its only job right now: let `VisitReturnLoxStatement` reject a `return`
that sits at top-level code.

```
enclosing = _currentFunction     // save
_currentFunction = Function       // enter
   ...resolve body...
_currentFunction = enclosing     // restore
```

---

## Runtime side (why the distance matters)

Once resolution is done, `_locals` holds `expr -> distance`. At runtime:

- `LookupVariable(name, expr)`: if `expr` is in `_locals` → `Environment.GetAt(distance, name)`;
  otherwise → `Globals.Get(name)`.
- `Environment.GetAt(distance)` → `Ancestor(distance).Get(name)` → hop `distance` enclosing
  environments, then read. No name search, no ambiguity — the scope is frozen at resolve time.
- Assignment mirrors this with `AssignAt(distance, ...)`.

---

## The whole trip for one variable

```
var a = 1;
{ fun f() { print a; } f(); }
```

1. `VisitVariableLoxStatement(a)` → Declare(a=false), Define(a=true) at global-ish scope.
2. Block → BeginScope.
3. `VisitFunctionLoxStatement(f)` → Declare+Define f, then ResolveFunction:
   BeginScope (params), resolve body.
4. Body `print a` → `VisitVariableLoxExpression(a)` → ResolveLocal walks out until it finds `a`,
   computes the hop count, calls `interpreter.Resolve(exprForA, distance)`.
5. EndScope (fn), EndScope (block).
6. **Runtime:** interpreting `print a` → `LookupVariable` finds distance in `_locals`
   → `GetAt(distance)` hops that many environments → reads the *correct* `a`, even though the
   call happens in a different environment than where `a` lived.
