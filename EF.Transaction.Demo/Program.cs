using EF.Transaction.Demo.Entitys;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace EF.Transaction.Demo
{
    internal class Program
    {
        static MyDbContext dbContext = new MyDbContext();
        static void Main(string[] args)
        {
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


            //using (var tran = dbContext.Database.BeginTransaction())
            //{
            //    AddStudent4();
            //    AddStudent3();
            //    AddStudent2();
            //    AddStudent();
            //    tran.Commit();
            //}
        }

        /// <summary>
        /// 测试获取主键id的值
        /// </summary>
        static void AddStudent4()
        {
            using (var tran = new MyDbContext().Database.BeginTransaction())
            {
                var stu = new Student("测试4");
                dbContext.Students.Add(stu);
                dbContext.SaveChanges();
                Console.WriteLine(stu.Id);
                tran.Commit();
            }
        }

        /// <summary>
        /// 测试嵌套事务
        /// </summary>
        static void AddStudent3()
        {
            using (var tran = new MyDbContext().Database.BeginTransaction())
            {
                var stu = new Student("测试3");
                dbContext.Students.Add(stu);
                dbContext.SaveChanges();
                tran.Commit();
            }
        }

        /// <summary>
        /// 正常保存
        /// </summary>
        static void AddStudent2()
        {
            var stu = new Student("测试2");
            dbContext.Students.Add(stu);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// 随机异常
        /// </summary>
        /// <exception cref="Exception"></exception>
        static void AddStudent()
        {
            var stu = new Student("测试");
            dbContext.Students.Add(stu);
            dbContext.SaveChanges();

            var r = new Random().Next(1, 10);
            if (r % 3 == 0)
            {
                throw new Exception("事务回滚");
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
    }
}