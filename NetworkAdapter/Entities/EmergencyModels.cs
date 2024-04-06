using NetworkAdapter.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code.Scripts.NetworkAdapter.Entities
{
    public class EmergencyVM
    {
        public List<NoticeDto> ForceUpdates { get; set; } = new List<NoticeDto>();
    }

    public class EmergencyParams
    {
        public int ClientVersionNum { get; set; }
        public string Platform { get; set; }
    }

}
