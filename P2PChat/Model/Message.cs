using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PChat.Model
{
    [Serializable]
    public class Message : BaseModel
    {
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public User Author { get; set; }
        public bool SystemMessage { get; set; }

        public string Signature
        {
            get { return String.Format("[{0:hh:mm:ss}]  {1}", Time, Author.Name); }
        }
    }
}
