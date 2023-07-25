using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF.Transaction.Demo.Entitys
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Student(string name)
        {
            Name = name;
        }
    }
}
