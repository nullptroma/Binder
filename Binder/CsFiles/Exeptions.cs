using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Binder
{
    class CommandNotFound:Exception//исключение если команда не найдена
    {
        public CommandNotFound(string msg):base(msg)
        {
        }
    }

    class BreakedExeption : Exception//исключение для оператор break
    {
        public BreakedExeption(string msg) : base(msg)
        {
        }
    }

    class ReturnExeption : Exception//исключение для оператора return
    {
        public ReturnExeption(string msg) : base(msg)
        {
        }
    }
}
