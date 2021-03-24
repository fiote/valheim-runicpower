using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunicPower.Core {

	[Serializable]
	public class ClassesConfig {
		public List<ClassSkill> classes = new List<ClassSkill>();
	}

	[Serializable]
	public class ClassSkill {
		public int id;
		public string name;
		public string description;
		public string icon;
		public bool implemented;

		public static int GetIdByName(string name) {
			var cskill = RunicPower.cskills.Find(x => x.name == name + " Class");
			return (cskill != null) ? cskill.id : -1;
		}
	}
}
