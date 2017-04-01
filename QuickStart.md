# Introduction to Undo Manager
Undo Manager is designed for easy to use where clients can register methods to be invoked, should an undo be requested. When you perform an operation that changes the state of an object, you can also record with the undo manager an operation that can reverse this operation.
## Undo Operations
An undo operation is a method with its arguments, needed to revert an object to its former state. Since the Undo Manager supports redo, the undo operations should be revertive. Means that the method which is registered for an undo operation should be able to register itself as the redo operation.

## Undo Redo Stacks
The Undo Manager maintains two stack, one for the undo and one for the redo operations. A registered undo operation will be pushed on top of the stack. Performing undo will remove and execute the top most undo operation from the undo stack.
If an undo operation is registered while performing undo, it will be pushed on top of the redo stack. Usually the method of an undo operation will register itself its reverse operation. See the examples below.

## Register Undo Operations
To register an undo operation simply record the corresponding method with its argument.

{{
UndoManager.DefaultUndoManager.RegisterInvocation(this, p => p.UndoOperation());
}}
The `RegisterInvocation` method requires the target instance of the method call and a [http://msdn.microsoft.com/en-us/library/bb397687.aspx lambda expression](http___msdn.microsoft.com_en-us_library_bb397687.aspx-lambda-expression) describing the method call.
## Performing Undo and Redo
Undo and Redo is as simple as calling the `Undo` or `Redo`  method of the `UndoManager`.

{{
UndoManager.DefaultUndoManager.Undo();
}}
Performing undo or redo will commit the currently opened transactions and than invoke all undo operations in that transaction.
## Undo, Redo - Whatever
Lets assume we have a canvas where we can set the background color. Generally these settings can be undone, so lets have a look how this could look like.

{{
public class Canvas
{
    private Color backgroundColor = Color.White;

    public void SetBackgroundColor(Color color)
    {
        Color oldBackgroundColor = this.backgroundColor;
        this.backgroundColor = color;

        UndoManager.DefaultUndoManager.RegisterInvocation(this, p => p.SetBackroundColor(oldBackgroundColor));
    }
}
}}
What happens now when `SetBackgroundColor` gets invoked? The method registers its revers operation (the same method with the old color as parameter) at the Undo Manager. While undo gets performed, the redo operation automatically gets registered by the undo operation and so on.

Sometimes it is not possible to register the same method as undo operation. Our canvas also can add and remove points.

{{
public void Add(Point point)
{
    // some add logic
    UndoManager.DefaultUndoManager.RegisterInvocation(this, p => p.Remove(point));
}

public void Remove(Point point)
{
    // some remove logic
     UndoManager.DefaultUndoManager.RegisterInvocation(this, p => p.Add(point));
}
}}
In this case the methods register an undo operations, which revert the change.  If add gets called, remove will be registered as undo operation. And also here, performing undo will register its redo operation.

## Working with Transactions
Multiple undo operations can be added to a transactions private stack. When the transaction gets committed, the whole transaction will be pushed on top of the undo stack. There are two ways to create and commit a transaction:
* Within a `using` statement.
* Explizit call of commit.

The `using` statement will automatically commit the transaction while closing the brackets.

{{
using(UndoManager.DefaultUndoManager.CreateTransaction())
{
   canvas.SetBackgroundColor(Color.Red);
   canvas.Add(new Point(10, 23));
}
}}

When we don't use a `using`statement we have to commit the transaction ourself.

{{
UndoManager.DefaultUndoManager.CreateTransaction();

canvas.SetBackgroundColor(Color.Red);
canvas.Add(new Point(10, 23));

UndoManager.DefaultUndoManager.CommitTransaction();
}}

In both cases, performing undo will invoke all in the transaction registered undo operation in reverse order. If a transaction is not yet committed, while performing undo, Undo Manager will automatically commit the transaction before performing undo.

## Rollback a Transaction
As long as a Transaction is not committed it will not be pushed on the undo stack and can be rolled back. Rolling back a transaction will invoke all contained undo operations, but they won't be pushed on the undo nor on the redo stack.

{{
UndoManager.DefaultUndoManager.CreateTransaction();

canvas.SetBackgroundColor(Color.Red);
canvas.Add(new Point(10, 23));

UndoManager.DefaultUndoManager.RollbackTransaction();
}}