using NetworkAdapter.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Entities
{
    public class WofConsumeResult
    {
        public UserWOF NextWOF { get; set; }
        public int CoinsBefor { get; set; }
        public int CoinsAfter { get; set; }
    }

    public class FreeCoinConsumResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int CoinBefor { get; set; } = 0;
        public int CoinAfter { get; set; } = 0;
    }

}
