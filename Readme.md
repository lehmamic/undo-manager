[![Build status](https://ci.appveyor.com/api/projects/status/lenfqey6oopw5r0i/branch/master?svg=true)](https://ci.appveyor.com/project/lehmamic/undo-manager/branch/master) [![NuGet version](https://badge.fury.io/nu/Diskordia.UndoManager.svg)](https://badge.fury.io/nu/Diskordia.UndoManager)

> This library is archived and not maintained anymore.

# Project Description
Undo Manager is a recorder of undo and redo operations for .NET with the goal to take full advantage of .NET 4.0 and C# 4.0 features (i.e. lambda expressions and dynamics). The Undo Manager API is very easy to use and does not require a deep understanding of the topic.

# Why another undo framework
Most undo frameworks uses so called actions or commands to describe the undo and redo actions, which forces you to adjust your architecture to use a 3rd party object structure. So far no problem, but in case you need to exchange this framework it is quite a bit work to rework these former used object structure.

Furthermore I found it not convenient to keep adding actions for every operation which supports the undo operations. I prefer a declarative way which is more easy to use.

# Simple example
```csharp
this.Add("Monday");
this.Add("Wednesday");

UndoManager.DefaultUndoManager.Undo();

this.Add("Thuesday");

public void Add(string item)
{
    Console.WriteLine("Add {0}", item);
    UndoManager.DefaultUndoManager.RegisterInvocation(this, p => p.Remove(item)));
}

public void Remove(string item)
{
    Console.WriteLine("Remove {0}", item);
    UndoManager.DefaultUndoManager.RegisterInvocation(this, p => p.Add(item)));
}
```
See [QuickStart](https://github.com/lehmamic/undo-manager/wiki/Quickstart) for more examples.
