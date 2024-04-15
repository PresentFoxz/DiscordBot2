using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextCommandFramework.Models
{
    public class Weapon
    {
        public int Id { get; set; }
        public int Level { get; set; } = 1;
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Value { get; set; }
    }
}
