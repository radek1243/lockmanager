using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LockManager.Data
{
    class Session
    {
        public string Status { get; set; }
        public string Username { get; set; }
        public string Sid { get; set; }
        public string Serial { get; set; }
        public string Block { get; set; }

        public Session(string Status, string Username, string Sid, string Serial, string Block)
        {
            this.Status = Status;
            this.Username = Username;
            this.Sid = Sid;
            this.Serial = Serial;
            this.Block = Block;
        }
    }
}
