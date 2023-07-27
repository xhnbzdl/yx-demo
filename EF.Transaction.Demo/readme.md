1. ����ֻҪ���ύ���Զ��ع�
2. ����Χ�ڵĴ���ֻҪ�����쳣��������ͻ��Զ��ع�
3. �������Ƕ�ף�ֻҪ����������û���ύ����ʹ�ڲ����������ύ��Ҳ�ᱻ�ع���
4. ������Χ�ڣ�������ʱ��ȡʵ�屣��������Id����������������쳣�����ݽ����ع���

### `TransactionScope`

1. ʹ��`TransactionScope`ʱ����һ��Ҫʹ��`using`�������װ���������㴴����һ��`TransactionScope`ʱ���൱�ڿ�����һ�����񣬶�����������Ч��Χ����`TransactionScope`ʵ����զ�������򣬻�����`TransactionScope`ʵ��ӵ���߱�ʹ�õ�������������һ��������ʵ����

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

2. ֻ�е�`TransactionScope`������`Dispose()`������Ż��������ύ��`Complete()`�����ǽ�������Ϊ���״̬����׼���ύ������ʹ��ʱ�����ȵ���`Complete()`�ٵ���`Dispose����`������ȱһ���ɡ�

3. `TransactionScope`ͬ����������Ƕ�ף����Ҳ�һ��Ҫʹ��`using`������Ƕ��`using`����д����������һ������Ƕ�׵�ʾ����

   ```c#
   static void Main(string[] args)
   {
       // ���scope��ִ��Complete��Dispose����ʹusing��װ�Ĵ���������һ�������ҵ�����TransactionScope��Complete��Dispose�����
       // ����Ҳ���ᱻ�������ύ�����ݿ�
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

   