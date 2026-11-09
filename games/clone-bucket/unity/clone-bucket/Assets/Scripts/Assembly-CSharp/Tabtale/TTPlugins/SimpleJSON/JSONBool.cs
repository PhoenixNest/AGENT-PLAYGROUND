using System.Text;

namespace Tabtale.TTPlugins.SimpleJSON
{
	public class JSONBool : JSONNode
	{
		private bool m_Data;

		public override JSONNodeType Tag => (JSONNodeType)0;

		public override bool IsBoolean => false;

		public override string Value
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public override bool AsBool
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public override Enumerator GetEnumerator()
		{
			return default;
		}

		public JSONBool(bool aData)
		{
		}

		public JSONBool(string aData)
		{
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
		}

		public override bool Equals(object obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}
