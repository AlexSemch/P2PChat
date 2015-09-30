using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using P2PChat.Annotations;

namespace P2PChat.Model
{
    [Serializable]
    public class User : BaseModel
    {

        public User(IPAddress ipAddress, string name)
        {
            this._ipAddress = ipAddress;
            this._name = name;
        }

        private readonly IPAddress _ipAddress;

        public IPAddress IpAddress
        {
            get { return _ipAddress; }
        }

        private readonly string _name;

        public string Name
        {
            get { return _name; }
        }


        #region Equals override

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        protected bool Equals(User other)
        {
            return Equals(_ipAddress, other._ipAddress) && string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_ipAddress != null ? _ipAddress.GetHashCode() : 0)*397) ^
                       (Name != null ? Name.GetHashCode() : 0);
            }
        }

        #endregion

    }
}
