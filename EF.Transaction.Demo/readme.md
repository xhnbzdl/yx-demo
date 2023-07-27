1. 事务只要不提交会自动回滚
2. 事务范围内的代码只要发生异常，该事务就会自动回滚
3. 事务可以嵌套，只要最外层的事务没有提交，即使内部事务正常提交，也会被回滚。
4. 在事务范围内，可以临时获取实体保存后的主键Id，但如果后续发生异常，数据将被回滚。

### `TransactionScope`

1. 使用`TransactionScope`时，不一定要使用`using`将代码包装起来，当你创建了一个`TransactionScope`时就相当于开启了一个事务，而这个事务的生效范围则是`TransactionScope`实例所咋的作用域，或者是`TransactionScope`实例拥有者被使用的作用域。如下是一个正常的实例：

   ```c#
   static void Main(string[] args)
   {
       using (UnitOfWorkImpl unitOfWork = UnitOfWorkImpl.Begin())
       {
           var stu = new Student("TranscationScopeTest");
           dbContext.Students.Add(stu);
           dbContext.SaveChanges();
           Console.WriteLine(stu.Id);
       }
   }
           
    class UnitOfWorkImpl : IDisposable
    {
        public static UnitOfWorkImpl Begin()
        {
            var u = new UnitOfWorkImpl();
            u.StartTransaction();
            return u;
        }
        public TransactionScope CurrentTransaction { get; set; }
   
        public void StartTransaction()
        {
            CurrentTransaction = new TransactionScope();
        }
   
        public void Dispose()
        {
            CurrentTransaction.Complete();
            CurrentTransaction.Dispose();
            CurrentTransaction = null;
        }
    }
   ```

2. 只有当`TransactionScope`调用了`Dispose()`，事务才会真正的提交。`Complete()`方法是将事务标记为完成状态，并准备提交。所以使用时必须先调用`Complete()`再调用`Dispose（）`，二者缺一不可。

3. `TransactionScope`同样可以事务嵌套，并且不一定要使用`using`里面再嵌套`using`这种写法。如下是一种事务嵌套的示例：

   ```c#
   static void Main(string[] args)
   {
       // 如果scope不执行Complete和Dispose，即使using包装的代码块完成了一个事务并且调用了TransactionScope的Complete和Dispose，这个
       // 事务也不会被真正的提交到数据库
       var scope = new TransactionScope();
   
       using (UnitOfWorkImpl unitOfWork = UnitOfWorkImpl.Begin())
       {
           var stu = new Student("TranscationScopeTest");
           dbContext.Students.Add(stu);
           dbContext.SaveChanges();
           Console.WriteLine(stu.Id);
       }
       scope.Complete();
       scope.Dispose();
   }
   ```

   